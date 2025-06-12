using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Sirenix.OdinInspector;
using Unity.Netcode;
public class ObjectBuffController : NetworkBehaviour
{
    //オブジェクトを破壊->バフをつけるコントローラー
    [SerializeField] private GameObject PartedObj;
    [SerializeField] private GameObject JoinedObj;
    [SerializeField] private float BrokenBlendDuration;
    private PrayController pray_controller;
    private SoundObjectEventWithEnemySensor sound_object_event_with_enemy_sensor;
    [SerializeField] private GameObject SoundObjectSoundArea;
    private SkinnedMeshRenderer PartedSkinnedMeshRenderer;
    public enum BuffType{
        Speed,
        Otonashi,
        ItemDrop
    }
    [SerializeField]
    public BuffType buff_effect_type;
    [ShowIf("buff_effect_type", BuffType.ItemDrop)]
    [SerializeField]
    private GameObject InstantiateItemPrefabOnDestroy;

    private PlayerStatus player_status;
    private PlayerMoveScript player_move_script;
    private FootStepSoundController foot_step_sound_controller;
    private SubNoticedUIController sub_noticed_ui_controller;
    private Tween DestroyAnimationTween;
    private NetworkObject m_NetworkObject;
    // Start is called before the first frame update
    // void Start()
    // {
    //     // pray_controller = this.gameObject.GetComponent<PrayController>();
    //     // pray_controller.PrayCompletedEvent.AddListener(ObjectPartDestroy);
    //     // PartedSkinnedMeshRenderer = PartedObj.GetComponent<SkinnedMeshRenderer>();
    //     // player_status = PlayerStatus.LocalInstance;
    //     // player_move_script = player_status.gameObject.GetComponent<PlayerMoveScript>();
    //     // foot_step_sound_controller = player_status.foot_step_sound_controller;
    //     // sub_noticed_ui_controller = player_status.sub_noticed_ui_controller;
    //     // sound_object_event_with_enemy_sensor = this.gameObject.GetComponent<SoundObjectEventWithEnemySensor>();
    // }

    void OnEnable()
    {
        PlayerStatus.OnLocalPlayerSpawned += InitOnPlayerSpawn;
    }
    void OnDisable()
    {
        PlayerStatus.OnLocalPlayerSpawned -= InitOnPlayerSpawn;
    }
    private void InitOnPlayerSpawn(PlayerStatus _playerStatus){
        player_status = _playerStatus;
        player_move_script = player_status.gameObject.GetComponent<PlayerMoveScript>();
        foot_step_sound_controller = player_status.foot_step_sound_controller;
        sub_noticed_ui_controller = player_status.sub_noticed_ui_controller;
    }
    public override void OnNetworkSpawn()
    {
        pray_controller = this.gameObject.GetComponent<PrayController>();
        pray_controller.PrayCompletedEvent.AddListener(ObjectPartDestroyForClients);
        pray_controller.PrayCompletedEventForLocal.AddListener(ObjectPartDestroyForLocal);
        PartedSkinnedMeshRenderer = PartedObj.GetComponent<SkinnedMeshRenderer>();
        m_NetworkObject = this.gameObject.GetComponent<NetworkObject>();
        sound_object_event_with_enemy_sensor = this.gameObject.GetComponent<SoundObjectEventWithEnemySensor>();
    }

    // Update is called once per frame
    // void Update()
    // {

    // }

    //バラバラにして破壊する 全クライアントに共有
    public void ObjectPartDestroyForClients()
    {
        PartedObj.SetActive(true);
        JoinedObj.SetActive(false);
        SoundObjectSoundArea.gameObject.SetActive(true);
        sound_object_event_with_enemy_sensor.SensorSoundPlay();
        DestroyAnimationTween = DOTween.To(
            () => PartedSkinnedMeshRenderer.GetBlendShapeWeight(0),
            x => PartedSkinnedMeshRenderer.SetBlendShapeWeight(0, x),
            130f,
            BrokenBlendDuration
        ).SetEase(Ease.Linear);
        DestroyAnimationTween.OnComplete(() =>
        {
            Destroy(this.gameObject, 1.1f);
        });
    }
    //ローカルのみで実行する。
    public void ObjectPartDestroyForLocal()
    {
        DestroyAnimationTween.OnComplete(() =>
        {
            OnBrokenCompleted();
        });   
    }
    [Button]
    public void OnBrokenCompleted(){
        if(buff_effect_type == BuffType.Speed){
            sub_noticed_ui_controller.Notice("脚力が上がった。");
            player_move_script.MoveSpeedUpRequestServerRpc(1, 60);
        }else if(buff_effect_type == BuffType.ItemDrop){

            Instantiate(InstantiateItemPrefabOnDestroy, this.transform.position, Quaternion.identity);
        }else if(buff_effect_type == BuffType.Otonashi){
            sub_noticed_ui_controller.Notice("自分の足音が消えた");
            foot_step_sound_controller.FootStepCant(60);
        }
    }
}
