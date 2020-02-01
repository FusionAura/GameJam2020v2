using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class VecEdge
{
    const float MASK_INTERSECT_OFFSET = 100f;

    private struct VecLineDef
    {
        public Vector2 P1;
        public Vector2 P2;

        public VecLineDef(Vector2 P1, Vector2 P2)
        {
            this.P1 = P1;
            this.P2 = P2;
        }
    }

    #region Fields
    private List<VecMask> masks;    // Reference to the 0-2 VecMasks that this Edge is a part of.
    private Vector2 vec2DNorm;
    private Vector3 vec3DNorm;
    private HashSet<VecLineDef> vecLineDefs;
    #endregion

    #region Properties
    public VecVert[] Verts;
    public DrawType DType { get; private set; }
    public bool IsVisible { get; private set; }         // Whether this line should be drawn or be masked.
    public bool IsIntersection { get; private set; }    // Whether this line should be an intersection for other lines.
    /// <summary>
    /// The vector of this VecEdge in screen space.
    /// </summary>
    public Vector2 Vec2D { get; private set; }
    public Vector2 Normal2D { get; private set; }

    public VecBoundingBox BB { get; private set; }

    public List<EdgeIntersection> Intersections { get; private set; } // List of all intersections with IsIntersection Edges happening on this frame.

    /// <summary>
    /// The vector of this VecEdge in world space.
    /// </summary>
    public Vector3 Vec3D { get; private set; }
    #endregion

    public enum DrawType
    {
        NeverDraw   = 0,
        Normal      = 1,
        AlwaysDraw  = 2
    }

	public VecEdge(VecVert[] verts, int type)
    {
        this.Verts = verts;
        masks = new List<VecMask>(); // Start off with no mask.

        DType = (DrawType)type;
        BB = new VecBoundingBox();

        Intersections = new List<EdgeIntersection>();
        vecLineDefs = new HashSet<VecLineDef>();
    }

    public void RegisterVecMaskParent(VecMask mask)
    {
        masks.Add(mask);
    }

    public void LateUpdate()
    {
        RecalcIntersectionAndVisibility();
        vecLineDefs.Clear();

        // An edge that isn't drawn and isn't used by others for calculations doesn't need to update its values.
        if (!IsIntersection && !IsVisible) return; 

        RecalcEdgeVectors();
        Intersections.Clear(); // Clear list of intersections (they will be repopulated this frame).

        // Recalc BoundingBox2D.
        BB.Recalculate(Verts);

        // Normals are only used for calculating intersections (for now), so an edge that isn't an intersection needs not recalculate.
        if (IsIntersection && masks.Count > 0) {

            Normal2D = -new Vector2(vec2DNorm.y, -vec2DNorm.x);

            // Normal2D needs to be flipped when calculating intersections if the active mask it belongs to.
            if (!masks[0].Culled) {
                if (masks[0].IsEdgeFlipped(this)) Normal2D *= -1f;
            }
            else {
                if (masks[1].IsEdgeFlipped(this)) Normal2D *= -1f;
            }
        }
        else {
            Normal2D = Vector2.zero;
        }
    }

    private void RecalcEdgeVectors()
    {
        Vec2D = Verts[1].Screen - Verts[0].Screen;
        Vec3D = Verts[1].World - Verts[0].World;
        vec2DNorm = Vec2D.normalized;
        vec3DNorm = Vec3D.normalized;
    }

    private void RecalcIntersectionAndVisibility()
    {
        switch (masks.Count)
        {
            case 0:
                // No point in checking for intersections against a line with no mask.
                IsIntersection = false;
                // If this edge doesn't belong to any mask, then it should always be visible.
                IsVisible = DType == DrawType.NeverDraw ? false : true;

                break;

            case 1:
                // If this edge belongs to one mask, then only be visible if the mask isn't culled.
                // Since it has only one mask, this edge will always be one of the outer edges, and therefore, visible normally.
                IsIntersection = !masks[0].Culled;
                IsVisible = DType == DrawType.NeverDraw ? false : !masks[0].Culled;

                break;

            case 2:
                // If this edge belongs to two masks, only check intersections if it's an outer edge (only one of its masks is culled).
                IsIntersection = masks[0].Culled != masks[1].Culled;
                // Only visible if one mask is culled and the other isn't.
                switch (DType)
                {
                    case DrawType.NeverDraw:
                        IsVisible = false;
                        break;

                    case DrawType.Normal:
                        IsVisible = IsIntersection;
                        break;

                    case DrawType.AlwaysDraw:
                        IsVisible = !masks[0].Culled || !masks[1].Culled;
                        break;
                }

                break;
        }

        // Record that the VecVerts used by this VecEdge are visible this frame.
        if (IsVisible)
        {
            Verts[0].SetToVisible();
            Verts[1].SetToVisible();
        }
    }

    private bool CheckEdgeSharesVerts(VecEdge otherEdge)
    {
        if (
            Verts[0] == otherEdge.Verts[1] ||
            Verts[1] == otherEdge.Verts[0] ||
            Verts[0] == otherEdge.Verts[0] ||
            Verts[1] == otherEdge.Verts[1]
        ) return true;

        return false;
    }

    // INTERSECTIONS

    /// <summary>
    /// Iterate through all masks in the mesh and store all intersections.
    /// </summary>
    /// <param name="isMeshMine">Whether the mesh passed in contains this VecEdge.</param>
    public void CalcAndStoreIntersections(VecMesh vecMesh, bool isMeshMine)
    {
        if (!IsVisible) return; // If this edge isn't being drawn, then no need to clip it.

        Vector2 vec = Vec2D;
        VecVert v = Verts[0];

        // Go through all masks...
        foreach (var m in vecMesh.Masks)
        {
            // If this mask is culled, ignore it.
            if (m.Culled) continue;

            // If this is one of this edge's masks, ignore it.
            if (isMeshMine && masks.Contains(m)) {
                continue; // SLOW?
            }

            // If the both points of this line are completely in front of this mask, ignore it.
            if (Verts[0].Depth < m.BB.MinCorner.z &&
                Verts[1].Depth < m.BB.MinCorner.z) continue;

            // If no BB collision, ignore it.
            if (!m.BB.CheckCol2D(BB)) continue;

            // Check vert nesting if necessary.
            //if (checkVertMaskNest) v.CheckAndIncrementMaskNestCount(m);
            //if (checkVertMaskNest1) Verts[1].CheckAndIncrementMaskNestCount(m);

            // Calculate mask depth.
            // If the mask being checked belongs to the vert, then it will offset itself slightly (fixes some 50/50 errors).
            var v1Offset2D = Vector2.zero;
            var v1Offset3D = Vector3.zero;
            var v2Offset2D = Vector2.zero;
            var v2Offset3D = Vector3.zero;

            if (Verts[0].Masks.Contains(m))
            {
                v1Offset2D = vec2DNorm * MASK_INTERSECT_OFFSET;
                //v1Offset3D = vec3DNorm * MASK_INTERSECT_OFFSET / 100f;
            }

            if (Verts[1].Masks.Contains(m))
            {
                v2Offset2D = -vec2DNorm * MASK_INTERSECT_OFFSET;
                //v2Offset3D = -vec3DNorm * MASK_INTERSECT_OFFSET / 100f;
            }

            bool v1N = v.CheckMaskIntersection(m,
                                              v1Offset2D,
                                              v1Offset3D);
            bool v2N = Verts[1].CheckMaskIntersection(m,
                                                     -v2Offset2D,
                                                     -v2Offset3D);
            if (v1N) v.AddMaskInFront(m);
            if (v2N) Verts[1].AddMaskInFront(m);

            // Check each Edge for an intersection.
            foreach (var e in m.Edges)
            {
                // If the Edge isn't an intersection, ignore it.
                if (!e.IsIntersection) continue;

                // If the mask's Edge shares a VecVert with this one, it can be skipped.

                /**
                
                TBA! Need to check if the edge is going into a mask that shares only one vert with it, and add an interection there!
                The glitches in the teapot's spout are caused by this.
                No need to do it for the endpoint though, only the starting one.

                ALSO! Need to check if obscured by foreground mask (middle of teapot spout)... that ones gonna be challenging.

                */

                // TEMP. And slightly redundant.
                /*if (m.ContainsVert(v))
                {
                    if (m.IsPointInside2D(v + vec * 0.0001f)) n++;
                }*/

                if (CheckEdgeSharesVerts(e)) continue; // <------- this needs to return the specific vert that's shared.

                // If the mask's Edge has no 2D bb collision, skip it.
                if (!BB.CheckCol2D(e.BB)) continue;

                // Check for intersections with the edge. Note that they returned EdgeIntersection is relative to the vec Vector, which
                // is traveling away from whichever VecVert this VecEdge has that has the highest MaskNestCount.
                Vector2 v2D = v.Screen + v1Offset2D;
                Vector3 v3D = v.World + v1Offset3D;

                EdgeIntersection intersection = GetEdge2DIntersection(v2D, vec, e);

                // If intersection with Edge found.
                if (!intersection.Equals(EdgeIntersection.Null))
                {
                    // Check that the this edges intersection is further away than the masks intersection.
                    var i1 = Camera.main.GetDistance(v3D + (Vec3D * intersection.T));
                    var i2 = Camera.main.GetDistance(e.Verts[0].World + e.Vec3D * (intersection.U));
                    //if (!i1) Console.WriteLine(i1);

                    //DebugDraw.DrawSphere(v + (v == Verts[0] ? Vec3D : -Vec3D) * (intersection.T), 0.1f, Color.Green);
                    //DebugDraw.DrawSphere(e.Verts[0] + e.Vec3D * (intersection.U), 0.1f, Color.Blue);

                    // Store it.
                    if (i1 > i2) Intersections.Add(intersection);
                    /*MEngine.Debug.DebugDraw.DrawPoint(
                        intersection.Pos.ToPoint() + (intersection.Intersection == EdgeIntersection.IntersectionType.ENTRY ? new Point(5, 0) : Point.Zero),
                        intersection.Intersection == EdgeIntersection.IntersectionType.ENTRY ? Color.Red : Color.Blue);*/
                }
            }
        }
    }

    public void CalcVecLines()
    {
        // Sort the intersections by T value.
        Intersections.Sort((a, b) => a.T.CompareTo(b.T));
        CalcIntersections(Verts[0], Vec2D);
    }

    private void CalcIntersections(
            VecVert v,
            Vector2 vec
        )
    {
        var nestCount = v.GetMasksInFrontCount();

        var flipped = false; //nestCount > Verts[1].GetMasksInFrontCount();

        // Finally, determine which parts of the edge are visible.
        int n = (nestCount < 0) ? 0 : nestCount;

        List<Vector2> temp = new List<Vector2>();
        if (n == 0) temp.Add(v.Screen);
        foreach (var e in Intersections)
        {
            if (n == 0) temp.Add(e.Pos);
            n += e.Intersection == EdgeIntersection.IntersectionType.ENTRY ? (flipped ? -1 : 1) : (flipped ? 1 : -1); // Change the nesting value.
            if (n == 0) temp.Add(e.Pos);
        }
        if (n == 0) temp.Add(v.Screen + vec);
        //Console.WriteLine(intersections.Count());

        //if (temp.Count() % 2 != 0) Console.WriteLine("A");
        for (var i = 0; i < temp.Count - (temp.Count % 2); i += 2)
        {
            //MEngine.Debug.DebugDraw.DrawLine(temp[i], temp[i + 1], Color.LightBlue);

            vecLineDefs.Add(new VecLineDef(temp[i], temp[i + 1]));


            //MEngine.Debug.DebugDraw.DrawPoint(temp[i].ToPoint(), Color.LightBlue);
            //MEngine.Debug.DebugDraw.DrawPoint(temp[i+1].ToPoint(), Color.LightBlue);
        }
    }

    /// <summary>
    /// Stores the intersection point, and the multi for the distance from P1 of the intersection.
    /// </summary>
    public struct EdgeIntersection
    {
        /// <summary>
        /// Location of the intersection in screen space.
        /// </summary>
        public Vector2 Pos;
        /// <summary>
        /// Value from 0 to 1 that, if the intersectING vector is multiplied by, will yield a vector from P1 to the interection point.
        /// </summary>
        public float U;
        /// <summary>
        /// Value from 0 to 1 that, if the intersectED vector is multiplied by, will yield a vector from P1 to the interection point.
        /// </summary>
        public float T;

        /// <summary>
        /// Is this an intersection of the line going OUT of the VecMask or IN to a VecMask.
        /// </summary>
        public enum IntersectionType
        {
            ENTRY,
            EXIT
        }
        public IntersectionType Intersection;

        #region NullIntersection
        public static EdgeIntersection Null { get; private set; }
        static EdgeIntersection()
        {
            Null = new EdgeIntersection(Vector2.zero, -1f, -1f, IntersectionType.EXIT);
        }
        #endregion

        public EdgeIntersection(Vector2 Intersection, float U, float T, IntersectionType itype)
        {
            this.Pos = Intersection;
            this.U = U;
            this.T = T;
            this.Intersection = itype;
        }
    }

    private EdgeIntersection GetEdge2DIntersection(Vector2 myV0, Vector2 myVec2D, VecEdge other)
    {
        // First off, ensure that this Edge isn't completely in front of the mask Edge.
        // If the maskEdges closest point to the Camera is still smaller than this Edges furthest point, then no masking should occur.
        //if (MaxDepth <= maskEdge.MinDepth) return EdgeIntersection.Null;

        // This edge
        Vector2 p = myV0;
        Vector2 r = myVec2D;

        // mask's edge.
        Vector2 q = other.Verts[0].Screen;
        Vector2 s = other.Vec2D;

        Vector2 q_p = q - p;
        float rXs = r.Cross(s);

        // If the lines are parallel, or in the exact same direction and position, then ignore the intersection.
        if (rXs == 0f) return EdgeIntersection.Null;

        float t = (q_p).Cross(s) / rXs;
        float u = (q_p).Cross(r) / rXs;

        // Segments aren't parallel, but don't intersect.
        if (t < 0f || t > 1f || u < 0f || u > 1f) return EdgeIntersection.Null;

        // Determine if this intersection was the edge entering or exiting the mask.
        var angle = Mathf.Abs(Vector2.SignedAngle(other.Normal2D, vec2DNorm));
        //var sameDirection = other.Normal2D.Cross(vec2DNorm) > 0;
        var sameDirection = angle > 90f;
        var itype = sameDirection ? EdgeIntersection.IntersectionType.ENTRY : EdgeIntersection.IntersectionType.EXIT;

        //var sameDirection = Math.Abs(Math2D.AngleBetweenDirected(other.Normal2D, Vector2.Normalize(myVec2D))) < Math.PI/2f ? false : true;

        /*DebugDraw.DrawText(cross.ToString(), p + r * t);
        DebugDraw.DrawLine(myV0, myV0 + myVec2D*2f, Color.Green);
        DebugDraw.DrawLine(q, q + other.Normal2D*10f, Color.GreenYellow);*/

        // I think I got U and T swapped.

        return new EdgeIntersection(p + r * t, u, t, itype);
    }


    public void DebugDraw()
    {
        // Determine whether to draw based on DrawType.
        bool draw = true;

        /*switch (this.Type)
        {
            case DrawType.Normal:

        }*/

        /*if (!IsVisible)
        {
            EditorDebug.DrawPoint(Verts[0].Screen + (Vec2D*0.2f), Color.cyan);
            EditorDebug.DrawPoint(Verts[0].Screen + (Vec2D*0.4f), Color.cyan);
            EditorDebug.DrawPoint(Verts[0].Screen + (Vec2D*0.6f), Color.cyan);
            EditorDebug.DrawPoint(Verts[0].Screen + (Vec2D*0.8f), Color.cyan);
            return;
        }*/

        //VecLine.DrawVecLine(Verts[0].Screen, Verts[1].Screen, 0.4f);

        if (EditorDebug.DEBUG_ENABLED) { 
            foreach(var i in Intersections)
            {
                EditorDebug.DrawPoint(i.Pos + Vector2.up * 4f, i.Intersection == EdgeIntersection.IntersectionType.ENTRY ? Color.red : Color.blue);
            }
        }
        /*
        EditorDebug.DrawPoint(Verts[0].Screen + Normal2D * 20f, Color.green);
        EditorDebug.DrawPoint(Verts[0].Screen + vec2DNorm * 20f, Color.magenta);*/

        foreach (var l in vecLineDefs)
        {
            VecLine.DrawVecLine(l.P1, l.P2, 0.3f);
        }
    }

    /// <summary>
    /// Creates (and returns) a GameObject with a Mesh containing a single line.
    /// Returns null if the VecEdge is Invisible at the time of calling.
    /// </summary>
    public void CreateExplosionGO(Transform parent)
    {
        if (!this.IsVisible) return;

        GameObject go = new GameObject("VecEdge_ExplosionParticle");

        var _vec3D = Verts[1].World - Verts[0].World;

        go.transform.position = Verts[0].World;
        go.transform.rotation = Quaternion.LookRotation(_vec3D);

        go.transform.localScale = new Vector3(1f, 1f, _vec3D.magnitude);
        go.transform.parent = parent;
        go.transform.parent = null;

        GameObject go2 = new GameObject("VecMeshGO");
        go2.transform.parent = go.transform;
        var vm = go2.AddComponent<VecMesh>();
        vm.MeshData = VecMeshProcessor.One;
        vm.Lifetime = UnityEngine.Random.Range(3f, 7f);

        // Physics colliders
        var cc = go.AddComponent<CapsuleCollider>();
        cc.direction = 2;
        cc.radius = 0.1f;

        go.AddComponent<Rigidbody>();

        
    }
}
