using System.Collections;
using UnityEngine;
using DG.Tweening;
using Cinemachine;
using Sirenix.OdinInspector;

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

    [Title("敵に発見された時の演出関連")]
    public GameObject Discoveried;
    public GameObject DiscoveriedSound;
    public GameObject DiscoveryVolume;
    [Title("巫女出現")]
    public AudioSource MikoApperanceSound;

    [Title("敵に殺される時の演出")]
    public GameObject PlayerKill;

    private AudioClip MainDefaultBGM;
    
    private CinemachineImpulseSource cinemachine_impulse_source;
    private GameControllerOnAllScene game_controller_on_all_scene;
    private URPFxController urp_fx_controller;
    private PlayerStatus player_status;
    private CurseController curse_controller;

    
    [SerializeField]
    // 現在の状態
    private HorrorState currentState = HorrorState.None;
    [SerializeField]
    // 「近い敵」の数
    public int nearEnemyCount = 0;

    [SerializeField]
    // 「プレイヤーを発見している敵」の数
    public int discoveredEnemyCount = 0;

    //======================================================
    // 初期化
    //======================================================
    void Start()
    {
        cinemachine_impulse_source = GetComponent<CinemachineImpulseSource>();
        MainDefaultBGM = MainBGM.clip;
        player_status = PlayerStatus.instance;
        curse_controller = CurseController.instance;
        game_controller_on_all_scene = GameControllerOnAllScene.instance;
        urp_fx_controller = game_controller_on_all_scene.GetComponent<URPFxController>();
    }

    private void Update()
    {
        Debug.Log("Status: " + currentState);
        if(player_status.PlayerDeaded){
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
        Main.SetActive(false);
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
        //if (currentState == newState) return;
        //一度リセットしてから新しいStateに切り替える。 同時に同じ演出は行わない設定になってる。
        ResetSetting();
        //ExitCurrentState();
        currentState = newState;
        switch (newState)
        {
            case HorrorState.None:
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
        Main.SetActive(false);
        ToEnemyNear.SetActive(true);

        urp_fx_controller.CheapCameraChange(true);
        urp_fx_controller.VHSChange(true);
    }

    /// <summary>
    /// NearEnemy の演出を解除して通常状態へ
    /// </summary>
    public void NearEnemyReset()
    {
        // フェードアウト等をしたあと、最終的に None へ
        ToEnemyNearBGMAudioSource.DOFade(0, 1)
            .SetEase(Ease.Linear)
            .SetLink(this.gameObject)
            .OnComplete(() =>
            {
                ToEnemyNearBGMAudioSource.volume = 1;
            });

        ToEnemyNearHeartBeatSoundSource.DOFade(0, 1)
            .SetEase(Ease.Linear)
            .SetLink(this.gameObject)
            .OnComplete(() =>
            {
                ToEnemyNearHeartBeatSoundSource.volume = 1;
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
        curse_controller.CurseIncrease(10.0f);

        DiscoveriedSound.SetActive(false);
        DiscoveriedSound.SetActive(true);

        urp_fx_controller.CheapCameraChange(false);
        urp_fx_controller.VHSChange(false);
        urp_fx_controller.InteferecesChange(true);
        urp_fx_controller.HackedChange(true);

        Main.SetActive(false);
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

    /// <summary>
    /// 全部リセットして None 状態に戻す
    /// </summary>
    public void MainBGMReset(){
        MainBGM.clip = MainDefaultBGM;
    }
    public void ResetSetting()
    {
        Debug.Log("ResetSettingResetSettingResetSettingResetSettingResetSettingResetSettingResetSettingHOrrorDirector");
        //StartCoroutine(ResetCoroutine());
        // 見つかった演出のオブジェクトをOFF
        Discoveried.SetActive(false);
        // 近い演出のオブジェクトをOFF
        ToEnemyNear.SetActive(false);
        urp_fx_controller.CheapCameraChange(false);
        urp_fx_controller.VHSChange(false);
        urp_fx_controller.InteferecesChange(false);

        Main.SetActive(true);
        if(MainBGMAutoChange){
            MainBGM.clip = MainDefaultBGM;
        }
        MainBGM.volume = 1;
        MainBGM.Play();
    }

    
}
