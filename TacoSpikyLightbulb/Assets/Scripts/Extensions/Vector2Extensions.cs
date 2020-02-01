using UnityEngine;

public static class Vector2Extensions
{
    public static float Cross(this Vector2 v, Vector2 other)
    {
        return v.x * other.y - v.y * other.x;
    }
}
