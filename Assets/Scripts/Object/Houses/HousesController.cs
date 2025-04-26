using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Sirenix.OdinInspector;
using FronkonGames.Glitches.Hacked;
public class HousesController : SerializedMonoBehaviour
{
    public List<HouseDirector> house_directors;
    private GameProgressController game_progress_controller;
    // Start is called before the first frame update
    void Start()
    {
        house_directors = new List<HouseDirector>(FindObjectsOfType<HouseDirector>().ToList());
        game_progress_controller = GameProgressController.instance;
        if(house_directors.Count != 0){
            StartCoroutine(ButsudanRandomCreate());
        }
    }

    // Update is called once per frame
    void Update()
    {
    }

    private IEnumerator ButsudanRandomCreate()
    {

        List<int> histroy_r = new List<int>();
        foreach(HouseDirector house_director in house_directors)
        {
            foreach(GameObject Butsudan in house_director.Butsudans)
            {
                Butsudan.gameObject.SetActive(false);
            }
        }
        yield return null;

        int i = 0;

        // 1. House をシャッフル
        var shuffled = house_directors.OrderBy(_ => Random.value).ToList();
        // 2. 必要数分だけループ
        for (int idx = 0; idx < Mathf.Min(game_progress_controller.NeedPrayCount, shuffled.Count); idx++)
        {
            var hd = shuffled[idx];
            // hd.Butsudans からランダムに１つ選ぶ
            int j = Random.Range(0, hd.Butsudans.Count);
            hd.Butsudans[j].SetActive(true);
            hd.ButsudanExists = true;
            // 少し待って次のフレームに続行
            // yield return null;
        }

    }
}
