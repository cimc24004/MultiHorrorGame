using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OshiireMouzyaController : MonoBehaviour
{
    [SerializeField]
    private Collidered OshiireAttackRange;
    [SerializeField]
    private DoorAttackedController door_attacked_controller;
    [SerializeField]
    private AudioSource OshiireAttackSource;
    [SerializeField]
    private AudioClip OshiireAttackAudioClip;
    [SerializeField]
    private AudioClip OshiireBrokeAudioClip;

    [SerializeField]
    private float OshiireAttackWaitTime = 1;
    [SerializeField]
    private float OshiireAttackWaitTimeNow = 0;


    private DiscoveryPlayerController discovery_player_controller;
    private bool OshiireAttackMode;
    private Animator m_Animator;
    private bool BrokenOshiire;
    private EnemyStatus enemy_status;
    // Start is called before the first frame update
    void Start()
    {
        discovery_player_controller = this.gameObject.GetComponent<DiscoveryPlayerController>();
        m_Animator = this.gameObject.GetComponent<Animator>();
        enemy_status = this.gameObject.GetComponent<EnemyStatus>();
        foreach(Transform door in door_attacked_controller.gameObject.transform)
        {
            Debug.Log("Door:r:r::" + door.gameObject.name);
            if(door.tag == "HideActionObject")
            {
                door.tag = "Untagged";
            }
        }

    }

    // Update is called once per frame
    void Update()
    {
        if (OshiireAttackRange.Collider)
        {
            discovery_player_controller.DiscoveryPlayerNow = true;
            OshiireAttackMode = true;
            
        }
        else
        {
        }

        if (OshiireAttackMode)
        {
            if (!BrokenOshiire)
            {
                enemy_status.enemy_move_state = EnemyStatus.EnemyMoveState.Stop;
                OshiireAttackWaitTimeNow += Time.deltaTime;
                if(OshiireAttackWaitTimeNow > OshiireAttackWaitTime)
                {
                    AttackToOshiire(1);
                    OshiireAttackWaitTimeNow = 0;
                }
            }
        }
    }

    public void AttackToOshiire(int AttackDoorPlusCount)
    {
        if (BrokenOshiire) return;
        bool broken = door_attacked_controller.DoorAttacked(AttackDoorPlusCount);
        if (broken)
        {
            BrokenOshiire = true;
            OshiireAttackMode = false;
            OshiireAttackSource.PlayOneShot(OshiireBrokeAudioClip);
            OshiireAttackSource.PlayOneShot(OshiireAttackAudioClip);
        }
        else
        {
            BrokenOshiire = false;
            OshiireAttackSource.PlayOneShot(OshiireAttackAudioClip);
        }
    }
}
