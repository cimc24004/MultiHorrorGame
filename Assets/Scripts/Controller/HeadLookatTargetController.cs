using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.Animations.Rigging;
using DG.Tweening;
using static UnityEngine.Rendering.DebugUI;
using Unity.Netcode;
using UnityEngine.Events;
using System.Linq;
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
    public List<GameObject> lookPlayers;
    public GameObject LastPlayerKey;
    private PlayerStatus player_status;
    [SerializeField]
    private CollideredForPlayers m_collidered_for_players;
    private PlayerAllController playerAllController;
    private DiscoveryPlayerController discoveryPlayerController;
    private float rig_weights;
    private bool m_NearNow = false;
    private EnemyStatus enemyStatus;
    public UnityEvent AddTargetHeadLookPlayerUnityEvent;
    public bool LookAtTargetNow;
    private bool NearNow
    {
        set
        {
            if (this.m_NearNow != value && IsServer)
            {
                //true falseに変わったときの処理
                RigWeightChangeClientRpc(value);

                this.m_NearNow = value;
            }

        }
        get
        {
            return this.m_NearNow;
        }
    }

    public override void OnNetworkSpawn()
    {
        // 1回だけ初期化
        // 最初は空のソースをセットしておく
        rig_builder = this.gameObject.GetComponent<RigBuilder>();
        WeightedTransformArray sources = new WeightedTransformArray();
        multi_aim_constraint.data.sourceObjects = sources;
        rig_builder.Build();

        if (!IsServer) return;
        enemyStatus = this.gameObject.GetComponent<EnemyStatus>();
        discoveryPlayerController = this.gameObject.GetComponent<DiscoveryPlayerController>();
        playerAllController = PlayerAllController.instance;
        m_collidered_for_players.OnCollideredAddedObjectEvent.AddListener(AddTarget);
        m_collidered_for_players.OnCollideredRemovedObjectEvent.AddListener(RemoveTarget);

    }
    // Update is called once per frame
    void Update()
    {
        if (!IsServer) return;
        if (m_collidered_for_players.CollideredObjes.Count != 0)
        {
            PlayerStatus collidered_playerStatus = playerAllController.SpawnedPlayersWithPlayerStatuses[m_collidered_for_players.CollideredObjes[0]];
            if (Physics.Raycast(enemyStatus.EyePosition.transform.position, (collidered_playerStatus.CrouchHeadPos.transform.position - enemyStatus.EyePosition.transform.position).normalized, out RaycastHit hit, Mathf.Infinity, discoveryPlayerController.original_discovery_player_status.layerMask))
            {
                Debug.Log("Hitしているオブジェクト: " + hit.collider.gameObject);
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

    [ClientRpc]
    private void RigWeightChangeClientRpc(bool _NearNow)
    {
        if (_NearNow)
        {
            //Trueに切り替わるときはWeightを徐々に増やす。
            rig_weights = 0;
            DOTween.To(() => rig_weights, x => rig_weights = x, 1f, 0.5f)
            .SetEase(Ease.Linear) // イージングを変更可能（Ease.OutQuad, Ease.InOutSine など）
            .OnUpdate(() =>
            {
                Debug.Log("OnUpdagte---------- m_NearNowwww");
                rig.weight = rig_weights;
            }).OnComplete(() =>
            {
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
            .OnUpdate(() =>
            {
                rig.weight = rig_weights;
                LookAtTargetNow = false;
            });
        }
    }
    void AddTarget(GameObject collider_obj)
    {
        lookPlayers.Add(collider_obj);
        this.LastPlayerKey = collider_obj;
        this.AddTargetHeadLookPlayerUnityEvent.Invoke();
        if (m_collidered_for_players.CollideredObjes.Count != 0) return;
        ulong targetId = collider_obj.GetComponent<NetworkObject>().NetworkObjectId;
        SetLookTargetClientRpc(targetId);   // 全クライアントに通知
    }

    public void RemoveTarget(GameObject collider_obj)
    {
        if (m_collidered_for_players.CollideredObjes.Count != 1) return;
        this.lookPlayers.Remove(collider_obj);
        if (this.lookPlayers.Count != 0)
        {
            this.LastPlayerKey = this.lookPlayers.Last();
        }
        ClearLookTargetClientRpc();
    }
    [ClientRpc]
    void SetLookTargetClientRpc(ulong playerNetId)
    {
        if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(playerNetId, out var obj)) return;

        var camRoot = obj.GetComponent<PlayerStatus>().CameraRoot.transform;

        var src = new WeightedTransformArray();
        src.Add(new WeightedTransform(camRoot, 1f));
        multi_aim_constraint.data.sourceObjects = src;
        rig_builder.Build();              // ← クライアント側でも Build
    }

    [ClientRpc]
    void ClearLookTargetClientRpc()
    {
        multi_aim_constraint.data.sourceObjects = new WeightedTransformArray();
        rig_builder.Build();
    }
}
