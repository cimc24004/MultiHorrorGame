using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Sirenix.OdinInspector;
using Unity.Netcode;

public class HousesController : NetworkSingleton<HousesController>
{
    [Header("必ず ON にする仏壇の総数")]
    [MinValue(1)]
    public int TotalButsudanToSpawn = 5;
    public List<HouseDirector> house_directors;
    public List<SilhouetteChangeController> ButudansSilhouetteChangeControllers = new List<SilhouetteChangeController>();
    private GameNetworkController game_network_controller;
    // ------------------------------- Unity -------------------------------
    public override void OnNetworkSpawn()
    {
        house_directors = FindObjectsOfType<HouseDirector>().ToList();
        game_network_controller = GameNetworkController.instance;
        if (IsServer && house_directors.Count > 0)
        {
            SpawnButsudans();
        }

    }
    private void SpawnButsudans()
    {
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

        // ── 残さない仏壇と付随オブジェクトを Despawn して非表示
        foreach (var grp in allGroups.Except(selectedGroups))
        {
            // 仏壇本体
            var bn = grp.Butsudan.GetComponent<NetworkObject>();
            if (bn.IsSpawned)
                game_network_controller.despawnedIds.Add(bn.NetworkObjectId);

                bn.Despawn(destroy: true);

            // ExtraPackages のオブジェクト
            foreach (var pkg in grp.ExtraPackages)
                foreach (var o in pkg.Objs)
                {
                    if (o == null) continue;
                    Debug.Log(o.gameObject);
                    var on = o.GetComponent<NetworkObject>();
                    if (on.IsSpawned)
                        game_network_controller.despawnedIds.Add(on.NetworkObjectId);
                        on.Despawn(destroy: true);
                }
        }

        // ── 残す仏壇だけ Silhouette に登録＆フラグ立て、付随オブジェクトはパッケージ単位で Despawn
        foreach (var grp in selectedGroups)
        {
            // シルエット用に登録
            ButudansSilhouetteChangeControllers.Add(
                grp.Butsudan.GetComponent<SilhouetteChangeController>()
            );
            // フラグ立て
            var hd = house_directors.First(h => h.Groups.Contains(grp));
            hd.ButsudanExists = true;

            // 付随オブジェクトはパッケージ毎にリセット
            var chosen = ChooseOnePackage(grp.ExtraPackages);
            foreach (var pkg in grp.ExtraPackages)
                foreach (var o in pkg.Objs)
                {
                    if (o == null) continue;
                    var no = o.GetComponent<NetworkObject>();
                    Debug.Log(no.gameObject);
                    // 選ばれてないものだけ Despawn
                    if (no.IsSpawned && pkg != chosen)
                        game_network_controller.despawnedIds.Add(no.NetworkObjectId);
                        no.Despawn(destroy: true);
                }
            // 選ばれたパッケージ内のオブジェクトは SceneObject なので初期状態のまま（Spawn()/SetActive() 不要）
        }
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
