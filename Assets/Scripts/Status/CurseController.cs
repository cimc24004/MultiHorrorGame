using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Linq;
using DG.Tweening;
using UnityEngine.Events;
using UnityEngine.UI;
public class CurseController : Singleton<CurseController>
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
    public float CurseValueNow = 0;
    [SerializeField]
    [Header("呪いゲージのMAX値")]
    private float CurseMaxValue = 100;

    [SerializeField] private Slider CurseGageUI;

    [SerializeField] public UnityEvent2 OnValueChangeCurseEvent;
    [SerializeField] public UnityEvent2 OnCurseGageMaxEvent;

    private GameObject InstantiatedMiko;
    private UIFadeController ui_fade_controller;
    private PlayerStatus player_status;
    private PlayerStaminaController player_stamina_controller;
    private ItemBelongingsController item_belongings_controller;
    private ItemMenuController item_menu_controller;
    private bool CurseMaxedEventNow;
    [SerializeField]
    //呪いゲージがマックスになった時イベントが起こり続ける時間
    private float CurseMaxEventWhileTime;
    [SerializeField]
    private float CurseMaxEventWhileTimeNow;
    [SerializeField]
    private CurseEventType curse_event_type;

    private ItemStatus locked_item_status;
    private HorrorDirector horror_director;

    [SerializeField] private AudioClip AppearanceMikoBGM;
    private void Start()
    {
        player_status = PlayerStatus.instance;
        item_belongings_controller = player_status.ItemController.GetComponent<ItemBelongingsController>();
        item_menu_controller = item_belongings_controller.item_menu_controller;
        player_stamina_controller = player_status.gameObject.GetComponent<PlayerStaminaController>();
        ui_fade_controller = CurseGageUI.GetComponent<UIFadeController>();
        horror_director = HorrorDirector.instance;
        this.OnValueChangeCurseEvent.AddListener(() =>
        {
            ui_fade_controller.AutoFadeIn();
        });
        this.OnCurseGageMaxEvent.AddListener(() =>
        {
            CurseMaxed(CurseMaxEventWhileTime);
        });

    }
    private void Update()
    {
        CurseGageUI.DOValue(this.CurseValueNow / 100.0f, 0.3f).SetEase(Ease.Linear).SetLink(this.gameObject);
        if (CurseMaxedEventNow)
        {
            CurseMaxEventWhileTimeNow += Time.deltaTime;
            

            if(curse_event_type == CurseEventType.PlayerStamina)
            {
                PlayerStaminaZeroUpdate();
            }

            if(CurseMaxEventWhileTimeNow > CurseMaxEventWhileTime)
            {
                CurseMaxedReset();
            }
        }
    }
    private void FixedUpdate()
    {
        CurseIncrease(0.01f);
    }

    public void CurseMaxed(float _CurseMaxEventWhileTime)
    {
        //すでに呪いゲージがマックスの時のイベントが行われているなら無視
        if (CurseMaxedEventNow) return;

        this.CurseMaxEventWhileTime = _CurseMaxEventWhileTime;

        this.CurseMaxEventWhileTimeNow = 0;
        int i = Random.Range(0, 3);
        if(i == 0)
        {
            MikoInstantiate();

        }
        else if(i == 1)
        {
            ItemLock();
        }
        else if(i == 2)
        {
            PlayerStaminaZero();
        }
        //ランダムで何かの関数を実行するとする
        CurseMaxedEventNow = true;
    }
    private void CurseMaxedReset()
    {
        if(curse_event_type == CurseEventType.MikoInstantiate)
        {
            if (InstantiatedMiko != null)
            {
                if(horror_director.nearEnemyCount == 0 && horror_director.discoveredEnemyCount == 0){
                    horror_director.EnemyLostPlayer();
                    horror_director.ChangeHorrorState(HorrorState.None);
                }
                horror_director.MainBGMAutoChange = true;
                Destroy(InstantiatedMiko, 0.1f);
            }
        }
        if (curse_event_type == CurseEventType.ItemLock)
        {

            //リセットされるときにしっかりとアイテムはロック解除しておく。
            item_belongings_controller.CanUseItem(true, locked_item_status);
        }
        
        this.CurseDecrease(this.CurseValueNow);
        CurseMaxEventWhileTimeNow = 0;
        CurseMaxedEventNow = false;
    }
    private void MikoInstantiate()
    {

        //もうすでに作ってあるなら無視
        if (InstantiatedMiko != null) return;
        
        curse_event_type = CurseEventType.MikoInstantiate;
        InstantiatedMiko = Instantiate(MikoPrefab, MikoApperancePlaceTrans.position, Quaternion.identity);
        horror_director.MikoApperanceSound.Play();
        horror_director.MainBGM.clip = AppearanceMikoBGM;
        horror_director.MainBGMAutoChange = false;
    }
    private void ItemLock()
    {
        if (!item_belongings_controller.ExistsItems())
        {
            return;
        }
        int item_lock_random = Random.Range(0, item_belongings_controller.belonging_items.Count);
        KeyValuePair<ItemStatus, ItemBelongingsController.ItemCountWithPermit> item_value_pair = item_belongings_controller.belonging_items.ElementAt(item_lock_random);
        locked_item_status = item_value_pair.Key;
        //アイテムを使えなくする。
        item_belongings_controller.CanUseItem(false, locked_item_status);
        curse_event_type = CurseEventType.ItemLock;
    }
    private void PlayerStaminaZero()
    {
        curse_event_type = CurseEventType.PlayerStamina;
    }

    private void PlayerStaminaZeroUpdate()
    {
        player_status.StaminaValueNow = 0;
        player_stamina_controller.ConsumeStamina();
    }

    [Button]
    public void CurseIncrease(float CurseRetakePower)
    {
        OnValueChangeCurseEvent.Invoke();
        if (CurseMaxValue >= CurseValueNow + CurseRetakePower)
        {
            CurseValueNow += CurseRetakePower;
        }
        else
        {
            CurseValueNow = CurseMaxValue;
        }
        if (CurseMaxValue <= CurseValueNow)
        {
            //呪いゲージがMAXを超えたなら
            OnCurseGageMaxEvent.Invoke();
        }
    }
    [Button]
    public void CurseDecrease(float CurseReducedPower)
    {
        if ((CurseValueNow - CurseReducedPower) >= 0)
        {
            OnValueChangeCurseEvent.Invoke();

            CurseValueNow -= CurseReducedPower;
        }
        else
        {
            CurseValueNow = 0;
        }
    }
}
