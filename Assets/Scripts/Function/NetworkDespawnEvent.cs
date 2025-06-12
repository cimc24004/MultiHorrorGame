using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Sirenix.OdinInspector;
public class NetworkDespawnEvent : NetworkBehaviour
{
    public bool OnStart;
    [ShowIf("OnStart")]
    public float OnStartDestroyTime;
    private NetworkObject m_NetworkObject;
    public override void OnNetworkSpawn()
    {
        m_NetworkObject = this.gameObject.GetComponent<NetworkObject>();
        if (OnStart)
        {
            DODestroyWithTime(OnStartDestroyTime);
        }
    }

    public void DODestroyWithTime(float _time)
    {
        StartCoroutine(DODestroyIEnumerator(_time));
    }
    //UnityEventから実行したい為にこの関数は存在する。
    private IEnumerator DODestroyIEnumerator(float WaitTime)
    {
        yield return new WaitForSeconds(WaitTime);
        m_NetworkObject.Despawn(true);
    }
}
