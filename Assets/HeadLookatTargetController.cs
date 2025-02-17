using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.Animations.Rigging;
using DG.Tweening;
using static UnityEngine.Rendering.DebugUI;
public class HeadLookatTargetController : SerializedMonoBehaviour
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

    private GameObject CameraRoot;
    private GameObject Player;
    private PlayerStatus player_status;
    [SerializeField]
    private float DistanceWithSeeingTarget;
    [SerializeField]
    private float DistanceToTarget;

    private float rig_weights;
    private bool m_NearNow = false;
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
                        rig.weight = rig_weights;
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
    // Start is called before the first frame update
    void Start()
    {
        player_status = PlayerStatus.instance;  
        Player = player_status.gameObject;
        CameraRoot = player_status.CameraRoot.gameObject;

    }

    // Update is called once per frame
    void Update()
    {
        DistanceToTarget = Vector3.Distance(this.transform.position, TargetCheck().transform.position);
        if(DistanceToTarget <= DistanceWithSeeingTarget)
        {
            NearNow = true;
        }
        else
        {
            NearNow = false;
        }
    }

    private GameObject TargetCheck()
    {
        if(m_target == Target.CameraRoot)
        {
            return CameraRoot;
        }
        else
        {
            return target_obj;
        }
    }
}
