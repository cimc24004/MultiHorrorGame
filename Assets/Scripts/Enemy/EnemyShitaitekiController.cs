using Unity.Netcode;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.AI;
using DG.Tweening;            // ★ 追加

public class EnemyShitaitekiController : NetworkBehaviour
{
    [SerializeField] private bool MoveNow = false;
    [SerializeField] private AudioSource ScreamSound;
    private CurseController curseController;
    private DiscoveryPlayerController discoveryPlayerController;
    private Animator m_Animator;
    private FollowObjectByNav m_FollowObjectByNav;
    private EnemyStatus enemyStatus;
    private EnemyToPlayerKillController enemyToPlayerKillController;
    private EnemyActionForDoorController enemyActionForDoorController;
    private MoveSpeedController moveSpeedController;
    private float DefaultWalkSpeed;
    private float DefaultRunSpeed;
    [SerializeField] private PlayerNearSensor playerNearSensor;
    // スピード用の tween を管理して途中で Kill できるようにする
    private Sequence speedTweenSeq;

    // スピードを tween で滑らかに変更
    private void TweenSpeed(
        float targetRun,
        float targetWalk,
        float targetAnimator,
        float duration  = 1f,
        Ease  ease      = Ease.OutQuad,
        TweenCallback onComplete = null   // ★ 追加
    ){
        // 走行中の tween があれば止める
        if (speedTweenSeq != null && speedTweenSeq.IsActive())
            speedTweenSeq.Kill();

        speedTweenSeq = DOTween.Sequence()
            .Join(DOTween.To(() => moveSpeedController.RunSpeed,
                            x => moveSpeedController.RunSpeed = x,
                            targetRun, duration).SetEase(ease))
            .Join(DOTween.To(() => moveSpeedController.WalkSpeed,
                            x => moveSpeedController.WalkSpeed = x,
                            targetWalk, duration).SetEase(ease))
            .Join(DOTween.To(() => m_Animator.speed, x => m_Animator.speed = x,targetAnimator, duration))
            .OnComplete(onComplete);       // ★ 完了時に呼ぶ
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public override void OnNetworkSpawn()
    {
        discoveryPlayerController = this.gameObject.GetComponent<DiscoveryPlayerController>();
        m_Animator = this.gameObject.GetComponent<Animator>();
        enemyToPlayerKillController = this.gameObject.GetComponent<EnemyToPlayerKillController>();
        m_FollowObjectByNav = this.gameObject.GetComponent<FollowObjectByNav>();
        moveSpeedController = this.gameObject.GetComponent<MoveSpeedController>();
        DefaultWalkSpeed = moveSpeedController.WalkSpeed;
        DefaultRunSpeed = moveSpeedController.RunSpeed;
        enemyStatus = this.gameObject.GetComponent<EnemyStatus>();
        enemyActionForDoorController = this.gameObject.GetComponent<EnemyActionForDoorController>();
        curseController = GameController.instance.gameObject.GetComponent<CurseController>();
        curseController.ShitaitekiMovingEvent.AddListener(Move);
        curseController.ShitaitekiStoppingEvent.AddListener(Stop);
        this.Stop();
    }
    // Update is called once per frame
    void Update()
    {

    }
    [Button]
    public void Move()
    {
        //徐々にSpeedをDefaultRunSpeedにする。
        TweenSpeed(DefaultRunSpeed, DefaultWalkSpeed, 1, 2f, Ease.OutQuad,
        () =>
        {
            // 走り出し完了！
            Debug.Log("Move tween 完了");
            // ここで追加処理（BGM 切替えやエフェクト再生など）もできる
            playerNearSensor.IsNearToKillEffect = true;
            discoveryPlayerController.DiscoveryPlayerBySightning = true;
            m_FollowObjectByNav.MovePermit = true;
            enemyToPlayerKillController.enabled = true;
            enemyActionForDoorController.enabled = true;
            enemyStatus.foot_step_sound_controller.enabled = true;
            MoveNow = true;
        });
        ScreamSound.Play();
    }
    [Button]
    public void Stop()
    {
        //徐々にSpeedを0に
        // moveSpeedController.RunSpeed = 0;
        // moveSpeedController.WalkSpeed = 0;
        // m_Animator.speed = 0;

        ScreamSound.Stop();
        TweenSpeed(0, 0, 0, 2f, Ease.OutQuad,
        () =>
        {
            enemyToPlayerKillController.enabled = false;
            enemyStatus.enemy_move_state = EnemyStatus.EnemyMoveState.Stop;
            playerNearSensor.IsNearToKillEffect = false;
            discoveryPlayerController.DiscoveryPlayerBySightning = false;
            enemyStatus.foot_step_sound_controller.enabled = false;
            discoveryPlayerController.ForceLostSight();
            m_FollowObjectByNav.MovePermit = false;
            MoveNow = false;
            enemyActionForDoorController.enabled = false;
        });
    }
}
