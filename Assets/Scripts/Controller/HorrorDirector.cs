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
    public GameObject NearEnemyVolume;

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
        NearEnemyVolume.SetActive(true);
    }

    public void NearEnemyReset()
    {
        MainBGM.DOFade(1, 0).SetEase(Ease.Linear).SetLink(this.gameObject).OnComplete(() =>
        {

            NearEnemyVolume.SetActive(false);
            ResetSetting();
        });
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
        MainBGM.volume = 1;
        MainBGM.Play();
        DiscoveriedSound.DOFade(1,0).OnComplete(() => { 
            DiscoveriedSound.volume = 1;
        });


    }

}
