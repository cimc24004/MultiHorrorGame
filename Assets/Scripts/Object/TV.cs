
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Unity.Netcode;
public class TV : MonoBehaviour
{
    private ButsudansReveal butsudans_reveal;
    [SerializeField]private Material TurnOnTVMaterialMonitor;
    [SerializeField] private Material TurnOffTVMaterialMonitor;
    [SerializeField] private MeshRenderer TVMonitorMeshRenderer;
    [SerializeField] private AudioSource TVAudioSource;
    [SerializeField] private AudioSource HouseRevealAudioSource;
    // Start is called before the first frame update
    void Start()
    {
        butsudans_reveal = this.gameObject.GetComponent<ButsudansReveal>();
    }

    public void TurnOn(){
        // ① 今の配列を取る
        var mats = TVMonitorMeshRenderer.sharedMaterials;   // ← 複製配列

        // ② 差し替える
        if (mats.Length > 1) mats[1] = TurnOnTVMaterialMonitor;

        // ③ 戻し直す
        TVMonitorMeshRenderer.sharedMaterials = mats;

        TVAudioSource.Play();
        DOVirtual.DelayedCall(1, () => {
            TurnOff();
        });
    }
    public void TurnOff()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            butsudans_reveal.RevealButsudansAction();
        }

        var mats = TVMonitorMeshRenderer.sharedMaterials;
        if (mats.Length > 1) mats[1] = TurnOffTVMaterialMonitor;

        TVMonitorMeshRenderer.sharedMaterials = mats;
        HouseRevealAudioSource.Play();
        TVAudioSource.Stop();
    }
}
