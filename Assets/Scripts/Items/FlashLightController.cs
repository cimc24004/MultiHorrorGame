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

    [Header("強度(段階) 弱→強 の順で設定")]
    [Tooltip("空なら [3,6,10] を自動で入れる")]
    public List<float> IntensitySteps = new List<float>() { 3f, 6f, 10f };

    [SerializeField] private AudioSource LightAudioSource;
    [SerializeField] private AudioClip LightOnClip;
    [SerializeField] private AudioClip LightOffClip;
    [SerializeField] private AudioClip LightStepClip;

    private PlayerInput player_input;
    private PlayerStatus player_status;

    // ON/OFF（全員読み取り・サーバ書き込み）
    private NetworkVariable<bool> LightSwitchMode = new NetworkVariable<bool>(
        false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    // 段階インデックス（全員読み取り・サーバ書き込み）
    private NetworkVariable<int> CurrentIntensityIndex = new NetworkVariable<int>(
        0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    void OnEnable()
    {
        PlayerStatus.OnLocalPlayerSpawned += InitOnPlayerSpawn;
    }

    void OnDisable()
    {
        PlayerStatus.OnLocalPlayerSpawned -= InitOnPlayerSpawn;

        LightSwitchMode.OnValueChanged -= OnLightSwitchChanged;
        CurrentIntensityIndex.OnValueChanged -= OnIntensityIndexChanged;

        if (IsOwner && player_input != null)
        {
            var map = player_input.actions;
            if (map != null)
            {
                var a = map.FindAction("LightSwitchAction", throwIfNotFound: false);
                if (a != null) a.performed -= LightInputPerformed;
                var b = map.FindAction("LightStepAction", throwIfNotFound: false);
                if (b != null) b.performed -= LightStepPerformed;
            }
        }
    }

    public override void OnNetworkSpawn()
    {
        if (IntensitySteps == null || IntensitySteps.Count == 0)
            IntensitySteps = new List<float>() { 3f, 6f, 10f };

        LightSwitchMode.OnValueChanged += OnLightSwitchChanged;
        CurrentIntensityIndex.OnValueChanged += OnIntensityIndexChanged;

        if (Light) Light.enabled = LightSwitchMode.Value;
        ApplyIntensityFromIndex(CurrentIntensityIndex.Value);

        if (IsOwner && player_input != null)
            SubscribeInputs();
    }

    private void InitOnPlayerSpawn(PlayerStatus _playerStatus)
    {
        player_status = _playerStatus;
        player_input = player_status.player_input;

        if (IsOwner)
            SubscribeInputs();
    }

    private void SubscribeInputs()
    {
        var map = player_input?.actions;
        if (map == null) return;

        var switchAction = map.FindAction("LightSwitchAction", throwIfNotFound: false);
        if (switchAction != null)
            switchAction.performed += LightInputPerformed;

        var stepAction = map.FindAction("LightStepAction", throwIfNotFound: false);
        if (stepAction != null)
            stepAction.performed += LightStepPerformed;
    }

    // ON/OFF
    private void LightInputPerformed(InputAction.CallbackContext ctx)
    {
        LightOnOffChangeRequestServerRpc();
    }

    // 段階UP（次へ）
    private void LightStepPerformed(InputAction.CallbackContext ctx)
    {
        StepIntensityRequestServerRpc();
    }

    [ServerRpc]
    private void LightOnOffChangeRequestServerRpc(ServerRpcParams rpcParams = default)
    {
        if (!LightPermit || !HaveLight) return;

        bool next = !LightSwitchMode.Value;
        LightSwitchMode.Value = next;

        // 消灯時は段階リセット
        if (next)
            ClampAndFixStepServer();
    }

    [ServerRpc]
    private void StepIntensityRequestServerRpc(ServerRpcParams rpcParams = default)
    {
        if (!LightPermit || !HaveLight) return;
        if (!LightSwitchMode.Value) return; // 消灯中は無視
        if (IntensitySteps == null || IntensitySteps.Count == 0) return;

        int next = (CurrentIntensityIndex.Value + 1) % IntensitySteps.Count;
        CurrentIntensityIndex.Value = next;
    }

    private void OnLightSwitchChanged(bool prev, bool nowOn)
    {
        if (!Light) return;

        if (nowOn)
        {
            Light.enabled = true;
            ApplyIntensityFromIndex(CurrentIntensityIndex.Value);
            if (LightAudioSource && LightOnClip) LightAudioSource.PlayOneShot(LightOnClip);
        }
        else
        {
            if (LightAudioSource && LightOffClip) LightAudioSource.PlayOneShot(LightOffClip);
            Light.enabled = false;
        }
    }

    private void OnIntensityIndexChanged(int prev, int cur)
    {
        ApplyIntensityFromIndex(cur);
        if (LightAudioSource && LightStepClip && LightSwitchMode.Value)
            LightAudioSource.PlayOneShot(LightStepClip);
    }

    private void ApplyIntensityFromIndex(int index)
    {
        if (!Light) return;
        if (IntensitySteps == null || IntensitySteps.Count == 0) return;
        int i = Mathf.Clamp(index, 0, IntensitySteps.Count - 1);
        Light.intensity = IntensitySteps[i];
    }

    private void ClampAndFixStepServer()
    {
        if (IntensitySteps == null || IntensitySteps.Count == 0)
            IntensitySteps = new List<float>() { 3f, 6f, 10f };
        CurrentIntensityIndex.Value = Mathf.Clamp(CurrentIntensityIndex.Value, 0, IntensitySteps.Count - 1);
    }
}
