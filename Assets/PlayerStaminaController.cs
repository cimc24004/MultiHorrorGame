using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
public class PlayerStaminaController : MonoBehaviour
{

    public Slider stamina_slider;
    public Color red_color;
    [Header("スタミナがいま消費中かどうか(走ってるときなど)")]
    public bool StaminaConsumeNow;
    [Header("スタミナ増加中かどうか")]
    public bool StaminaIncreaseNow;
    private Color default_color;
    private Image fillRectImageUI;
    private PlayerStatus player_status;
    private float stamina_retake_start_time_now = 0;
    // Start is called before the first frame update
    void Start()
    {
        fillRectImageUI = stamina_slider.fillRect.gameObject.GetComponent<Image>();
        default_color = stamina_slider.colors.normalColor;
        player_status = this.gameObject.GetComponent<PlayerStatus>();
    }

    // Update is called once per frame
    void Update()
    {
        if(player_status.StaminaValueNow <= 100)
        {
            stamina_slider.DOValue(player_status.StaminaValueNow / 100.0f, 0.1f).SetLink(this.gameObject);
        }
        if (player_status.StaminaValueNow <= player_status.CanMinStaminaRun)
        {
            fillRectImageUI.color = red_color;
        }
        else
        {
            fillRectImageUI.color = default_color;
        }


    }
    private void FixedUpdate()
    {
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
    public void ConsumeStamina()
    {
        player_status.StaminaValueNow -= player_status.StaminaCost;
        StaminaConsumeNow = true;
    }
    public void StaminaRetake()
    {
        //スタミナ回復
        if(player_status.StaminaValueNow / 100.0f <= 1)
        {
            player_status.StaminaValueNow += player_status.StaminaRetakePower;
            StaminaIncreaseNow = true;
        }
        else
        {
            StaminaIncreaseNow = false;
        }
    }
    public bool StaminaCheck()
    {
        if (player_status.StaminaValueNow <= player_status.CanMinStaminaRun)
        {
            return false;
        }
        else
        {
            return true;
        }
    }
}
