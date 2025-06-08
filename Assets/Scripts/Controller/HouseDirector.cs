using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using Unity.Netcode;

/// <summary>
/// 仏壇本体と、それに紐づく追加オブジェクトのグループ
/// </summary>
[System.Serializable]
public class ButsudanGroup
{
    [Tooltip("仏壇本体の GameObject")]
    public GameObject Butsudan;

    [Tooltip("仏壇が ON のときに追加でスポーンさせるパッケージ群")]
    public List<RandomSpawnPackage> ExtraPackages = new();
}

/// <summary>
/// ランダムにスポーンさせるオブジェクトパッケージ
/// </summary>
[System.Serializable]
public class RandomSpawnPackage
{
    [LabelText("スポーン対象オブジェクト群")]
    public List<GameObject> Objs = new();

    [LabelText("出現確率 (0〜1)")]
    [Range(0f, 1f)]
    public float SpawnChance = 1f;   // 1 = 100%, 0.25 = 25%
}

public class HouseDirector : NetworkSingleton<HorrorDirector>
{
    private HousesController houses_controller;

    [ReadOnly]
    public bool ButsudanExists;

    [LabelText("仏壇 + 付属オブジェクトグループ")]
    public List<ButsudanGroup> Groups = new();   // 仏壇セット

    [Header("ランダムに出現するオブジェクト")]
    public List<GameObject> RandomObjs;

    [Header("確率の分母 (1 / N)")]
    public int RandomObjProbability = 1;          // 1 なら必ず出現、2 なら 1/2

    // ──────────────────────────────────────
    // 初期化
    // ──────────────────────────────────────
    private void Start()
    {
        houses_controller = HousesController.Instance;

        // いったん全て非表示
        foreach (var obj in RandomObjs)
        {
            obj.SetActive(false);
        }
        // サーバーでのみランダム生成
        if (Unity.Netcode.NetworkManager.Singleton.IsServer)
        {
            // AllClientsRandomInstantiateRequestClientRpc();
        }
    }
    [ServerRpc]
    private void RandomInstantiateObjectsRequestServerRpc()
    {

    }
    [ClientRpc]
    private void AllClientsRandomInstantiateRequestClientRpc()
    {
        foreach (var obj in RandomObjs)
        {
            int r = Random.Range(0, RandomObjProbability);
            if (r == 0)
            {
                var networkObject = obj.GetComponent<Unity.Netcode.NetworkObject>();

                // まだ Network Spawn されていなければ Spawn
                if (networkObject != null && !networkObject.IsSpawned)
                {
                    networkObject.Spawn();
                }

                obj.SetActive(true);
            }
        }
    }
}

