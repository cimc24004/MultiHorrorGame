using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.Events;
using UnityEngine.UI;
public class PrayController : MonoBehaviour
{
    [Header("•K—v‚È‹F‚èŽžŠÔ")]
    public float NeedToPrayTime;
    public bool PrayNow;
    public bool PrayPermit = true;

    public float PrayTimeNow;

    public UnityEvent PrayCompletedEvent;
    public Image PrayGageImage;
    public GameObject PrayGageUI;
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        PrayGageUI.SetActive(false);
        if (PrayNow && PrayPermit)
        {
            PrayGageUI.SetActive(true);
            PrayGageImage.fillAmount =  PrayTimeNow / NeedToPrayTime;
            PrayTimeNow += Time.deltaTime;
            if (PrayTimeNow > NeedToPrayTime)
            {
                Debug.Log("CompletedPray!!!");
                PrayCompleted();
            }
            PrayNow = false;
        }
    }
    public void Pray()
    {
        Debug.Log("Prayyyyyyyyyyyyyyyyyyyyyy");

        PrayNow = true;
    }
    public void PrayStop()
    {
        PrayPermit = false;
        PrayNow = false;
        PrayTimeNow = 0;
    }
    private void PrayCompleted()
    {
        PrayCompletedEvent.Invoke();
        this.gameObject.tag = "Untagged";
        PrayNow = false;
        PrayTimeNow = 0;
    }
}
