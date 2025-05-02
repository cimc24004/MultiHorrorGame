using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Sirenix.OdinInspector;

public class HousesController : Singleton<HousesController>
{
    [Header("必ず ON にする仏壇の総数")]
    [MinValue(1)]
    public int TotalButsudanToSpawn = 5;

    private List<HouseDirector> house_directors;

    // ------------------------------- Unity -------------------------------
    private void Start()
    {
        house_directors = FindObjectsOfType<HouseDirector>().ToList();

        // ① すべての仏壇 & 付随オブジェクトを OFF
        foreach (var hd in house_directors)
        {
            foreach (var g in hd.Groups)
            {
                if (g.Butsudan) g.Butsudan.SetActive(false);

                foreach (var p in g.ExtraPackages)
                    foreach (var o in p.Objs)
                        if (o) o.SetActive(false);
            }
        }

        // ② コルーチン開始
        if (house_directors.Count > 0)
            StartCoroutine(SpawnButsudans());
    }

    // ------------------------- 仏壇ランダム生成 -------------------------
    private IEnumerator SpawnButsudans()
    {
        // HouseDirector と Group のペアを列挙
        var pairs = new List<(HouseDirector hd, ButsudanGroup grp)>();
        foreach (var hd in house_directors)
            foreach (var g in hd.Groups)
                if (g.Butsudan) pairs.Add((hd, g));

        if (pairs.Count == 0) yield break;

        // シャッフル
        pairs = pairs.OrderBy(_ => Random.value).ToList();

        // 指定数をクリップ
        int spawnNum = Mathf.Min(TotalButsudanToSpawn, pairs.Count);

        for (int i = 0; i < spawnNum; i++)
        {
            var (hd, grp) = pairs[i];

            // 1) 仏壇 ON
            grp.Butsudan.SetActive(true);
            hd.ButsudanExists = true;

            // 2) ExtraPackages から 1 つ必ず発動
            if (grp.ExtraPackages.Count > 0)
            {
                var pack = ChooseOnePackage(grp.ExtraPackages);
                foreach (var obj in pack.Objs)
                    if (obj) obj.SetActive(true);
            }

            yield return null;   // 負荷分散
        }
    }

    // ------------------------ Helper ------------------------
    private RandomSpawnPackage ChooseOnePackage(List<RandomSpawnPackage> packs)
    {
        var cands = packs.Where(p => Random.value <= p.SpawnChance).ToList();
        if (cands.Count == 0) cands = packs;        // 全外れなら強制採用
        return cands[Random.Range(0, cands.Count)];
    }
}
