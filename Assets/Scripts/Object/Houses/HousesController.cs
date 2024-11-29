using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Sirenix.OdinInspector;
public class HousesController : SerializedMonoBehaviour
{
    public List<HouseDirector> house_directors;
    private GameProgressController game_progress_controller;
    // Start is called before the first frame update
    void Start()
    {
        house_directors = new List<HouseDirector>(FindObjectsOfType<HouseDirector>().ToList());
        game_progress_controller = GameProgressController.instance;


        StartCoroutine(ButsudanRandomCreate());
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

        while (true)
        {
            if (i >= game_progress_controller.NeedPrayCount) break;
            //NeedPrayCountï™ÇæÇØButsudanÇçÏê¨Ç∑ÇÈÇ∆Ç¢Ç§Ç±Ç∆

            int r = Random.Range(0, house_directors.Count);
            
            List<GameObject> Butsudans = house_directors[r].Butsudans;
            
            int r_2 = Random.Range(0, house_directors[r].Butsudans.Count);
            
            if (!Butsudans[r_2].activeSelf && !house_directors[r].ButsudanExists)
            {
                house_directors[r].ButsudanExists = true;
                Butsudans[r_2].SetActive(true);
                i++;
            }
        }

    }
}
