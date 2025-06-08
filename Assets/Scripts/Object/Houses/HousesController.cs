using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Sirenix.OdinInspector;
using Unity.Netcode;
using DG.Tweening;

public class HousesController : NetworkBehaviour
{
    [Header("必ず ON にする仏壇の総数")]
    [MinValue(1)]
    public int TotalButsudanToSpawn = 5;
    public List<HouseDirector> house_directors;
    public List<SilhouetteChangeController> ButudansSilhouetteChangeControllers = new List<SilhouetteChangeController>();
    private GameNetworkController game_network_controller;
    public static HousesController Instance { get; private set; }
    [ShowInInspector] public NetworkList<ulong> TargetHideNetworkLists = new NetworkList<ulong>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    // ------------------------------- Unity -------------------------------
    public override void OnNetworkSpawn()
    {
        house_directors = FindObjectsOfType<HouseDirector>().ToList();
        game_network_controller = GameNetworkController.instance;
        Debug.Log("game_network_controller: " + game_network_controller);

        if (IsServer)
        {
            // サーバー（ホスト）側でのみロジック実行
            RequestHideButsudansWithGroupServerRpc();
        }
        else
        {
            if (!IsHost)
            {
                // クライアント側は NetworkList の変更を監視
                ApplyHideObjects();
            }
        }
    }


    // オブジェクトを非表示にする処理（サーバー・クライアント共通）
    private void ApplyHideObjects()
    {
        foreach (ulong target_network_id in TargetHideNetworkLists)
        {
            // NetworkObjectが存在するかチェックしてからアクセス
            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(target_network_id, out NetworkObject networkObject))
            {
                if (networkObject != null && networkObject.gameObject != null)
                {
                    networkObject.gameObject.SetActive(false);
                }
            }
            else
            {
                Debug.LogWarning($"NetworkObject with ID {target_network_id} not found in SpawnedObjects dictionary");
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestHideButsudansWithGroupServerRpc()
    {
        if (!IsServer) return; // 念のため、サーバーでのみ実行
        
        // サーバー側でまず非表示にすべきオブジェクトを収納しておく。
        TargetSetActiveFalseIds();
        
        // サーバー側でも即座に適用
        ApplyHideObjects();
    }

    // サーバー側でのみ実行されるロジック
    public List<ulong> TargetSetActiveFalseIds()
    {
        if (!IsServer)
        {
            Debug.LogWarning("TargetSetActiveFalseIds should only be called on server!");
            return new List<ulong>();
        }

        List<ulong> target_network_ids = new List<ulong>();
        
        // NetworkListをクリア（これによりクライアントに同期される）
        TargetHideNetworkLists.Clear();
        
        // ── リセット
        ButudansSilhouetteChangeControllers.Clear();
        foreach (var hd in house_directors)
            hd.ButsudanExists = false;

        // ── 全グループ取得
        var allGroups = house_directors
            .SelectMany(hd => hd.Groups)
            .Where(g => g.Butsudan != null)
            .ToList();

        // ── ランダムで残すグループを選択
        var shuffledHouses = house_directors.OrderBy(_ => Random.value).ToList();
        int keepCount = Mathf.Min(TotalButsudanToSpawn, shuffledHouses.Count);

        var selectedGroups = new List<ButsudanGroup>();
        for (int i = 0; i < keepCount; i++)
        {
            var hd = shuffledHouses[i];
            if (hd.Groups == null || hd.Groups.Count == 0) continue;

            var grp = hd.Groups[Random.Range(0, hd.Groups.Count)];
            if (grp.Butsudan != null)
                selectedGroups.Add(grp);
        }

        // ── 残さない仏壇と付随オブジェクトを非表示対象として追加
        foreach (var grp in allGroups.Except(selectedGroups))
        {
            // 仏壇のNetworkObjectを確認してから追加
            NetworkObject Butsudan_NetworkObject = grp.Butsudan.GetComponent<NetworkObject>();
            if (Butsudan_NetworkObject != null && Butsudan_NetworkObject.IsSpawned)
            {
                target_network_ids.Add(Butsudan_NetworkObject.NetworkObjectId);
                TargetHideNetworkLists.Add(Butsudan_NetworkObject.NetworkObjectId); // NetworkListに追加（自動同期）
            }

            // ExtraPackages のオブジェクト
            foreach (var pkg in grp.ExtraPackages)
                foreach (var o in pkg.Objs)
                {
                    if (o == null) continue;
                    NetworkObject ExtraPackage_NetworkObject = o.GetComponent<NetworkObject>();
                    if (ExtraPackage_NetworkObject != null && ExtraPackage_NetworkObject.IsSpawned)
                    {
                        target_network_ids.Add(ExtraPackage_NetworkObject.NetworkObjectId);
                        TargetHideNetworkLists.Add(ExtraPackage_NetworkObject.NetworkObjectId); // NetworkListに追加（自動同期）
                    }
                }
        }

        // ── 残す仏壇だけ Silhouette に登録＆フラグ立て、付随オブジェクトはパッケージ単位で処理
        foreach (var grp in selectedGroups)
        {
            // シルエット用に登録
            var silhouetteController = grp.Butsudan.GetComponent<SilhouetteChangeController>();
            if (silhouetteController != null)
            {
                ButudansSilhouetteChangeControllers.Add(silhouetteController);
            }
            
            // フラグ立て
            var hd = house_directors.FirstOrDefault(h => h.Groups.Contains(grp));
            if (hd != null)
            {
                hd.ButsudanExists = true;
            }

            // 付随オブジェクトはパッケージ毎にリセット
            var chosen = ChooseOnePackage(grp.ExtraPackages);
            foreach (var pkg in grp.ExtraPackages)
                foreach (var o in pkg.Objs)
                {
                    if (o == null) continue;
                    // 選ばれてないものだけ非表示対象に追加
                    if (pkg != chosen)
                    {
                        NetworkObject ExtraPackage_NetworkObject = o.GetComponent<NetworkObject>();
                        if (ExtraPackage_NetworkObject != null && ExtraPackage_NetworkObject.IsSpawned)
                        {
                            Debug.Log("ExtraPackage_NetworkObject != null && ExtraPackage_NetworkObject.IsSpawned 2");
                            target_network_ids.Add(ExtraPackage_NetworkObject.NetworkObjectId);
                            TargetHideNetworkLists.Add(ExtraPackage_NetworkObject.NetworkObjectId); // NetworkListに追加（自動同期）
                        }
                    }
                }
            // 選ばれたパッケージ内のオブジェクトは SceneObject なので初期状態のまま（Spawn()/SetActive() 不要）
        }
        return target_network_ids;
    }

    public void RevealButusdans(float RevealTime)
    {
        foreach (SilhouetteChangeController silhouette_change_controller in ButudansSilhouetteChangeControllers)
        {
            if (silhouette_change_controller != null)
            {
                silhouette_change_controller.SilhouetteChangeWithAutoReset(RevealTime);
            }
        }
    }

    // ------------------------ Helper ------------------------
    private RandomSpawnPackage ChooseOnePackage(List<RandomSpawnPackage> packs)
    {
        if (packs == null || packs.Count == 0) return null;

        // ① SpawnChance 昇順で並べ替え（低確率 → 高確率）
        var ordered = packs.OrderBy(p => p.SpawnChance).ToList();

        // ② 順に抽選して最初に当たった 1 つを返す
        foreach (var p in ordered)
            if (Random.value <= p.SpawnChance)
                return p;                        // 当選！

        // ③ 全ハズレ → 並べ替え済みリストからランダム選択
        return ordered[Random.Range(0, ordered.Count)];
    }

}