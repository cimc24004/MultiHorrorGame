using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
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
    [ShowInInspector] public NetworkList<ulong> TargetHideNetworkLists = new NetworkList<ulong>();
    private PlayerStatus player_status;
    public static event Action<HousesController> OnHousesControllerInited;
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
    }
    void OnDisable()
    {
        PlayerStatus.OnLocalPlayerSpawned -= InitOnPlayerSpawn;
        TargetHideNetworkLists.OnListChanged -= OnTargetHideListChanged;
    }
    private void InitOnPlayerSpawn(PlayerStatus _playerStatus)
    {
        player_status = _playerStatus;
        Init_HousesController();
    }
    private void Init_HousesController()
    {
        Debug.Log("HousesController------InitOnPlayerSpawn");
        
        if (IsServer)
        {
            // サーバー（ホスト）側でのみロジック実行
            Debug.Log("HousesController.IsServer");
            // サーバー側でまず非表示にすべきオブジェクトを収納しておく。
            TargetSetActiveFalseIds();
            
            // サーバー側でも即座に適用
            ApplyHideObjects();
        }
        else
        {
            // if (IsHost) return;
            Debug.Log("Not HousesController.IsServer && Not IsHost");
            // クライアント側は NetworkList の変更を監視
            TargetHideNetworkLists.OnListChanged += OnTargetHideListChanged;
            // 遅延接続したクライアント用：既にNetworkListにデータがある場合は即座に適用
            if (TargetHideNetworkLists.Count > 0)
            {
                Debug.Log($"Client found existing NetworkList data ({TargetHideNetworkLists.Count} items), applying immediately");
                // 少し遅延させてからオブジェクトの非表示を実行（NetworkObjectの準備を待つ）
                StartCoroutine(DelayedApplyHideObjects());
            }
        }
        if (IsServer)
        {
            OnHousesControllerInited?.Invoke(this);
        }
    }
    // ------------------------------- Unity -------------------------------
    public override void OnNetworkSpawn()
    {
        house_directors = FindObjectsOfType<HouseDirector>().ToList();
    }

    // 遅延実行用コルーチン
    private IEnumerator DelayedApplyHideObjects()
    {
        // 1フレーム待機してからNetworkObjectの準備を確認
        yield return null;
        
        // さらに少し待機（NetworkObjectの同期完了を待つ）
        yield return new WaitForSeconds(0.1f);
        
        ApplyHideObjects();
    }

    // NetworkListが変更されたときにクライアント側で実行される
    private void OnTargetHideListChanged(NetworkListEvent<ulong> changeEvent)
    {
        if (IsServer) return; // サーバー側では実行しない（二重実行防止）
        
        Debug.Log($"NetworkList changed on client: {changeEvent.Type}, Value: {changeEvent.Value}");
        
        // NetworkListの全要素に対してオブジェクトを非表示にする
        ApplyHideObjects();
    }

    // オブジェクトを非表示にする処理（サーバー・クライアント共通）
    private void ApplyHideObjects()
    {
        int successCount = 0;
        int failCount = 0;
        Debug.Log("ここではApplyHideObjectsを実行します。");
        Debug.Log("ApplyHideObjectsのTargetHideNetworkListsの中身の個数をチェックします、");
        Debug.Log("TargetHideNetworkLists.Count: " + TargetHideNetworkLists.Count);
        foreach (ulong target_network_id in TargetHideNetworkLists)
        {

            // NetworkObjectが存在するかチェックしてからアクセス
            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(target_network_id, out NetworkObject networkObject))
            {

                if (networkObject != null && networkObject.gameObject != null && networkObject.gameObject.activeInHierarchy)
                {
                    networkObject.gameObject.SetActive(false);
                    successCount++;
                    Debug.Log($"Successfully hid NetworkObject ID: {target_network_id}");
                }
            }
            else
            {
                failCount++;
                Debug.LogWarning($"NetworkObject with ID {target_network_id} not found in SpawnedObjects dictionary");
            }
        }
        
        Debug.Log($"ApplyHideObjects completed - Success: {successCount}, Failed: {failCount}");
    }


    // サーバー側でのみ実行されるロジック
    public void TargetSetActiveFalseIds()
    {
        if (!IsServer)
        {
            Debug.LogWarning("TargetSetActiveFalseIds should only be called on server!");
            return;
        }

        
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
        var shuffledHouses = house_directors.OrderBy(_ => UnityEngine.Random.value).ToList();
        int keepCount = Mathf.Min(TotalButsudanToSpawn, shuffledHouses.Count);

        var selectedGroups = new List<ButsudanGroup>();
        for (int i = 0; i < keepCount; i++)
        {
            var hd = shuffledHouses[i];
            if (hd.Groups == null || hd.Groups.Count == 0) continue;

            var grp = hd.Groups[UnityEngine.Random.Range(0, hd.Groups.Count)];
            if (grp.Butsudan != null)
                selectedGroups.Add(grp);
        }
        foreach (HouseDirector house_director in house_directors)
        {
            //HouseDirectorのRandomObjを日表示していく。 
            //RandomObj当たったものだけ非表示
            List<GameObject> RandomObjects = house_director.RandomObjs;
            for (int i = 0; i < RandomObjects.Count; i++)
            {
                int random = UnityEngine.Random.Range(0, 5 - house_director.RandomObjProbability);
                Debug.Log("RandomDirector.RnadomObjects.random: " + random);
                if (random == 0)
                {
                    GameObject TargetRandomObj = RandomObjects[i];
                    NetworkObject RandomObjects_NetworkObject = TargetRandomObj.GetComponent<NetworkObject>();
                    TargetHideNetworkLists.Add(RandomObjects_NetworkObject.NetworkObjectId);// NetworkListに追加（自動同期）
                }

            }
        }
        // ── 残さない仏壇と付随オブジェクトを非表示対象として追加
        foreach (var grp in allGroups.Except(selectedGroups))
        {
            // 仏壇のNetworkObjectを確認してから追加
            NetworkObject Butsudan_NetworkObject = grp.Butsudan.GetComponent<NetworkObject>();
            if (Butsudan_NetworkObject != null && Butsudan_NetworkObject.IsSpawned)
            {
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
                    NetworkObject ExtraPackage_NetworkObject = o.GetComponent<NetworkObject>();
                    if (pkg != chosen)
                    {
                        if (ExtraPackage_NetworkObject != null && ExtraPackage_NetworkObject.IsSpawned)
                        {
                            Debug.Log("ExtraPackage_NetworkObject != null && ExtraPackage_NetworkObject.IsSpawned 2");
                            TargetHideNetworkLists.Add(ExtraPackage_NetworkObject.NetworkObjectId); // NetworkListに追加（自動同期）
                        }
                    }
                }

            //選ばれたやつは 削除しないで置く必ず
            foreach (var pkg in grp.ExtraPackages)
                foreach (var o in pkg.Objs)
                {
                    if (pkg == chosen)
                    {
                        NetworkObject ExtraPackage_NetworkObject = o.GetComponent<NetworkObject>();
                        if (TargetHideNetworkLists.Contains(ExtraPackage_NetworkObject.NetworkObjectId))
                        {
                            TargetHideNetworkLists.Remove(ExtraPackage_NetworkObject.NetworkObjectId); // NetworkListに追加（自動同期）
                        }
                    }
                }

            // 選ばれたパッケージ内のオブジェクトは SceneObject なので初期状態のまま（Spawn()/SetActive() 不要）
        }
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

    // ------------------------ Helper ------------------------
    private RandomSpawnPackage ChooseOnePackage(List<RandomSpawnPackage> packs)
    {
        if (packs == null || packs.Count == 0) return null;

        // ① SpawnChance 昇順で並べ替え（低確率 → 高確率）
        var ordered = packs.OrderBy(p => p.SpawnChance).ToList();

        // ② 順に抽選して最初に当たった 1 つを返す
        foreach (var p in ordered)
            if (UnityEngine.Random.value <= p.SpawnChance)
                return p;                        // 当選！

        // ③ 全ハズレ → 並べ替え済みリストからランダム選択
        return ordered[UnityEngine.Random.Range(0, ordered.Count)];
    }

    private void OnDestroy()
    {
        if (TargetHideNetworkLists != null)
        {
            TargetHideNetworkLists.OnListChanged -= OnTargetHideListChanged;
        }
    }
}