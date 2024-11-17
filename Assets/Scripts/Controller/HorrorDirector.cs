using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Cinemachine;
using Sirenix.OdinInspector;
public class HorrorDirector : Singleton<HorrorDirector>
{
    public AudioSource MainBGM;
    public AudioSource DiscoveriedSound;
    private CinemachineImpulseSource cinemachine_impulse_source;
    public AudioClip NearEnemyBGM;
    public AudioClip DiscoveriedBGM;
    public AudioClip MainDefaultBGM;

    public GameObject DiscoveryVolume;

    public bool DiscoveryNow;
    // Start is called before the first frame update
    void Start()
    {
        cinemachine_impulse_source = this.gameObject.GetComponent<CinemachineImpulseSource>();
        MainDefaultBGM = MainBGM.clip;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void NearEnemy()
    {
        if (DiscoveryNow) return;
        MainBGM.clip = NearEnemyBGM;
        MainBGM.Play();
    }
    [Button]
    public void DiscoveriedHorror()
    {
        MainBGM.clip = DiscoveriedBGM;
        MainBGM.Play();
        DiscoveryNow = true;
        DiscoveriedSound.Play();

        DiscoveryVolume.SetActive(true);
        DOVirtual.DelayedCall(1, () => {
            DiscoveryVolume.SetActive(false);
        });
        //cinemachine_impulse_source.GenerateImpulse();
    }
    public void ResetSetting()
    {
        MainBGM.clip = MainDefaultBGM;
        MainBGM.Play();
        DiscoveriedSound.DOFade(1,0).OnComplete(() => { 
            DiscoveriedSound.volume = 1;
        });


    }

}
