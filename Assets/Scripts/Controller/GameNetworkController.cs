using UnityEngine;
using Unity.Netcode;
using TMPro;
using System.Collections.Generic;
using Unity.Netcode.Components; // ClientNetworkTransform
using System.Collections;
using Unity.Multiplayer.Samples.Utilities.ClientAuthority;

public class GameNetworkController : Singleton<GameNetworkController>
{
    [SerializeField] private Transform     PlayerCreatePos;
    [SerializeField] private NetworkObject m_playerPrefab;

    private readonly Dictionary<ulong, NetworkObject> _players = new();

    void OnEnable()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        }
    }
    void OnDisable()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }
    }

    void Start()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
                TrySpawnPlayerFor(clientId);
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        if (!NetworkManager.Singleton.IsServer) return;
        TrySpawnPlayerFor(clientId);
    }

    private void OnClientDisconnected(ulong clientId)
    {
        if (!NetworkManager.Singleton.IsServer) return;
        if (_players.TryGetValue(clientId, out var no) && no != null && no.IsSpawned)
            no.Despawn(true);
        _players.Remove(clientId);
    }

    private void TrySpawnPlayerFor(ulong clientId)
    {
        if (!NetworkManager.Singleton.IsServer) return;
        if (_players.ContainsKey(clientId)) return;

        var pos = PlayerCreatePos ? PlayerCreatePos.position : Vector3.zero;
        var rot = PlayerCreatePos ? PlayerCreatePos.rotation : Quaternion.identity;

        var pObj = Instantiate(m_playerPrefab, pos, rot);
        pObj.SpawnAsPlayerObject(clientId, true);
        _players[clientId] = pObj;

        // オーナー本人だけに初期Teleportを指示
        SendOwnerSpawnTransformClientRpc(pObj.NetworkObjectId, pos, rot, Target(clientId));
    }

    private static ClientRpcParams Target(ulong clientId) => new ClientRpcParams
    {
        Send = new ClientRpcSendParams { TargetClientIds = new[] { clientId } }
    };

    [ClientRpc]
    private void SendOwnerSpawnTransformClientRpc(ulong playerNetId, Vector3 pos, Quaternion rot, ClientRpcParams rpcParams = default)
    {
        if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(playerNetId, out var no))
            return;

        // ★ オーナー以外は触らない（non-authoritative防止）
        if (!no.IsOwner) return;

        var go  = no.gameObject;
        var cnt = go.GetComponent<ClientNetworkTransform>();

        // Teleport（CNT推奨。無ければ transform に直書きフォールバック）
        if (cnt != null) cnt.Teleport(pos, rot, go.transform.localScale);
        else             go.transform.SetPositionAndRotation(pos, rot);

        // ★ 確実にONに戻す（Latchがあれば経路で、無ければ直ON）
        var latch = go.GetComponent<OwnerSpawnLatch>();
        var cc    = go.GetComponentInChildren<CharacterController>(true);
        if (latch != null)
        {
            latch.ReleaseAfterTeleport();
        }
        else if (cc != null)
        {
            // 最低限のフォールバック
            StartCoroutine(EnableNextFixed(cc));
        }

        Debug.Log($"[GNC] initial teleport sent to owner (local {NetworkManager.Singleton.LocalClientId})");
    }

    private IEnumerator EnableNextFixed(CharacterController cc)
    {
        yield return null;
        yield return new WaitForFixedUpdate();
        if (cc != null) cc.enabled = true;
        Debug.Log("[GNC] CC enabled by fallback");
    }
}
