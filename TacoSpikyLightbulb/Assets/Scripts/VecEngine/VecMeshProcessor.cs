using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

/**
 * Converts a formatted .txt into a VecMesh object.
 * 
 * TBA: Have this be a preprocessor function in future.
 */
public static class VecMeshProcessor
{
	/**
	 * Stores processed TextAsset data.
	 */
	public struct VecMeshData
	{
		/// <summary>
		/// Any edge of the mesh (visible/not visible/part of face/not part of face.
		/// </summary>
		public struct Edge
		{
			public short[] VertIdxs;
			public short Type; // 0: never visible | 1: visible only when 1 of its masks is | 2: always visible (if not culled).

			public Edge(short[] VertIndexes, short Type)
			{
				this.VertIdxs = VertIndexes;
				this.Type = Type;
			}

			/// <summary>
			/// Returns an array of Verts by creating an array from the VertIdxs value applied to AllVerts.
			/// </summary>
			/// <param name="AllVerts"></param>
			/// <returns></returns>
			public Vector3[] GetVerts(Vector3[] AllVerts)
			{
				List<Vector3> v = new List<Vector3>();
				foreach (short e in VertIdxs)
					v.Add(AllVerts[e]);

				return v.ToArray();
			}
		}

		/// <summary>
		/// A tri containing references to the indexes of its 3 Edges.
		/// </summary>
		public struct Face
		{
			public short[] VertIdxs;
			public short[] EdgeIdxs;
			public Face(short[] VertIdxs, short[] EdgeIdxs)
			{
				if (VertIdxs.Length != 3) throw new System.Exception("A Face must have exactly 3 Verts");
				this.VertIdxs = VertIdxs;

				if (EdgeIdxs.Length != 3) throw new System.Exception("A Face must have exactly 3 Edges");
				this.EdgeIdxs = EdgeIdxs;
			}
		}

		public Matrix4x4 InitialTransform;
		public string Name;

		public Vector3[] Verts;
		public Edge[] Edges;
		public Face[] Faces;

		public VecMeshData(string Name, Matrix4x4 InitialTransform, Vector3[] Verts, Edge[] Edges, Face[] Faces)
		{
			this.Name = Name;
			this.InitialTransform = InitialTransform;
			this.Verts = Verts;
			this.Edges = Edges;
			this.Faces = Faces;
		}
	}

	public static VecMeshData One { get; private set; } // Single edge drawn from Vector3.zero to Vector3.forward
	static VecMeshProcessor()
	{
		//Null = new EdgeIntersection(Vector2.zero, -1f, -1f, IntersectionType.EXIT);
		One = new VecMeshData(
			"One",
			Matrix4x4.identity,
			new Vector3[] {
				Vector3.zero,
				Vector3.forward
			},
			new VecMeshData.Edge[] {
				new VecMeshData.Edge(new short[]{0, 1}, 2)
			},
			new VecMeshData.Face[] {

			}
		);
	}

	public static VecMeshData[] ProcessTextAsset(TextAsset asset)
	{
		// Store all extracted VecMeshData (which will be used to create a VecModel later).
		List<VecMeshData> meshes = new List<VecMeshData>();

		string[] lines = asset.text.Split('\n');

		///

		List<Vector3>           mVerts = new List<Vector3>();			// ALL verts in the current mesh
		List<VecMeshData.Edge>  mEdges = new List<VecMeshData.Edge>();	// ALL edges in the current mesh
		List<VecMeshData.Face>  mFaces = new List<VecMeshData.Face>();  // ALL faces in the current mesh

		string curMeshName = string.Empty;
		Matrix4x4 curMeshScale = Matrix4x4.identity;
		Matrix4x4 curMeshRotation = Matrix4x4.identity;
		Matrix4x4 curMeshTranslation = Matrix4x4.identity;

		// When called, creates a new mesh using the above values and adds it to meshes.
		Action createMesh = () =>
		{
			meshes.Add(new VecMeshData(
				curMeshName,
				curMeshScale * curMeshRotation * curMeshTranslation,
				mVerts.ToArray(),
				mEdges.ToArray(),
				mFaces.ToArray()
			));

			// Blank the lists for the next mesh (if there is one).
			mVerts.Clear();
			mEdges.Clear();
			mFaces.Clear();
		};

		// Each edge can be used by 0 - 2 Faces. To prevent redundancy, only one Edge is made and shared between them.
		// Only adds an Edge to mEdges if an identical one isn't already in the list.
		// TBA: Have it override Edge Types?
		// <returns>Index of the new/existing Edge</returns>
		Func<VecMeshData.Edge, short> AddEdge = (edgeToAdd) => {

			short i = 0;
			foreach (var e in mEdges)
			{
				if (
					   (e.VertIdxs[0] == edgeToAdd.VertIdxs[0] && e.VertIdxs[1] == edgeToAdd.VertIdxs[1]) ||
					   (e.VertIdxs[0] == edgeToAdd.VertIdxs[1] && e.VertIdxs[1] == edgeToAdd.VertIdxs[0])
					)
				{
					return i; // Identical edge already in List. Do not add redundant edge.
				}

				i++;
			}

			mEdges.Add(edgeToAdd);
			return (short)(mEdges.Count - 1);
		};

		// Process the data line by line.
		foreach (var e in lines)
		{
			if (e.Length == 0) continue;

			switch (e[0])
			{
				/* Starting a new mesh */
				case 'o':

					// Store the previous verts + edges in a Mesh first (if one has been read in).
					if (curMeshName != string.Empty) createMesh();

					curMeshName = e.Substring(2).Trim();
					break;

				// Verts
				case 'v':
					float[] f = SplitStringToFloats(e);
					mVerts.Add(new Vector3(f[0], f[1], f[2]));
					break;

				// Edges with Type == 1 (temp).
				case 'l':
					short[] s = SplitStringToShorts(e);
					AddEdge(new VecMeshData.Edge(s, 2));
					break;

				case 'e':
					short[] sa = SplitStringToShorts(e);
					AddEdge(new VecMeshData.Edge(sa, 1));
					break;

				// Tris (for the masks)
				case 'f':
					short[] s1 = SplitStringToShorts(e);
					short[] indexes = new short[3];

					indexes[0] = AddEdge(new VecMeshData.Edge(new short[] { s1[0], s1[1] }, 1));
					indexes[1] = AddEdge(new VecMeshData.Edge(new short[] { s1[1], s1[2] }, 1));
					indexes[2] = AddEdge(new VecMeshData.Edge(new short[] { s1[2], s1[0] }, 1));

					mFaces.Add(new VecMeshData.Face(s1, indexes));
					break;

				// Translation
				case 't':
					float[] fT = SplitStringToFloats(e);
					curMeshTranslation = Matrix4x4.Translate(new Vector3(fT[0], fT[2], -fT[1]));
					break;

				// Rotation
				case 'r':
					// Change to r
					float[] r = SplitStringToFloats(e);
					Matrix4x4 q = Matrix4x4.Rotate(new Quaternion(r[3], r[0], r[1], r[2]));

					// Swap Y and Z axis.
					// TBA: This is still not quite right, but getting closer.
					//Matrix4x4 m = Matrix4x4.LookAt(Vector3.zero, q., -q.Up);

					/*Vector3 v;
					Quaternion quat;
					m.Decompose(out v, out quat, out v);

					rotation =
						Matrix.CreateFromQuaternion(quat) *
						Matrix.CreateRotationX((float)Math.PI / 2f);*/

					curMeshRotation = q;
					curMeshRotation *= Matrix4x4.Rotate(Quaternion.AngleAxis(-90f, Vector3.right));

					break;

				// Scale
				case 's':
					float[] fS = SplitStringToFloats(e);
					curMeshScale = Matrix4x4.Scale(new Vector3(fS[0], fS[1], fS[2]));
					break;
			}
		}

		// Store the last Mesh.
		createMesh();

		return meshes.ToArray();
	}

	/// <summary>
	/// Extracts all floating point values from a string (make sure no non-floating values are in the string before running).
	/// </summary>
	/// <param name="str"></param>
	/// <returns></returns>
	private static float[] SplitStringToFloats(string str)
	{
		List<float> f = new List<float>();

		string[] s = str.Split(' ');
		for (int i = 1; i < s.Length; i++)
			f.Add(float.Parse(s[i]));

		return f.ToArray();
	}

	/// <summary>
	/// Extracts all short values from a string (make sure no non-short values are in the string before running).
	/// </summary>
	/// <param name="str"></param>
	/// <returns></returns>
	private static short[] SplitStringToShorts(string str)
	{
		List<short> sh = new List<short>();

		string[] s = str.Split(' ');
		for (int i = 1; i < s.Length; i++)
			sh.Add(short.Parse(s[i]));

		return sh.ToArray();
	}
}
