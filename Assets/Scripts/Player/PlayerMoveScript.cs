using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerMoveScript : MonoBehaviour
{
    //プレイヤー移動スクリプト
    private Rigidbody rigid;
    public float RunSpeed;
    public float NowSpeed;
    //このカメラの方向とプレイヤーの向く方向が同じになるようにする。
    public GameObject DirectionCamera;

    private float TimeWaitNow;
    private PlayerInput player_input;
    private Vector3 MoveKeyValue;
    private InputAction MoveAction;
    private TimeManager time_manager;
    // Start is called before the first frame update
    void Start()
    {
        rigid = this.gameObject.GetComponent<Rigidbody>();
        NowSpeed = RunSpeed;
        player_input = this.gameObject.GetComponent<PlayerInput>();
        MoveAction = player_input.currentActionMap["Move"];
        time_manager = TimeManager.instance;
    }

    // Update is called once per frame
    void Update()
    {
        MoveKeyValue = MoveAction.ReadValue<Vector2>();

    }
    private void FixedUpdate()
    {
        if(MoveKeyValue.magnitude != 0)
        {
            this.transform.localRotation = Quaternion.Euler(0, DirectionCamera.transform.rotation.eulerAngles.y, 0);
            rigid.velocity = ((this.transform.forward * MoveKeyValue.y) + (this.transform.right * MoveKeyValue.x)) * NowSpeed;
        }
        else
        {
            rigid.velocity = Vector3.zero;
        }
    }
}
