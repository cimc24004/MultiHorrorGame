using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class LocalPlayerMeshInActive : NetworkBehaviour
{
    [SerializeField] private List<GameObject> target_inactive_objects;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            return;
        }
        foreach (GameObject target_inactive_object in target_inactive_objects)
        {
            target_inactive_object.gameObject.SetActive(false);
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
