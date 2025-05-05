using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeedKeysController : Singleton<NeedKeysController>
{
    //すでに取得した鍵の個数
    public int GottenKeyCount;
    [SerializeField] public OpenPermitByKeyController before_open_permit_by_key_controller;
    //鍵が必要な部屋数
    [SerializeField] public int NeedKeysCount; 
    public List<OpenPermitByKeyController> open_permit_by_key_controllers;
    // Start is called before the first frame update
    void Start()
    {
        foreach(GameObject lock_doors_controller in GameObject.FindGameObjectsWithTag("LockDoorsController")){
            NeedKeysCount++;
        }
    }


    public bool NeedKeyCheck(){
        if(GottenKeyCount >= NeedKeysCount){
            return false;
        }else{
            return true;
        }
    }
}
