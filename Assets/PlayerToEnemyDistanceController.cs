using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerToEnemyDistanceController : MonoBehaviour
{
    //ìGÇ∆ÇÃãóó£Ç≈êSîèâπÇè„Ç∞ÇÈÅB
    [SerializeField] private float to_enemy_distance;

    public GameObject Parent;
    private AudioSource audio_source;
    private HorrorDirector horror_director;
    private bool once;
    // Start is called before the first frame update
    void Start()
    {
        audio_source = this.gameObject.GetComponent<AudioSource>();
        horror_director = HorrorDirector.instance;
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerEnter(Collider other)
    {

    }
    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag == "Enemy")
        {
            to_enemy_distance = Vector3.Distance(this.transform.position, other.gameObject.transform.position);
            Debug.Log("Enemy  ??" + other.gameObject.name + "Distance->" + to_enemy_distance);
            if (!once)
            {
                NearDistanceEvent();
                once = true;
            }
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if(other.gameObject.tag == "Enemy")
        {
            FarDistanceEvent();
            once = false;
        }
    }

    private void NearDistanceEvent()
    {
        if (horror_director.DiscoveryNow) return;
        audio_source.Play();
        horror_director.NearEnemy();
        Parent.SetActive(true);
    }

    private void FarDistanceEvent()
    {
        horror_director.ResetSetting();
        audio_source.Stop();
        Parent.SetActive(false);
    }
}
