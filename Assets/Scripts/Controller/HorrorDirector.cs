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
    public AudioSource ToEnemyNearBGMAudioSource;
    public AudioSource ToEnemyNearHeartBeatSoundSource;

    public GameObject Discoveried;
    public GameObject DiscoveriedSound;
    public GameObject DiscoveryVolume;


    private AudioClip MainDefaultBGM;
    private CinemachineImpulseSource cinemachine_impulse_source;
    private GameControllerOnAllScene game_controller_on_all_scene;
    private URPFxController urp_fx_controller;

    public bool DiscoveryNow;
    // Start is called before the first frame update
    void Start()
    {
        cinemachine_impulse_source = this.gameObject.GetComponent<CinemachineImpulseSource>();
        MainDefaultBGM = MainBGM.clip;
        game_controller_on_all_scene = GameControllerOnAllScene.instance;
        urp_fx_controller = game_controller_on_all_scene.gameObject.GetComponent<URPFxController>();
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
        urp_fx_controller.CorruptMemoryChange(true);
    }

    public void NearEnemyReset()
    {
        ToEnemyNearBGMAudioSource.DOFade(0, 1).SetEase(Ease.Linear).SetLink(this.gameObject).OnComplete(() =>
        {
            ToEnemyNearBGMAudioSource.volume = 1;
            ResetSetting();
        });
        ToEnemyNearHeartBeatSoundSource.DOFade(0, 1).SetEase(Ease.Linear).SetLink(this.gameObject).OnComplete(() =>
        {
            ToEnemyNearHeartBeatSoundSource.volume = 1;
        });
        urp_fx_controller.CorruptMemoryChange(false);
    }
    [Button]
    public void DiscoveriedHorror()
    {
        ToEnemyNear.SetActive(false);
        DiscoveriedSound.SetActive(false);
        DiscoveriedSound.SetActive(true);
        urp_fx_controller.CorruptMemoryChange(false);

        urp_fx_controller.InteferecesChange(true);

        Main.SetActive(false);
        DiscoveryVolume.SetActive(true);

        DiscoveryNow = true;
        Discoveried.SetActive(true);
        DOVirtual.DelayedCall(1, () => {
            DiscoveryVolume.SetActive(false);
            urp_fx_controller.InteferecesChange(false);
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
            urp_fx_controller.CorruptMemoryChange(false);
            urp_fx_controller.InteferecesChange(false);
            Main.SetActive(true);
            MainBGM.clip = MainDefaultBGM;
            MainBGM.volume = 1;
            MainBGM.Play();
        }

    } 
}
