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
    public List<SilhouetteChangeController> ButudansSilhouetteChangeControllers;
    // ------------------------------- Unity -------------------------------
    public override void OnNetworkSpawn()
    {
        house_directors = FindObjectsOfType<HouseDirector>().ToList();
        
        // ① すべての仏壇 & 付随オブジェクトを OFF
        foreach (var hd in house_directors)
        {
            foreach (var g in hd.Groups)
            {
                if (g.Butsudan) g.Butsudan.SetActive(false);

                foreach (var p in g.ExtraPackages)
                    foreach (var o in p.Objs){
                        if (o){
                            // NetworkObject networkObject = o.GetComponent<NetworkObject>();
                            // if(networkObject != null){
                            //     networkObject.Despawn();
                            // }
                            o.SetActive(false);
                        } 

                    }                        
            }
        }

        // ② コルーチン開始
        if (house_directors.Count > 0)
            SpawnButsudans();
    }
    public void RevealButusdans(float RevealTime){
        foreach(SilhouetteChangeController silhouette_change_controller in ButudansSilhouetteChangeControllers){
            silhouette_change_controller.SilhouetteChangeWithAutoReset(RevealTime);
        }
    }
    private void SpawnButsudans()
    {
        // ① HouseDirector をシャッフル
        var shuffledHouses = house_directors.OrderBy(_ => Random.value).ToList();

        // ② 「出現させる家数」を決定
        int houseCountToUse = Mathf.Min(TotalButsudanToSpawn, shuffledHouses.Count);

        // ③ 上から houseCountToUse 軒ぶんループ
        for (int i = 0; i < houseCountToUse; i++)
        {
            var hd = shuffledHouses[i];

            if (hd.Groups == null || hd.Groups.Count == 0)       // グループ無い家はスキップ
                continue;

            // ── この家の仏壇グループから 1 つランダム選択 ──
            var grp = hd.Groups[Random.Range(0, hd.Groups.Count)];

            if (grp.Butsudan == null)                            // 仏壇が設定されていない
                continue;

            // 1) 仏壇 ON
            grp.Butsudan.SetActive(true);
            ButudansSilhouetteChangeControllers.Add(grp.Butsudan.GetComponent<SilhouetteChangeController>());
            hd.ButsudanExists = true;

            // 2) ExtraPackages から 1 パッケージ必ず発動
            if (grp.ExtraPackages.Count > 0)
            {
                var pack = ChooseOnePackage(grp.ExtraPackages);
                foreach (var obj in pack.Objs){
                    if (obj){
                        NetworkObject networkObject = obj.GetComponent<NetworkObject>();
                        // if(networkObject != null){
                        //     networkObject.Spawn();
                        // }
                        obj.SetActive(true);
                    } 

                }
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
