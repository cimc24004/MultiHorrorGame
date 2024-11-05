using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Sirenix.OdinInspector;
public class HousesController : SerializedMonoBehaviour
{
    public List<HouseDirector> house_directors;
    private PrayToDeliverItemAppearPlace pray_to_deliver_item_appear_place;
    // Start is called before the first frame update
    void Start()
    {
        pray_to_deliver_item_appear_place = PrayToDeliverItemAppearPlace.instance;
        house_directors = new List<HouseDirector>(FindObjectsOfType<HouseDirector>().ToList());

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
            house_director.Butsudan.gameObject.SetActive(false);
        }
        yield return null;


        int i = 0;

        while (true)
        {
            if (i >= pray_to_deliver_item_appear_place.NeedPrayCount) break;

            int r = Random.Range(0, pray_to_deliver_item_appear_place.NeedPrayCount);
            GameObject Butsudan = house_directors[r].Butsudan;
            if (!Butsudan.activeSelf)
            {
                house_directors[r].Butsudan.SetActive(true);
                i++;
            }

        }

    }
}
