using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
public class ProgressWithMapUIController : MonoBehaviour
{
    public GameObject ProgressWithMapCanvasObject;
    public InputActionReference tab_input_action_reference;
    
    public TextMeshProUGUI ButsudanPrayCountTextUI;
    public Toggle ButsudanAllPrayedToggleUI;


    public Toggle DeliverItemGetedToggleUI;

    private GameStateController game_state_controller;

    private bool Opended;
    // Start is called before the first frame update
    void Start()
    {
        game_state_controller = GameControllerOnAllScene.instance.gameObject.GetComponent<GameStateController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (tab_input_action_reference.action.triggered)
        {
            if (!Opended)
            {
                //‚Ð‚ç‚­
                game_state_controller.MenuOnGamePlayMode(false);
                ProgressWithMapCanvasObject.SetActive(true);
                Opended = true;
            }
            else
            {
                //‚Æ‚¶‚½
                game_state_controller.GameplayMode();
                ProgressWithMapCanvasObject.SetActive(false);
                Opended = false;
            }

        }
    }

    public void ButsudanPrayCount()
    {

    }
}
