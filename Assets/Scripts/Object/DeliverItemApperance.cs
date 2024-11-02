using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrayToDeliverItemAppearPlace : Singleton<PrayToDeliverItemAppearPlace>
{
    public Transform DeliverItemApperancePlace;
    private ItemDatabases item_databases;
    private AudioSource audio_source;
    [Header("必要な仏壇に祈る回数")]
    public int NeedPrayCount = 1;

    [Header("現在の祈り回数")]
    public int PrayCountNow = 0;

    // Start is called before the first frame update
    void Start()
    {
        item_databases = ItemDatabases.instance;
        audio_source = this.gameObject.GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void OnPrayCompleted()
    {
        //どこかの仏壇で祈りがコンプリートされたとき。
        PrayCountNow++;
        if(NeedPrayCount <= PrayCountNow)
        {
            //必要な祈り回数が必要になったら最終イベントアイテムを出現させる。
            DeliverItemAppear();
        }
    }
    public void DeliverItemAppear()
    {

        //int r = Random.Range(0, item_databases.ForDeliverItem.Count);

        Instantiate(item_databases.ForDeliverItem.ItemPutObjectPrefab, DeliverItemApperancePlace.transform.position, Quaternion.identity);
        Debug.Log("a-------------------------------------");
        audio_source.Play();
    }
}
