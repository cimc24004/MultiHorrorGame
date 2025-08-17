using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class MiniTutorialDatabases : Singleton<MiniTutorialDatabases>
{

    [Header("インスペクタで登録するデータ群")]
    [SerializeField] private List<MiniTutorialEntryData> entries = new List<MiniTutorialEntryData>();

    // 参照用インデックス
    private Dictionary<int, MiniTutorialEntryData> _index = new Dictionary<int, MiniTutorialEntryData>();

    private void Start() => RebuildIndex();

    // Editorで値をいじったとき自動でインデックス作り直し
    private void OnValidate() => RebuildIndex();

    /// <summary>リストからID辞書を再構築</summary>
    public void RebuildIndex()
    {
        if (entries == null) entries = new List<MiniTutorialEntryData>();
        _index.Clear();

        foreach (var e in entries)
        {
            if (e == null) continue;
            if (_index.ContainsKey(e.id))
            {
                Debug.LogWarning($"[MiniTutorialDatabases] 重複ID {e.id} は最初の登録を優先します。");
                continue;
            }
            _index.Add(e.id, e);
        }
    }

    public bool TryGet(int id, out MiniTutorialEntryData entry) => _index.TryGetValue(id, out entry);

    public MiniTutorialEntryData Get(int id)
    {
        if (_index.TryGetValue(id, out var e)) return e;
        Debug.LogWarning($"[MiniTutorialDatabases] ID {id} は見つからん。");
        return null;
    }


    /// <summary>全件の読み取り用（コピー不要ならこれでOK）</summary>
    public IReadOnlyList<MiniTutorialEntryData> GetAll() => entries;

#if UNITY_EDITOR
    [ContextMenu("Sort by ID (Editor)")]
    private void SortById()
    {
        entries = entries.OrderBy(x => x.id).ToList();
        EditorUtility.SetDirty(this);
    }
#endif
}
