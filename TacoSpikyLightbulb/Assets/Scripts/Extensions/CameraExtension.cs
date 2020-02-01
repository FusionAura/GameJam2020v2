using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CameraExtension
{
    /// <summary>
    /// Returns the distance of the pos from the Camera.
    /// </summary>
    /// <param name="camera"></param>
    /// <param name="pos"></param>
    /// <returns></returns>
    public static float GetDistance(this Camera camera, Vector3 pos)
    {
        Vector3 heading = pos - camera.transform.position;
        return Vector3.Dot(heading, camera.transform.forward);
    }
}
