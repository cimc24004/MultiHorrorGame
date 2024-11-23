using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.Serialization;

public class HouseDirector : SerializedMonoBehaviour
{
    //public GameObject Butsudan;
    public List<GameObject> Butsudans;
    public bool ButsudanExists;

    [Header("ランダムで作成したいオブジェクトを指定 ")]
    public List<GameObject> RandomObjs;
    [Header("ランダムの確率")]
    public int RandomObjProbablity;
    // Start is called before the first frame update
    void Start()
    {

        foreach(GameObject obj in RandomObjs)
        {
            obj.SetActive(false);
        }

        foreach(GameObject obj in RandomObjs)
        {
            int r = Random.Range(0, RandomObjProbablity);
            Debug.Log("r: " + r);
            if(r == 0)
            {
                obj.SetActive(true);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
