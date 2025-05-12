using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.InputSystem;
public class FlashLightController : Singleton<FlashLightController>
{
    private FlashLightBatteryUIController flash_light_battery_ui_controller;
    public bool HaveLight = true;
    public bool LightPermit = true;
    [ShowIf("HaveLight", true)]
    [Header("懐中電灯(?)オブジェクトをセット")]
    public GameObject LightObject;
    public float LightPower = 100;

    [Header("~秒ごとに")]
    [SerializeField] private float PerTimeForLightPowerDown = 1;
    [Header("~秒ごとに減る電池量")]
    [SerializeField] private float LightPowerDown = 1;
    private float LightPowerDownTimeNow;
    [SerializeField] private AudioSource LightAudioSource;
    [SerializeField] private AudioClip LightOnClip;
    [SerializeField] private AudioClip LightOffClip;
    private PlayerInput player_input;
    private bool LightSwitchMode = false;
    private PlayerStatus player_status;
    private SubNoticedUIController sub_noticed_ui_controller;
    private Vector3 DefaultPosition;
    private Quaternion DefaultRotation;
    // Start is called before the first frame update
    void Start()
    {
        player_status = PlayerStatus.LocalInstance;
        player_input = player_status.player_input;
        sub_noticed_ui_controller = player_status.sub_noticed_ui_controller;
        flash_light_battery_ui_controller = FlashLightBatteryUIController.instance;
    }

    // Update is called once per frame
    void Update()
    {
        if(LightSwitchMode){
            LightPowerDownTimeNow += Time.deltaTime;
            if(LightPowerDownTimeNow > PerTimeForLightPowerDown){
                LightPowerDecrease();
                LightPowerDownTimeNow = 0;
            }
        }
        if(LightPower > 0){
            LightPermit = true;
        }else{
            LightPermit = false;
        }
        if (HaveLight && LightPermit)
        {
            if (player_input.currentActionMap["LightSwitchAction"].triggered)
            {
                if (!LightSwitchMode)
                {
                    LightOn();
                }
                else
                {
                    LightOff();
                }
            }
        }
    }

    private void LightPowerDecrease(){
        LightPower -= LightPowerDown;
        if(LightPower <= 0){
            LightOff();
            sub_noticed_ui_controller.Notice("懐中電灯のバッテリーが切れた");
        }
        if(LightPower % 25 == 0){
            flash_light_battery_ui_controller.UIChangeOnBatteryDecrease();
        }
    }
    public void LightPowerToMax(){
        LightPower = 100;
        flash_light_battery_ui_controller.UIChangeOnBatteryToMax();
    }
    private void LightOn(){
        //ライトをつける。
        LightObject.SetActive(true);
        LightAudioSource.PlayOneShot(LightOnClip);
        LightSwitchMode = true;
    }
    private void LightOff(){
        //ライトをけす。
        LightAudioSource.PlayOneShot(LightOffClip);
        LightObject.SetActive(false);
        LightSwitchMode = false;
    }
}
