using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Sirenix.OdinInspector;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class EnemyDatabases : Singleton<EnemyDatabases>
{
    [SerializeField] private List<DiscoveryPlayerStatus> enemy_discovery_statuses;
    [SerializeField] private List<SoundSensorStatus> enemy_sound_sensor_statuses;
    [SerializeField]

#if UNITY_EDITOR
    [ExecuteInEditMode]
    [Button]
    public void StatusSet(string _className)
    {
        Type type = GetTypeByName(_className);
        if (type == null || !typeof(ScriptableObject).IsAssignableFrom(type))
        {
            Debug.LogError($"Type '{_className}' が見つからない、または ScriptableObject のサブクラスではありません。");
            return;
        }

        string[] guids = AssetDatabase.FindAssets($"t:{_className}");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if(_className == "DiscoveryPlayerStatus")
            {
                DiscoveryPlayerStatus asset = AssetDatabase.LoadAssetAtPath(path, type) as DiscoveryPlayerStatus;

                if (asset != null)
                {
                    enemy_discovery_statuses.Add(asset);
                    Debug.Log($"追加: {asset.name}");
                }
            }else if(_className == "SoundSensorStatus")
            {
                SoundSensorStatus asset = AssetDatabase.LoadAssetAtPath(path, type) as SoundSensorStatus;

                if (asset != null)
                {
                    enemy_sound_sensor_statuses.Add(asset);
                    Debug.Log($"追加: {asset.name}");
                }
            }

        }

    }
#endif

    private Type GetTypeByName(string className)
    {
        // 現在のアセンブリ内で該当するクラスを探す
        Type type = Type.GetType(className);
        if (type == null)
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = assembly.GetType(className);
                if (type != null)
                    break;
            }
        }
        return type;
    }
}
