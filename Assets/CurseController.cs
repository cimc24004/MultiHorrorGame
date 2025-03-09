using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
public class CurseController : Singleton<CurseController>
{
    public enum CurseEventType
    {
        MikoInstantiate,
        ItemLoss
    }

    [SerializeField] private GameObject MikoPrefab;
    [SerializeField] private Transform MikoApperancePlaceTrans;

    private GameObject InstantiatedMiko;
    private PlayerStatus player_status;
    private bool CurseMaxedNow;
    //呪いゲージがマックスになった時イベントが起こり続ける時間
    private float CurseMaxEventWhileTime;
    private float CurseMaxEventWhileTimeNow;

    private CurseEventType curse_event_type;

    private void Start()
    {
        player_status = PlayerStatus.instance;
    }
    private void Update()
    {
        if (CurseMaxedNow)
        {
            CurseMaxEventWhileTimeNow += Time.deltaTime;
            if(CurseMaxEventWhileTimeNow > CurseMaxEventWhileTime)
            {
                CurseMaxedReset();
            }
        }
    }


    public void CurseMaxed(float _CurseMaxEventWhileTime)
    {
        //すでに呪いゲージがマックスの時のイベントが行われているなら無視
        if (CurseMaxedNow) return;

        this.CurseMaxEventWhileTime = _CurseMaxEventWhileTime;

        this.CurseMaxEventWhileTimeNow = 0;
        //ランダムで何かの関数を実行するとする
        MikoInstantiate();


        CurseMaxedNow = true;
    }
    private void CurseMaxedReset()
    {

        if (InstantiatedMiko != null)
        {
            Destroy(InstantiatedMiko);
        }

        player_status.CurseValueNow = 0;
        CurseMaxEventWhileTimeNow = 0;
        CurseMaxedNow = false;
    }
    private void MikoInstantiate()
    {

        //もうすでに作ってあるなら無視
        if (InstantiatedMiko != null) return;
        curse_event_type = CurseEventType.MikoInstantiate;
        InstantiatedMiko = Instantiate(MikoPrefab, MikoApperancePlaceTrans.position, Quaternion.identity);
    }

    
}
