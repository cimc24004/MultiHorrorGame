using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
[RequireComponent(typeof(NetworkObject))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(NetworkAnimator))] // サーバー権限同期

public class AnimSpeedSync_ServerAuth : NetworkBehaviour
{
    static readonly int SpeedMul = Animator.StringToHash("SpeedMul");
    Animator anim;

    void Awake() => anim = GetComponent<Animator>();

    public void SetAnimSpeed(float mul)
    {
        mul = Mathf.Clamp(mul, 0f, 3f);
        if (IsServer) anim.SetFloat(SpeedMul, mul);
        else if (IsOwner) SetAnimSpeedServerRpc(mul);
    }
    public float GetAnimSpeed()
    {
        return anim.GetFloat(SpeedMul);
    }
    [ServerRpc]
    void SetAnimSpeedServerRpc(float mul)
        => anim.SetFloat(SpeedMul, Mathf.Clamp(mul, 0f, 3f));

}
