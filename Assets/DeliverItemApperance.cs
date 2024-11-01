using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeliverItemApperance : MonoBehaviour
{
    public Transform DeliverItemApperancePlace;
    private ItemDatabases item_databases;
    private AudioSource audio_source;
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
    public void DeliverItemAppear()
    {

        //int r = Random.Range(0, item_databases.ForDeliverItem.Count);

        Instantiate(item_databases.ForDeliverItem.ItemPutObjectPrefab, DeliverItemApperancePlace.transform.position, Quaternion.identity);
        audio_source.Play();
    }
}
