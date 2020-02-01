using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Works similarly to a BoundingBox3D, however, also offers the option to check if a BB is behind another (as in, colliding in x and y, but not z).
/// </summary>
public class VecBoundingBox
{
    private Vector3 minCorner;
    private Vector3 maxCorner;
    public Vector3 MinCorner { get { return minCorner; } }
    public Vector3 MaxCorner { get { return maxCorner; } }

    public VecBoundingBox()
    {

    }

    public VecBoundingBox(Vector3[] p)
    {
        Recalculate(p);
    }

    public float Width
    {
        get
        {
            return maxCorner.x - minCorner.x;
        }
    }

    public float Height
    {
        get
        {
            return maxCorner.y - minCorner.y;
        }
    }

    public float Depth
    {
        get
        {
            return maxCorner.z - minCorner.z;
        }
    }

    /// <summary>
    /// Legacy. Do not use for VecEngine calculations.
    /// </summary>
    /// <param name="p"></param>
    public void Recalculate(Vector3[] p)
    {
        if (p.Length == 0) throw new System.Exception("ERROR: Cannot recalculate BoundingBox2D with a 0 length Vector2 array.");

        if (p.Length == 1)
        {
            minCorner = p[0];
            maxCorner = p[0];
        }
        else
        {
            minCorner = Vector3.one * float.PositiveInfinity;
            maxCorner = -Vector3.one * float.PositiveInfinity;

            foreach (Vector3 e in p)
            {
                if (e.x < minCorner.x) minCorner.x = e.x;
                if (e.x > maxCorner.x) maxCorner.x = e.x;

                if (e.y < minCorner.y) minCorner.y = e.y;
                if (e.y > maxCorner.y) maxCorner.y = e.y;

                if (e.z < minCorner.z) minCorner.z = e.z;
                if (e.z > maxCorner.z) maxCorner.z = e.z;
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="vecVert"></param>
    public void Recalculate(ICollection<VecVert> vvs)
    {
        minCorner = Vector3.one * float.PositiveInfinity;
        maxCorner = -Vector3.one * float.PositiveInfinity;

        foreach (var v in vvs)
        {
            if (v.Screen.x < minCorner.x) minCorner.x = v.Screen.x;
            if (v.Screen.x > maxCorner.x) maxCorner.x = v.Screen.x;

            if (v.Screen.y < minCorner.y) minCorner.y = v.Screen.y;
            if (v.Screen.y > maxCorner.y) maxCorner.y = v.Screen.y;

            if (v.Depth < minCorner.z) minCorner.z = v.Depth;
            if (v.Depth > maxCorner.z) maxCorner.z = v.Depth;
        }
    }

    /// <summary>
    /// Bad implementation. Will do for now. Should use Interfaces.
    /// </summary>
    /// <param name="vvs"></param>
    public void Recalculate(List<VecMask> vms)
    {
        if (vms.Count == 1)
        {
            minCorner = vms[0].BB.minCorner;
            maxCorner = vms[0].BB.maxCorner;
        }
        else
        {
            minCorner = Vector3.one * float.PositiveInfinity;
            maxCorner = -Vector3.one * float.PositiveInfinity;

            foreach (var v in vms)
            {
                if (v.BB.minCorner.x < minCorner.x) minCorner.x = v.BB.minCorner.x;
                if (v.BB.maxCorner.x > maxCorner.x) maxCorner.x = v.BB.maxCorner.x;

                if (v.BB.minCorner.y < minCorner.y) minCorner.y = v.BB.minCorner.y;
                if (v.BB.maxCorner.y > maxCorner.y) maxCorner.y = v.BB.maxCorner.y;

                if (v.BB.minCorner.z < minCorner.z) minCorner.z = v.BB.minCorner.z;
                if (v.BB.maxCorner.z > maxCorner.z) maxCorner.z = v.BB.maxCorner.z;
            }
        }
    }

    public bool CheckCol(VecBoundingBox other)
    {
        if (minCorner.x > other.maxCorner.x) return false;
        if (maxCorner.x < other.minCorner.x) return false;

        if (minCorner.y > other.maxCorner.y) return false;
        if (maxCorner.y < other.minCorner.y) return false;

        if (minCorner.z > other.maxCorner.z) return false;
        if (maxCorner.z < other.minCorner.z) return false;

        return true;
    }

    public bool CheckCol(Vector3 pos)
    {
        if (minCorner.x > pos.x) return false;
        if (maxCorner.x < pos.x) return false;

        if (minCorner.y > pos.y) return false;
        if (maxCorner.y < pos.y) return false;

        if (minCorner.z > pos.z) return false;
        if (maxCorner.z < pos.z) return false;

        return true;
    }

    /// <summary>
    /// Returns true if screenPos is inside the BB's X & Y, and if the screenPos's Z value in or behind the BB's.
    /// </summary>
    /// <param name="screenPos"></param>
    /// <returns></returns>
    public bool CheckColBehind(Vector2 screenPos, float depth)
    {
        if (minCorner.x > screenPos.x) return false;
        if (maxCorner.x < screenPos.x) return false;

        if (minCorner.y > screenPos.y) return false;
        if (maxCorner.y < screenPos.y) return false;

        if (minCorner.z > depth) return false;

        return true;
    }

    /// <summary>
    /// Returns true if "other" is behind this BB.
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public bool CheckColBehind(VecBoundingBox other)
    {
        if (minCorner.x > other.maxCorner.x) return false;
        if (maxCorner.x < other.minCorner.x) return false;

        if (minCorner.y > other.maxCorner.y) return false;
        if (maxCorner.y < other.minCorner.y) return false;

        if (minCorner.z > other.maxCorner.z) return false;
        
        return true;
    }

    public bool CheckCol2D(VecBoundingBox other)
    {
        if (minCorner.x > other.maxCorner.x) return false;
        if (maxCorner.x < other.minCorner.x) return false;

        if (minCorner.y > other.maxCorner.y) return false;
        if (maxCorner.y < other.minCorner.y) return false;

        return true;
    }

    public bool CheckCol2D(Vector2 screenPos)
    {
        if (minCorner.x > screenPos.x) return false;
        if (maxCorner.x < screenPos.x) return false;

        if (minCorner.y > screenPos.y) return false;
        if (maxCorner.y < screenPos.y) return false;

        return true;
    }

    public void DrawDebug()
    {
        EditorDebug.DrawRect(new Rect(minCorner, maxCorner - minCorner), Color.green);
    }
}
