using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
public class CrossHairStatus : SerializedMonoBehaviour
{
    //�N���X�w�A�̃X�e�[�^�X

    public enum EventColHitedStatus{
        None,
        Item,
        HideObj,
        OpenObj
    }

    //�f�t�H���g�̃N���X�w�A�A�C�R���I�u�W�F�N�g
    public Image DefaultCrossHairIcon;
    //�`�F�b�N�̃N���X�w�A�A�C�R���I�u�W�F�N�g
    public Image CheckCrossHairIcon;

    //CrossHairEvent�́@�h�A��A�C�e���ȂǂɃ��C�L���X�g���������Ă���̂��ǂ���
    public bool RaycastHitedExistsByCrossHairEvent;
    public string HitedTag;
    public GameObject HitedObj;
    private PlayerStatus player_status;
    public EventColHitedStatus event_col_hited_status;
    private bool InitPlayer;
    // Start is called before the first frame update
    private void OnEnable()
    {
        PlayerStatus.OnLocalPlayerSpawned += InitWhenPlayerSpawned;
    }
    private void OnDisable()
    {
        PlayerStatus.OnLocalPlayerSpawned -= InitWhenPlayerSpawned;
    }
    private void InitWhenPlayerSpawned(PlayerStatus _playerStatus){
        player_status = _playerStatus;
        InitPlayer = true;
    }
    // Update is called once per frame
    void Update()
    {
        if(!InitPlayer) return;
        //�v���C���[���B��Ă�����A�܂��̓h�A��A�C�e���ȂǂɃ��C�L���X�g���������Ă���Ȃ�� �`�F�b�N�摜��\���B
        if (RaycastHitedExistsByCrossHairEvent || player_status.HidingNow)
        {
            SpriteChange(_CheckMode: true);
        }
        else
        {
            SpriteChange(_CheckMode: false);
        }
    }


    public void SpriteChange( bool _CheckMode)
    {
        //�`�F�b�N���[�h�ɂ���ĕ\������摜��ύX����B
        if(_CheckMode == true)
        {

            DefaultCrossHairIcon.gameObject.SetActive(false);
            CheckCrossHairIcon.gameObject.SetActive(true);
        }
        else
        {

            DefaultCrossHairIcon.gameObject.SetActive(true);
            CheckCrossHairIcon.gameObject.SetActive(false);

        }
    }
}
