using Unity.Netcode;
using UnityEngine;
using System.Collections;

[DisallowMultipleComponent]
public class OwnerSpawnLatch : NetworkBehaviour
{
    private CharacterController cc;
    public bool Latched { get; private set; }
    [SerializeField] private float failSafeSeconds = 2f; // 解除が来なければタイムアウトでON

    void Awake()
    {
        cc = GetComponentInChildren<CharacterController>(true);
    }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner || cc == null) return;

        cc.enabled = false;
        Latched = true;
        StartCoroutine(FailSafeEnable());
        Debug.Log($"[OwnerSpawnLatch] OFF at spawn (client {NetworkManager.Singleton.LocalClientId})");
    }

    /// <summary>初期Teleport後に GameNetworkController から呼ぶ</summary>
    public void ReleaseAfterTeleport()
    {
        if (!IsOwner || cc == null || !Latched) return;
        StopAllCoroutines(); // FailSafe停止
        StartCoroutine(ReleaseRoutine());
    }

    private IEnumerator ReleaseRoutine()
    {
        // 1フレ + 固定更新待ってからON（めり込み/落下防止）
        yield return null;
        yield return new WaitForFixedUpdate();
        cc.enabled = true;
        Latched = false;
        Debug.Log("[OwnerSpawnLatch] ON by ReleaseAfterTeleport()");

        // ★ ここで「操作開始OK」をプレイヤーに通知
        var pms = GetComponent<PlayerMoveScript>();
        if (pms != null) pms.OwnerGainedControl();
    }

    private IEnumerator FailSafeEnable()
    {
        float t = 0f;
        while (t < failSafeSeconds && Latched)
        {
            t += Time.unscaledDeltaTime;
            yield return null;
        }
        if (Latched && cc != null)
        {
            cc.enabled = true;
            Latched = false;
            Debug.LogWarning("[OwnerSpawnLatch] ON by FailSafe (timeout)");

            var pms = GetComponent<PlayerMoveScript>();
            if (pms != null) pms.OwnerGainedControl();
        }
    }
}
