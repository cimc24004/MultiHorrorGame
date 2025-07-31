using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Sirenix.OdinInspector;
[System.Serializable] // Unityに「このクラスをシリアライズしてね」と教えるおまじない
public class ReadEventSentenceData
{
    // ここにTextAreaを付ける！ (minLines, maxLines)で見える行数を調整できる
    [TextArea(3, 10)]
    [HideLabel] // "Sentence"っていうラベルを非表示にしてスッキリさせる
    public string Sentence;
}
public class ReadEventSentenceDatabase : Singleton<ReadEventSentenceDatabase>
{
    [TextArea]
    public Dictionary<int, ReadEventSentenceData> readEventSentenceDatas = new Dictionary<int, ReadEventSentenceData>();
    public ReadEventSentenceData FindReadEventSentenceDataById(int _id)
    {
        return readEventSentenceDatas[_id];
    }
}
