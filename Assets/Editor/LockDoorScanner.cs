// Assets/Editor/LockDoorControllerScanner.cs
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class LockDoorControllerScanner : EditorWindow
{
    private Vector2 _scroll;
    private readonly List<LockDoorsController> _controllers = new();
    private readonly List<(LockDoorsController owner, TheDoorController door)> _missingOpenPermit = new();
    private readonly List<(LockDoorsController owner, int index)> _nullDoorSlots = new();

    private int _totalDoorsScanned;

    [MenuItem("Tools/Doors/Scan Missing OpenPermit")]
    public static void Open()
    {
        var win = GetWindow<LockDoorControllerScanner>("Door OpenPermit Scanner");
        win.minSize = new Vector2(520, 360);
        win.Scan();
        win.Show();
    }

    void OnGUI()
    {
        EditorGUILayout.Space(4);
        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Rescan", GUILayout.Height(24)))
            {
                Scan();
            }
            GUILayout.FlexibleSpace();
        }

        EditorGUILayout.Space(6);
        EditorGUILayout.LabelField("Scan Summary", EditorStyles.boldLabel);
        using (new EditorGUI.IndentLevelScope())
        {
            EditorGUILayout.LabelField($"LockDoorsController (scene): {_controllers.Count}");
            EditorGUILayout.LabelField($"Doors scanned: {_totalDoorsScanned}");
            EditorGUILayout.LabelField($"Missing open_permit_by_key_controller: {_missingOpenPermit.Count}", _missingOpenPermit.Count > 0 ? WarnStyle() : EditorStyles.label);
            EditorGUILayout.LabelField($"Null door slots in list: {_nullDoorSlots.Count}", _nullDoorSlots.Count > 0 ? WarnStyle() : EditorStyles.label);
        }

        EditorGUILayout.Space(10);
        _scroll = EditorGUILayout.BeginScrollView(_scroll);

        DrawMissingSection();
        EditorGUILayout.Space(12);
        DrawNullSlotsSection();

        EditorGUILayout.EndScrollView();

        EditorGUILayout.HelpBox(
            "LockDoorsController の will_lock_the_doors に登録された TheDoorController をチェックして、" +
            "open_permit_by_key_controller が未設定のものを列挙しているよ。\n" +
            "行の Select で選択、Ping でハイライトできる。",
            MessageType.Info);
    }

    private void DrawMissingSection()
    {
        EditorGUILayout.LabelField("Missing open_permit_by_key_controller", EditorStyles.boldLabel);

        if (_missingOpenPermit.Count == 0)
        {
            EditorGUILayout.HelpBox("不足しているものは見つからなかった。👍", MessageType.None);
            return;
        }

        // コントローラーごとにグループ化して表示
        foreach (var group in _missingOpenPermit.GroupBy(x => x.owner))
        {
            var owner = group.Key;
            using (new EditorGUILayout.VerticalScope("box"))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.ObjectField("LockDoorsController", owner, typeof(LockDoorsController), true);
                    if (GUILayout.Button("Select", GUILayout.Width(70)))
                        Selection.activeObject = owner.gameObject;
                    if (GUILayout.Button("Ping", GUILayout.Width(60)))
                        EditorGUIUtility.PingObject(owner.gameObject);
                }

                foreach (var item in group)
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.ObjectField("Door", item.door, typeof(TheDoorController), true);
                        if (GUILayout.Button("Select", GUILayout.Width(70)))
                            Selection.activeObject = item.door ? item.door.gameObject : null;
                        if (GUILayout.Button("Ping", GUILayout.Width(60)) && item.door)
                            EditorGUIUtility.PingObject(item.door.gameObject);
                    }
                }
            }
        }
    }

    private void DrawNullSlotsSection()
    {
        EditorGUILayout.LabelField("Null door slots in will_lock_the_doors", EditorStyles.boldLabel);

        if (_nullDoorSlots.Count == 0)
        {
            EditorGUILayout.HelpBox("Null スロットは見つからなかった。", MessageType.None);
            return;
        }

        foreach (var group in _nullDoorSlots.GroupBy(x => x.owner))
        {
            var owner = group.Key;
            using (new EditorGUILayout.VerticalScope("box"))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.ObjectField("LockDoorsController", owner, typeof(LockDoorsController), true);
                    if (GUILayout.Button("Select", GUILayout.Width(70)))
                        Selection.activeObject = owner.gameObject;
                    if (GUILayout.Button("Ping", GUILayout.Width(60)))
                        EditorGUIUtility.PingObject(owner.gameObject);
                }

                foreach (var (o, idx) in group)
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.LabelField($"Null at index [{idx}]", GUILayout.MinWidth(120));
                    }
                }
            }
        }
    }

    private void Scan()
    {
        _controllers.Clear();
        _missingOpenPermit.Clear();
        _nullDoorSlots.Clear();
        _totalDoorsScanned = 0;

        // シーン内の LockDoorsController（無効含む）だけ取得
#if UNITY_2023_1_OR_NEWER
        var ctrls = Object.FindObjectsByType<LockDoorsController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
#else
        var ctrls = Object.FindObjectsOfType<LockDoorsController>(true);
#endif
        _controllers.AddRange(ctrls.Where(c => c != null && !EditorUtility.IsPersistent(c))); // プロジェクト内アセットは除外

        foreach (var ctrl in _controllers)
        {
            // will_lock_the_doors は private [SerializeField] なので SerializedObject 経由で読む
            var so = new SerializedObject(ctrl);
            var listProp = so.FindProperty("will_lock_the_doors");
            if (listProp == null || !listProp.isArray) continue;

            var seen = new HashSet<TheDoorController>(); // 重複登録のノイズを避ける
            for (int i = 0; i < listProp.arraySize; i++)
            {
                var elem = listProp.GetArrayElementAtIndex(i);
                var door = elem.objectReferenceValue as TheDoorController;

                if (door == null)
                {
                    _nullDoorSlots.Add((ctrl, i));
                    continue;
                }

                if (seen.Contains(door)) continue; // 同じ参照が複数回あっても1回だけ数える
                seen.Add(door);

                _totalDoorsScanned++;

                // open_permit_by_key_controller が未設定かチェック
                if (door.open_permit_by_key_controller == null)
                {
                    _missingOpenPermit.Add((ctrl, door));
                }
            }
        }

        Repaint();

        // コンソールにも要約を出しておく
        Debug.Log($"[DoorScanner] controllers={_controllers.Count}, doorsScanned={_totalDoorsScanned}, " +
                  $"missingOpenPermit={_missingOpenPermit.Count}, nullSlots={_nullDoorSlots.Count}");
    }

    private static GUIStyle WarnStyle()
    {
        var s = new GUIStyle(EditorStyles.label);
        s.normal.textColor = new Color(0.85f, 0.2f, 0.2f);
        return s;
    }
}
