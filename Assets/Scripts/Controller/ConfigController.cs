using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System;
using UnityEngine.SceneManagement;
using System.IO;

public class ConfigController : SingleTonDontDestory<ConfigController>
{
    public GameControllerOnAllScene game_controller_all_scene;
    public ConfigData config;
    public ConfigData defaultConfig;
    [SerializeField] private string configPath;
    private ConfigUI configUI;

    [SerializeField] private Slider MasterVolumeSlider;
    [SerializeField] private Slider BgmVolumeSlider;
    [SerializeField] private Slider SfxVolumeSlider;

    protected override void Awake()
    {
        base.Awake();
        // 設定ファイルのパスを初期化
        configPath = Path.Combine(Application.persistentDataPath, "config.json");
        Debug.Log("configPath: " + configPath);
        // 設定をロード
        LoadConfig();
    }

    void Start()
    {
        configUI = ConfigUI.instance;
        MasterVolumeSlider.value = config.masterVolume;
        SfxVolumeSlider.value = config.sfxVolume;
        BgmVolumeSlider.value = config.masterVolume;
        configUI.CloseConfigMenuButton.onClick.AddListener(() =>
        {
            CloseConfigMenu();
        });
        configUI.SaveConfigButton.onClick.AddListener(() =>
        {
            SaveConfig();
        });
        MasterVolumeSlider.onValueChanged.AddListener((value) =>
        {
            config.masterVolume = value;
        });
        SfxVolumeSlider.onValueChanged.AddListener((value) =>
        {
            config.sfxVolume = value;

        });
        BgmVolumeSlider.onValueChanged.AddListener((value) =>
        {
            config.bgmVolume = value;
        });
    }
    private void ValueUISet(ConfigData _configData)
    {
        MasterVolumeSlider.value = _configData.masterVolume;
        BgmVolumeSlider.value = _configData.bgmVolume;
        SfxVolumeSlider.value = _configData.sfxVolume;
    }
    public void OpenConfigMenu()
    {
        configUI.configPanel.gameObject.SetActive(true);
    }
    
    public void CloseConfigMenu()
    {
        configUI.configPanel.gameObject.SetActive(false);
    }
    
    [SerializeField] private bool Opended = false;

    public void SaveConfig()
    {
        try
        {
            string json = JsonUtility.ToJson(config, true);
            File.WriteAllText(configPath, json);
            Debug.Log($"設定を保存しました: {configPath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"設定の保存に失敗しました: {e.Message}");
        }
    }

    public void LoadConfig()
    {
        try
        {
            if (File.Exists(configPath))
            {
                string json = File.ReadAllText(configPath);
                config = JsonUtility.FromJson<ConfigData>(json);
                Debug.Log($"設定をロードしました: {configPath}");
            }
            else
            {
                config = new ConfigData();
                Debug.Log("設定ファイルが見つからないため、デフォルト設定を使用します");
            }
            ApplyConfig();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"設定の読み込みに失敗しました: {e.Message}");
            config = new ConfigData();
            ApplyConfig();
        }
    }

    private void ApplyConfig()
    {
        Screen.SetResolution(config.screenWidth, config.screenHeight, config.fullscreen);
        QualitySettings.SetQualityLevel(config.qualityLevel);
    }
}