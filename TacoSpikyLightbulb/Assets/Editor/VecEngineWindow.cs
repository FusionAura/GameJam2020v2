using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

// Basic debug information for VecModels while in the editor.

class VecEngineWindow : EditorWindow
{
    [MenuItem("Window/" + "VecEngine")]
    public static void Init()
    {
        // Get existing open window or if none, make a new one:
        VecEngineWindow window = GetWindow<VecEngineWindow>();
        window.title = "VecEngineWindow";
        //SceneView.onSceneGUIDelegate += OnScene;
    }

    // Window has been selected
    void OnFocus()
    {
        // Remove delegate listener if it has previously
        // been assigned.
        SceneView.duringSceneGui -= OnSceneGUI;
        // Add (or re-add) the delegate.
        SceneView.duringSceneGui += OnSceneGUI;
    }

    void OnDestroy()
    {
        // When the window is destroyed, remove the delegate
        // so that it will no longer do any drawing.
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    public void OnSceneGUI(SceneView view)
    {
        var vecModels = Object.FindObjectsOfType<VecModel>();

        Handles.color = Color.magenta;

        foreach (var vecModel in vecModels)
        {
            if (!vecModel.VecMeshAsset) continue;

            // Show the mesh in edit mode.
            List<VecMesh> vecMeshes = new List<VecMesh>();
            VecMeshProcessor.VecMeshData[] vmds = VecMeshProcessor.ProcessTextAsset(vecModel.VecMeshAsset);

            /*Handles.Label(vecModel.transform.position,
                vecModel.VecMeshAsset.name.ToString());*/

            //t *= Matrix4x4.Rotate(Quaternion.AngleAxis(90f, Vector3.right));
            //t *= Matrix4x4.Rotate(Quaternion.AngleAxis(180f, Vector3.up));

            //Handles.BeginGUI();
            // Do your drawing here using GUI.
            foreach (var vmd in vmds) { 
                foreach(var edge in vmd.Edges) {
                    var t = vecModel.transform.localToWorldMatrix;
                    t *= vmd.InitialTransform;

                    Handles.DrawLine(
                        t.MultiplyPoint(vmd.Verts[edge.VertIdxs[0]]),
                        t.MultiplyPoint(vmd.Verts[edge.VertIdxs[1]])
                    );
                }
            }
            //Handles.EndGUI();
        }

        /*VecModel handleExample = (VecModel)target;
        if (handleExample == null)
        {
            return;
        }*/
    }
}