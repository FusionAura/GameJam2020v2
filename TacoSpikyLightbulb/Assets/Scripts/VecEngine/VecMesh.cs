using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VecMesh : MonoBehaviour
{
	#region Fields
	private VecVert[] verts;
    private VecEdge[] edges;
    private VecMask[] masks;
	#endregion

	#region Properties
    public VecVert[] Verts { get { return verts; } }
    public VecEdge[] Edges { get { return edges; } }
    public VecMask[] Masks { get { return masks; } }

    //public float Depth { get; private set; }    // Distance of the VecMesh from the camera (based on its transform).

    /**
     * Some Edges/Masks won't need to be checked in CalcPhase2,
     * So the ones which will be checked should be stored in here in CalcPhase1.
     */
    public List<VecVert> VertsToMask { get; private set; }
    public List<VecEdge> EdgesToMask { get; private set; }
    public List<VecMask> MasksToApply { get; private set; }

    public VecBoundingBox BB { get; private set; }

    public bool HasMasks { get; private set; } = true;

    public bool Hidden = true; // Don't draw VecMeshes on the first frame they exist.
    #endregion

    //private Matrix4x4 initialTransform;
    public VecMeshProcessor.VecMeshData MeshData;
    public float Lifetime = -1f; // If >= 0f, then the VecMesh will destroy itself after that amount of time.

    /// <summary>
    /// How the Mesh will be masked by other meshes based on depth.
    /// </summary>
    public enum MaskedBy
    {
        All,        // Read depth normally. Apply every mask in front of each edge (very expensive)
        Depth,      // Only apply masks from Meshes with a lower depth (in front of me) value (mildly expensive)
        None        // Do not get affected by any mask.
    }

    public bool SelfMask { get; private set; } = true; // Should this Meshes masks affect its visible Edges?

    public VecMesh()
    {
        VertsToMask = new List<VecVert>();
        EdgesToMask = new List<VecEdge>();
        MasksToApply = new List<VecMask>();
        BB = new VecBoundingBox();
    }

    public void Start()
    {
        ExtractMeshData(MeshData);
    }

    private void ExtractMeshData(VecMeshProcessor.VecMeshData meshData)
    {
        // Apparently, Unity transform can't just be assigned to. As such, this is necessary to assign the initialTransform.
        this.transform.localScale = meshData.InitialTransform.ExtractScale();
        this.transform.localRotation = meshData.InitialTransform.ExtractRotation();
        this.transform.localPosition = meshData.InitialTransform.ExtractPosition();

        // VERTS
        List<VecVert> _verts = new List<VecVert>();
        foreach (var e in meshData.Verts)
            _verts.Add(new VecVert(e, this));

        verts = _verts.ToArray();


        // EDGES
        List<VecEdge> _edges = new List<VecEdge>();
        foreach (var e in meshData.Edges)
        {
            // Get references to the vert objects that were just created.
            VecVert[] v = new VecVert[2];
            for (var i = 0; i < 2; i++)
                v[i] = verts[e.VertIdxs[i]];

            _edges.Add(new VecEdge(v, e.Type));
        }

        edges = _edges.ToArray();


        // MASKS
        List<VecMask> _masks = new List<VecMask>();
        foreach (var e in meshData.Faces)
        {
            // Get references to the vert objects that were just created.
            VecVert[] v = new VecVert[3];
            for (var i = 0; i < 3; i++)
                v[i] = verts[e.VertIdxs[i]];

            // Get references to the edge objects that were just created.
            VecEdge[] ve = new VecEdge[3];
            for (var i = 0; i < 3; i++)
                ve[i] = edges[e.EdgeIdxs[i]];

            _masks.Add(new VecMask(v, ve));
        }

        masks = _masks.ToArray();

        HasMasks = masks.Length != 0; // VecMeshes with no masks are excluded from being checked.

        // Assign references between shapes.
        /*foreach (var e in edges) e.StoreVecMaskParents(masks);
        foreach (var v in verts)
        {
            v.StoreVecMaskParents(masks);
            v.StoreVecEdgeParents(edges);
        }*/

        // Register the VecMesh for drawing (TBA: unregistering it).
        VecManager.Instance.RegisterVecMesh(this);

        this.name = meshData.Name;
    }

    /// <summary>
    /// Pre-intersection calculation.
    /// </summary>
    public void PreCalc()
    {
        VertsToMask.Clear();
        EdgesToMask.Clear();
        MasksToApply.Clear();

        //CalcDepth();

        foreach (var v in verts) {
            v.LateUpdate();
            //if (v.IsVisible) VertsToMask.Add(v); // Nuts. Doesn't work (since Edges are calculated after this). I don't really want another loop for now.
        }
        foreach (var m in masks) {
            m.LateUpdate();
            if (!m.Culled) MasksToApply.Add(m); // These masks will be used to mask other visible edges.
        }
        foreach (var e in edges) {
            e.LateUpdate();
            if (e.IsVisible) EdgesToMask.Add(e); // These edges will be affected by masks.
        }

        //BB.Recalculate(VertsToMask); // Note: BB encases all VISIBLE Edges (everything that needs to be masked).
        BB.Recalculate(verts);
    }

    /// <summary>
    /// Masking occurs here.
    /// </summary>
    /// <param name="allRegisteredMeshes"></param>
    public void PostCalc(ICollection<VecMesh> allRegisteredMeshes)
    {
        // 1. Get a list of all meshes to check.
        List<VecMesh> meshesToCheck = new List<VecMesh>();

        foreach(var mesh in allRegisteredMeshes)
        {
            // Mask self.
            if (mesh == this) {
                if (SelfMask) meshesToCheck.Add(mesh);
                continue;
            }

            // If the current mesh's minDepth is greater than this meshes maxDepth, then it is behind it.
            // Then, stop iterating (as allRegisteredMeshes have been sorted by min depth).
            if (mesh.BB.MinCorner.z > this.BB.MaxCorner.z) {
                break;
            }
            // Store a reference to all meshes that are BB intersecting and in front of this one.
            if (mesh.BB.CheckColBehind(this.BB)) {
                meshesToCheck.Add(mesh);
            }
                /*// Determine how many masks each VecVert is behind.
                foreach (var v in verts)
                {
                    foreach (var m in masks)
                    {
                        if (!v.IsVisible) continue;

                        if (v.IsVertBehindMask(m))
                            v.IncrementMaskNestCount();
                    }
                }*/

            // Easy check. Ignore self.
            //if (mesh == this) continue;
        }

        // 2. Check for edge intersections against each mask in each of the meshesToCheck.
        foreach(var e in EdgesToMask)
        {
            foreach(var m in meshesToCheck) {
                e.CalcAndStoreIntersections(m, m == this);
            }
        }

        // 3. Have each Edge sort its intersections and create VecLines.
        foreach (var e in EdgesToMask)
        {
            e.CalcVecLines();
        }

        // Lifetime
        if (Lifetime >= 0f)
        {
            Lifetime -= Time.deltaTime;
            if (Lifetime < 0f) Destroy(this);
        }
    }

    /// <summary>
    /// When called, will destroy this Mesh and explode its VecLines outwards.
    /// </summary>
    public void Explode()
    {
        // Create explosions.
        foreach (var e in edges)
        {
            e.CreateExplosionGO(this.transform.parent);
        }

        Destroy(this);
        Debug.Log("VecMesh \"" + this.name + "\" exploded.");
    }

    public void OnDestroy()
    {
        VecManager.Instance.UnregisterVecMesh(this);
    }

    void OnGUI()
    {
        if (Hidden)
        {
            Hidden = false;
            return;
        }

        useGUILayout = false; // Apparently makes this run a lot faster.

        /*foreach (var e in verts)
        {
            EditorDebug.DrawPoint(e.TempGetPLocal());
        }*/

        //foreach (var e in masks)
        //    e.DebugDraw();

        if (EditorDebug.DEBUG_ENABLED)
        { 
            foreach (var v in verts)
            {
                v.DrawDebug();
            }
        }

        foreach (var e in edges)
        {
            e.DebugDraw();
        }

        //BB.DrawDebug();

        /*
        var p = Camera.main.WorldToScreenPoint(transform.position);
        p.y *= -1f;
        p.y += Screen.height;
        EditorDebug.DrawPoint(
            p,
            Color.magenta);*/

        /*foreach (var e in vecMeshes)
        {
            e.Draw();
        }*/

        //Vector3 test = Vector3.right * 10f;

        //VecLine.DrawVecLine(Vector3.zero, Matrix4x4.Rotate(Quaternion.AngleAxis(Time.time * 100f, Vector3.forward)).MultiplyVector(test));
    }
}
