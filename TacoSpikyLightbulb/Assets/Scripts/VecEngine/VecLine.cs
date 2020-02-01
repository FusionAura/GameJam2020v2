using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VecLine
{
	private const float LINE_WIDTH = 64f;							// Width of the line when it's width modifier is 1 (should be the same as the line sprite).
	private static readonly Vector2 LINE_TEX_P1_OFFSET;				// Offset of P1 in the line texture.
	private static Rect lineRectScreen;
	private static Rect lineRectSource;
	
	private List<Vector3> vertices;
	private static Texture2D LineTex;
	private static Material LineMat;

	static VecLine()
	{
		LINE_TEX_P1_OFFSET = new Vector2(-59f, -LINE_WIDTH / 2f);

		// Handles line width and length.
		lineRectScreen = new Rect(
			0,
			0,
			LINE_WIDTH,     // Length
			LINE_WIDTH      // Width
		);

		// UV mapping
		lineRectSource = new Rect(Vector2.zero, Vector2.one);

		LineTex = Resources.Load<Texture2D>("Textures/tex_line");
		LineMat = Resources.Load<Material>("Textures/mat_line");
	}

	/*private void ExtractMeshData()
	{
		vertices = new List<Vector3>();

		var mesh = GetComponent<MeshFilter>().mesh;
		
		mesh.GetVertices(vertices);

		Debug.Log("Vertices.Count: " + vertices.Count);
	}*/

	// Update is called once per frame
	void Update()
	{
		
	}

	public static void DrawVecLine(Vector2 p1, Vector2 p2, float lineWidth = 1f)
	{
		// Determine how long the line really is vs. how long the texture needs to be to line up its points on p1 and p2.
		// The line texture has a glow around the actual endpoints, so extra padding needs to be added to offset that glow.
		Vector2 vec = p2 - p1;
		vec /= lineWidth; // Modify the length if the line's width is modified.

		float vecMag = vec.magnitude;

		float texWidth = vecMag + -LINE_TEX_P1_OFFSET.x * 2f;

		// Draw the vecline texture so that its P1 offset lands on p1.
		Vector2 topLeft = p1 + LINE_TEX_P1_OFFSET;

		// Modify the screen rect.
		lineRectScreen.width = texWidth;
		lineRectScreen.x = topLeft.x;
		lineRectScreen.y = topLeft.y;


		// Transform the line.
		Matrix4x4 originalMat = GUI.matrix;

		if (lineWidth != 1f) GUIUtility.ScaleAroundPivot(Vector2.one * lineWidth, p1);

		// Get rotation.
		float angle = Vector2.SignedAngle(Vector2.right, vec);

		GUIUtility.RotateAroundPivot(angle, p1);

		//Graphics.

		// Draw it.
		// Vertex colouring doesn't seem to be working...
		// Note: could be made more efficient (pretty sure this is using 9 quads to draw the line, when 3 would be enough).
		Graphics.DrawTexture(
				lineRectScreen,
				LineTex,
				lineRectSource,
				63,
				63,
				0,
				0,
				Color.white,
				LineMat);

		GUI.matrix = originalMat;
	}

	public static void DrawVecLine(Vector3 p1, Vector3 p2, float lineWidth = 1f)
	{
		Vector2 p1Screen = Camera.main.WorldToScreenPoint(p1);
		Vector2 p2Screen = Camera.main.WorldToScreenPoint(p2);

		DrawVecLine(p1Screen, p2Screen, lineWidth);
	}

	/*void OnGUI()
	{
		foreach(var vert in vertices)
		{
			Vector3 transformedVert = transform.TransformPoint(vert);
			Vector2 screenVert = Camera.main.WorldToScreenPoint(transformedVert);

			DrawVecLine(screenVert, screenVert + Vector2.right * 100f, 2f);

			break;
		}
	}*/
}
