using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System;

public class ConfigController : SingleTonDontDestory<ConfigController>
{
    [Serializable]
    public class ConfigStatus
    {

        public string config_name;
        public float value;

        public GameObject UI;

        public ConfigStatus(string name, float value)
        {
            this.config_name = name;
            this.value = value;
        }

    }
    

    [SerializeField] private PlayerInput player_input;


    public ConfigStatus MouseSensivityStatus = new ConfigStatus("MouseSensivity", 0);




    public void OnChangeSliderMouseSens(float _sens, GameObject me)
    {
        if(MouseSensivityStatus.UI == null)
        {
            MouseSensivityStatus.UI = me;
        }
        MouseSensivityStatus.value = _sens;
    }
}
