using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Sirenix.OdinInspector;
using Unity.Netcode;

public class HousesController : NetworkBehaviour
{
    [Header("必ず ON にする仏壇の総数")]
    [MinValue(1)]
    public int TotalButsudanToSpawn = 5;
    public List<HouseDirector> house_directors;
    public List<SilhouetteChangeController> ButudansSilhouetteChangeControllers = new List<SilhouetteChangeController>();
    private GameNetworkController game_network_controller;
    public static HousesController Instance { get; private set; }

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

        RequestHideButsudansWithGroupServerRpc();
    }
    [ServerRpc]
    private void RequestHideButsudansWithGroupServerRpc()
    {
        var hideIds = ComputeHideIds();
        HideOnAllClientsClientRpc(hideIds.ToArray(), false);

    }
    [ClientRpc]
    private void HideOnAllClientsClientRpc(ulong[] objectIds, bool setActive)
    {
        var spawnManager = NetworkManager.Singleton.SpawnManager;
        foreach (var id in objectIds)
        {
            if (spawnManager.SpawnedObjects.TryGetValue(id, out var netObj))
            {
                // シーンオブジェクトにも動的Spawnオブジェクトにも対応
                netObj.gameObject.SetActive(setActive);
            }
            else
            {
                Debug.LogWarning($"HideObjectsClientRpc: NetworkObjectId {id} が見つかりませんでした。");
            }
        }
    }
    private List<ulong> ComputeHideIds()
    {
        // ── リセット
        ButudansSilhouetteChangeControllers.Clear();
        foreach (var hd in house_directors)
            hd.ButsudanExists = false;

            var hideIds = new List<ulong>();
        // ── 1: 全グループを取得 ──
        var allGroups = house_directors
            .SelectMany(hd => hd.Groups)
            .Where(g => g.Butsudan != null)
            .ToList();

        // ── 2: ランダムで残すグループを選択 ──
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

        // ── 3: 残さないグループのオブジェクトIDを収集 ──
        foreach (var grp in allGroups.Except(selectedGroups))
        {
            // 本体
            var butNO = grp.Butsudan.GetComponent<NetworkObject>();
            if (butNO != null && butNO.IsSpawned)
                hideIds.Add(butNO.NetworkObjectId);

            // 付随オブジェクト
            foreach (var pkg in grp.ExtraPackages)
            {
                foreach (var o in pkg.Objs)
                {
                    if (o == null) continue;
                    var no = o.GetComponent<NetworkObject>();
                    if (no != null && no.IsSpawned)
                        hideIds.Add(no.NetworkObjectId);
                }
            }
        }

        return hideIds;

    }

    public void RevealButusdans(float RevealTime)
    {
        foreach (SilhouetteChangeController silhouette_change_controller in ButudansSilhouetteChangeControllers)
        {
            silhouette_change_controller.SilhouetteChangeWithAutoReset(RevealTime);
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
