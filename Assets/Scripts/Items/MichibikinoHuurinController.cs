using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Linq; 
using Sirenix.OdinInspector;
public class MichibikinoHuurinController : SerializedMonoBehaviour
{
    private AudioSource audio_source;
    //範囲内にある導きオブジェクトたち
    [SerializeField] public Dictionary<GameObject, float> MichibikiObjectWithDistanceIntoRange;
    private Animator m_Animator;
    // Start is called before the first frame update
    void Start()
    {
        m_Animator = this.gameObject.GetComponent<Animator>();
        audio_source = this.gameObject.GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log("m_Animator.speed = " + m_Animator.speed);
        if(MichibikiObjectWithDistanceIntoRange.Count > 0){
            Michibiki();
            audio_source.enabled = true;
        }else{
            m_Animator.speed = 0;
            audio_source.enabled = false;
        }
    }
    private void Michibiki(){
        // 一番近いオブジェクトを探す
        var minPair = MichibikiObjectWithDistanceIntoRange
            .OrderBy(pair => pair.Value)
            .First();

        float distance = minPair.Value;

        // 距離が大きいほどスピードが小さくなる（0に近づく）逆数ベース
        float speed = 1f / (1f + distance);

        // 一応0〜1以内に収めておく
        speed = Mathf.Clamp01(speed);

        m_Animator.speed = speed;

        
    }


}
