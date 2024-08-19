using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using DG.Tweening;
public class CanCheckActionUIController : SerializedMonoBehaviour
{
    //調べることができることを示すための画像を表示させるためのコントローラー
    [Header("CanCheckUIを複数個指定したい場合はtrueにする")]
    public bool multiple;

    [HideIf("multiple", true)]
    [Required]
    public GameObject CanCheckUICanvas;

    [ShowIf("multiple", true)]
    [Required]
    public List<GameObject> CanCheckUICanvases;


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
            }
        }
    }
    [Button]
    public void DoDisplay()
    {
        if (multiple)
        {
            foreach(GameObject canvas in CanCheckUICanvases)
            {
                canvas.SetActive(true);
            }
        }
        else
        {
            CollideredToPlayer = true;
            CanCheckUICanvas.SetActive(true);
        }
    }
    [Button]
    public void DoHide()
    {
        if (multiple)
        {
            foreach (GameObject canvas in CanCheckUICanvases)
            {
                canvas.SetActive(false);
            }
        }
        else
        {
            CollideredToPlayer = false;
            CanCheckUICanvas.SetActive(false);

        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Player")
        {
            DoDisplay();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.gameObject.tag == "Player")
        {
            DoHide();        
        }
    }
}
