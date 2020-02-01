using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VecVert
{
	#region Fields
	private readonly VecMesh parent; // Hopefully each vert having a reference to its parent won't be too wasteful.

	private Vector3 world;      // Real position of the vert in the world
	private Vector3 local;      // Position of the vert relative to its parent.
	private Vector2 screen;     // Projected position of the vert on the screen
	private float depth;        // Distance from Camera

	private VecEdge[] edges;		// Edges this vert belongs to
	private List<VecMask> masks;    // Reference to the VecMasks that this vert is a part of.

	private HashSet<VecMask> masksInFront;	// How many masks are in front of this vert. Stores only unique entries.
	#endregion

	#region Properties
	public Vector2 Screen { get { return screen; } }
	public Vector3 World { get { return world; } }
	public bool IsVisible { get; private set; }		// Whether this vert belongs to ANY edge that is currently visible.
	/// <summary>
	/// Distance of the VecVert from the mask.
	/// </summary>
	public float Depth { get { return depth; } }

	public List<VecMask> Masks { get { return masks; } }
	#endregion

	public VecVert(Vector3 local, VecMesh parent)
    {
        this.local = local;
		this.parent = parent;

		masks = new List<VecMask>();
		masksInFront = new HashSet<VecMask>();

	}

	/// <summary>
	/// After transforms have been applied, do this.
	/// </summary>
	public void LateUpdate()
	{
		RecalculateWorldPos();
		RecalculateScreenPos();
	}

	public void RegisterVecMaskParent(VecMask mask)
	{
		masks.Add(mask);
	}

	private void RecalculateWorldPos()
	{
		world = parent.transform.TransformPoint(local);
	}

	/// <summary>
	/// Calculate the vert's position on the Screen once per frame.
	/// </summary>
	private void RecalculateScreenPos()
	{
		masksInFront.Clear();
		IsVisible = false; // If any VecEdge IsVisible, it will set its VecVert to visible.

		screen = Camera.main.WorldToScreenPoint(world);
		// Flip the Y value so that the Screen pos lines up with the World pos.
		screen.y = (screen.y * -1f) + UnityEngine.Screen.height; // Flip the points.

		// Calculate distance from Camera.
		depth = Camera.main.GetDistance(world);
	}

	public void SetToVisible()
	{
		IsVisible = true;
	}

	/*public bool IsVertBehindMask(VecMask mask)
	{
		// A vert can never be inside of a mask it itself creates, so don't check them.
		if (masks.Contains(mask)) return false; // VERY SLOW! May just be faster to check the masks and then subtract them afterwards.

		// BoundingBox check
		if (!mask.BB.CheckColBehind(screen, depth)) return false; // Mask's BB is completely behind this vert. Don't check it.

		// Check if the vert is behind the mask.
		if (!mask.IsPointBehindMaskPlane(world)) return false;

		// The vert is behind the mask. Now the triangle check can be performed.
		if (!mask.IsPointInside2D(screen)) return false;
		
		return true;
	}*/

	public bool CheckMaskIntersection(VecMask m, Vector2 offset2D, Vector3 offset3D)
	{
		Vector2 s = Screen;
		Vector3 w = World;

		//if (masks.Contains(m))
		{
			//return false;
			s += offset2D;
			w += offset3D;
		}

		// BoundingBox check
		if (!m.BB.CheckCol2D(s)) return false; // Mask's BB isn't covering this vert. Don't check it.

		// Check if the vert is behind the mask.
		if (!m.IsPointBehindMaskPlane(w)) return false;

		// The vert is behind the mask. Now the triangle check can be performed.
		if (!m.IsPointInside2D(s)) return false;

		return true;
	}

	public void AddMaskInFront(VecMask mask)
	{
		if (!masksInFront.Contains(mask))
			masksInFront.Add(mask);
	}

    public Vector3 TempGetPLocal()
    {
        return screen;
    }

	public Vector3 TempGetPWorld()
	{
		return world;
	}

	public int GetMasksInFrontCount()
	{
		return masksInFront.Count;
	}

	public void DrawDebug()
	{
		var mifc = GetMasksInFrontCount();
		if (mifc >= 0)
		EditorDebug.DrawText(screen, mifc.ToString());

		if (!IsVisible)
			EditorDebug.DrawPoint(Screen + Vector2.down * 20f, Color.magenta);
	}
}
