using System.Collections.Generic;
using UnityEngine;  
[System.Serializable]
public class ConfigData
{
    // ウィンドウ設定
    public int screenWidth = 1920;
    public int screenHeight = 1080;
    public bool fullscreen = true;
    
    // グラフィック設定
    public int qualityLevel = 2;
    public bool vsync = true;
    public int targetFrameRate = 60;
    
    // オーディオ設定
    public float masterVolume = 1.0f;
    public float bgmVolume = 0.8f;
    public float sfxVolume = 0.9f;
    
    // キーコンフィグ
    // public Dictionary<string, KeyCode> keyBindings = new Dictionary<string, KeyCode>();
}