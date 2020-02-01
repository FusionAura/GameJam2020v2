using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Draw calls to be used exclusively in the editor.
/// </summary>
public static class EditorDebug
{
    public static bool DEBUG_ENABLED = false;

    private static readonly Texture2D backgroundTexture = Texture2D.whiteTexture;
    private static readonly GUIStyle textureStyle = new GUIStyle { normal = new GUIStyleState { background = backgroundTexture } };

    private const float POINT_SIZE = 10f;

    public static void DrawFilledRect(Rect rect, Color color, GUIContent content = null)
    {
        var backgroundColor = GUI.backgroundColor;
        GUI.backgroundColor = color;
        GUI.Box(rect, content ?? GUIContent.none, textureStyle);
        GUI.backgroundColor = backgroundColor;
    }

    public static void DrawRect(Rect rect, Color color)
    {
        UnityEditor.Handles.DrawSolidRectangleWithOutline(rect, Color.clear, color);
    }

    public static void DrawText(Vector2 pos, string text)
    {
        var backgroundColor = GUI.backgroundColor;
        var contentColor = GUI.contentColor;
        GUI.backgroundColor = Color.clear;

        GUI.contentColor = Color.black;
        GUI.Label(new Rect(pos - Vector2.one * 7f + Vector2.left, Vector2.one * 20f), text);
        GUI.Label(new Rect(pos - Vector2.one * 7f + Vector2.right, Vector2.one * 20f), text);
        GUI.Label(new Rect(pos - Vector2.one * 7f + Vector2.up, Vector2.one * 20f), text);
        GUI.Label(new Rect(pos - Vector2.one * 7f + Vector2.down, Vector2.one * 20f), text);

        GUI.contentColor = Color.yellow;
        GUI.Label(new Rect(pos - Vector2.one * 7f, Vector2.one * 20f), text);
        
        GUI.contentColor = contentColor;
        GUI.backgroundColor = backgroundColor;
    }

    public static void DrawPoint(Vector2 position, Color color)
    {
        DrawFilledRect(new Rect(position - Vector2.one * POINT_SIZE / 2f, Vector2.one * POINT_SIZE / 2f), color);
    }

    public static void DrawPoint(Vector2 position)
    {
        DrawPoint(position, Color.red);
    }
}
