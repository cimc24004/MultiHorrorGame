using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System;
using UnityEngine.SceneManagement;
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

    public GameObject ConfigCanvas;

    public ConfigStatus MouseSensivityStatus = new ConfigStatus("MouseSensivity", 0);

    private StageController stage_controller;

    public GameControllerOnAllScene game_controller_all_scene;
    
    private GameStateController game_state_controller;

    public InputActionReference CommonActionESC;
    //
    [SerializeField]private bool Opended = false;
    private void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        stage_controller = StageController.instance;
        game_state_controller = game_controller_all_scene.gameObject.GetComponent<GameStateController>();
    }

    private void Update()
    {
        if (CommonActionESC.action.triggered)
        {
            if (!Opended)
            {
                //‚Ð‚ç‚¢‚½
                game_state_controller.MenuOnGamePlayMode(true);
                ConfigCanvas.gameObject.SetActive(true);
                Opended = true;
            }
            else
            {

                //‚Æ‚¶‚½
                game_state_controller.GameplayMode();
                ConfigCanvas.gameObject.SetActive(false);
                Opended = false;
            }

        }
    }

    public void OnChangeSliderMouseSens(float _sens, GameObject me)
    {
        if(MouseSensivityStatus.UI == null)
        {
            MouseSensivityStatus.UI = me;
        }
        MouseSensivityStatus.value = _sens;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        stage_controller = StageController.instance;
        Debug.Log("StageController: " + stage_controller);
    }
}
