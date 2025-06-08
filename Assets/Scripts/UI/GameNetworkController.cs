using UnityEngine;
using Unity.Netcode;
using Steamworks;
using TMPro;
using MagicaCloth;
using System.Reflection;
using Unity.Collections;
using System;
using System.Collections;

public class GameNetworkController : Singleton<GameNetworkController>
{
    [SerializeField] private Transform PlayerCreatePos;  // プレイヤー湧き位置
    [SerializeField] private NetworkObject m_playerPrefab;   // プレハブ
    [SerializeField] private TMP_InputField m_joinLobbyID;    // LobbyID入力

    void Start()
    {
        // NetworkManager.Singleton.NetworkConfig.ProtocolVersion = 1;
        // NetworkManager.Singleton.NetworkConfig.EnableSceneManagement = true;

        Debug.Log(NetworkManager.Singleton.NetworkConfig.NetworkTransport != null
            ? "Transport OK"
            : "Transport NULL!!");
        StartCoroutine(DumpWhenReady());
        if (NetworkManager.Singleton.IsServer)
        {
            Debug.Log("これ呼ばれますかああ？ SpawnButsudansNextFrame");
            StartCoroutine(SpawnButsudanNextFrame());
        }
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            Debug.Log($"OnClientConnected: {clientId}");
            

            if (!NetworkManager.Singleton.IsServer) return;
            Vector3 pos = PlayerCreatePos ? PlayerCreatePos.position : Vector3.zero;
            NetworkObject pObj = Instantiate(m_playerPrefab, pos, Quaternion.identity);
            pObj.SpawnAsPlayerObject(clientId);

            Debug.Log($"GameNetworkController: クライアント {clientId} のプレイヤー生成 ObjectID={pObj.NetworkObjectId}");

        }

        // NetworkManager.Singleton.NetworkConfig.EnableSceneManagement = false;
    }
    System.Collections.IEnumerator DumpWhenReady()
    {
        yield return new WaitUntil(() =>
            NetworkManager.Singleton != null &&
            NetworkManager.Singleton.SceneManager != null);
        DumpConfig();
        DumpRawConfigBase64();
    }

    // ① DumpConfig : Prefab をインデックス付きで表示するよう変更
    // ────────────────────────────────────────────────────────────────
    static void DumpConfig()
    {
        var cfg  = NetworkManager.Singleton.NetworkConfig;
        ulong h  = cfg.GetConfig();               // 全体ハッシュ

        Debug.Log($"=== NETWORK CONFIG DUMP ===");
        Debug.Log($"HASH                  : 0x{h:X16}");
        Debug.Log($"ProtocolVersion       : {cfg.ProtocolVersion}");
        Debug.Log($"EnableSceneManagement : {cfg.EnableSceneManagement}");

        // ─ プレハブ一覧（順番も重要なのでインデックス付き）─
        var list = cfg.Prefabs.Prefabs;           // ← 二段階アクセス
        Debug.Log($"Prefab Count          : {list.Count}");
        for (int i = 0; i < list.Count; i++)
        {
    #if UNITY_NETCODE_140_OR_NEWER
            Debug.Log($"{i,2}: {list[i].Prefab.name,-32} PrefabHash={list[i].PrefabHash}");
    #else
            Debug.Log($"{i,2}: {list[i].Prefab.name,-32} PrefabHash={list[i].SourcePrefabGlobalObjectIdHash}");
    #endif
        }

        // ─ シーン一覧 ─
        if (cfg.EnableSceneManagement && NetworkManager.Singleton.SceneManager != null)
        {
            var scenes = NetworkManager.Singleton.SceneManager.GetSynchronizedScenes();
            Debug.Log($"Scene Count           : {scenes.Count}");
            foreach (var s in scenes)
                Debug.Log($"  {s.name}");
        }

        Debug.Log($"=== END DUMP ===");
    }

    // ────────────────────────────────────────────────────────────────
    // ② DumpRawConfigBase64 : Serialize / InternalSerialize の両方を探し、
    //    それでも無いときは NetworkConfig.GetConfigBytes() を使う方式に変更
    // ────────────────────────────────────────────────────────────────
    static void DumpRawConfigBase64()
    {
        var cfg   = NetworkManager.Singleton.NetworkConfig;
        var flags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;

        MethodInfo serialize =
            typeof(NetworkConfig).GetMethod("Serialize",        flags)
        ?? typeof(NetworkConfig).GetMethod("InternalSerialize", flags);

        byte[] bytes;

        if (serialize != null)
        {
            using var writer = new FastBufferWriter(32 * 1024, Allocator.Temp);
            serialize.Invoke(cfg, new object[] { writer, 0 });   // targetTransportId=0
            bytes = writer.ToArray();                            // 引数なしの ToArray()
        }
        else
        {
            // ★ NGO 1.7+ など：GetConfigBytes が用意されている場合はこちらで
            MethodInfo getBytes =
                typeof(NetworkConfig).GetMethod("GetConfigBytes", flags);
            if (getBytes == null)
            {
                Debug.LogError("NetworkConfig のシリアライズ関数が見つかりませんでした。Strip Level かバージョンをご確認ください。");
                return;
            }
            bytes = (byte[])getBytes.Invoke(cfg, null);
        }

        string b64 = Convert.ToBase64String(bytes);
        Debug.Log($"=== RAW CONFIG BASE64 ===\n{b64}\n=== END ===");
    }
    IEnumerator SpawnButsudanNextFrame()
    {
        yield return new WaitForSeconds(10);  // 1フレーム待つ
        Debug.Log("HousesController.Instance.SpawnButsudans--------------- OnNextFrame");

    }
}
