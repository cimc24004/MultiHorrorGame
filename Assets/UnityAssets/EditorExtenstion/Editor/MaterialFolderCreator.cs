using UnityEngine;
using UnityEditor;
using System.IO;

public class MaterialFolderCreator : EditorWindow
{
    private string rootFolder = "Assets/Materials";

    [MenuItem("Tools/Create Folders From Materials")]
    public static void ShowWindow()
    {
        GetWindow<MaterialFolderCreator>("Material Folder Creator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Settings", EditorStyles.boldLabel);
        rootFolder = EditorGUILayout.TextField("Root Folder", rootFolder);

        if (GUILayout.Button("Create Folders and Move Materials"))
        {
            CreateFoldersAndMoveMaterials();
        }
    }

    private void CreateFoldersAndMoveMaterials()
    {
        string[] materialGUIDs = AssetDatabase.FindAssets("t:Material", new[] { rootFolder });

        foreach (string guid in materialGUIDs)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            Material material = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
            if (material != null)
            {
                string folderPath = Path.Combine(rootFolder, material.name);
                if (!AssetDatabase.IsValidFolder(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                    Debug.Log($"Created folder: {folderPath}");
                }

                string newAssetPath = Path.Combine(folderPath, Path.GetFileName(assetPath));
                newAssetPath = AssetDatabase.GenerateUniqueAssetPath(newAssetPath);

                AssetDatabase.MoveAsset(assetPath, newAssetPath);
                Debug.Log($"Moved {material.name} to {newAssetPath}");
            }
        }

        AssetDatabase.Refresh();
    }
}