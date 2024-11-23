using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TextDisplay : MonoBehaviour
{
    public string textToDisplay = "";
    public Color textColor = Color.red;
    private void Start()
    {
        
    }
    private void OnDrawGizmos()
    {
        GUIStyle style = new GUIStyle();
        style.normal.textColor = textColor;
        style.fontSize = 25;
        style.alignment = TextAnchor.MiddleCenter; // テキストの位置を中心に調整

        // テキストのカラーを設定
        Handles.color = textColor;
        Handles.Label(transform.position, textToDisplay, style);
    }
}
