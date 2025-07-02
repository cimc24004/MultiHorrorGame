using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.Animations.Rigging;
using DG.Tweening;
using static UnityEngine.Rendering.DebugUI;
using Unity.Netcode;
public class HeadLookatTargetController : NetworkBehaviour
{
    private enum Target
    {
        CameraRoot,
        Others
    }
    [SerializeField]
    private Target m_target;

    [ShowIf("m_target", Target.Others)]
    
    [SerializeField] private GameObject target_obj;
    [SerializeField] private Rig rig;
    [SerializeField] private MultiAimConstraint multi_aim_constraint;
    private RigBuilder rig_builder;

    private GameObject CameraRoot;
    public GameObject Player;
    private PlayerStatus player_status;
    [SerializeField]
    private CollideredForPlayers m_collidered_for_players;
    private PlayerAllController playerAllController;
    private DiscoveryPlayerController discoveryPlayerController;
    private float rig_weights;
    private bool m_NearNow = false;

    public bool LookAtTargetNow;
    private bool NearNow
    {
        set
        {
            if(this.m_NearNow != value)
            {
                //true falseに変わったときの処理
                if(value == true)
                {
                    //Trueに切り替わるときはWeightを徐々に増やす。
                    rig_weights = 0;
                    DOTween.To(() => rig_weights, x => rig_weights = x, 1f, 0.5f)
                    .SetEase(Ease.Linear) // イージングを変更可能（Ease.OutQuad, Ease.InOutSine など）
                    .OnUpdate(() => {
                        Debug.Log("OnUpdagte---------- m_NearNowwww");
                        rig.weight = rig_weights;
                    }).OnComplete(() => {
                        LookAtTargetNow = true;
                    });
                }
                else
                {
                    //Falseに切り替わるときはWeightを徐々に減らす。
                    Debug.Log("Falseに切り替わりました！！！");
                    rig_weights = 1;
                    DOTween.To(() => rig_weights, x => rig_weights = x, 0f, 0.5f)
                    .SetEase(Ease.Linear) // イージングを変更可能（Ease.OutQuad, Ease.InOutSine など）
                    .OnUpdate(() => {
                        rig.weight = rig_weights;
                        LookAtTargetNow = false;
                    });
                }


                this.m_NearNow = value;
            }

        }
        get
        {
            return this.m_NearNow;
        }
    }

    public override void OnNetworkSpawn(){
        if (!IsServer) return;
        rig_builder = this.gameObject.GetComponent<RigBuilder>();
        discoveryPlayerController = this.gameObject.GetComponent<DiscoveryPlayerController>();
        playerAllController = PlayerAllController.instance;
        // 1回だけ初期化
        WeightedTransformArray sources = new WeightedTransformArray();
        // 最初は空のソースをセットしておく
        multi_aim_constraint.data.sourceObjects = sources;
        rig_builder = GetComponent<RigBuilder>();
        rig_builder.Build();
        m_collidered_for_players.OnCollideredAddedObjectEvent.AddListener((collider_obj) =>
        {
            if (m_collidered_for_players.CollideredObjes.Count == 0)
            {
                Debug.Log("^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ m_collidered_for_players_check");
                Player = collider_obj;
                player_status = playerAllController.SpawnedPlayersWithPlayerStatuses[Player];
                CameraRoot = player_status.CameraRoot.gameObject;
                sources.Add(new WeightedTransform(CameraRoot.transform, 1f));
                multi_aim_constraint.data.sourceObjects = sources;
                rig_builder.Build();
            }


        });
        m_collidered_for_players.OnCollideredRemovedObjectEvent.AddListener((exit_collider_obj) =>
        {
            if (m_collidered_for_players.CollideredObjes.Count == 1)
            {
                Player = null;
                sources.Clear();
                multi_aim_constraint.data.sourceObjects = sources;
                rig_builder.Build();

            }
        });
    }
    // Update is called once per frame
    void Update()
    {
        if (!IsServer) return;
        if (m_collidered_for_players.CollideredObjes.Count != 0)
        {
            PlayerStatus collidered_playerStatus = playerAllController.SpawnedPlayersWithPlayerStatuses[m_collidered_for_players.CollideredObjes[0]];
            if (Physics.Raycast(this.transform.position, (collidered_playerStatus.HeadPos.transform.position - this.transform.position).normalized, out RaycastHit hit, Mathf.Infinity, discoveryPlayerController.original_discovery_player_status.layerMask))
            {
                if (hit.collider.gameObject.tag == "Player")
                {
                    NearNow = true;
                }
                else
                {
                    NearNow = false;
                }
            }
            else
            {
                NearNow = false;
            }
        }
        else
        {
            NearNow = false;
        }
    }

}
