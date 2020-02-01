using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VecMask
{
    #region Fields
    private VecVert[] verts;
    private MaskEdge[] edges;
    private Matrix4x4 maskMatrix; // Used when calculating whether a VecVert is behind the mask's backface or not.
	#endregion

	#region Properties
    public VecVert[] Verts { get { return verts; } }
    public VecEdge[] Edges { get; private set; } // REDUNDANT WITH THE OTHER edges FIELD. FIX LATER.

    public VecBoundingBox BB { get; private set; } // BoundingBox for this mask.

    /// <summary>
    /// What side of the VecMask is facing the camera.
    /// </summary>
    public bool BackfaceVisible { get; private set; }
    
    /// <summary>
    ///  = false if not visible due to culling or offscreen.
    /// </summary>
    public bool Culled { get; set; }

    #endregion

    /// <summary>
    /// Stores a reference to a VecEdge and also whether its verts should be flipped for this mask.
    /// </summary>
    private struct MaskEdge
    {
        public VecEdge VecEdge;
        public bool VertsFLipped;

        public MaskEdge(VecEdge VecEdge, bool VertsFLipped)
        {
            this.VecEdge = VecEdge;
            this.VertsFLipped = VertsFLipped;
        }
    }

    public VecMask(VecVert[] verts, VecEdge[] edges)
    {
        InitVerts(verts);
        InitEdges(edges);
        BB = new VecBoundingBox();
    }

    private void InitVerts(VecVert[] verts)
    {
        this.verts = verts;
        foreach (var v in verts)
        {
            v.RegisterVecMaskParent(this);
        }
    }

    private void InitEdges(VecEdge[] edges)
    {
        this.edges = new MaskEdge[edges.Length];
        this.Edges = edges;

        // Determine if the each Edge's vert values are flipped for this mask.
        int i = 0;
        foreach (var e in edges)
        {
            bool flipped = false;

            if (
                    (verts[0] == e.Verts[0] && verts[1] == e.Verts[1]) ||
                    (verts[1] == e.Verts[0] && verts[2] == e.Verts[1]) ||
                    (verts[2] == e.Verts[0] && verts[0] == e.Verts[1])
                )
            {
                flipped = true;
            }

            // Add this mask to the edges references.
            e.RegisterVecMaskParent(this);
            this.edges[i] = new MaskEdge(e, flipped);
            i++;
        }
    }

    public void LateUpdate()
    {
        RecalcBackface();
        RecalcCulling();
        RecalcMaskMatrix();
        BB.Recalculate(verts);
    }

    /// <summary>
    /// Returns true if this Masks verts are in the opposite order to the edge's.
    /// Note: there's likely a more efficient way of doing this.
    /// </summary>
    /// <param name="edge"></param>
    /// <returns></returns>
    public bool IsEdgeFlipped(VecEdge edge)
    {
        foreach(var e in edges) {
            if (e.VecEdge == edge) return e.VertsFLipped;
        }
        return false;
    }

    private void RecalcMaskMatrix()
    {
        // Recalculate transform and normal.
        Vector3 normal = Math3D.GetNormal(verts[0].World, verts[1].World, verts[2].World);
        maskMatrix = Matrix4x4.LookAt(
            Vector3.zero,
            normal,
            //edges[0].Vec3D Don't use an edge for this calculation, as the verts may not be in the correct order due to the edge being shared.
            (verts[1].World - verts[0].World).normalized
        );
    }

    /// <summary>
    /// Determine whether the frontface or backface were facing the Camera.
    /// </summary>
    private void RecalcBackface()
    {
        float a = 0f;
        for (int i = 0; i < Verts.Length - 1; i++)
            a += (Verts[i + 1].Screen.x - Verts[i].Screen.x) *
                 (Verts[i + 1].Screen.y + Verts[i].Screen.y);
        a += (Verts[0].Screen.x - Verts[2].Screen.x) *
             (Verts[0].Screen.y + Verts[2].Screen.y);

        BackfaceVisible = a > 0f;
    }

    /// <summary>
    /// Check if the mask should affect anything (based on backface culling and the CullMode set).
    /// </summary>
    private void RecalcCulling()
    {
        /*switch (CullMode)
        {
            case CullMode.None:
                Culled = false;
                break;

            case CullMode.CullCounterClockwiseFace:
                Culled = BackfaceVisible;
                break;

            case CullMode.CullClockwiseFace:
                Culled = !BackfaceVisible;
                break;
        }*/

        Culled = BackfaceVisible;
    }

    /// <summary>
    /// Returns true if the point is on the backface side of the plane.
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    public bool IsPointBehindMaskPlane(Vector3 pos)
    {
        //var p = Vector3.Transform(pos, -Matrix.Invert(transform));

        //var v1 = Vector3.Transform(verts[0], -Matrix.Invert(transform));

        var p = (maskMatrix.inverse).MultiplyVector(pos);
        var v1 = (maskMatrix.inverse).MultiplyVector(verts[0].World);

        return p.z < v1.z; // There we go; fixed it.
    }

    /// <summary>
    /// Returns true if the 2D point is inside of this mask in 2D space alone. Does NOT check depth.
    /// </summary>
    /// <param name="p"></param>
    /// <returns></returns>
    public bool IsPointInside2D(Vector2 p)
    {
        if (Culled) return false;

        float d1, d2, d3;
        bool has_neg, has_pos;

        d1 = Sign(p, Verts[0].Screen, Verts[1].Screen);
        d2 = Sign(p, Verts[1].Screen, Verts[2].Screen);
        d3 = Sign(p, Verts[2].Screen, Verts[0].Screen);

        has_neg = (d1 < 0) || (d2 < 0) || (d3 < 0);
        has_pos = (d1 > 0) || (d2 > 0) || (d3 > 0);

        return !(has_neg && has_pos);
    }

    private float Sign(Vector2 p1, Vector2 p2, Vector2 p3)
    {
        return (p1.x - p3.x) * (p2.y - p3.y) - (p2.x - p3.x) * (p1.y - p3.y);
    }

    public void DebugDraw()
    {
        

        //BB.DrawDebug();
    }
}
