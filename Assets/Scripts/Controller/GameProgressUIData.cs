using UnityEngine;
using Sirenix.OdinInspector;
using TMPro;
using System;
using Sirenix.Serialization;
using System.Collections;
using System.Collections.Generic;

public class GameProgressUIData : SerializedMonoBehaviour
{
    public class GameToDoListUIData{
        public TextMeshProUGUI TodoTextUI;
        public GameObject TodoBox;
    }

    [Header("UI情報")]
    [ShowInInspector]
    [OdinSerialize]
    [SerializeField] public Dictionary<string, GameToDoListUIData> TodoName_with_game_todo_list_ui_datas;
    [SerializeField]public TextMeshProUGUI ButsudanCountNowTextUI;
}
