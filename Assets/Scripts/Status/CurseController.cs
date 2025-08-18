using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Linq;
using DG.Tweening;
using UnityEngine.Events;
using UnityEngine.UI;
using System;
using Unity.Netcode;
public class CurseController : NetworkSingleton<CurseController>
{
    public enum CurseEventType
    {
        MikoInstantiate,
        ShitaitekiMove,
        PlayerStamina
    }

    [SerializeField] private GameObject MikoPrefab;
    [SerializeField] private Transform MikoApperancePlaceTrans;

    [SerializeField]
    [Header("現在の呪いゲージ値")]
    public NetworkVariable<float> CurseValueNow = new NetworkVariable<float>(0);
    [SerializeField]
    [Header("呪いゲージのMAX値")]
    private float CurseMaxValue = 100;

    [SerializeField] private Slider CurseGageUI;

    private GameObject InstantiatedMiko;
    private PlayerStatus player_status;
    private SubNoticedUIController sub_noticed_ui_controller;
    private PlayerStaminaController player_stamina_controller;
    public bool CurseMaxedEventNow;
    [SerializeField]
    //呪いゲージがマックスになった時イベントが起こり続ける時間
    private float CurseMaxEventWhileTime;
    [SerializeField]
    private float CurseMaxEventWhileTimeNow;
    [SerializeField]
    private CurseEventType curse_event_type;
    private HorrorDirector horror_director;
    public UnityEvent ShitaitekiMovingEvent;
    public UnityEvent ShitaitekiStoppingEvent;
    [SerializeField] public AudioClip AppearanceMikoBGM;

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
        player_stamina_controller = player_status.gameObject.GetComponent<PlayerStaminaController>();
        sub_noticed_ui_controller = player_status.sub_noticed_ui_controller;
        this.CurseValueNow.OnValueChanged += OnValueChangeCurseEvent;
    }
    public override void OnNetworkSpawn()
    {
        horror_director = HorrorDirector.instance;
    }
    private void Update()
    {
        if (!IsServer) return;
        if (CurseMaxedEventNow)
        {
            //サーバー側で実行する。
            CurseMaxEventWhileTimeNow += Time.deltaTime;

            if (CurseMaxEventWhileTimeNow > CurseMaxEventWhileTime)
            {
                //マックスに達してある程度時間がたったら0にするといった感じ。
                this.CurseDecrease(this.CurseValueNow.Value);
                CurseMaxedReset();
            }
        }
    }
    private void FixedUpdate()
    {
        if (!IsServer) return;
        CurseIncrease(0.01f);
    }

    public void CurseMaxed(float _CurseMaxEventWhileTime)
    {
        //サーバー側で実行される。
        //すでに呪いゲージがマックスの時のイベントが行われているなら無視
        if (CurseMaxedEventNow) return;

        this.CurseMaxEventWhileTime = _CurseMaxEventWhileTime;
        this.CurseMaxEventWhileTimeNow = 0;
        int i = UnityEngine.Random.Range(0, 2);
        // int i = 1;
        //ランダムで何かの関数を実行するとする
        CurseMaxedEventNow = true;
        if (i == 0)
        {
            //いったんてきのどうきをしてからここはおこなう。
            MikoInstantiate();

        }
        else if (i == 1)
        {
            ShitaitekiMove();
        }
        else if (i == 2)
        {
            //個々もあとで同期行う
            PlayerStaminaZero();
        }

    }
    public void CurseMaxedReset()
    {
        HorrorDirector horror_director = HorrorDirector.instance;
        sub_noticed_ui_controller.Notice("呪いの影響がすべて消滅した……");
        if (curse_event_type == CurseEventType.MikoInstantiate)
        {
            if (InstantiatedMiko != null)
            {
                if (horror_director.nearEnemyCount == 0 && horror_director.discoveredEnemyCount == 0)
                {
                    horror_director.ChangeHorrorState(HorrorState.None);
                }
                horror_director.MainBGMReset();
                horror_director.EnemyLostPlayer();
                horror_director.MainBGMAutoChange = true;

                Destroy(InstantiatedMiko, 0.1f);
                InstantiatedMiko = null;
            }
        }
        if (curse_event_type == CurseEventType.ShitaitekiMove)
        {
            ShitaitekiStop();
        }
        if (curse_event_type == CurseEventType.PlayerStamina)
        {
            this.PlayerStaminaZeroResetToAllPlayerClientRpc();
        }
        CurseMaxEventWhileTimeNow = 0;
        CurseMaxedEventNow = false;
    }
    private void MikoInstantiate()
    {

        //もうすでに作ってあるなら無視
        if (InstantiatedMiko != null) return;
        MikoInstantiatedClientRpc();
        curse_event_type = CurseEventType.MikoInstantiate;
        InstantiatedMiko = Instantiate(MikoPrefab, MikoApperancePlaceTrans.position, Quaternion.identity);
        InstantiatedMiko.GetComponent<NetworkObject>().Spawn();
        horror_director.MikoApperance();

    }
    private void PlayerStaminaZero()
    {
        //サーバー側で実行される。
        curse_event_type = CurseEventType.PlayerStamina;
        PlayerStaminaZeroToAllPlayerClientRpc();

    }
    [ClientRpc]
    private void PlayerStaminaZeroToAllPlayerClientRpc()
    {
        sub_noticed_ui_controller.Notice("呪いの力が溢れ出した……！");
        DOVirtual.DelayedCall(1, () =>
        {
            sub_noticed_ui_controller.Notice("しばらくの間スタミナが枯渇し、\n動きが鈍くなる……");
        });
        player_stamina_controller.StaminaEnableDisableRequestServerRpc(false);
    }
    [ClientRpc]
    private void PlayerStaminaZeroResetToAllPlayerClientRpc()
    {
        player_stamina_controller.StaminaEnableDisableRequestServerRpc(true);
    }

    [Button]
    public void CurseIncrease(float CurseRetakePower)
    {
        //CurseIncreaseはServerが側で実行
        if (CurseMaxValue >= CurseValueNow.Value + CurseRetakePower)
        {
            CurseValueNow.Value += CurseRetakePower;
        }
        else
        {
            CurseValueNow.Value = CurseMaxValue;
        }
        if (CurseMaxValue <= CurseValueNow.Value)
        {
            //呪いゲージがMAXを超えたなら
            CurseMaxed(CurseMaxEventWhileTime);
        }
    }
    [ServerRpc(RequireOwnership = false)]
    public void CurseDecreaseRequestFromOutsideRequestServerRpc(float _curseReducedPower)
    {
        //外部からCurseDecreaseを実行するならこれ！ クライアントからこの関数を呼ぶ
        if (this.CurseMaxedEventNow)
        {
            //呪い値がマックスならば、CurseMaxedEventをリセット！
            this.CurseMaxedReset();
        }
        CurseDecreaseFromOutsideForAllClientRpc();
        CurseDecrease(_curseReducedPower);
    }
    [ClientRpc]
    private void CurseDecreaseFromOutsideForAllClientRpc()
    {
        player_status.sub_noticed_ui_controller.Notice("呪い値が下がった");
    }
    [Button]
    private void CurseDecrease(float CurseReducedPower)
    {
        //Server側で実行
        if ((CurseValueNow.Value - CurseReducedPower) >= 0)
        {
            CurseValueNow.Value -= CurseReducedPower;
        }
        else
        {
            CurseValueNow.Value = 0;
        }
    }

    private void OnValueChangeCurseEvent(float old, float newValue)
    {
        CurseGageUI.DOValue(newValue / 100.0f, 0.3f).SetEase(Ease.Linear).SetLink(this.gameObject);
    }
    [ClientRpc]
    private void MikoInstantiatedClientRpc()
    {
        sub_noticed_ui_controller.Notice("呪いの力が溢れ出した……！");
        DOVirtual.DelayedCall(1, () =>
        {
            sub_noticed_ui_controller.Notice("闇に潜む巫女が目を覚ました……");
        });
    }

    private void ShitaitekiMove()
    {
        curse_event_type = CurseEventType.ShitaitekiMove;
        ShitaitekiMovingEvent.Invoke();
        ShitaitekiMoveClientRpc();
    }
    private void ShitaitekiStop()
    {
        ShitaitekiStoppingEvent.Invoke();
    }
    [ClientRpc]
    private void ShitaitekiMoveClientRpc()
    {
        sub_noticed_ui_controller.Notice("呪いの力が溢れ出した……！");
        DOVirtual.DelayedCall(1, () =>
        {
            sub_noticed_ui_controller.Notice("すべての腐り人が動き出した……");
        });
    }


}
