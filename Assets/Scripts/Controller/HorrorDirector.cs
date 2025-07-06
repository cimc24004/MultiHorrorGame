using System.Collections;
using UnityEngine;
using DG.Tweening;
using Cinemachine;
using Sirenix.OdinInspector;
using UnityEngine.Rendering;
using System;
public enum HorrorState
{
    None,             // 通常状態
    EnemyNear,        // 敵が近い
    Discovered,       // 敵に見つかった

    PlayerKilled //殺される。
}

public class HorrorDirector : Singleton<HorrorDirector>
{
    public bool MainBGMAutoChange = true;
    [Title("メインBGM関連")]
    public GameObject Main;
    public AudioSource MainBGM;

    [Title("敵が近い時の演出関連")]
    public GameObject ToEnemyNear;
    public AudioSource ToEnemyNearBGMAudioSource;
    public AudioSource ToEnemyNearHeartBeatSoundSource;
    [SerializeField] private AudioSource GhostAppearToNearBGM;
    [SerializeField] private AudioFadeController ToEnemyNearBGMAudioFadeController;
    [SerializeField] private AudioFadeController ToEnemyNearHeartBeatAudioFadeController;
    [SerializeField] private AudioFadeController MainBGMAudioFadeController;
    [Title("敵に発見された時の演出関連")]
    public GameObject Discoveried;
    public GameObject DiscoveriedSound;
    public GameObject DiscoveryVolume;

    [Title("巫女出現")]
    public AudioSource MikoApperanceSound;

    [Title("敵に殺される時の演出")]
    public GameObject PlayerKill;

    [Title("敵に殺されそう&&近づいてくるときの演出")]
    [SerializeField] private AudioSource EnemyNearToKillBGM;
    [SerializeField] private float maxDetectDistance = 15f;
    [SerializeField] private float minDetectDistance = 2f;
    [SerializeField] private PostProcessVolumeController EnemyNearToKillVolumeController;
    private AudioClip MainDefaultBGM;
    
    private GameControllerOnAllScene game_controller_on_all_scene;
    private URPFxController urp_fx_controller;
    private PlayerStatus player_status;
    private CurseController curse_controller;
    private CameraShakeController cameraShakeController;
    private ClosestEnemyController closest_enemy_controller;
    [Title("だるまさんがころんだゲーム演出")]
    [SerializeField]
    private GameObject DarumasanGame;
    [Header("For Debug")]
    [SerializeField]
    // 現在の状態
    private HorrorState currentState = HorrorState.None;
    [SerializeField]
    // 「近い敵」の数
    public int nearEnemyCount = 0;

    [SerializeField]
    // 「プレイヤーを発見している敵」の数
    public int discoveredEnemyCount = 0;
    void OnEnable()
    {
        PlayerStatus.OnLocalPlayerSpawned += InitOnPlayerSpawn;
    }
    void OnDisable()
    {
        PlayerStatus.OnLocalPlayerSpawned -= InitOnPlayerSpawn;
    }
    private void InitOnPlayerSpawn(PlayerStatus _playerStatus)
    {
        player_status = _playerStatus;
        cameraShakeController = CameraShakeController.instance;
        game_controller_on_all_scene = GameControllerOnAllScene.instance;
        urp_fx_controller = game_controller_on_all_scene.GetComponent<URPFxController>();
        closest_enemy_controller = ClosestEnemyController.instance;
        MainDefaultBGM = MainBGM.clip;
        curse_controller = GameController.instance.gameObject.GetComponent<CurseController>();
    }

    private void Update()
    {
        if(player_status == null) return;
        if (closest_enemy_controller.ClosestEnemyWithDistance.Key != null && closest_enemy_controller.ClosestEnemyWithDistance.Value.player_near_sensor.IsNearToKillEffect)
        {
            // 距離を 0?1 正規化
            float dist = closest_enemy_controller.ClosestEnemyWithDistance.Value.Distance;
            float t = Mathf.Clamp01(
                (maxDetectDistance - dist) / (maxDetectDistance - minDetectDistance)
            );

            // シーン内で毎フレームフェード値を更新
            EnemyNearToKillVolumeController.FadeTo(
                t,
                duration: 0.1f,            // サクッと追従したいなら短時間
                onUpdate: fadeVal =>
                {
                    EnemyNearToKillBGM.volume = fadeVal;  // 同期して音量もコントロール
                }
            );
        }
        else
        {
            // 敵がいなければ 0 へフェードアウト
            EnemyNearToKillVolumeController.FadeTo(0f, 0.2f, fadeVal => EnemyNearToKillBGM.volume = fadeVal);
        }

        if(player_status.PlayerKilledScene){
            discoveredEnemyCount = 0;
            nearEnemyCount = 0;
            ChangeHorrorState(HorrorState.PlayerKilled);
        
        }else{
        }
    }
    //======================================================
    // 敵が「近い」状態を管理するメソッド
    //======================================================

    /// <summary>
    /// 敵がプレイヤーに近づいたときに呼ぶ
    /// </summary>
    public void AddNearEnemy()
    {
        nearEnemyCount++;


        // 0 -> 1 になったタイミングで NearEnemy 演出開始
        if (nearEnemyCount >= 1 && discoveredEnemyCount == 0)
        {
            if(nearEnemyCount == 1){
                GhostAppearToNearBGM.Play();
            }
            ChangeHorrorState(HorrorState.EnemyNear);
        }
    }

    /// <summary>
    /// 敵がプレイヤーから離れた(近くなくなった)時に呼ぶ
    /// </summary>
    public void RemoveNearEnemy()
    {
        nearEnemyCount = Mathf.Max(nearEnemyCount - 1, 0);
    
        // もし今が EnemyNear 状態で、カウントが 0 になったらリセット
        // （ただし既にDiscoveredになっていたらそちらが優先）
        if (nearEnemyCount == 0 && currentState == HorrorState.EnemyNear)
        {
            NearEnemyReset();
        }
    }


    //======================================================
    // 敵が「プレイヤーを発見」状態を管理するメソッド
    //======================================================

    /// <summary>
    /// ある敵がプレイヤーを見つけた(発見状態に入った)ときに呼ぶ
    /// </summary>
    public void EnemyDiscoveredPlayer()
    {
        discoveredEnemyCount++;

        ChangeHorrorState(HorrorState.Discovered);
    }

    public void PlayerKilled(){
        PlayerKill.SetActive(true);
        // Main.SetActive(false);
        urp_fx_controller.CheapCameraChange(true);
        urp_fx_controller.CorruptMemoryChange(true);
        urp_fx_controller.HackedChange(true);
    }
    /// <summary>
    /// 敵がプレイヤーを見失ったときに呼ぶ
    /// </summary>
    public void EnemyLostPlayer()
    {
        discoveredEnemyCount = Mathf.Max(discoveredEnemyCount - 1, 0);

        // 全員が見失った場合は、Discovered 演出を解除
        // ただし nearEnemy がまだあるなら、EnemyNear に戻す
        if (discoveredEnemyCount == 0)
        {
            // ここで「発見状態の終了処理」を経て
            // nearEnemyCount が1以上あれば EnemyNear,
            // それもなければ None に切り替える
            if (nearEnemyCount > 0)
            {
                ChangeHorrorState(HorrorState.EnemyNear);
            }
            else
            {
                ChangeHorrorState(HorrorState.None);
            }
        }
    }


    //======================================================
    // 状態を変更するときは ChangeHorrorState を通す
    //======================================================

    public void ChangeHorrorState(HorrorState newState)
    {
        Debug.Log("ChangeHorrorStateです！！！！！！！！: " + newState);
        // 同じ状態なら変化しない
        // if (currentState == newState) return;
        //一度リセットしてから新しいStateに切り替える。 同時に同じ演出は行わない設定になってる。
        // ResetSetting();
        //ExitCurrentState();
        currentState = newState;
        switch (newState)
        {
            case HorrorState.None:
                // MainBGMReset();
                ResetSetting();
                break;
            case HorrorState.EnemyNear:
                NearEnemy();
                break;
            case HorrorState.Discovered:
                DiscoveriedHorror();
                break;
            case HorrorState.PlayerKilled:
                PlayerKilled();
                break;
        }

    }
    public void MikoApperance(){
        this.MainBGM.clip = curse_controller.AppearanceMikoBGM;
        MikoApperanceSound.Play();
        MainBGMAudioFadeController.PlayWithFadeIn(1);
        this.MainBGMAutoChange = false;
    }
    /// <summary>
    /// 現状態から離脱するときの後処理
    /// </summary>
    public void ExitCurrentState()
    {
        switch (currentState)
        {
            case HorrorState.EnemyNear:
                // 近い状態の終了処理があれば
                break;
            case HorrorState.Discovered:
                // 発見状態の終了処理があれば
                // 例: Discoveried.SetActive(false); ...
                break;
            case HorrorState.None:
            default:
                // 特に無し
                break;
        }
    }


    /// <summary>
    /// 敵が近い(NearEnemy)ときの演出
    /// </summary>
    public void NearEnemy()
    {
        Debug.Log("NearEnemy関数が実行されました");
        // Main.SetActive(false);
        ResetSetting();
        ToEnemyNear.SetActive(true);
        ToEnemyNearBGMAudioFadeController.PlayWithFadeIn(1, () => {

        });
        ToEnemyNearHeartBeatAudioFadeController.PlayWithFadeIn(1, () => {

        });
        urp_fx_controller.CheapCameraChange(true);
        urp_fx_controller.VHSChange(true);
    }

    /// <summary>
    /// NearEnemy の演出を解除して通常状態へ
    /// </summary>
    public void NearEnemyReset()
    {
        // フェードアウト等をしたあと、最終的に None へ

        ToEnemyNearBGMAudioFadeController.FadeOutAndStop(0, () => {
            // ToEnemyNearBGMAudioSource.volume = 1;
        });

        ToEnemyNearHeartBeatAudioFadeController.FadeOutAndStop(0,() => {
            // ToEnemyNearHeartBeatSoundSource.volume = 1;
        });
        urp_fx_controller.CheapCameraChange(false);
        urp_fx_controller.VHSChange(false);

        ToEnemyNear.SetActive(false);

        // 最終的に None へ
        ChangeHorrorState(HorrorState.None);
    }

    /// <summary>
    /// 敵に見つかった(Discovered)ときの演出
    /// </summary>
    public void DiscoveriedHorror()
    {
        ResetSetting();

        DiscoveriedSound.SetActive(false);
        DiscoveriedSound.SetActive(true);
        cameraShakeController.ShakeCamera(1f, CameraShakeController.CameraImpulseSourceType.Discoveried);
        urp_fx_controller.CheapCameraChange(false);
        urp_fx_controller.VHSChange(false);
        urp_fx_controller.InteferecesChange(true);
        urp_fx_controller.HackedChange(true);

        // Main.SetActive(false);
        DiscoveryVolume.SetActive(true);

        Discoveried.SetActive(true);

        // 一定秒数後にボリュームオフなど
        DOVirtual.DelayedCall(1, () =>
        {
            DiscoveryVolume.SetActive(false);
            urp_fx_controller.InteferecesChange(false);
            urp_fx_controller.HackedChange(false);
        });
    }
    public void DarumasanGameEffect(bool GameNow)
    {
        //だるまさんゲームが進行している間のエフェクト
        if (GameNow)
        {
            DarumasanGame.SetActive(true);
            urp_fx_controller.VHSChange(true);

        }
        else
        {
            DarumasanGame.SetActive(false);
            urp_fx_controller.VHSChange(false);
        }
    }
    /// <summary>
    /// 全部リセットして None 状態に戻す
    /// </summary>
    public void MainBGMReset(){
        MainBGM.clip = MainDefaultBGM;
        MainBGMAudioFadeController.PlayWithFadeIn(1);
        if(MainBGMAutoChange){
            MainBGM.clip = MainDefaultBGM;
        }
    }
    public void ResetSetting()
    {
        //StartCoroutine(ResetCoroutine());
        // 見つかった演出のオブジェクトをOFF
        Discoveried.SetActive(false);
        // 近い演出のオブジェクトをOFF
        ToEnemyNear.SetActive(false);
        PlayerKill.SetActive(false);
        urp_fx_controller.CheapCameraChange(false);
        urp_fx_controller.VHSChange(false);
        urp_fx_controller.HackedChange(false);
        urp_fx_controller.InteferecesChange(false);
        urp_fx_controller.CorruptMemoryChange(false);
        // Main.SetActive(true);

        // MainBGMAudioFadeController.PlayWithFadeIn(1);
        // MainBGM.volume = 1;
        // MainBGM.Play();
    }

    
}
