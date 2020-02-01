using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Math3D
{
    /// <summary>
    /// Calculates and returns a normal based on 3 verts.
    /// </summary>
    /// <param name="v1"></param>
    /// <param name="v2"></param>
    /// <param name="v3"></param>
    /// <returns></returns>
    public static Vector3 GetNormal(Vector3 v1, Vector3 v2, Vector3 v3)
    {
        Vector3 dir = Vector3.Cross(v2 - v1, v3 - v1);
        return Vector3.Normalize(dir);
    }
    public static Vector3 GetNormal(Vector3[] v)
    {
        return GetNormal(v[0], v[1], v[2]);
    }

    #region Extensions

    #endregion
}
