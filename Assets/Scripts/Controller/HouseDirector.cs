using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
[System.Serializable]
public class ButsudanGroup
{
    [Tooltip("d{? GameObject")]
    public GameObject Butsudan;

    [Tooltip("?d ON ???pbP[WQ")]
    public List<RandomSpawnPackage> ExtraPackages = new();
}
[System.Serializable]
public class RandomSpawnPackage
{
    [LabelText("IuWFNgQ")]
    public List<GameObject> Objs = new();

    [LabelText("om (0?1)")]
    [Range(0f, 1f)]
    public float SpawnChance = 1f;   // 1 = m, 0.25 = 25%
}
public class HouseDirector : SerializedMonoBehaviour
{
    private HousesController houses_controller;
    public bool ButsudanExists;
    [LabelText("d + tIuWFNgZbg")]
    public List<ButsudanGroup> Groups = new();   // Butsudan ?K???

    [Header("_??IuWFNgw ")]
    public List<GameObject> RandomObjs;
    [Header("_?m")]
    public int RandomObjProbablity;
    // [Header("er?w")]
    // public GameObject TV;
    // Start is called before the first frame update
    void Start()
    {
        houses_controller = HousesController.Instance;

        foreach (GameObject obj in RandomObjs)
        {
            obj.SetActive(false);
        }

        if (Unity.Netcode.NetworkManager.Singleton.IsServer)
        {
            foreach (GameObject obj in RandomObjs)
            {
                int r = Random.Range(0, RandomObjProbablity);
                Debug.Log("r: " + r);
                if (r == 0)
                {
                    var networkObject = obj.GetComponent<Unity.Netcode.NetworkObject>();
                    if (networkObject != null && !networkObject.IsSpawned)
                    {
                        networkObject.Spawn();
                    }
                    obj.SetActive(true);
                }
            }
        }
    }

}
