using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Cinemachine;
using Sirenix.OdinInspector;
public class HorrorDirector : Singleton<HorrorDirector>
{
    public GameObject Main;
    public AudioSource MainBGM;

    public GameObject ToEnemyNear;
    public AudioSource ToEnemyNearAudioSource;

    public GameObject Discoveried;
    public GameObject DiscoveriedSound;
    public GameObject DiscoveryVolume;


    private AudioClip MainDefaultBGM;
    private CinemachineImpulseSource cinemachine_impulse_source;

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
        Main.SetActive(false);
        ToEnemyNear.SetActive(true);
    }

    public void NearEnemyReset()
    {
        ToEnemyNearAudioSource.DOFade(0, 1).SetEase(Ease.Linear).SetLink(this.gameObject).OnComplete(() =>
        {
            ToEnemyNearAudioSource.volume = 1;
            ResetSetting();
        });
    }
    [Button]
    public void DiscoveriedHorror()
    {
        ToEnemyNear.SetActive(false);
        DiscoveriedSound.SetActive(false);
        DiscoveriedSound.SetActive(true);
        Main.SetActive(false);
        DiscoveryVolume.SetActive(true);

        DiscoveryNow = true;
        Discoveried.SetActive(true);
        DOVirtual.DelayedCall(1, () => {
            DiscoveryVolume.SetActive(false);
        });
        //cinemachine_impulse_source.GenerateImpulse();
    }
    public void ResetSetting()
    {
        StartCoroutine(check());


    }
    private IEnumerator check()
    {
        yield return null;
        if (!DiscoveryNow) {
            ToEnemyNear.SetActive(false);
            Discoveried.SetActive(false);
            Main.SetActive(true);
            MainBGM.clip = MainDefaultBGM;
            MainBGM.volume = 1;
            MainBGM.Play();
        }

    } 
}
