using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace southws
{
	[CustomPreview(typeof(MeshFilter))]
	public class PolygonCounterForMeshRenderer : ObjectPreview
	{
		void OnValidate() {
			SceneView.RepaintAll();
		}

	    GameObject gameObject;
	    Editor gameObjectEditor;
//	    Texture2D previewBackgroundTexture;

	    int vertices;
	    int polygons;

	    public override bool HasPreviewGUI()
	    {
	        return true;
	    }

	    public override GUIContent GetPreviewTitle()
	    {
	        MeshFilter filter = target as MeshFilter;
			if (filter != null && filter.sharedMesh != null) {
	        	vertices = filter.sharedMesh.vertices.Length;
	        	polygons = filter.sharedMesh.triangles.Length/3;
			}

	        string str = "Vertices(verts): " + vertices 
	                   + "   Triangles(tris): " + polygons;
	        GUIContent _title = new GUIContent(target.name + " " + str);
	        return _title;
	    }

	    public override void OnPreviewGUI(Rect r, GUIStyle background)
	    {
	        base.OnPreviewGUI(r, background);

	        EditorGUI.BeginChangeCheck();
	        
	        MeshFilter myClass = target as MeshFilter;
	        gameObject = myClass.gameObject;

	        if(EditorGUI.EndChangeCheck())
	        {
	            if(gameObjectEditor != null) Editor.DestroyImmediate(gameObjectEditor);
	        }
	      
	        if (gameObject != null)
	        {
	            GUIStyle bgColor = new GUIStyle();
//	            bgColor.normal.background = previewBackgroundTexture;
	            if (gameObjectEditor == null)
	                gameObjectEditor = Editor.CreateEditor(gameObject);
	     
	//            gameObjectEditor.OnInteractivePreviewGUI(r,bgColor);
	        }
	    }
	}


	[CustomPreview(typeof(SkinnedMeshRenderer))]
	public class PolygonCounterForSkinnedMeshRenderer : ObjectPreview
	{

	    GameObject gameObject;
	    Editor gameObjectEditor;
//	    Texture2D previewBackgroundTexture;

	    int vertices;
	    int polygons;

	    public override bool HasPreviewGUI()
	    {
	        return true;
	    }

	    public override GUIContent GetPreviewTitle()
	    {
	    
	        SkinnedMeshRenderer skin = target as SkinnedMeshRenderer;

			if (skin != null && skin.sharedMesh != null) {
	        	vertices = skin.sharedMesh.vertices.Length;
	        	polygons = skin.sharedMesh.triangles.Length/3;
			}
	        string str = "Vertices(verts): " + vertices 
	                   + "   Triangles(tris): " + polygons;
	        GUIContent _title = new GUIContent(target.name + " " + str);
	        return _title;
	    }

	    public override void OnPreviewGUI(Rect r, GUIStyle background)
	    {
	        base.OnPreviewGUI(r, background);

	        EditorGUI.BeginChangeCheck();
	        
	        SkinnedMeshRenderer myClass = target as SkinnedMeshRenderer;
	        gameObject = myClass.gameObject;

	        if(EditorGUI.EndChangeCheck())
	        {
	            if(gameObjectEditor != null) Editor.DestroyImmediate(gameObjectEditor);
	        }
	      
	        if (gameObject != null)
	        {
	            GUIStyle bgColor = new GUIStyle();
//	            bgColor.normal.background = previewBackgroundTexture;
	            if (gameObjectEditor == null)
	                gameObjectEditor = Editor.CreateEditor(gameObject);

	//            gameObjectEditor.OnInteractivePreviewGUI(r,bgColor);
	        }
	    }
	}
}
