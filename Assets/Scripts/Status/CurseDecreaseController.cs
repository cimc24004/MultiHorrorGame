using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering;
public class CurseDecreaseController : MonoBehaviour
{
    private CurseController curse_controller;
    [SerializeField] private float CurseDecreaseAmount = 10;
    // Start is called before the first frame update

    void Start()
    {
        curse_controller = CurseController.Instance
            ?? FindObjectOfType<CurseController>();
    }
    public void CurseDecrease()
    {
        //サーバー or クライアントから実行される。
        //PrayCompletedEventForServer.Invoke(サーバーから) もしくは InstantiateScriptのStartEvent(クライアントから)から呼ばれる。
        //PrayCompletedEventForServerから呼ばれる場合でもServerRpcってつけているがホストクライアント構成だからこれでも動く。
        curse_controller.CurseDecreaseRequestFromOutsideRequestServerRpc(CurseDecreaseAmount);
    }
    
}
