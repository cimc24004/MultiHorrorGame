using UnityEngine;

public class EnemyToPlayerKillController : MonoBehaviour
{
    public Transform boxCenter;                  // 範囲の中心（敵の目の前に置く）
    public Vector3   boxHalfExtents = new(0.3f, 0.6f, 0.4f);
    public LayerMask layerMask;    // Player レイヤーだけにしておく

    private DiscoveryPlayerController discovery;
    private PlayerStatus              playerStatus;
    private Collider[] hitBuffer = new Collider[4];   // 最大ヒット数を決め打ちで確保

    void Start()
    {
        discovery    = GetComponent<DiscoveryPlayerController>();
        playerStatus = PlayerStatus.LocalInstance;
    }

    void Update()
    {
        return;
        if (playerStatus.Dead.Value == true || playerStatus.HidingNow.Value == true || !discovery.DiscoveryPlayerNow)
            return;

        // Box の向きは敵の forward に合わせる
        Quaternion orientation = boxCenter.rotation;

        int hitCount = Physics.OverlapBoxNonAlloc(
            boxCenter.position,
            boxHalfExtents,
            hitBuffer,
            orientation,
            layerMask,
            QueryTriggerInteraction.Ignore);

        for (int i = 0; i < hitCount; i++)
        {
            if (hitBuffer[i].CompareTag("Player"))
            {
                playerStatus.DeadEvent(this.gameObject);
                break;
            }
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (!boxCenter) return;
        Gizmos.color = Color.cyan;
        Matrix4x4 m = Matrix4x4.TRS(boxCenter.position, boxCenter.rotation, Vector3.one);
        Gizmos.matrix = m;
        Gizmos.DrawWireCube(Vector3.zero, boxHalfExtents * 2);
    }
#endif
}
