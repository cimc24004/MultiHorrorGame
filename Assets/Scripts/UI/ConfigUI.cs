using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ConfigUI : Singleton<ConfigUI>
{
    public GameObject configPanel;
    public Button CloseConfigMenuButton;
    public Button ApplyConfigButton;
    public Button ResetConfigButton;
    public bool OpenNow;
    [SerializeField] public Slider2 MasterVolumeSlider;
    [SerializeField] public Slider2 BgmVolumeSlider;
    [SerializeField] public Slider2 SfxVolumeSlider;
    [SerializeField] public Slider2 MouseSensitivitySlider;
}
