using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ScoreTimeManager : NetworkSingleton<ScoreTimeManager>
{
    public NetworkVariable<float> ScoreTimeNow = new NetworkVariable<float>(0);
    [SerializeField] private bool MeasureNow;
    void Update()
    {
        if (IsServer)
        {
            if (MeasureNow)
            {
                ScoreTimeNow.Value += Time.deltaTime;
            }
        }

    }
    public void MeasureTimeStart()
    {
        MeasureTimeStartRequestServerRpc();
    }
    [ServerRpc(RequireOwnership = false)]
    private void MeasureTimeStartRequestServerRpc()
    {
        MeasureNow = true;
    }
}
