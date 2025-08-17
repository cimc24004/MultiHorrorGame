using UnityEngine;
using System;
[Serializable]
[CreateAssetMenu(fileName = "MiniTutorialEntryData", menuName = "ScriptableObjects/CreateMiniTutorialEntryData")]
public class MiniTutorialEntryData : ScriptableObject
{
    public enum OnlyOnce
    {
        None,
        ForEver,
        Scene
    }
    [Tooltip("ユニークID")]
    public int id;

    [Tooltip("タイトル")]
    public string title;

    [Tooltip("説明文")]
    [TextArea(2, 6)]
    public string description;

    [Tooltip("一度だけチュートリアルを表示するかどうか\n None: なし ForEver: 永遠に Scene: ロードしない限り表示しない")]
    public OnlyOnce onlyOnce = OnlyOnce.None;

}
