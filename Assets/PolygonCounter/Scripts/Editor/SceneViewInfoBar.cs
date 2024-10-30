using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System;

namespace southws
{
	[InitializeOnLoad]
	[ExecuteAlways]
	public class SceneViewInfoBar
	{
	    static int activeVertices;
	    static int activeTriangles;
	    static int activeMeshes;
	    static int activeSkins;
	    static int vertices;
	    static int triangles;
	    static int meshes;
	    static int skins;
	    static int missing;

		static SceneViewInfoBar()
		{
			SceneView.onSceneGUIDelegate += OnSceneGUI;
			EditorApplication.delayCall += OnDelayCall;
			EditorApplication.hierarchyChanged += OnHierarchyChanged;
			EditorApplication.projectChanged += OnProjectChanged;
		}

	    void OnEnable(){
	    }

	    private static void OnDelayCall(){
			CountPolygon();
			SceneView.RepaintAll();
	    }

	    private static void OnHierarchyChanged(){
			CountPolygon();
			SceneView.RepaintAll();
	    }

	    private static void OnProjectChanged(){
			CountPolygon();
			SceneView.RepaintAll();
	    }


		private static void OnSceneGUI(SceneView sceneView)
		{
			Handles.BeginGUI();
			GUILayout.BeginArea(new Rect(0,GetSceneViewHeight(sceneView) - (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing), 
				Screen.width / EditorGUIUtility.pixelsPerPoint, 
				Screen.height / EditorGUIUtility.pixelsPerPoint));
	 
			GUILayout.BeginHorizontal("toolbar");
			{
				GUILayout.Label($"Verts: {activeVertices} / {vertices}", EditorStyles.label);
				GUILayout.Label($"Tris: {activeTriangles} / {triangles}", EditorStyles.label);

				GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));

				GUILayout.Label($"Meshes: {activeMeshes} / {meshes}", EditorStyles.label);
				GUILayout.Label($"Skins: {activeSkins} / {skins}", EditorStyles.label);

				if(missing >= 1) {
					GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
					GUILayout.Label($"Missing: {missing}", EditorStyles.label);
				}

				if(GUILayout.Button("Refresh", EditorStyles.toolbarButton)) {
					CountPolygon();
				}

				GUILayout.FlexibleSpace();
		
			}
			GUILayout.EndHorizontal();
			GUILayout.EndArea();
			Handles.EndGUI();
		}

	    static float GetSceneViewHeight(SceneView sceneView)
	    {
	        return sceneView.position.width / sceneView.camera.aspect;
	    }

	    private static void CountPolygon(){

	    	activeVertices = 0;
	    	activeTriangles = 0;
	    	activeMeshes = 0;
	    	activeSkins = 0;
	        vertices = 0;
	        triangles = 0;
	        skins = 0;
	        meshes = 0;
	        missing = 0;
	        List<GameObject> objectsInScene = new List<GameObject>();
	        
	        foreach (GameObject go in Resources.FindObjectsOfTypeAll(typeof(GameObject))) {

	            if (!EditorUtility.IsPersistent(go.transform.root.gameObject) && !(go.hideFlags == HideFlags.NotEditable || go.hideFlags == HideFlags.HideAndDontSave))
				{
	                objectsInScene.Add(go);
				}
			}
	        foreach (GameObject obj in objectsInScene) {

	            SkinnedMeshRenderer skin = obj.GetComponent<SkinnedMeshRenderer> ();
                MeshFilter mesh = obj.GetComponent<MeshFilter> ();
                MeshRenderer meshRenderer = obj.GetComponent<MeshRenderer> ();

				if (skin != null && skin.sharedMesh == null) {
					missing += 1;

				} else if (skin != null) {
						skins += 1;
						int vert = skin.sharedMesh.vertices.Length;
						vertices += vert;
						int polygon = skin.sharedMesh.triangles.Length / 3;
					triangles += polygon;

		            if (obj.activeInHierarchy && skin.enabled) {
						activeSkins += 1;
						activeVertices += vert;
						activeTriangles += polygon;
					}

	            }

	            if (mesh != null && mesh.sharedMesh == null) {
					missing += 1;

				} else if (mesh != null) {
					meshes += 1;
					int vert = mesh.sharedMesh.vertices.Length;
					vertices += vert;
					int polygon = mesh.sharedMesh.triangles.Length / 3;
					triangles += polygon;

		            if (obj.activeInHierarchy && meshRenderer.enabled) {
						activeMeshes += 1;
						activeVertices += vert;
						activeTriangles += polygon;
					}
	            }
	        }
	//        Debug.LogFormat ("Enabled Vertices(verts) : {0} , Triangles(tris) : {1} in Scene" ,
	//            vertices , triangles);
	    }
	}
}
