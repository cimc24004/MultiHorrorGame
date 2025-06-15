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
        ItemLock,
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
    private ItemBelongingsController item_belongings_controller;
    public bool CurseMaxedEventNow;
    [SerializeField]
    //呪いゲージがマックスになった時イベントが起こり続ける時間
    private float CurseMaxEventWhileTime;
    [SerializeField]
    private float CurseMaxEventWhileTimeNow;
    [SerializeField]
    private CurseEventType curse_event_type;

    private ItemStatus locked_item_status;
    private HorrorDirector horror_director;

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
        item_belongings_controller = player_status.ItemController.GetComponent<ItemBelongingsController>();
        player_stamina_controller = player_status.gameObject.GetComponent<PlayerStaminaController>();
        sub_noticed_ui_controller = player_status.sub_noticed_ui_controller;
        this.CurseValueNow.OnValueChanged += OnValueChangeCurseEvent;
    }
    public override void OnNetworkSpawn()
    {
        horror_director = HorrorDirector.Instance;
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
        // int i = UnityEngine.Random.Range(0, 3);
        int i = 2;
        //ランダムで何かの関数を実行するとする
        CurseMaxedEventNow = true;
        if (i == 0)
        {
            //いったんてきのどうきをしてからここはおこなう。
            return;
            MikoInstantiate();

        }
        else if (i == 1)
        {
            ItemLock();
        }
        else if (i == 2)
        {
            //個々もあとで同期行う
            PlayerStaminaZero();
        }

    }
    public void CurseMaxedReset()
    {
        HorrorDirector horror_director = HorrorDirector.Instance;
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
        if (curse_event_type == CurseEventType.ItemLock)
        {

            //リセットされるときにしっかりとアイテムはロック解除しておく。
            ItemUnlockClientRpc();
            // item_belongings_controller.RequestCanUseItem(true, locked_item_status);
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
    private void ItemLock()
    {
        //サーバー側で実行される。

        //アイテムを使えなくする。
        ItemLockClientRpc();
        curse_event_type = CurseEventType.ItemLock;
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
    [Button]
    public void CurseDecrease(float CurseReducedPower)
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
    [ClientRpc]
    private void ItemLockClientRpc(ClientRpcParams rpcParams = default)
    {
        var ibc = player_status.ItemController
                    .GetComponent<ItemBelongingsController>();
        var targets = ibc.belonging_items.Keys
            .Where(x => x.item_type == ItemStatus.ItemType.Consumable
                     || x.item_type == ItemStatus.ItemType.CanThrow)
            .ToList();
        if (targets.Count == 0) return;

        var chosen = targets[UnityEngine.Random.Range(0, targets.Count)];
        ibc.RequestCanUseItem(false, chosen);
        sub_noticed_ui_controller.Notice("呪いの力が溢れ出した……！");
        DOVirtual.DelayedCall(1, () =>
        {
            sub_noticed_ui_controller.Notice($"{chosen.ItemName}は呪いによって\nしばらくの間使用不能になった……");
        });
        locked_item_status = chosen;
    }
    [ClientRpc]
    private void ItemUnlockClientRpc(ClientRpcParams rpcParams = default)
    {

        if (locked_item_status != null)
        {
            item_belongings_controller.RequestCanUseItem(true, locked_item_status);
            sub_noticed_ui_controller.Notice(
                $"「{locked_item_status.ItemName}」の封印が解けた……"
            );
        }
    }
    // [ClientRpc]
    // private void CurseMaxedOfItemLockClientRpc()
    // {
    //     //すべてのプレイヤーで実行される。 
    //     CurseMaxedRequestToAllPlayerServerRpc();


    // sub_noticed_ui_controller.Notice("呪いの力が溢れ出した……！");
    // DOVirtual.DelayedCall(1, () =>
    // {
    //     sub_noticed_ui_controller.Notice($"{locked_item_status.ItemName}は呪いによって\nしばらくの間使用不能になった……");
    // });



    // }
    // [ServerRpc]
    // private void CurseMaxedRequestToAllPlayerServerRpc()
    // {
    //     //すべてのプレイヤーをサーバーがチェックする。

    //     if (!item_belongings_controller.ExistsItems()) return;
    //     List<ItemStatus> target_item_statues = item_belongings_controller.belonging_items.Keys.Where(x => x.item_type == ItemStatus.ItemType.Consumable || x.item_type == ItemStatus.ItemType.CanThrow).ToList();
    //     if (target_item_statues.Count == 0)
    //     {
    //         //もし該当するアイテムなかったらもう一回。
    //         CurseMaxed(CurseMaxEventWhileTime);
    //         return;
    //     }
    //     int item_lock_random = UnityEngine.Random.Range(0, target_item_statues.Count);

    //     ItemStatus taget_item_status = target_item_statues.ElementAt(item_lock_random);
    //     locked_item_status = taget_item_status;

    //     item_belongings_controller.RequestCanUseItem(false, locked_item_status);
    // }
}
