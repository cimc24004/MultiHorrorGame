using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
[CreateAssetMenu(fileName = "SoundSensorStatus", menuName = "ScriptableObjects/Enemy/SoundSensorStatus")]
public class SoundSensorStatus : SerializedScriptableObject
{
    [Header("音がなった場所についた後戻るまで判断する時間。")]
    public float ArriveSoundPlaceWaitTimeToReturn;
    [Header("プレイヤーの状態によって音に気づく距離を変える。")]
    [Space]
    [Header("プレイヤーが走っている時")]
    public float RealizeSoundPlayerRunDistance;
    [Header("プレイヤーが歩いている時")]
    public float RealizeSoundPlayerWalkDistance;
    [Header("プレイヤーがしゃがみ歩きしてるとき")]
    public float RealizeSoundPlayerCrouchWalkDistance;
    [Space]
    [Header("音が鳴ったさきのポイント歩き移動する速さ")]
    public float SpeedWalkWhenRealizeSound = 1;
    [Header("音が鳴ったさきのポイント走り移動する速さ")]
    public float SpeedRunWhenRealizeSound = 4;
    [Header("探すのをあきらめて戻るときの歩く速さ")]
    public float SpeedWhenReturn = 1;


    public void AllStatusChange(float _change)
    {
        ArriveSoundPlaceWaitTimeToReturn = _change;
    }
}
