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
    [SerializeField] private MultiAimConstraint multi_aim_constraint;
    private RigBuilder rig_builder;

    private GameObject CameraRoot;
    private GameObject Player;
    private PlayerStatus player_status;
    [SerializeField]
    private Collidered m_collidered;

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
    // Start is called before the first frame update
    void Start()
    {
        player_status = PlayerStatus.LocalInstance;  
        Player = player_status.gameObject;
        CameraRoot = player_status.CameraRoot.gameObject;
        rig_builder = this.gameObject.GetComponent<RigBuilder>();
        WeightedTransformArray sources = new WeightedTransformArray();
        sources.Add(new WeightedTransform(CameraRoot.transform, 1f));
        multi_aim_constraint.data.sourceObjects = sources;

        rig_builder.Build();
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log("m_Collidered.collider: " + m_collidered.Collider);
        if (m_collidered.Collider)
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
