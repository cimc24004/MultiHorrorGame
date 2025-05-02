using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
[System.Serializable]
public class ButsudanGroup
{
    [Tooltip("仏壇本体の GameObject")]
    public GameObject Butsudan;

    [Tooltip("この仏壇が ON になった時だけ判定するパッケージ群")]
    public List<RandomSpawnPackage> ExtraPackages = new();
}
[System.Serializable]
public class RandomSpawnPackage
{
    [LabelText("候補オブジェクト群")]
    public List<GameObject> Objs = new();

    [LabelText("出現確率 (0?1)")]
    [Range(0f, 1f)]
    public float SpawnChance = 1f;   // 1 = 確定, 0.25 = 25%
}
public class HouseDirector : SerializedMonoBehaviour
{
    public SilhouetteChangeController silhouette_change_controller;
    //public GameObject Butsudan;
    public bool ButsudanExists;
    [LabelText("仏壇 + 付随オブジェクトセット")]
    public List<ButsudanGroup> Groups = new();   // Butsudan は必ずこちらにも含まれる

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
