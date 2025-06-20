using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Sirenix.OdinInspector;
using DG.Tweening;
public class ItemEffectController : NetworkSingleton<ItemEffectController>
{
    [System.Serializable]
    private class SenkouItemUse
    {
        public float SenkouUsingTimeNow;
        public float SenkouUsingTime;
        public bool usingNow;
        public DiscoveryPlayerStatus discovery_player_status;
        public SoundSensorStatus sound_sensor_status;
    }
    [SerializeField]
    private SenkouItemUse senkou_item_use = new SenkouItemUse();
    private EnemyAllController enemy_all_controller;
    public ulong ItemUsePlayerNetworkId;
    private PlayerStatus localPlayerStatus;
    void OnEnable()
    {
        PlayerStatus.OnLocalPlayerSpawned += InitOnLocalPlayerSpawn;
        if (PlayerStatus.LocalInstance != null)
        {
            InitOnLocalPlayerSpawn(PlayerStatus.LocalInstance);
        }
    }

    void OnDisable()
    {
        PlayerStatus.OnLocalPlayerSpawned -= InitOnLocalPlayerSpawn;

    }

    private void InitOnLocalPlayerSpawn(PlayerStatus _localPlayerStatus)
    {
        // ローカルプレイヤーがスポーンしたときに、そのプレイヤーのコンポーネントを参照する
        // これにより、効果音などがローカルプレイヤーに対して正しく動作するようになる
        localPlayerStatus = _localPlayerStatus;
    }
    // Start is called before the first frame update
    public override void OnNetworkSpawn()
    {
        enemy_all_controller = EnemyAllController.instance;
        Debug.Log("ItemEffectControllerStart: " + enemy_all_controller);
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsServer) return;
        if (senkou_item_use.usingNow)
        {
            senkou_item_use.SenkouUsingTimeNow += Time.deltaTime;
            if (senkou_item_use.SenkouUsingTimeNow > senkou_item_use.SenkouUsingTime)
            {
                //線香の効果時間が終わりました
                //サーバーから実行されるねえ
                enemy_all_controller.AllEnemySoundSensorChange(null, true);
                enemy_all_controller.AllDiscoveryPlayerSoundSensorChange(null, true);
                var target = MakeTargetParams(lastUserClientId);
                this.SenkouEffectFinishedForUsedPlayerClientRpc(target);
                senkou_item_use.SenkouUsingTimeNow = 0;
            }
        }
    }
    [ServerRpc(RequireOwnership = false)]
    public void SenkouUseRequestServerRpc(float _time, ServerRpcParams rpcParams = default)
    {
        //サーバーから実行するよー
        ItemUsePlayerNetworkId = rpcParams.Receive.SenderClientId;   // ←ここ
        senkou_item_use.SenkouUsingTime = _time;
        senkou_item_use.usingNow = true;
        var target = MakeTargetParams(ItemUsePlayerNetworkId);

        SenkouEffectStartForUsedPlayerClientRpc(target);
        enemy_all_controller.AllEnemySoundSensorChange(senkou_item_use.sound_sensor_status, false);
        enemy_all_controller.AllDiscoveryPlayerSoundSensorChange(senkou_item_use.discovery_player_status, false);
        senkou_item_use.SenkouUsingTimeNow = 0;
    }
    [ClientRpc]
    private void SenkouEffectFinishedForUsedPlayerClientRpc(ClientRpcParams rpcParams = default)
    {
        //線香を利用した人だけに送るクライアントRpc 
        //線香の効果が切れたら実行される
        DOVirtual.DelayedCall(0.5f, () =>
        {
            localPlayerStatus.sub_noticed_ui_controller.Notice("線香の効果が切れた..");
        });
    }
    [ClientRpc]
    private void SenkouEffectStartForUsedPlayerClientRpc(ClientRpcParams rpcParams = default)
    {
        //線香を利用した人だけに送るクライアントRpc 
        //線香の効果が切れたら実行される
        DOVirtual.DelayedCall(0.5f, () =>
        {
            localPlayerStatus.sub_noticed_ui_controller.Notice("線香の効果が発動..");
        });
        DOVirtual.DelayedCall(1f, () =>
        {
            localPlayerStatus.sub_noticed_ui_controller.Notice("敵の索敵能力が弱体化した..!!");
        });

    }
    

    /* ---------- 共通ユーティリティ ---------- */
    private ulong lastUserClientId;   // ← セッション中に保持しておく

    private ClientRpcParams MakeTargetParams(ulong clientId)
    {
        // 毎回 new で OK。GC 気になるなら static 使い回しでも可
        return new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new[] { clientId }
            }
        };
    }
}
