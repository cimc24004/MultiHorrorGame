using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

public class SilhouetteChangeController : SerializedMonoBehaviour
{
    public bool SilhouetteNow;
    [Header("ある程度の範囲に入ったらシルエット表示を終了するかどうか")]
    [SerializeField] private bool SilhouetteAutoHideInRange;
    [ShowIf("SilhouetteAutoHideInRange")]
    [SerializeField] private Collidered CollideredForSilhouetteAutoHide;
    [SerializeField]
    private List<GameObject> TargetChangeSilhouetteObjs;
    [Header("シルエット表示を辞めて、初期化するときのレイヤーネーム")]
    [SerializeField]
    private string ResetLayerName = "Default";
    [Header("時間で自動的にシルエット表示をリセットに切り替えるかどうか")]

    [SerializeField]
    private float ToResetSilhouetteTime;
    [SerializeField]
    private float ToResetSilhouetteTimeNow;
    private bool SilhouetteAutoResetStart;
    void Start()
    {
        if(SilhouetteAutoHideInRange){
            CollideredForSilhouetteAutoHide.OnTriggerEnterEvent.AddListener(() => {
                if(SilhouetteNow){
                    SilhouetteChange(false, false);
                }
            });
            CollideredForSilhouetteAutoHide.OnTriggerExitEvent.AddListener(() => {
                if(SilhouetteNow){
                    SilhouetteChange(true, false);
                }
            });
        }

    }
    void Update()
    {
        if(SilhouetteAutoResetStart){
            ToResetSilhouetteTimeNow += Time.deltaTime;
            if(ToResetSilhouetteTime < ToResetSilhouetteTimeNow){
                SilhouetteChange(false);
                SilhouetteAutoResetStart = false;
                ToResetSilhouetteTimeNow = 0;
            }
        }


    }
    public void SilhouetteChange(bool _silhouette, bool AutoSilhouetteModeChange = true){
        if(AutoSilhouetteModeChange){
            SilhouetteNow = _silhouette;
        }
        if(_silhouette){
            foreach(GameObject target in TargetChangeSilhouetteObjs ){
                target.gameObject.layer = LayerMask.NameToLayer("NoSilhouette");
            }
        }else{
            foreach(GameObject target in TargetChangeSilhouetteObjs ){
                target.gameObject.layer = LayerMask.NameToLayer(ResetLayerName);
            }
        }
    }

    public void SilhouetteChangeWithAutoReset(float _time){
        SilhouetteChange(true);
        ToResetSilhouetteTimeNow = 0;
        ToResetSilhouetteTime = _time;
        SilhouetteAutoResetStart = true;
    }
}
