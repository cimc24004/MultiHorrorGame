#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.AssetImporters; // Unity 2020.2 以降

// ファイル名例: "AutoRemapMaterialsPostprocessor.cs"
class AutoRemapMaterialsPostprocessor : AssetPostprocessor
{
    // EditorPrefs 用のキー（ユニークなキーにしてください）
    private const string REMAP_ENABLE_KEY = "AutoRemapMaterialsPostprocessor_Enable";

    /// <summary>
    /// メニューから呼び出すトグル処理
    /// メニューをクリックするとオン／オフが切り替わります
    /// </summary>
    [MenuItem("Tools/Toggle Auto Material Remap")]
    private static void ToggleRemap()
    {
        bool current = EditorPrefs.GetBool(REMAP_ENABLE_KEY, true); // デフォルトtrue
        bool next = !current;                                       // 反転する
        EditorPrefs.SetBool(REMAP_ENABLE_KEY, next);

        Debug.Log($"[AutoRemap] マテリアル自動リマップを {(next ? "有効" : "無効")} にしました。");
    }

    /// <summary>
    /// メニュー表示やチェックマークを更新する Validate 系
    /// </summary>
    [MenuItem("Tools/Toggle Auto Material Remap", true)]
    private static bool ToggleRemapValidate()
    {
        bool current = EditorPrefs.GetBool(REMAP_ENABLE_KEY, true);
        // メニューの前にチェックマークを付ける
        Menu.SetChecked("Tools/Toggle Auto Material Remap", current);
        return true;
    }

    /// <summary>
    /// モデルに含まれる各マテリアルがインポートされる直前に呼ばれるコールバック。
    /// ここで Remapped Materials のマッピングを設定できる。
    /// </summary>
    void OnPreprocessMaterialDescription(MaterialDescription description, Material material, AnimationClip[] clips)
    {
        // オフならば何もせず終了
        bool isRemapEnabled = EditorPrefs.GetBool(REMAP_ENABLE_KEY, true);
        if (!isRemapEnabled) return;

        // このモデル（FBX等）のインポーターを取得
        ModelImporter modelImporter = assetImporter as ModelImporter;
        if (modelImporter == null) return;

        // モデル内に埋め込まれているマテリアル名を取得
        // たとえば FBX に "tatami.001" というマテリアルが含まれていれば "tatami.001" が取れる
        string fbxMatName = description.materialName;

        // プロジェクト内で同名の Material アセットを検索
        // （同名が複数ある場合は先頭ヒットだけ使う簡易実装）
        string[] guids = AssetDatabase.FindAssets(fbxMatName + " t:Material");
        if (guids.Length > 0)
        {
            string matPath = AssetDatabase.GUIDToAssetPath(guids[0]);
            Material foundMat = AssetDatabase.LoadAssetAtPath<Material>(matPath);

            if (foundMat != null)
            {
                // 「FBX 内に含まれるマテリアル名 → プロジェクト内のMaterial」を紐づける
                // これにより Model Import Settings の "Remapped Materials" 欄が自動で埋まる
                modelImporter.AddRemap(
                    new AssetImporter.SourceAssetIdentifier(typeof(Material), fbxMatName),
                    foundMat
                );

                Debug.Log(
                    $"[AutoRemap] 「{fbxMatName}」をインポート時にマテリアル「{foundMat.name}」へ自動で差し替えました\n"
                    + $"  FBX: {assetPath}\n"
                    + $"  Remap To: {matPath}"
                );
            }
        }
    }
}
#endif
