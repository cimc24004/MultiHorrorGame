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
    public ConfigData currentConfig;
    public ConfigData defaultConfig;
    [SerializeField] private string configPath;
    private ConfigUI configUI;
    private CMFPSController cMFPSController;
    private PauseMenuUI pauseMenuUI;
    protected override void Awake()
    {
        base.Awake();
        // 設定ファイルのパスを初期化
        configPath = Path.Combine(Application.persistentDataPath, "config.json");
        Debug.Log("configPath: " + configPath);
        // 最初は設定をロード
    }
    private void OnEnable()
    {
        PlayerStatus.OnLocalPlayerSpawned += InitWhenPlayerSpawned;
    }
    private void OnDisable()
    {
        PlayerStatus.OnLocalPlayerSpawned -= InitWhenPlayerSpawned;
    }
    private void InitWhenPlayerSpawned(PlayerStatus _player_status)
    {
        cMFPSController = CMFPSController.instance;
        LoadConfig();
    }
    void Start()
    {
        configUI = ConfigUI.instance;
        pauseMenuUI = PauseMenuUI.instance;
        configUI.CloseConfigMenuButton.onClick.AddListener(() =>
        {

            CloseConfigMenu();
        });
        configUI.ApplyConfigButton.onClick.AddListener(() =>
        {
            SaveConfig();
            LoadConfig();
        });
        configUI.MasterVolumeSlider.onValueChanged.AddListener((value) =>
        {
            currentConfig.masterVolume = value;
        });
        configUI.SfxVolumeSlider.onValueChanged.AddListener((value) =>
        {
            currentConfig.sfxVolume = value;

        });
        configUI.BgmVolumeSlider.onValueChanged.AddListener((value) =>
        {
            currentConfig.bgmVolume = value;
        });
        configUI.MouseSensitivitySlider.onValueChanged.AddListener((value) =>
        {
            currentConfig.MouseSensitivity = value;
        });
        configUI.ResetConfigButton.onClick.AddListener(() =>
        {
            ResetToDefaultConfig();
        });
        ValueSetFromConfigData(currentConfig);
    }
    private void ValueSetFromConfigData(ConfigData _configData)
    {
        configUI.MasterVolumeSlider.value = _configData.masterVolume;
        configUI.BgmVolumeSlider.value = _configData.bgmVolume;
        configUI.SfxVolumeSlider.value = _configData.sfxVolume;
        configUI.MouseSensitivitySlider.value = _configData.MouseSensitivity;
    }
    public void OpenConfigMenu()
    {
        configUI.configPanel.gameObject.SetActive(true);
    }

    public void CloseConfigMenu()
    {
        pauseMenuUI.PausePanel.gameObject.SetActive(true);
        configUI.configPanel.gameObject.SetActive(false);
    }

    [SerializeField] private bool Opended = false;

    public void SaveConfig()
    {
        try
        {
            string json = JsonUtility.ToJson(currentConfig, true);
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
                currentConfig = JsonUtility.FromJson<ConfigData>(json);
                Debug.Log($"設定をロードしました: {configPath}");
            }
            else
            {
                currentConfig = new ConfigData();
                Debug.Log("設定ファイルが見つからないため、デフォルト設定を使用します");
            }
            ApplyConfig();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"設定の読み込みに失敗しました: {e.Message}");
            currentConfig = new ConfigData();
            ApplyConfig();
        }
    }

    private void ApplyConfig()
    {
        Debug.Log("ApplyConfig-----------------------");
        Screen.SetResolution(currentConfig.screenWidth, currentConfig.screenHeight, currentConfig.fullscreen);
        QualitySettings.SetQualityLevel(currentConfig.qualityLevel);
        cMFPSController.MouseSensivityChange(currentConfig.MouseSensitivity);
    }
    private void ResetToDefaultConfig()
    {
        currentConfig = defaultConfig;
        ValueSetFromConfigData(currentConfig);
        ApplyConfig();
        
    }
}