using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Linq;
public class ReadEventController : SerializedMonoBehaviour
{
    private GameController gameController;
    private ReadEventUIController readEventUIController;

    public List<int> ReadEventSentenceIds;  // 1ページごとに表示する文章
    private ReadEventSentenceDatabase readEventSentenceDatabase;
    void Start()
    {
        gameController = GameController.instance;
        readEventSentenceDatabase = ReadEventSentenceDatabase.instance;
        readEventUIController = gameController.readEventUIController;
    }

    // 本を読み始めるときに呼ばれる
    public void ReadEventStart()
    {
        // UIコントローラーに「全ページ文章リスト」を渡す
        // 以降UIで文章ページの制御が可能になる
        // List<ReadEventSentenceData> reading_sentences;
        List<string> reading_sentences = ReadEventSentenceIds
                              .Select(id => readEventSentenceDatabase.readEventSentenceDatas[id].Sentence)
                              .ToList();
        readEventUIController.StartReading(reading_sentences);
    }
}
