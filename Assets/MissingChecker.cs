#nullable enable
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// リンク切れ情報を表示する拡張機能
/// </summary>
/// <remarks>
/// <para>Unity2020.3.5f1で動作確認。</para>
/// <para>
/// <list type="bullet">
/// <item><description>コンポーネントがMissingとなっている場合は！のアイコンを表示し、エラーログも表示。</description></item>
/// <item><description>
/// コンポーネント内の参照項目にMissingとなっているものが存在する場合は！のアイコンを表示し、エラーログも表示。
///（ログはコンポーネント単位で最初に見つかったもののみ表示）
/// </description></item>
/// </list>
/// </para>
/// </remarks>
public class MissingChecker
{
    [MenuItem("Tools/Log Missing Script Count")]
    private static void LogMissingScriptCount()
    {
        var gameObject = Selection.activeGameObject;
        if (gameObject == null)
        {
            return;
        }
        
        var missingCount = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(gameObject);
        Debug.Log(missingCount);
    }
    
    [MenuItem("Tools/Remove Missing Scripts")]
    private static void RemoveMissingScripts()
    {
        var gameObject = Selection.activeGameObject;
        if (gameObject == null)
        {
            return;
        }
        
        var missingCount = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(gameObject);
        Debug.Log(missingCount);
    }
}
