using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Sirenix.OdinInspector;
using Unity.Netcode;
using DG.Tweening;
using System;

public class HousesController : NetworkBehaviour
{
    [Header("必ず ON にする仏壇の総数")]
    [MinValue(1)]
    public int TotalButsudanToSpawn = 14;

    public List<HouseDirector> house_directors;
    public List<SilhouetteChangeController> ButudansSilhouetteChangeControllers = new List<SilhouetteChangeController>();

    public static HousesController Instance { get; private set; }

    // ※ いったん ulong のまま（最小改修）。将来的には NetworkObjectReference 推奨。
    [ShowInInspector] public NetworkList<ulong> TargetHideNetworkLists = new NetworkList<ulong>();

    private PlayerStatus player_status;
    public static event Action<HousesController> OnHousesControllerInited;

    /*━━━━━━━━ ライフサイクル ━━━━━━━━*/
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

    void OnEnable()
    {
        PlayerStatus.OnLocalPlayerSpawned += InitOnPlayerSpawn;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        PlayerStatus.OnLocalPlayerSpawned -= InitOnPlayerSpawn;
        SceneManager.sceneLoaded -= OnSceneLoaded;
        TargetHideNetworkLists.OnListChanged -= OnTargetHideListChanged;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // シーン遷移時に取り直し（非アクティブも拾う）
        house_directors = FindObjectsOfType<HouseDirector>(true).ToList();
    }

    public override void OnNetworkSpawn()
    {
        // 初回スポーン時にも収集
        house_directors = FindObjectsOfType<HouseDirector>(true).ToList();
    }

    private void InitOnPlayerSpawn(PlayerStatus _playerStatus)
    {
        player_status = _playerStatus;
        Init_HousesController();
    }

    private void Init_HousesController()
    {
        Debug.Log("HousesController------InitOnPlayerSpawn");

        // 念のため null/空チェックで再収集
        if (house_directors == null || house_directors.Count == 0)
            house_directors = FindObjectsOfType<HouseDirector>(true).ToList();

        if (IsServer)
        {
            Debug.Log("HousesController.IsServer");

            // サーバー側：非表示IDリストを構築して同期
            TargetSetActiveFalseIds();

            // サーバー側でも即時適用（ホスト視点に反映）
            ApplyHideObjects();

            OnHousesControllerInited?.Invoke(this);
        }
        else
        {
            Debug.Log("Not HousesController.IsServer && Not IsHost");
            // クライアント側は変更監視
            TargetHideNetworkLists.OnListChanged += OnTargetHideListChanged;

            // 既にデータがある場合（遅延参加など）すぐ適用（ただしNetworkObjectの準備待ち付き）
            if (TargetHideNetworkLists.Count > 0)
            {
                Debug.Log($"Client found existing NetworkList data ({TargetHideNetworkLists.Count} items), applying immediately");
                StartCoroutine(DelayedApplyHideObjects());
            }
        }
    }

    /*━━━━━━━━ イベントハンドラ ━━━━━━━━*/

    // NetworkListが変更されたときにクライアント側で実行される
    private void OnTargetHideListChanged(NetworkListEvent<ulong> changeEvent)
    {
        if (IsServer) return; // サーバー側では実行しない（二重防止）

        Debug.Log($"NetworkList changed on client: {changeEvent.Type}, Value: {changeEvent.Value}");
        ApplyHideObjects();
    }

    // 遅延実行用コルーチン：全IDが解決できるまで少し待つ（最大5秒）
    private IEnumerator DelayedApplyHideObjects()
    {
        float deadline = Time.time + 5f;
        while (Time.time < deadline)
        {
            bool allResolvable = true;
            foreach (var id in TargetHideNetworkLists)
            {
                if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects.ContainsKey(id))
                {
                    allResolvable = false;
                    break;
                }
            }
            if (allResolvable) break;
            yield return null; // 1フレ待ち
        }
        ApplyHideObjects();
    }

    /*━━━━━━━━ メイン処理 ━━━━━━━━*/

    // オブジェクトを非表示にする（サーバー・クライアント共通）
    private void ApplyHideObjects()
    {
        int successCount = 0;
        int failCount = 0;

        // 重複を排除してから適用
        var uniqueIds = TargetHideNetworkLists;

        Debug.Log("ApplyHideObjects 実行: unique count = " + uniqueIds.Count);

        foreach (ulong target_network_id in uniqueIds)
        {
            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(target_network_id, out NetworkObject networkObject))
            {
                if (networkObject != null && networkObject.gameObject != null && networkObject.gameObject.activeInHierarchy)
                {
                    networkObject.gameObject.SetActive(false);
                    successCount++;
                    // Debug.Log($"Hide: {target_network_id} ({networkObject.gameObject.name})");
                }
            }
            else
            {
                failCount++;
                // Debug.LogWarning($"NetworkObject with ID {target_network_id} not found in SpawnedObjects");
            }
        }

        Debug.Log($"ApplyHideObjects completed - Success: {successCount}, Failed: {failCount}");
    }

    // サーバー側：非表示対象IDリストを構築（集合演算で toKeep が勝つ）
    public void TargetSetActiveFalseIds()
    {
        if (!IsServer)
        {
            Debug.LogWarning("TargetSetActiveFalseIds should only be called on server!");
            return;
        }

        // NetworkList をクリア（→クライアントに同期されるのは最後の Add 群）
        TargetHideNetworkLists.Clear();

        // ── リセット
        ButudansSilhouetteChangeControllers.Clear();
        foreach (var hd in house_directors)
            hd.ButsudanExists = false;

        // ── 全グループ取得
        var allGroups = house_directors
            .Where(hd => hd != null && hd.Groups != null)
            .SelectMany(hd => hd.Groups)
            .Where(g => g != null && g.Butsudan != null)
            .ToList();

        // ── 残すグループを「家単位で」ランダム選択
        var shuffledHouses = house_directors
            .Where(h => h != null && h.Groups != null && h.Groups.Count > 0)
            .OrderBy(_ => UnityEngine.Random.value)
            .ToList();

        int keepCount = Mathf.Min(TotalButsudanToSpawn, shuffledHouses.Count);

        var selectedGroups = new List<ButsudanGroup>();
        for (int i = 0; i < keepCount; i++)
        {
            var hd = shuffledHouses[i];
            var groups = hd.Groups;
            if (groups == null || groups.Count == 0) continue;

            var grp = groups[UnityEngine.Random.Range(0, groups.Count)];
            if (grp != null && grp.Butsudan != null)
                selectedGroups.Add(grp);
        }

        // === 集合で管理 ===
        var toHide = new HashSet<ulong>(); // 消したい候補
        var toKeep = new HashSet<ulong>(); // 絶対に残す（ホワイトリスト）

        // --- HouseDirector.RandomObjs: 当たったものだけ非表示にする ---
        foreach (HouseDirector house_director in house_directors)
        {
            if (house_director == null) continue;
            var randomObjects = house_director.RandomObjs;
            if (randomObjects == null) continue;

            // 「確率の分母 (1 / N)」の意味に合わせる。1なら必ず当たり。
            int denom = Mathf.Max(1, house_director.RandomObjProbability);
            foreach (var obj in randomObjects)
            {
                if (obj == null) continue;
                bool hit = UnityEngine.Random.Range(0, denom) == 0;
                if (hit) AddIdIfSpawned(obj, toHide);
            }
        }

        // --- 残さない仏壇と付随オブジェクト → toHide ---
        foreach (var grp in allGroups.Except(selectedGroups))
        {
            AddIdIfSpawned(grp.Butsudan, toHide);

            foreach (var pkg in grp.ExtraPackages)
            {
                if (pkg?.Objs == null) continue;
                foreach (var o in pkg.Objs)
                    AddIdIfSpawned(o, toHide);
            }
        }

        // --- 残す仏壇：シルエット登録＆ButsudanExists、付随は選ばれたパッケージだけ toKeep、他は toHide ---
        foreach (var grp in selectedGroups)
        {
            // シルエット
            var silhouette = grp.Butsudan.GetComponent<SilhouetteChangeController>();
            if (silhouette != null)
                ButudansSilhouetteChangeControllers.Add(silhouette);

            // フラグ
            var ownerHouse = house_directors.FirstOrDefault(h => h != null && h.Groups != null && h.Groups.Contains(grp));
            if (ownerHouse != null)
                ownerHouse.ButsudanExists = true;

            // パッケージ抽選
            var chosen = ChooseOnePackage(grp.ExtraPackages);

            foreach (var pkg in grp.ExtraPackages)
            {
                if (pkg?.Objs == null) continue;

                foreach (var o in pkg.Objs)
                {
                    if (o == null) continue;

                    if (pkg == chosen)
                        AddIdIfSpawned(o, toKeep);  // 残す
                    else
                        AddIdIfSpawned(o, toHide);  // 消す候補
                }
            }
        }

        // --- 最終決定：toHide - toKeep（ホワイトリスト優先） ---
        var finalHide = toHide.Except(toKeep);

        // --- NetworkList へ一意に流し込む ---
        int addCount = 0;
        foreach (var id in finalHide)
        {
            TargetHideNetworkLists.Add(id);
            addCount++;
        }

        Debug.Log($"TargetSetActiveFalseIds: toHide={toHide.Count}, toKeep={toKeep.Count}, finalHide(Add)={addCount}");
    }

    [ServerRpc(RequireOwnership = false)]
    public void RevealButusdansRequestServerRpc(float RevealTime)
    {
        foreach (SilhouetteChangeController silhouette_change_controller in ButudansSilhouetteChangeControllers)
        {
            if (silhouette_change_controller != null)
            {
                silhouette_change_controller.SilhouetteChangeWithAutoReset(RevealTime);
            }
        }
    }

    /*━━━━━━━━ Helper ━━━━━━━━*/

    private RandomSpawnPackage ChooseOnePackage(List<RandomSpawnPackage> packs)
    {
        if (packs == null || packs.Count == 0) return null;

        // ① SpawnChance 昇順（低確率 → 高確率）
        var ordered = packs.OrderBy(p => p.SpawnChance).ToList();

        // ② 順に抽選して最初に当たった 1 つ
        foreach (var p in ordered)
            if (UnityEngine.Random.value <= p.SpawnChance)
                return p;

        // ③ 全ハズレ → 並べ替え済みからランダム
        return ordered[UnityEngine.Random.Range(0, ordered.Count)];
    }

    private static bool TryGetNetworkId(GameObject go, out ulong id)
    {
        id = 0UL;
        if (go == null) return false;
        var no = go.GetComponent<NetworkObject>();
        if (no != null && no.IsSpawned)
        {
            id = no.NetworkObjectId;
            return true;
        }
        return false;
    }

    private static void AddIdIfSpawned(GameObject go, HashSet<ulong> set)
    {
        if (TryGetNetworkId(go, out var id)) set.Add(id);
    }

    private void OnDestroy()
    {
        if (TargetHideNetworkLists != null)
        {
            TargetHideNetworkLists.OnListChanged -= OnTargetHideListChanged;
        }
    }
}
