using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialChangeController : MonoBehaviour
{
    public bool MaterialChange;
    [Header("マテリアルが変更されてからデフォルトのマテリアルに戻るまでの時間")]
    [SerializeField] private float ToReturnDefaultTimeFromStartChanged;
    private float ToReturnDefaultTimeFromStartChangedNow;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (MaterialChange)
        {
            ToReturnDefaultTimeFromStartChangedNow += Time.deltaTime;
            if(ToReturnDefaultTimeFromStartChangedNow > ToReturnDefaultTimeFromStartChanged)
            {
                MaterialChange = false;
                ToReturnDefaultTimeFromStartChangedNow = 0;
            }
        }
        else
        {
            ToReturnDefaultTimeFromStartChangedNow = 0;
        }
    }
}
