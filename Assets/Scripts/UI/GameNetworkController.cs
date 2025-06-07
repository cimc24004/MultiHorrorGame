using UnityEngine;
using Unity.Netcode;
using Steamworks;
using TMPro;
using MagicaCloth;

public class GameNetworkController : Singleton<GameNetworkController>
{
    [SerializeField] private Transform PlayerCreatePos;  // プレイヤー湧き位置
    [SerializeField] private NetworkObject m_playerPrefab;   // プレハブ
    [SerializeField] private TMP_InputField m_joinLobbyID;    // LobbyID入力

    void Start()
    {
        NetworkManager.Singleton.NetworkConfig.ProtocolVersion = 1;
        Debug.Log(NetworkManager.Singleton.NetworkConfig.NetworkTransport != null
            ? "Transport OK"
            : "Transport NULL!!");
        StartCoroutine(DumpWhenReady());

    }
    System.Collections.IEnumerator DumpWhenReady()
    {
        yield return new WaitUntil(() =>
            NetworkManager.Singleton != null &&
            NetworkManager.Singleton.SceneManager != null);

        DumpConfig();
    }
    // ──────────────────────────────────────
    // ボタン：ロビー作成（ホスト）
    // ──────────────────────────────────────
    public void StartHost()
    {
        byte[] handshake = { 0x01 };
        NetworkManager.Singleton.NetworkConfig.ConnectionData = handshake;
        SteamLobby.Instance.CreateLobby();
        ulong hash = NetworkManager.Singleton.NetworkConfig.GetConfig(false);
        Debug.Log($"ConfigHash={hash:X8}");

        Debug.Log("GameNetworkController: StartHost() → CreateLobby() 呼んだ");
    }

    // ──────────────────────────────────────
    // ボタン：LobbyID を入力して参加（クライアント）
    // ──────────────────────────────────────
    public void StartClient()
    {
        string id = m_joinLobbyID.text.Trim();
        if (!ulong.TryParse(id, out ulong lobbyId))
        {
            Debug.LogError($"GameNetworkController: JoinLobbyID パース失敗 → {id}");
            return;
        }
        byte[] handshake = { 0x01 };
        NetworkManager.Singleton.NetworkConfig.ConnectionData = handshake;
        ulong hash = NetworkManager.Singleton.NetworkConfig.GetConfig(false);
        Debug.Log($"ConfigHash={hash:X8}");

        SteamLobby.Instance.JoinLobby(lobbyId);
        Debug.Log($"GameNetworkController: StartClient() → JoinLobby({lobbyId}) 呼んだ");
    }

    // ──────────────────────────────────────
    // ホスト側：クライアント承認後に呼ばれる
    // ──────────────────────────────────────
    public void OnClientConnected(ulong clientId)
    {
        Debug.Log($"OnClientConnected: {clientId}");

        Vector3 pos = PlayerCreatePos ? PlayerCreatePos.position : Vector3.zero;
        NetworkObject pObj = Instantiate(m_playerPrefab, pos, Quaternion.identity);
        pObj.SpawnAsPlayerObject(clientId);

        Debug.Log($"GameNetworkController: クライアント {clientId} のプレイヤー生成 ObjectID={pObj.NetworkObjectId}");
    }
    static void DumpConfig()
    {
        var cfg = NetworkManager.Singleton.NetworkConfig;

        // ─ 基本情報 ─
        ulong hash = cfg.GetConfig();           // 全体ハッシュ
        Debug.Log($"=== NETWORK CONFIG DUMP ===");
        Debug.Log($"HASH                  : 0x{hash:X16}");
        Debug.Log($"ProtocolVersion       : {cfg.ProtocolVersion}");
        Debug.Log($"EnableSceneManagement : {cfg.EnableSceneManagement}");

        // ─ プレハブ一覧 ─
        var prefabs = cfg.Prefabs.Prefabs;      // ← 二段階アクセス
        Debug.Log($"Prefab Count          : {prefabs.Count}");
        foreach (var p in prefabs)
        {
#if UNITY_NETCODE_140_OR_NEWER
            Debug.Log($"  {p.Prefab.name,-32} PrefabHash={p.PrefabHash}");
#else
            Debug.Log($"  {p.Prefab.name,-32} PrefabHash={p.SourcePrefabGlobalObjectIdHash}");
#endif
        }

        // ─ シーン一覧 ─
        if (cfg.EnableSceneManagement)
        {
            var scenes = NetworkManager.Singleton.SceneManager.GetSynchronizedScenes(); // 2.x 系
            Debug.Log($"Scene Count           : {scenes.Count}");
            foreach (var s in scenes)
                Debug.Log($"  {s.name}");
        }

        Debug.Log($"=== END DUMP ===");

    }
}
