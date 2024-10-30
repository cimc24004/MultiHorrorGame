using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace southws
{
	public class PolygonCounter : MonoBehaviour {

	    int activeVertices;
	    int activeTriangles;
	    int activeMeshes;
	    int activeSkins;
	    int vertices;
	    int triangles;
	    int meshes;
	    int skins;
	    int missing;

	    [ContextMenu("CountStart")]
	    void CountPolygon(){

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
#if UNITY_EDITOR
				if (!EditorUtility.IsPersistent(go.transform.root.gameObject)) {
#endif
		            if (!(go.hideFlags == HideFlags.NotEditable || go.hideFlags == HideFlags.HideAndDontSave))
					{
	    	            objectsInScene.Add(go);
					}
#if UNITY_EDITOR
				}
#endif
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
	        Debug.LogFormat ("Enabled Vertices(verts) : {0} , Triangles(tris) : {1} in Scene" ,
	            vertices , triangles);
	    }

#if UNITY_EDITOR

	    [CustomEditor(typeof(PolygonCounter))]
	    public class CountStartEditor : Editor {

	        PolygonCounter polygonCounter;

	        void OnEnable(){
	            polygonCounter = target as PolygonCounter;

	            polygonCounter.CountPolygon();
	        }

	        public override void OnInspectorGUI ()
	        {
	            base.OnInspectorGUI ();

	            polygonCounter.vertices = EditorGUILayout.IntField( "Vertices (verts)", polygonCounter.activeVertices );
	            polygonCounter.triangles = EditorGUILayout.IntField( "Triangles (tris)", polygonCounter.activeTriangles );

	            if (GUILayout.Button("更新")){
	                polygonCounter.CountPolygon();
	            }
	        }
	    }

#endif

	}
}