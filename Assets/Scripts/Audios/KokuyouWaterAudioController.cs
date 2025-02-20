using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class KokuyouWaterAudioController : MonoBehaviour
{
    private AudioSource audio_source;
    [SerializeField]
    private EnemyStatus enemy_status;
    [SerializeField]
    private AudioClip IdleWaterClip;
    [SerializeField]
    private AudioClip MoveWaterClip;

    private EnemyStatus.EnemyMoveState before_enemy_move_state;
    private float DefaultVolume;
    // Start is called before the first frame update
    void Start()
    {
        audio_source = this.gameObject.GetComponent<AudioSource>();
        audio_source.Play();
        enemy_status.OnChangeEnemyMoveEvent.AddListener(OnChangeMoveState);
        DefaultVolume = audio_source.volume;
    }

    // Update is called once per frame
    void Update()
    {
        if(before_enemy_move_state != enemy_status.enemy_move_state)
        {

        }
    }
    private void OnChangeMoveState()
    {
        Debug.Log("OnChangeMoveState----> " + enemy_status.enemy_move_state.ToString());
        if (enemy_status.enemy_move_state == EnemyStatus.EnemyMoveState.Walk || enemy_status.enemy_move_state == EnemyStatus.EnemyMoveState.Run)
        {
            audio_source.clip = MoveWaterClip;
        }
        else if (enemy_status.enemy_move_state == EnemyStatus.EnemyMoveState.Stop)
        {
            audio_source.clip = IdleWaterClip;
        }
        audio_source.Play();
        audio_source.volume = 0;
        audio_source.DOFade(DefaultVolume, 0.1f).SetLink(this.gameObject).SetEase(Ease.Linear).OnComplete(() => {
        
        });
    }
}
