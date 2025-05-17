using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.InputSystem;
using Unity.Netcode;

public class FlashLightController : NetworkBehaviour
{
    private FlashLightBatteryUIController flash_light_battery_ui_controller;
    public bool HaveLight = true;
    public bool LightPermit = true;
    [ShowIf("HaveLight", true)]
    [Header("懐中電灯(?)オブジェクトをセット")]
    public GameObject LightObject;
    public NetworkVariable<float> LightPower = new NetworkVariable<float>(100f);

    [Header("~秒ごとに")]
    [SerializeField] private float PerTimeForLightPowerDown = 1f;
    [Header("~秒ごとに減る電池量")]
    [SerializeField] private float LightPowerDown = 1f;
    private float LightPowerDownTimeNow;

    [SerializeField] private AudioSource LightAudioSource;
    [SerializeField] private AudioClip LightOnClip;
    [SerializeField] private AudioClip LightOffClip;

    private PlayerInput player_input;
    private PlayerStatus player_status;
    private SubNoticedUIController sub_noticed_ui_controller;

    private bool InitPlayer = false;

    private NetworkVariable<bool> LightSwitchMode = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    void OnEnable()
    {
        PlayerStatus.OnLocalPlayerSpawned += InitOnPlayerSpawn;
    }

    void OnDisable()
    {
        PlayerStatus.OnLocalPlayerSpawned -= InitOnPlayerSpawn;
        LightSwitchMode.OnValueChanged -= OnLightSwitchChanged;
        if (IsOwner && player_input != null)
            player_input.actions["LightSwitchAction"].performed -= LightInputPerformed;
    }

    public override void OnNetworkSpawn()
    {
        LightSwitchMode.OnValueChanged += OnLightSwitchChanged;
        if (IsOwner && player_input != null)
            player_input.actions["LightSwitchAction"].performed += LightInputPerformed;
    }

    private void InitOnPlayerSpawn(PlayerStatus _playerStatus)
    {
        player_status = _playerStatus;
        player_input = player_status.player_input;
        sub_noticed_ui_controller = player_status.sub_noticed_ui_controller;
        flash_light_battery_ui_controller = FlashLightBatteryUIController.instance;
        InitPlayer = true;

        // 入力設定は OnNetworkSpawn でも行う
    }

    void Update()
    {
        if (!InitPlayer) return;

        if (IsServer)
        {
            if (LightSwitchMode.Value)
            {
                LightPowerDownTimeNow += Time.deltaTime;
                if (LightPowerDownTimeNow >= PerTimeForLightPowerDown)
                {
                    LightPowerDecrease();
                    LightPowerDownTimeNow = 0f;
                }
            }

            LightPermit = LightPower.Value > 0f;
        }
    }

    private void LightPowerDecrease()
    {
        LightPower.Value -= LightPowerDown;
        if (LightPower.Value <= 0f)
        {
            LightPower.Value = 0f;
            // バッテリー切れたら自動でオフに
            LightSwitchMode.Value = false;
            sub_noticed_ui_controller.Notice("懐中電灯のバッテリーが切れた");
        }

        // 25% 単位で UI 更新
        if (Mathf.Approximately(LightPower.Value % 25f, 0f))
        {
            flash_light_battery_ui_controller.UIChangeOnBatteryDecrease();
        }
    }

    public void LightPowerToMax()
    {
        if (!IsServer) return;
        LightPower.Value = 100f;
        flash_light_battery_ui_controller.UIChangeOnBatteryToMax();
    }

    private void LightInputPerformed(InputAction.CallbackContext ctx)
    {
        LightOnOffChangeRequestServerRpc();
    }

    [ServerRpc]
    private void LightOnOffChangeRequestServerRpc(ServerRpcParams rpcParams = default)
    {
        if (!LightPermit) return;
        LightSwitchMode.Value = !LightSwitchMode.Value;
    }

    private void OnLightSwitchChanged(bool previousValue, bool newValue)
    {
        if (newValue)
        {
            LightObject.SetActive(true);
            LightAudioSource.PlayOneShot(LightOnClip);
        }
        else
        {
            LightAudioSource.PlayOneShot(LightOffClip);
            LightObject.SetActive(false);
        }
    }
}
