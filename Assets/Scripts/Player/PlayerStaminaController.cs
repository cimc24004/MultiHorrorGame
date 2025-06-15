using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.Events;
using Unity.Netcode;
public class PlayerStaminaController : NetworkBehaviour
{

    public Slider stamina_slider;
    public Color red_color;
    [SerializeField] public GameObject StaminaCanvas;
    [Header("スタミナがいま消費中かどうか(走ってるときなど)")]
    public bool StaminaConsumeNow = false;
    [Header("スタミナ増加中かどうか")]
    public bool StaminaIncreaseNow = false;
    private Color default_color;
    private Image fillRectImageUI;
    private PlayerStatus player_status;
    private float stamina_retake_start_time_now = 0;
    private UIFadeController ui_fade_controller;
    // Start is called before the first frame update
    public override void OnNetworkSpawn()
    {
        fillRectImageUI = stamina_slider.fillRect.gameObject.GetComponent<Image>();
        default_color = fillRectImageUI.color;
        player_status = this.gameObject.GetComponent<PlayerStatus>();
        ui_fade_controller = stamina_slider.gameObject.GetComponent<UIFadeController>();
        // if (IsOwner)
        // {
            player_status.StaminaValueNow.OnValueChanged += OnValueChangedStamina;
        // }
    }

    private void FixedUpdate()
    {
        if (IsServer)
        {
            if (player_status.StaminaValueAutoIncrease.Value == false) {
                return;
            }
            if (!this.StaminaConsumeNow)
            {
                stamina_retake_start_time_now += Time.fixedDeltaTime;
                if (stamina_retake_start_time_now > 1)
                {
                    StaminaRetake();
                }
            }
            else
            {
                StaminaIncreaseNow = false;
                stamina_retake_start_time_now = 0;
            }
            this.StaminaConsumeNow = false;
        }

    }
    public void ConsumeStamina()
    {
        player_status.StaminaValueNow.Value -= player_status.StaminaCost;
        StaminaConsumeNow = true;
    }
    public void StaminaRetake()
    {
        //スタミナ回復
        if (player_status.StaminaValueNow.Value / 100.0f <= 1)
        {
            player_status.StaminaValueNow.Value += player_status.StaminaRetakePower;
            StaminaIncreaseNow = true;
        }
        else
        {
            StaminaIncreaseNow = false;
        }
    }
    public bool StaminaCheck()
    {
        if (player_status.StaminaValueNow.Value <= player_status.CanMinStaminaRun)
        {
            return false;
        }
        else
        {
            return true;
        }
    }
    [ServerRpc]
    public void StaminaEnableDisableRequestServerRpc(bool _enable)
    {
        if (!_enable)
        {
            this.player_status.StaminaValueAutoIncrease.Value = false;
            player_status.StaminaValueNow.Value = 0;
        }
        else
        {
            this.player_status.StaminaValueAutoIncrease.Value = true;
        }
    }
    private void OnValueChangedStamina(float newValue, float oldValue) {
        Debug.Log("OnValuChangedStamina");
        ui_fade_controller.AutoFadeIn();
        if (newValue <= 100)
        {
            stamina_slider.DOValue(newValue / 100.0f, 0.1f).SetLink(this.gameObject);
        }
        if (newValue <= player_status.CanMinStaminaRun)
        {
            fillRectImageUI.color = red_color;
        }
        else
        {
            fillRectImageUI.color = default_color;
        }
    }
}
