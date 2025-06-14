using System.Collections;
using DG.Tweening;
using Unity.Netcode;
using UnityEngine;

public class TV : NetworkBehaviour
{
    private ButsudansReveal butsudans_reveal;

    [SerializeField] private Material TurnOnTVMaterialMonitor;
    [SerializeField] private Material TurnOffTVMaterialMonitor;
    [SerializeField] private MeshRenderer TVMonitorMeshRenderer;
    [SerializeField] private AudioSource TVAudioSource;
    [SerializeField] private AudioSource HouseRevealAudioSource;

    void Awake()
    {
        butsudans_reveal = GetComponent<ButsudansReveal>();
        if (butsudans_reveal == null)
        {
            Debug.LogError("ButsudansReveal がアタッチされてないよ！");
        }
    }

    // ボタンやスクリプトから呼ぶ ON メソッド
    public void TurnOn()
    {
        if (IsServer)
        {
            DoTurnOn();
        }
        else
        {
            TurnOnServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void TurnOnServerRpc(ServerRpcParams rpcParams = default)
    {
        DoTurnOn();
    }

    private void DoTurnOn()
    {
        var mats = TVMonitorMeshRenderer.sharedMaterials;
        if (mats.Length > 1)
        {
            mats[1] = TurnOnTVMaterialMonitor;
            TVMonitorMeshRenderer.sharedMaterials = mats;
        }

        TVAudioSource.Play();
        // 1秒後に TurnOff() へ
        DOVirtual.DelayedCall(1f, () => TurnOff());
    }

    // ボタンやスクリプトから呼ぶ OFF メソッド
    public void TurnOff()
    {
        if (IsServer)
        {
            DoTurnOff();
        }
        else
        {
            TurnOffServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void TurnOffServerRpc(ServerRpcParams rpcParams = default)
    {
        DoTurnOff();
    }

    private void DoTurnOff()
    {
        // 仏壇位置表示のサーバーRPC（内部でClientRpc飛ぶ）
        butsudans_reveal.RevealButsudansServerRpc();

        // TV モニターOFF同期
        RequestTvMonitorClientRpc();
    }

    [ClientRpc]
    private void RequestTvMonitorClientRpc(ClientRpcParams rpcParams = default)
    {
        var mats = TVMonitorMeshRenderer.sharedMaterials;
        if (mats.Length > 1)
        {
            mats[1] = TurnOffTVMaterialMonitor;
            TVMonitorMeshRenderer.sharedMaterials = mats;
        }

        HouseRevealAudioSource.Play();
        TVAudioSource.Stop();
    }
}
