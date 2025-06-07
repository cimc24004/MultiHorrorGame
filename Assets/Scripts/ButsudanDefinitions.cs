// using System;
// using System.Collections.Generic;
// using UnityEngine;
// using Sirenix.OdinInspector;
// using Unity.Netcode;

// /// <summary>ランダムにスポーンさせるオブジェクトパッケージ</summary>
// [Serializable]
// public class RandomSpawnPackage
// {
//     [LabelText("スポーン対象 Prefab 群 (NetworkObject 必須)")]
//     public List<NetworkObject> PrefabList = new();

//     [LabelText("出現確率 (0〜1)")]
//     [Range(0f, 1f)] public float SpawnChance = 1f;   // 1 = 100%
// }

// /// <summary>仏壇本体と付随パッケージの 1 グループ</summary>
// [Serializable]
// public class ButsudanGroup
// {
//     [Tooltip("シーン上に置いた Marker (空オブジェクト)")]
//     public Transform Marker;

//     [Tooltip("仏壇本体の Prefab (NetworkObject 付き)")]
//     public NetworkObject ButsudanPrefab;

//     [Tooltip("仏壇 ON 時に追加スポーンさせるパッケージ群")]
//     public List<RandomSpawnPackage> ExtraPackages = new();
// }
