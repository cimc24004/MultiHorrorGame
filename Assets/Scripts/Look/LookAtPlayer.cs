using System.Collections;
using UnityEngine;
using DG.Tweening;

public class LookAtPlayer : MonoBehaviour
{
    // プレイヤーを見る為だけのスクリプト。
    public enum PlayMode
    {
        Update,
        Start
    }
    public enum LookAt
    {
        Player,
        CameraRoot
    }

    [Header("追いかける対象")]
    public LookAt look_at;

    [Header("一度だけ or 毎フレーム？")]
    public PlayMode play_mode;

    // キャッシュ用
    private PlayerStatus player_status;
    private Transform headPosTransform;
    private Transform cameraRootTransform;
    [SerializeField] private bool initialized = false;
    void OnEnable()
    {
        PlayerStatus.OnLocalPlayerSpawned += InitOnPlayerSpawn;
        if (PlayerStatus.LocalInstance != null)
        {
            InitOnPlayerSpawn(PlayerStatus.LocalInstance);
        }
    }
    void OnDisable()
    {
        PlayerStatus.OnLocalPlayerSpawned -= InitOnPlayerSpawn;
    }
    private void InitOnPlayerSpawn(PlayerStatus _playerStatus){
        player_status = _playerStatus;
        headPosTransform    = player_status.HeadPos.transform;
        cameraRootTransform = player_status.CameraRoot.transform;
        initialized = true;

        // Start モードなら一度だけ向かせる
        if (play_mode == PlayMode.Start)
        {
            Vector3 target = (look_at == LookAt.Player)
                ? headPosTransform.position
                : cameraRootTransform.position;

            transform
                .DODynamicLookAt(target, 0f)
                .SetLink(gameObject);
        }
    }

    private void LateUpdate()
    {
        // 準備完了＆Update モードのときだけ毎フレーム向く
        if (!initialized || play_mode != PlayMode.Update) return;

        Vector3 target = (look_at == LookAt.Player)
            ? headPosTransform.position
            : cameraRootTransform.position;

        transform
            .DODynamicLookAt(target, 0.3f)
            .SetLink(gameObject);
    }
}
