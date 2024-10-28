using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text.RegularExpressions;
using System;

public class MaterialSetTexture : EditorWindow
{
    Material material;

    string path_text;
    [MenuItem("Tools/マテリアル自動セット")]
    public static void ShowWindow()
    {
        var window = GetWindow<MaterialSetTexture>("マテリアル自動セット");
        window.Show();
;    }

    private void OnGUI()
    {
        GUILayout.Label("目的のパスを入力");
        path_text = GUILayout.TextField(path_text);
        if (GUILayout.Button("適用"))
        {
            DecidedEvent();
        }
        if (GUILayout.Button("リセット"))
        {
            ResetButton();
        }
    }

    private void ResetButton()
    {
        string[] Folders = AssetDatabase.GetSubFolders($"{path_text}");
        foreach (var fol in Folders)
        {
            Debug.Log(fol);
            List<string> fileNames = new List<string>(Directory.GetFiles(fol, "*", SearchOption.AllDirectories));
            foreach (string file in fileNames)
            {
                string ext = Path.GetExtension(file).ToLower();

                if (ext == ".mat")
                {
                    material = AssetDatabase.LoadAssetAtPath(file, typeof(Material)) as Material;
                    material.SetTexture("_MetallicGlossMap", null);
                    material.SetTexture("_MainTex", null);
                    material.SetTexture("_BumpMap", null);
                    material.SetTexture("_BaseMap", null);

                    material.SetTexture("_SpecGlossMap", null);
                }
            }

        }
    }
    private void DecidedEvent()
    {
        string[] Folders = AssetDatabase.GetSubFolders($"{path_text}");
        foreach(var fol in Folders) { 
            MaterialSet(fol);

        }
        //Debug.Log(material);
    }

    private void MaterialSet(string fol_path)
    {
        Debug.Log("MaterialsSet");
        //Material material = AssetDatabase.LoadAssetAtPath($"{Path}/", typeof(Material)) as Material;
        int i = 0;
        List<string> fileNames = new List<string>(Directory.GetFiles(fol_path, "*", SearchOption.AllDirectories));
        List<string> tmpDataList = new List<string>(fileNames);

        List<string> imageFileNames = new List<string>();
        foreach (string file in tmpDataList)
        {
           string ext = Path.GetExtension(file).ToLower();
            if (ext == ".meta")
            {
                fileNames.Remove(file);
            }
            else
            {
                //画像ファイルやったらー系

                if(ext == ".png" || ext == ".jpg")
                {
                    imageFileNames.Add(file);
                }
                else if(ext == ".mat")
                {
                    material = AssetDatabase.LoadAssetAtPath(file, typeof(Material)) as Material;
                }
            }
            i++;
        }

        foreach(string imageFileName in imageFileNames)
        {
            //ImageFileNameを取得 
            Texture tex = AssetDatabase.LoadAssetAtPath(imageFileName, typeof(Texture)) as Texture;
            if(imageFileName.IndexOf("Albedo") > 0 || imageFileName.IndexOf("BaseMap") > 0)
            {
                material.SetTexture("_BaseMap", tex);
            }else if(imageFileName.IndexOf("Normal") > 0)
            {
                material.SetTexture("_BumpMap", tex);

            }
            else if(imageFileName.IndexOf("Metallic") > 0)
            {
                material.SetTexture("_MetallicGlossMap", tex);

            }else if(imageFileName.IndexOf("Specular") > 0)
            {
                material.SetTexture("_SpecGlossMap", tex);

            }
            else
            {
                Debug.LogError($"{imageFileName}はマテリアルにセットすることができませんでした");
            }
            //material.SetTexture();

        }
        AssetDatabase.SaveAssets();
    }
}
