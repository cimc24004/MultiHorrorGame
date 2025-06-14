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

    public GameObject TargetImage;
    private BoxCollider can_check_display_range_collider;
    private GameObject PlayerOfOwner;
    private void Start()
    {
        can_check_display_range_collider = this.gameObject.GetComponent<BoxCollider>();
        DoHide();
    }
    private void OnEnable()
    {
        PlayerStatus.OnLocalPlayerSpawned += InitWhenPlayerSpawned;
        if (PlayerStatus.LocalInstance != null)
        {
            InitWhenPlayerSpawned(PlayerStatus.LocalInstance);
        }
    }
    private void OnDisable()
    {
        PlayerStatus.OnLocalPlayerSpawned -= InitWhenPlayerSpawned;
        if (PlayerStatus.LocalInstance != null)
        {
            InitWhenPlayerSpawned(PlayerStatus.LocalInstance);
        }
    }
    private void InitWhenPlayerSpawned(PlayerStatus _player_status)
    {
        PlayerOfOwner = _player_status.gameObject;
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
            CanCheckUICanvas.SetActive(false);

        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject == PlayerOfOwner)
        {
            DoDisplay();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.gameObject == PlayerOfOwner)
        {
            DoHide();        
        }
    }
}
