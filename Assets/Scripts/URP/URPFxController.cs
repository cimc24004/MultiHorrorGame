using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FronkonGames.Glitches.CheapCamera;
using FronkonGames.Glitches.Hacked;
using FronkonGames.Glitches.Artifacts;
using FronkonGames.Glitches.VHS;
using FronkonGames.Glitches.Interferences;
using FronkonGames.Glitches.CorruptMemory;

using Sirenix.OdinInspector;
public class URPFxController : SerializedMonoBehaviour
{
    private CorruptMemory.Settings corrupt_memory_settings;
    private Hacked.Settings hacked_settings;
    private Interferences.Settings interferences_settings;
    // Start is called before the first frame update
    void Start()
    {
        corrupt_memory_settings = CorruptMemory.GetSettings();
        hacked_settings = Hacked.GetSettings();
        interferences_settings = Interferences.GetSettings();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    [Button]
    public void CorruptMemoryChange(bool enable)
    {
        if (enable)
        {
            corrupt_memory_settings.enableProfiling = true;
            corrupt_memory_settings.intensity = 1;
        }
        else
        {
            corrupt_memory_settings.enableProfiling = false;
            corrupt_memory_settings.intensity = 0;
        }
    }
    [Button]
    public void HackedChange(bool enable)
    {
        if (enable)
        {
            hacked_settings.enableProfiling = true;
            hacked_settings.intensity = 1;
        }
        else
        {
            hacked_settings.enableProfiling = false;
            hacked_settings.intensity = 0;
        }
    }
    [Button]
    public void InteferecesChange(bool enable)
    {
        if (enable)
        {
            interferences_settings.enableProfiling = true;
            interferences_settings.intensity = 1;
        }
        else
        {
            interferences_settings.enableProfiling = false;
            interferences_settings.intensity = 0;
        }
    }
}
