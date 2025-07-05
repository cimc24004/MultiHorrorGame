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
public class URPFxController : Singleton<URPFxController>
{
    private CorruptMemory.Settings corrupt_memory_settings;
    private Hacked.Settings hacked_settings;
    private Interferences.Settings interferences_settings;
    private VHS.Settings vhs_settings;
    private CheapCamera.Settings cheap_camera_settings;
    [SerializeField]
    private Dictionary<string, float> intesity_settings_saved;
    // Start is called before the first frame update
    void Start()
    {
        corrupt_memory_settings = CorruptMemory.GetSettings();
        hacked_settings = Hacked.GetSettings();
        interferences_settings = Interferences.GetSettings();
        vhs_settings = VHS.GetSettings();
        cheap_camera_settings = CheapCamera.GetSettings();
        CheapCameraChange(false);
        CorruptMemoryChange(false);
        HackedChange(false);
        InteferecesChange(false);
        VHSChange(false);
    }

    // Update is called once per frame
    void Update()
    {

    }
    [Button]
    public void DisctionarySet()
    {
        corrupt_memory_settings = CorruptMemory.GetSettings();
        hacked_settings = Hacked.GetSettings();
        interferences_settings = Interferences.GetSettings();
        vhs_settings = VHS.GetSettings();
        cheap_camera_settings = CheapCamera.GetSettings();

        intesity_settings_saved = new Dictionary<string, float>() {
            { "Interferences", interferences_settings.intensity },
            { "VHS", vhs_settings.intensity },
            { "CheapCamera", cheap_camera_settings.intensity },
            {"CorruptMemory", corrupt_memory_settings.intensity},
            {"Hacked", hacked_settings.intensity}
        };
    }
    [Button]
    public void IntensityFromDictionary()
    {
        corrupt_memory_settings = CorruptMemory.GetSettings();
        hacked_settings = Hacked.GetSettings();
        interferences_settings = Interferences.GetSettings();
        vhs_settings = VHS.GetSettings();
        cheap_camera_settings = CheapCamera.GetSettings();
        interferences_settings.intensity = intesity_settings_saved["Interferences"];
        vhs_settings.intensity = intesity_settings_saved["VHS"];
        cheap_camera_settings.intensity = intesity_settings_saved["CheapCamera"];
        corrupt_memory_settings.intensity = intesity_settings_saved["CorruptMemory"];
        hacked_settings.intensity = intesity_settings_saved["Hacked"];

        
    }
    [Button]
    public void CorruptMemoryChange(bool enable)
    {
        if (enable)
        {
            corrupt_memory_settings.enableProfiling = true;
            corrupt_memory_settings.intensity = intesity_settings_saved["CorruptMemory"];
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
            hacked_settings.intensity = intesity_settings_saved["Hacked"];
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
            interferences_settings.intensity = intesity_settings_saved["Interferences"];
        }
        else
        {
            interferences_settings.enableProfiling = false;
            interferences_settings.intensity = 0;
        }
    }
    [Button]
    public void VHSChange(bool enable)
    {
        if (enable)
        {
            vhs_settings.enableProfiling = true;
            vhs_settings.intensity = intesity_settings_saved["VHS"];
        }
        else
        {
            vhs_settings.enableProfiling = false;
            vhs_settings.intensity = 0;
        }
    }

    [Button]
    public void CheapCameraChange(bool enable)
    {
        if (enable)
        {
            cheap_camera_settings.enableProfiling = true;
            cheap_camera_settings.intensity = intesity_settings_saved["CheapCamera"];
        }
        else
        {
            cheap_camera_settings.enableProfiling = false;
            cheap_camera_settings.intensity = 0;
        }
    }
}
