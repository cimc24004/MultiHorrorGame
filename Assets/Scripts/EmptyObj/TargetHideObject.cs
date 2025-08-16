using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Unity.Netcode;
using Unity.Multiplayer.Samples.Utilities.ClientAuthority;

public class TargetHideObject : NetworkBehaviour
{
    //どこに隠すか場所を指定！
    public Transform HideBeyondPos;
    //隠れた後その場所から出るときにどこに出るか。
    public Transform UnHideBeyondPos;

    //誰かが隠れているかどうか。
    public NetworkVariable<bool> SomeoneHided = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    //隠れるタイプは何なのか。
    public enum HideType
    {
        Stand,
        Crouch,
        FaceDown
    };
    [Header("隠れるタイプ")]
    [Tooltip("Stand: 立った状態 \n Crouch: 座った状態 \n FaceDown: うつ伏せ状態")]
    public HideType hide_type;

    [Header("Playerの子オブジェクトにあるAudioDatasに設定されてるaudioClipのKeyを指定。")]
    [SerializeField] private string AudioClipKey;

    [SerializeField] private UnityEvent2 HideAddEvent;
    [SerializeField] private UnityEvent2 UnHideAddEvent;
    [SerializeField] private UnityEvent LocalHideAddEvent;
    [SerializeField] private UnityEvent LocalUnHideAddEvent;

    private AudioSource _localPlayerHideActionAudioSource;
    private AudioDatas _localPlayerHideActionAudioDatas;

    void OnEnable()
    {
        PlayerStatus.OnLocalPlayerSpawned += InitOnLocalPlayerSpawn;
        SomeoneHided.OnValueChanged += OnSomeoneHidedChanged;
    }

    void OnDisable()
    {
        PlayerStatus.OnLocalPlayerSpawned -= InitOnLocalPlayerSpawn;
        if (SomeoneHided != null)
        {
            SomeoneHided.OnValueChanged -= OnSomeoneHidedChanged;
        }
    }

    private void InitOnLocalPlayerSpawn(PlayerStatus localPlayerStatus)
    {
        if (localPlayerStatus != null)
        {
            _localPlayerHideActionAudioDatas = localPlayerStatus.HideActionAudioDatas;
            if (_localPlayerHideActionAudioDatas != null)
            {
                _localPlayerHideActionAudioSource = _localPlayerHideActionAudioDatas.audioSource;
            }
        }
    }

    private void OnSomeoneHidedChanged(bool previousValue, bool newValue)
    {
        Debug.Log($"TargetHideObject '{this.gameObject.name}' SomeoneHided changed from {previousValue} to {newValue}");
        if (newValue)
        {
            HideAddEvent?.Invoke();
        }
        else
        {
            UnHideAddEvent?.Invoke();
        }
    }

    // ──────────────────────────────────
    // サーバー RPC：プレイヤーがこのオブジェクトに隠れることを要求
    // ──────────────────────────────────
    [ServerRpc(RequireOwnership = false)]
    public void HideRequestServerRpc(ulong playerNetId, ServerRpcParams rpcParams = default)
    {
        Debug.Log($"[Server] HideRequestServerRpc for object {this.gameObject.name} from playerNetId {playerNetId}");
        if (SomeoneHided.Value)
        {
            Debug.LogWarning($"[Server] Player {playerNetId} tried to hide in {this.gameObject.name}, but it's already occupied.");
            return;
        }

        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(playerNetId, out NetworkObject playerNetworkObj))
        {
            // ★ サーバーでは位置を動かさない（ClientAuthorityのため）
            SomeoneHided.Value = true;

            ClientRpcParams clientRpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams { TargetClientIds = new ulong[] { playerNetworkObj.OwnerClientId } }
            };
            HidingPlayerLocalActionsClientRpc(clientRpcParams);
        }
        else
        {
            Debug.LogError($"[Server] Player with NetId {playerNetId} not found for HideRequest.");
        }
    }

    // ──────────────────────────────────
    // サーバー RPC：プレイヤーがこのオブジェクトから出ることを要求
    // ──────────────────────────────────
    [ServerRpc(RequireOwnership = false)]
    public void UnHideRequestServerRpc(ulong playerNetId, ServerRpcParams rpcParams = default)
    {
        Debug.Log($"[Server] UnHideRequestServerRpc for object {this.gameObject.name} from playerNetId {playerNetId}");
        if (!SomeoneHided.Value)
        {
            Debug.LogWarning($"[Server] Player {playerNetId} tried to unhide from {this.gameObject.name}, but it's not occupied.");
        }

        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(playerNetId, out NetworkObject playerNetworkObj))
        {
            // ★ サーバーでは位置を動かさない（ClientAuthorityのため）
            SomeoneHided.Value = false;

            ClientRpcParams clientRpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams { TargetClientIds = new ulong[] { playerNetworkObj.OwnerClientId } }
            };
            UnhidingPlayerLocalActionsClientRpc(clientRpcParams);
        }
        else
        {
            Debug.LogError($"[Server] Player with NetId {playerNetId} not found for UnHideRequest.");
        }
    }

    // ──────────────────────────────────
    // クライアント RPC：隠れた本人向けのローカル処理（位置移動をオーナー自身で行う）
    // ──────────────────────────────────
    [ClientRpc]
    private void HidingPlayerLocalActionsClientRpc(ClientRpcParams clientRpcParams = default)
    {
        Debug.Log($"[Client {NetworkManager.Singleton.LocalClientId}] HidingPlayerLocalActionsClientRpc for object {this.gameObject.name}");

        // ★ ローカルプレイヤーをこの隠れオブジェクトの HideBeyondPos にテレポート
        var localPlayerObj = NetworkManager.Singleton.LocalClient?.PlayerObject?.gameObject;
        if (localPlayerObj != null && HideBeyondPos != null)
        {
            var cc  = localPlayerObj.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false; // 物理干渉防止（HideClientRpcでもOFFにしてるが順序の保険）

            // ClientNetworkTransform があれば Teleport、なければ直接代入
            var cnt = localPlayerObj.GetComponent<ClientNetworkTransform>();
            if (cnt != null)
            {
                // 一部バージョンでは Teleport のシグネチャが Nullable 引数
                // 互換性のために全値渡し版を使用
                cnt.Teleport(HideBeyondPos.position, HideBeyondPos.rotation, localPlayerObj.transform.localScale);
            }
            else
            {
                localPlayerObj.transform.SetPositionAndRotation(HideBeyondPos.position, HideBeyondPos.rotation);
            }
        }

        if (_localPlayerHideActionAudioSource != null && !string.IsNullOrEmpty(AudioClipKey) &&
            _localPlayerHideActionAudioDatas != null &&
            _localPlayerHideActionAudioDatas.audioClipDatasWithKey.ContainsKey(AudioClipKey))
        {
            _localPlayerHideActionAudioSource.PlayOneShot(
                _localPlayerHideActionAudioDatas.audioClipDatasWithKey[AudioClipKey]);
        }
        LocalHideAddEvent?.Invoke(); // RaycastPermit = false など
    }

    // ──────────────────────────────────
    // クライアント RPC：出た本人向けのローカル処理（位置移動をオーナー自身で行う）
    // ──────────────────────────────────
    [ClientRpc]
    private void UnhidingPlayerLocalActionsClientRpc(ClientRpcParams clientRpcParams = default)
    {
        Debug.Log($"[Client {NetworkManager.Singleton.LocalClientId}] UnhidingPlayerLocalActionsClientRpc for object {this.gameObject.name}");

        var localPlayerObj = NetworkManager.Singleton.LocalClient?.PlayerObject?.gameObject;
        if (localPlayerObj != null && UnHideBeyondPos != null)
        {
            var cc  = localPlayerObj.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false; // ここではONにしない。UnHideClientRpc側で最終的にONに戻す

            var cnt = localPlayerObj.GetComponent<ClientNetworkTransform>();
            if (cnt != null)
            {
                cnt.Teleport(UnHideBeyondPos.position, UnHideBeyondPos.rotation, localPlayerObj.transform.localScale);
            }
            else
            {
                localPlayerObj.transform.SetPositionAndRotation(UnHideBeyondPos.position, UnHideBeyondPos.rotation);
            }
        }

        if (_localPlayerHideActionAudioSource != null && !string.IsNullOrEmpty(AudioClipKey) &&
            _localPlayerHideActionAudioDatas != null &&
            _localPlayerHideActionAudioDatas.audioClipDatasWithKey.ContainsKey(AudioClipKey))
        {
            _localPlayerHideActionAudioSource.PlayOneShot(
                _localPlayerHideActionAudioDatas.audioClipDatasWithKey[AudioClipKey]);
        }
        LocalUnHideAddEvent?.Invoke(); // RaycastPermit = true など
    }
}
