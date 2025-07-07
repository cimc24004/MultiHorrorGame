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
    private string configPath;
    private ConfigUI configUI;
    void Start()
    {
        configUI = ConfigUI.instance;
        configUI.CloseConfigMenuButton.onClick.AddListener(() =>
        {
            CloseConfigMenu();
        });
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
        string json = JsonUtility.ToJson(config, true);
        File.WriteAllText(configPath, json);
    }

    public void LoadConfig()
    {
        if (File.Exists(configPath))
        {
            string json = File.ReadAllText(configPath);
            config = JsonUtility.FromJson<ConfigData>(json);
        }
        else
        {
            config = new ConfigData();
        }
        ApplyConfig();
    }

    private void ApplyConfig()
    {
        // 設定を実際に適用
        Screen.SetResolution(config.screenWidth, config.screenHeight, config.fullscreen);
        QualitySettings.SetQualityLevel(config.qualityLevel);
        // 他の設定も適用...
    }
}
