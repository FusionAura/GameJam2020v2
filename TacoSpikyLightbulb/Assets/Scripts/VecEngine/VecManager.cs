using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Singleton. Accessed with VecManager.instance.
/// </summary>
public class VecManager : MonoBehaviour
{
	#region Singleton
	private static VecManager instance;

    public static VecManager Instance { get { return instance; } }

    /// <summary>
    /// Prevent multiple VecManagers from ever existing.
    /// </summary>
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            instance = this;
        }

        _Awake();
    }
    #endregion

    #region Fields
    private List<VecMesh> meshes; // Bad. Should store the individual verts and edges like below.

    // All Verts/Edges/Masks that are currently acrive.
    //private List<VecVert> verts;
    //private List<VecEdge> edges;
    //private List<VecMask> masks;

    #endregion

    #region Properties

    #endregion

    private void _Awake()
    {
        meshes = new List<VecMesh>();

        /*verts = new List<VecVert>();
        edges = new List<VecEdge>();
        masks = new List<VecMask>();*/
    }

    private int SortVecMeshesByAscendingMinDepth(VecMesh vm1, VecMesh vm2)
    {
        return vm1.BB.MinCorner.z.CompareTo(vm2.BB.MinCorner.z);
    }

    public void LateUpdate()
    {
        //edgesToCheck.Clear();
        //masksToCheck.Clear();

        foreach(var m in meshes)
            m.PreCalc();

        // TBA: Ignore meshes with no masks.

        // Sort the meshes by ascending minDepth.
        // During masking, if a mesh's minDepth is greater than another's maxDepth, it can safely be ignored.
        // WARNING: May be buggy when used with non-expensive depth testing.
        meshes.Sort(SortVecMeshesByAscendingMinDepth);

        // WARNING! THIS INCREASES EXPONENTIALLY WITH THE AMOUNT OF MESHES ONSCREEN! VERY EXPENSIVE!
        foreach (var m in meshes)
            m.PostCalc(meshes);
    }

    /**
     * 
     * Should be done this way:
     * Loop through each VecMesh and call the LateUpdate for Verts Masks Edges (no masking yet).
     * Have each VecMesh create their OWN edgesToCheck and masksToCheck collections.
     * Then have the intersections be performed.
     * 
     * 
     * 
     * 
     */

    #region Register
    /// <summary>
    /// Registers the VecMesh to be drawn by the manager.
    /// </summary>
    /// <param name="vecMesh"></param>
    public void RegisterVecMesh(VecMesh vecMesh)
    {
        meshes.Add(vecMesh);
        //vecMesh.PreCalc();

        /*verts.AddRange(vecMesh.Verts);
        edges.AddRange(vecMesh.Edges);
        masks.AddRange(vecMesh.Masks);*/
    }

    public void UnregisterVecMesh(VecMesh vecMesh)
    {
        meshes.Remove(vecMesh);
    }
	#endregion
}
