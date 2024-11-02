using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Butsudan : MonoBehaviour
{
    private PrayToDeliverItemAppearPlace pray_to_deliver_item_appear_place;

    private void Start()
    {
        pray_to_deliver_item_appear_place = PrayToDeliverItemAppearPlace.instance;
    }
    public void ButsudanPrayCompleted()
    {
        Debug.Log("deliver_item_apperance: " + pray_to_deliver_item_appear_place) ;
        pray_to_deliver_item_appear_place.OnPrayCompleted();
    }
}
