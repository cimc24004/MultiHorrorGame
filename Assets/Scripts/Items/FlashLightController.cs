using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.InputSystem;
using Unity.Netcode;

public class FlashLightController : NetworkBehaviour
{
    public bool HaveLight = true;
    public bool LightPermit = true;
    [ShowIf("HaveLight", true)]
    [Header("懐中電灯(?)オブジェクトをセット")]
    public Light Light;

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
        InitPlayer = true;

        // 入力設定は OnNetworkSpawn でも行う
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
            Light.enabled = true;
            LightAudioSource.PlayOneShot(LightOnClip);
        }
        else
        {
            LightAudioSource.PlayOneShot(LightOffClip);
            Light.enabled = false;
        }
    }
}
