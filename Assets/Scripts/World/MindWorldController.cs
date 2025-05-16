using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class MindWorldController : MonoBehaviour
{
    private List<GameObject> Lights;
    private TurningOnOffLightController turning_on_off_light_controller;
    // Start is called before the first frame update
    void Start()
    {
        Invoke("AllLightOn", 1);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void AllLightOn()
    {
        Lights = GameObject.FindGameObjectsWithTag("Light").ToList();
        Debug.Log(Lights.Count);
        foreach (GameObject Light in Lights)
        {
            turning_on_off_light_controller = Light.GetComponent<TurningOnOffLightController>();
            Debug.Log(turning_on_off_light_controller);
            if (NetworkManager.Singleton.IsServer)
            {
                turning_on_off_light_controller.TurnOnOffByStatusClientRPC();
            }
        }
    }
}
