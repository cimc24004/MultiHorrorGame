using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using DG.Tweening;
public class CanCheckActionUIController : SerializedMonoBehaviour
{
    //調べることができることを示すための画像を表示させるためのコントローラー
    [Required]
    public GameObject CanCheckUICanvas;
    private PlayerStatus player_status;
    public bool CollideredToPlayer;
    private GameObject Player;
    public GameObject TargetImage;
    private BoxCollider can_check_display_range_collider;
    private void Start()
    {
        player_status = PlayerStatus.instance;
        Player = player_status.gameObject;
        can_check_display_range_collider = this.gameObject.GetComponent<BoxCollider>();
        DoHide();
    }
    private void LateUpdate()
    {
        if(CollideredToPlayer)
        {
            if(Player != null)
            {
                TargetImage.transform.DODynamicLookAt(player_status.HeadPos.transform.position, 0.3f);
            }
        }
    }
    [Button]
    public void DoDisplay()
    {
        CanCheckUICanvas.SetActive(true);
    }
    [Button]
    public void DoHide()
    {
        CanCheckUICanvas.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Player")
        {
            CollideredToPlayer = true;
            DoDisplay();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.gameObject.tag == "Player")
        {
            CollideredToPlayer = false;
            DoHide();        
        }
    }
}
