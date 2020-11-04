using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace Core.Terrain
{
	public class TerrainGeneratorComponent : MonoBehaviour
	{
		// Start is called before the first frame update
		void Start()
		{

		}

		// Update is called once per frame
		void Update()
		{

		}

	}

	public class TerrainGenerator
	{
		/// <summary>
		/// V1 of Terrain generation
		/// </summary>
		/// <param name="texture">texture to be used (this is converted to grayscale)</param>
		/// <param name="maxHeight">max height of terrain (represented by 1f in gray scale or 255 (white) in color)</param>
		/// <returns></returns>
		public static Mesh GenerateTerrain(int2 size,float[] heights, float maxHeight)
		{
			// Credit for this function goes to cjdev  Aug 21, 2015 at 07:06 AM 
			// source: https://answers.unity.com/questions/1033085/heightmap-to-mesh.html

			List<Vector3> verts = new List<Vector3>();
			List<int> tris = new List<int>();
			int width = size.x;
			int height = size.y;
			for (int i = 0; i < width; i++)
			{
				for (int j = 0; j < height; j++)
				{
					// converts all textures to grayscale
					//Add each new vertex in the plane
					verts.Add(new Vector3(i, heights[(i*j)+j], j));
					//Skip if a new square on the plane hasn't been formed
					if (i == 0 || j == 0) continue;

					// to prevent tearing in triangles we will use only the width
					// See cjdev's example for more info

					tris.Add(width * i + j); //Top right
					tris.Add(width * i + j - 1); //Bottom right
					tris.Add(width * (i - 1) + j - 1); //Bottom left - First triangle
					tris.Add(width * (i - 1) + j - 1); //Bottom left 
					tris.Add(width * (i - 1) + j); //Top left
					tris.Add(width * i + j); //Top right - Second triangle
				}
			}
			Vector2[] uvs = new Vector2[verts.Count];
			for (var i = 0; i < uvs.Length; i++) //Give UV coords X,Z world coords
				uvs[i] = new Vector2(verts[i].x, verts[i].z);
			//	GameObject plane = new GameObject("ProcPlane"); //Create GO and add necessary components
			//	plane.AddComponent<MeshFilter>();
			//	plane.AddComponent<MeshRenderer>();
			Mesh mesh = new Mesh();
			mesh.vertices = verts.ToArray(); //Assign verts, uvs, and tris to the mesh
			mesh.uv = uvs;
			mesh.triangles = tris.ToArray();
			mesh.RecalculateNormals(); //Determines which way the triangles are facing
									   //	plane.GetComponent<MeshFilter>().mesh = procMesh; //Assign Mesh object to MeshFilter
			return mesh;
		}
		/// <summary>
		/// V1 of Terrain generation
		/// </summary>
		/// <param name="texture">texture to be used (this is converted to grayscale)</param>
		/// <param name="maxHeight">max height of terrain (represented by 1f in gray scale or 255 (white) in color)</param>
		/// <returns></returns>
		public static Mesh GenerateTerrain(Texture2D texture, float maxHeight)
		{
			// Credit for this function goes to cjdev  Aug 21, 2015 at 07:06 AM 
			// source: https://answers.unity.com/questions/1033085/heightmap-to-mesh.html

			List<Vector3> verts = new List<Vector3>();
			List<int> tris = new List<int>();
			int width = texture.width;
			int height = texture.height;
			for (int i = 0; i < width; i++)
			{
				for (int j = 0; j < height; j++)
				{
					// converts all textures to grayscale
					//Add each new vertex in the plane
					verts.Add(new Vector3(i, texture.GetPixel(i, j).grayscale * maxHeight, j));
					//Skip if a new square on the plane hasn't been formed
					if (i == 0 || j == 0) continue;

					// to prevent tearing in triangles we will use only the width
					// See cjdev's example for more info

					tris.Add(width * i + j); //Top right
					tris.Add(width * i + j - 1); //Bottom right
					tris.Add(width * (i - 1) + j - 1); //Bottom left - First triangle
					tris.Add(width * (i - 1) + j - 1); //Bottom left 
					tris.Add(width * (i - 1) + j); //Top left
					tris.Add(width * i + j); //Top right - Second triangle
				}
			}
			Vector2[] uvs = new Vector2[verts.Count];
			for (var i = 0; i < uvs.Length; i++) //Give UV coords X,Z world coords
				uvs[i] = new Vector2(verts[i].x, verts[i].z);
			//	GameObject plane = new GameObject("ProcPlane"); //Create GO and add necessary components
			//	plane.AddComponent<MeshFilter>();
			//	plane.AddComponent<MeshRenderer>();
			Mesh mesh = new Mesh();
			mesh.vertices = verts.ToArray(); //Assign verts, uvs, and tris to the mesh
			mesh.uv = uvs;
			mesh.triangles = tris.ToArray();
			mesh.RecalculateNormals(); //Determines which way the triangles are facing
									   //	plane.GetComponent<MeshFilter>().mesh = procMesh; //Assign Mesh object to MeshFilter
			return mesh;
		}
	}
}