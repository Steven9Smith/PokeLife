using UnityEngine;
using System.Collections;
using Unity.Mathematics;
using Unity.Entities;
using Core.Common.Buffers;
using Core.CropSection;

namespace Core.Noise
{
	public class NoiseGeneratorComponent : MonoBehaviour
	{

		// Use this for initialization
		void Start()
		{

		}

		// Update is called once per frame
		void Update()
		{

		}
	}
	public class NoiseProfile
	{
		/// <summary>
		/// Noise Profile Mode
		/// </summary>
		public enum NoiseProfileMode
		{
			Texture,
			Map,
			Disabled
		}
		/// <summary>
		/// returns the index of the noise profile that has UseAsMainTexture set to true.
		/// if none are found then it returns -1.
		/// </summary>
		/// <returns></returns>
	/*	public static int GetIndexOfNoiseProfileWithUseAsMainTexture(List<NoiseProfileOptions> noiseProfileOptions)
		{
			for (int i = 0; i < noiseProfileOptions.Count; i++)
				if (noiseProfileOptions[i].useAsMainTexture)
					return i;
			return -1;
		}
		/// <summary>
		 /// returns the number of enabled noise profiles
		 /// </summary>
		 /// <returns></returns>
		public static int NumberOfEnabledNoiseProfiles(List<NoiseProfileOptions> noiseProfileOptions)
		{
			int a = 0;
			for (int i = 0; i < noiseProfileOptions.Count; i++)
				if (noiseProfileOptions[i].profileMode == NoiseProfileMode.Texture || noiseProfileOptions[i].profileMode == NoiseProfileMode.Map)
					a++;
			return a;
		}*/
	}

	[System.Serializable]
	public struct NoiseValueInterpertation
	{
		public bool3 ColliderInterperation1D;
		public bool3 ColliderInterperation2D;
		public bool3 ColliderInterperation3D;

		public bool Equals(NoiseValueInterpertation other) 
		{
			return ColliderInterperation1D.Equals(other.ColliderInterperation1D) &&
				 ColliderInterperation2D.Equals(other.ColliderInterperation2D) &&
				  ColliderInterperation3D.Equals(other.ColliderInterperation3D);
		}

		public override bool Equals(object obj)
		{
			return Equals((NoiseValueInterpertation)obj);
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
	}

	/// <summary>
	/// This class hold functions used in one or more classes
	/// </summary>
	public static class NoiseClass
	{
		/*
		 Picking the gradients

			For the noise function to be repeatable, i.e. always yield the same value for a given 
			input point, gradients need to be pseudo-random, not truly random. They need to have 
			enough variation to conceal the fact that the function is not truly random, but too 
			much variation will cause unpre-dictable behaviour for the noise function. A good 
			choice for 2D and higher is to pick gradients of unit length but different directions.
			For 2D, 8 or 16 gradients distributed around the unit circle is a good choice. For 3D,
			Ken Perlin’s recommended set of gradients is the midpoints of each of the 12 edges of
			a cube centered on the origin.

		*/

		public static float3 InvalidNoise => new float3(-2, 0, 0);
		/// <summary>
		/// Visual interpertation of the Noise Profile
		/// </summary>
		public enum VisualInterpertation
		{
			// A 2D Texture 
			Texture,
			// A 3D Shape
			Shape3D,
			// A 3D Terrain
			Terrain
		}
		/// <summary>
		/// Type of Noise
		/// </summary>
		public enum NoiseType
		{
			// Cellular noise, returning F1 and F2 in a float2.
			// Standard 3x3 search window for good F1 and F2 values
			CellularNoise,
			// Cellular noise, returning F1 and F2 in a float2.
			// Speeded up by umath.sing 2x2 search window instead of 3x3,
			// at the expense of some strong pattern artifacts.
			// F2 is often wrong and has sharp discontinuities.
			// If you need a smooth F2, use the slower 3x3 version.
			// F1 is sometimes wrong, too, but OK for most purposes.
			// TL;DR - Faster at the cost of accuracy and artifacts
			CellularNoise2x2,
			// Cellular noise, returning F1 and F2 in a float2.
			// Speeded up by umath.sing 2x2x2 search window instead of 3x3x3,
			// at the expense of some pattern artifacts.
			// F2 is often wrong and has sharp discontinuities.
			// If you need a good F2, use the slower 3x3x3 version.
			CellularNoise2x2x2,
			// Cellular noise, returning F1 and F2 in a float2.
			// 3x3x3 search region for good F2 everywhere, but a lot
			// slower than the 2x2x2 version.
			// The code below is a bit scary even to its author,
			// but it has at least half decent performance on a
			// math.modern GPU. In any case, it beats any software
			// implementation of Worley noise hands down.
			CellularNoise3x3x3,
			// Classic Perlin noise
			ClassicPerlinNoise,
			// Classic Perlin noise
			ClassicPerlinNoise3x3x3,
			// Classic Perlin noise
			ClassicPerlinNoise4x4x4x4,

			// Array and textureless GLSL 2D simplex noise function.
			SimplexNoise,
			// Array and textureless GLSL 2D/3D/4D simplex noise functions
			SimplexNoise3x3x3,
			// Array and textureless GLSL 2D/3D/4D simplex noise functions
			SimplexNoise4x4x4x4,
			// Array and textureless GLSL 2D/3D/4D simplex noise functions
			//	SimplexNoiseGradient,
			//
			// 2-D tiling simplex noise with rotating gradients and analytical derivative.
			// The first component of the 3-element return vector is the noise value,
			// and the second and third components are the x and y partial derivatives.
			//
			//	RotatingNoise,
			// Assumming pnoise is perlin noise
			PerlinNoise,
			PerlinNoise3x3x3,
			PerlinNoise4x4x4x4,

			SRNoise,
			SRDNoise,
			SRDNoise2D,
			SRNoise2D,
			SimplexNoise3x3x3Gradient,

			// Used when noise is merged with other using arithmetic
			Mixed

		}

		/// <summary>
		/// Returns the value of a NoiseType as a String
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public static string Type2String(NoiseType type)
		{
			switch (type)
			{
				case NoiseType.CellularNoise:
					return "Cellular";
				case NoiseType.CellularNoise2x2:
					return "Cellular2x2";
				case NoiseType.CellularNoise2x2x2:
					return "Cellular2x2x2";
				case NoiseType.CellularNoise3x3x3:
					return "Cellular3x3x3";
				case NoiseType.ClassicPerlinNoise:
					return "ClassicPerlinNoise";
				case NoiseType.ClassicPerlinNoise3x3x3:
					return "ClassicPerlinNoise3x3x3";
				case NoiseType.ClassicPerlinNoise4x4x4x4:
					return "ClassicPerlinNoise4x4x4x4";
				case NoiseType.PerlinNoise:
					return "PerlinNoise";
				case NoiseType.PerlinNoise3x3x3:
					return "PerlinNoise3x3x3";
				case NoiseType.PerlinNoise4x4x4x4:
					return "PerlinNOise4x4x4x4";
				case NoiseType.SimplexNoise:
					return "SimplexNoise";
				case NoiseType.SimplexNoise3x3x3:
					return "SimplexNoise3x3x3";
				case NoiseType.SimplexNoise4x4x4x4:
					return "SimplexNoise4x4x4x4";
				case NoiseType.SRDNoise2D:
					return "SRDNoise2D";
				case NoiseType.SRNoise:
					return "SRNoise";
				case NoiseType.SRDNoise:
					return "SRDNoise";
				case NoiseType.SRNoise2D:
					return "SRNoise2D";
				case NoiseType.SimplexNoise3x3x3Gradient:
					return "SimplexNoise3x3x3Gradient";
				case NoiseType.Mixed:
					return "Mixed";
				default:
					return "UNKNOWN_TYPE";
			}
		}

		/// <summary>
		/// Handles the custom colors of heights
		/// </summary>
		/// <param name="currentColor">current color your algorithm determined</param>
		/// <param name="customColor"></param>
		/// <returns></returns>
		private static Color HandleCustomColor(Color currentColor, CustomColorAttributes customColor)
		{
			currentColor = new Color(
				Mathf.Lerp(0, 1, Mathf.InverseLerp(-1, 1, currentColor.r)),
				Mathf.Lerp(0, 1, Mathf.InverseLerp(-1, 1, currentColor.g)),
				Mathf.Lerp(0, 1, Mathf.InverseLerp(-1, 1, currentColor.b))
				);
			if (customColor.Enabled.x && !customColor.Enabled.y)
			{
				float lowestHeight = 1f;
				Color tmpColor = currentColor;
				for (int i = 0; i < customColor.CustomColor.Length; i++)
				{
					// check if color is enabled
					if (customColor.CustomColor[i].Enable)
					{
						// now we need to see if the values match the height restriction
						if (currentColor.grayscale <= customColor.CustomColor[i].Height)
						{
							if (customColor.CustomColor[i].Height <= lowestHeight)
							{
								tmpColor = new Color(customColor.CustomColor[i].Color.x, customColor.CustomColor[i].Color.y, customColor.CustomColor[i].Color.z);
								lowestHeight = customColor.CustomColor[i].Height;
							}
						}
					}
				}
				return tmpColor;
			}
			else if (customColor.Enabled.x && customColor.Enabled.y)
			{
				float height = currentColor.grayscale;
				Gradient gradient = customColor.Gradient.GetGradient(height, out float newHeight);
				return gradient.Evaluate(newHeight);
			}
			return currentColor;
		}
		//////////////////////////////////////////////////////////////////////
		// Generate Noise (don't forget the return value goes from -1 to 1) //
		//////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Generates a noise based on the given funciton and input/attributes
		/// </summary>
		/// <param name="noiseType">Type of Noise</param>
		/// <param name="input">normal input parameters</param>
		/// <param name="attributes">extra attributes like gradient and rotation</param>
		/// <param name="debug">set this to true to see if a warning appears when you call the function</param>
		/// <returns></returns>
		public static float3 GenerateNoise(NoiseType noiseType, float2 input, float2 attributes, bool debug = false)
		{
			switch (noiseType)
			{
				case NoiseType.PerlinNoise:
					{
						// Classic Perlin noise, periodic variant
						return new float3(noise.pnoise(new float2(input.x, input.y), new float2(attributes.x, attributes.y)), 0, 0);
					}
				case NoiseType.SRNoise2D:
					{
						// 2-D non-tiling simplex noise with rotating gradients,
						// without the analytical derivative.
						return new float3(noise.srnoise(new float2(input.x, input.y), attributes.x), 0, 0);
					}
				case NoiseType.SRNoise:
					{
						return new float3(noise.srnoise(new float2(input.x, input.y)), 0, 0);
					}
				case NoiseType.SRDNoise2D:
					{
						// 2-D non-tiling simplex noise with rotating gradients and analytical derivative.
						// The first component of the 3-element return vector is the noise value,
						// and the second and third components are the x and y partial derivatives.
						return noise.srdnoise(new float2(input.x, input.y), attributes.x);
					}
				case NoiseType.SRDNoise:
					{
						// 2-D non-tiling simplex noise with rotating gradients,
						// without the analytical derivative.
						return noise.srdnoise(new float2(input.x, input.y));
					}
				case NoiseType.ClassicPerlinNoise:
					{
						return new float3(noise.cnoise(new float2(input.x, input.y)), 0, 0);
					}
				case NoiseType.SimplexNoise:
					{
						return new float3(noise.snoise(new float2(input.x, input.y)), 0, 0);
					}
				case NoiseType.CellularNoise2x2:
					{
						return new float3(noise.cellular2x2(new float2(input.x, input.y)), 0);
					}
				case NoiseType.CellularNoise:
					{
						return new float3(noise.cellular(new float2(input.x, input.y)), 0);
					}
				default:
					if (debug)
						Debug.LogWarning("Failed to find matching noise type \"" + noiseType + "\" please use a valid one and try again");
					return InvalidNoise;
			}
		}
		/// <summary>
		/// Generates a noise based on the given funciton and input/attributes
		/// </summary>
		/// <param name="noiseType">Type of Noise</param>
		/// <param name="input">normal input parameters</param>
		/// <param name="attributes">extra attributes like gradient and rotation</param>
		/// <param name="debug">set this to true to see if a warning appears when you call the function</param>
		/// <returns></returns>
		public static float3 GenerateNoise(NoiseType noiseType, float3 input, float3 attributes, bool debug = false)
		{
			switch (noiseType)
			{
				case NoiseType.PerlinNoise3x3x3:
					{
						// Classic Perlin noise, periodic variant
						return new float3(noise.pnoise(new float3(input.x, input.y, input.z), new float3(attributes.x, attributes.y, attributes.z)), 0, 0);
					}
				case NoiseType.ClassicPerlinNoise3x3x3:
					{
						// Classic Perlin noise
						return new float3(noise.cnoise(new float3(input.x, input.y, input.z)), 0, 0);
					}
				case NoiseType.SimplexNoise3x3x3:
					{
						return new float3(noise.snoise(new float3(input.x, input.y, input.z)), 0, 0);
					}
				case NoiseType.SimplexNoise3x3x3Gradient:
					{
						// idk what to do with the gradient
						float3 gradient = new float3();
						return new float3(noise.snoise(new float3(input.x, input.y, input.z), out gradient), 0, 0);
					}
				case NoiseType.CellularNoise2x2x2:
					{
						// Cellular noise, returning F1 and F2 in a float2.
						// Speeded up by umath.sing 2x2x2 search window instead of 3x3x3,
						// at the expense of some pattern artifacts.
						// F2 is often wrong and has sharp discontinuities.
						// If you need a good F2, use the slower 3x3x3 version.
						return new float3(noise.cellular2x2x2(new float3(input.x, input.y, input.z)), 0);
					}
				case NoiseType.CellularNoise3x3x3:
					{
						// Cellular noise, returning F1 and F2 in a float2.
						// 3x3x3 search region for good F2 everywhere, but a lot
						// slower than the 2x2x2 version.
						// The code below is a bit scary even to its author,
						// but it has at least half decent performance on a
						// math.modern GPU. In any case, it beats any software
						// implementation of Worley noise hands down.
						return new float3(noise.cellular(new float3(input.x, input.y, input.z)), 0);
					}
				default:
					if (debug)
						Debug.LogWarning("Failed to find matching noise type \"" + noiseType + "\" please use a valid one and try again");
					return InvalidNoise;


			}

		}
		/// <summary>
		/// Generates a noise based on the given funciton and input/attributes
		/// </summary>
		/// <param name="noiseType">Type of Noise</param>
		/// <param name="input">normal input parameters</param>
		/// <param name="attributes">extra attributes like gradient and rotation</param>
		/// <param name="debug">set this to true to see if a warning appears when you call the function</param>
		/// <returns></returns>
		public static float3 GenerateNoise(NoiseType noiseType, float4 input, bool debug = false)
		{
			switch (noiseType)
			{
				case NoiseType.PerlinNoise4x4x4x4:
					//		Debug.LogWarning("This Scene doesnto currently support 4D inputs because idk what they are");
					return new float3(noise.pnoise(new float4(input.x, input.y, input.z, input.w), input), 0, 0);
				case NoiseType.ClassicPerlinNoise4x4x4x4:
					//		Debug.LogWarning("This Scene doesnto currently support 4D inputs because idk what they are");
					return new float3(noise.cnoise(new float4(input.x, input.y, input.z, input.w)), 0, 0);
				case NoiseType.SimplexNoise4x4x4x4:
					return new float3(noise.snoise(new float4(input.x, input.y, input.z, input.w)), 0, 0);

				default:
					if (debug)
						Debug.LogWarning("Failed to find matching noise type \"" + noiseType + "\" please use a valid one and try again");
					return InvalidNoise;
			}

		}
			//////////////////////////////////
			// Generate Noise within bounds //
			//////////////////////////////////
		/// <summary>
		/// creates an array of noise values to be returned
		/// </summary>
		/// <param name="noiseType">the type of noise</param>
		/// <param name="size">size in 2 dimensions</param>
		/// <param name="attributes">extra attributes to add</param>
		/// <param name="debug">output any warnings or errors</param>
		/// <returns></returns>
		public static float3[] GenerateNoiseWithBounds(NoiseType noiseType, int2 size, float2 attributes, bool debug = false)
		{
			float3[] values = new float3[size.x * size.y];
			for (int i = 0; i < size.x; i++)
				for (int j = 0; j < size.y; j++)
					values[(i * j) + j] = GenerateNoise(noiseType, new float2(i, j), attributes, debug);
			return values;
		}
		/// <summary>
		///	adds noise values to the buffer
		/// </summary>
		/// <param name="noiseType">the type of noise</param>
		/// <param name="size">size in 2 dimensions</param>
		/// <param name="attributes">extra attributes to add</param>
		/// <param name="debug">output any warnings or errors</param>
		public static void GenerateNoiseWithBounds(DynamicBuffer<Float3BufferElement> buffer,NoiseType noiseType, int2 size, float2 attributes, bool debug = false)
		{
			for (int i = 0; i < size.x; i++)
				for (int j = 0; j < size.y; j++)
					buffer.Add(new Float3BufferElement { Value = GenerateNoise(noiseType, new float2(i, j), attributes, debug)});
		}
		/// <summary>
		/// creates an array of noise values to be returned
		/// </summary>
		/// <param name="noiseType">the type of noise</param>
		/// <param name="size">size in 2 dimensions</param>
		/// <param name="attributes">extra attributes to add</param>
		/// <param name="debug">output any warnings or errors</param>
		/// <returns></returns>
		public static float3[] GenerateNoiseWithBounds(NoiseType noiseType, int3 size, float3 attributes, bool debug = false)
		{
			float3[] values = new float3[size.x * size.y];
			for (int i = 0; i < size.x; i++)
				for (int j = 0; j < size.y; j++)
					for(int k = 0; k < size.z;k++)
						values[(i * j) + j] = GenerateNoise(noiseType, new float3(i, j,k), attributes, debug);
			return values;
		}
		/// <summary>
		///	adds noise values to the buffer
		/// </summary>
		/// <param name="noiseType">the type of noise</param>
		/// <param name="size">size in 2 dimensions</param>
		/// <param name="attributes">extra attributes to add</param>
		/// <param name="debug">output any warnings or errors</param>
		public static void GenerateNoiseWithBounds(DynamicBuffer<Float3BufferElement> buffer, NoiseType noiseType, int3 size, float3 attributes, bool debug = false)
		{
			for (int i = 0; i < size.x; i++)
				for (int j = 0; j < size.y; j++)
					for (int k = 0; k < size.z; k++)
						buffer.Add(new Float3BufferElement { Value = GenerateNoise(noiseType, new float3(i, j, k), attributes, debug) });
		}
		/// <summary>
		/// creates an array of noise values to be returned
		/// </summary>
		/// <param name="noiseType">the type of noise</param>
		/// <param name="size">size in 2 dimensions</param>
		/// <param name="attributes">extra attributes to add</param>
		/// <param name="debug">output any warnings or errors</param>
		/// <returns></returns>
		public static float3[] GenerateNoiseWithBounds(NoiseType noiseType, int4 size, bool debug = false)
		{
			float3[] values = new float3[size.x * size.y];
			for (int i = 0; i < size.x; i++)
				for (int j = 0; j < size.y; j++)
					for (int k = 0; k < size.z; k++)
						for(int l = 0; l < size.w;l++)
							values[(i * j) + j] = GenerateNoise(noiseType, new float4(i, j, k,l), debug);
			return values;
		}
		/// <summary>
		///	adds noise values to the buffer
		/// </summary>
		/// <param name="noiseType">the type of noise</param>
		/// <param name="size">size in 2 dimensions</param>
		/// <param name="attributes">extra attributes to add</param>
		/// <param name="debug">output any warnings or errors</param>
		public static void GenerateNoiseWithBounds(DynamicBuffer<Float3BufferElement> buffer, NoiseType noiseType, int4 size,bool debug = false)
		{
			for (int i = 0; i < size.x; i++)
				for (int j = 0; j < size.y; j++)
					for (int k = 0; k < size.z; k++)
						for (int l = 0; l < size.w; l++)
							buffer.Add(new Float3BufferElement { Value = GenerateNoise(noiseType, new float4(i, j, k,l), debug) });
		}
			///////////////////////////////////////
			// Convert Noise To HeightMap Values //
			///////////////////////////////////////
		
		/// <summary>
		/// converts the given noise value into a height value (just a float)
		/// </summary>
		/// <param name="noiseValue">the float3 representation of a noise value</param>
		/// <param name="noiseAttribute">1 = x value, 2 = y value, 3+ = z value</param>
		/// <returns></returns>
		public static float CovertNoiseValueToHeightValue(float3 noiseValue,int noiseAttribute = 0)
		{
			return noiseAttribute == 0 ? math.lerp(0, 1, Mathf.InverseLerp(-1, 1, noiseValue.x)) : noiseAttribute == 1 ? math.lerp(0, 1, Mathf.InverseLerp(-1, 1, noiseValue.y)) : math.lerp(0, 1, Mathf.InverseLerp(-1, 1, noiseValue.z));
		}
		/// <summary>
		/// converts the given noise value into a height value (just a float)
		/// </summary>
		/// <param name="noiseValue">the Float3BufferElement representation of a noise value</param>
		/// <param name="noiseAttribute">1 = x value, 2 = y value, 3+ = z value</param>
		/// <returns></returns>
		public static FloatBufferElement ConvertNoiseBufferElementToHeightBufferElement(Float3BufferElement float3Element,int noiseAttribute = 0)
		{
			return noiseAttribute == 0 ? new FloatBufferElement { Value = float3Element.Value.x } : noiseAttribute == 1 ?
				new FloatBufferElement { Value = float3Element.Value.y } : new FloatBufferElement { Value = float3Element.Value.z };
		}



		public static int ReturnDimension(NoiseClass.NoiseType type)
		{
			switch (type)
			{
				case NoiseClass.NoiseType.SimplexNoise4x4x4x4:
				case NoiseClass.NoiseType.PerlinNoise3x3x3:
				case NoiseClass.NoiseType.ClassicPerlinNoise3x3x3:
				case NoiseClass.NoiseType.SimplexNoise3x3x3:
				case NoiseClass.NoiseType.SimplexNoise3x3x3Gradient:
				case NoiseClass.NoiseType.PerlinNoise:
				case NoiseClass.NoiseType.SRNoise2D:
				case NoiseClass.NoiseType.SRNoise:
				case NoiseClass.NoiseType.ClassicPerlinNoise:
				case NoiseClass.NoiseType.SimplexNoise:
				case NoiseClass.NoiseType.ClassicPerlinNoise4x4x4x4:
				case NoiseClass.NoiseType.PerlinNoise4x4x4x4:
					return 1;
				case NoiseClass.NoiseType.CellularNoise2x2x2:
				case NoiseClass.NoiseType.CellularNoise3x3x3:
				case NoiseClass.NoiseType.CellularNoise:
				case NoiseClass.NoiseType.CellularNoise2x2:
					return 2;
				case NoiseClass.NoiseType.SRDNoise:
				case NoiseClass.NoiseType.SRDNoise2D:
					return 3;

			}
			return 0;
		}
		// Old Functions

	/// <summary>
		/// returns a copy of the given Texture2D
		/// </summary>
		/// <param name="source">source Texture2D</param>
		/// <returns></returns>
		public static Texture2D CopyTexture2D(Texture2D source)
		{
			Texture2D texture = new Texture2D(source.width, source.height);
			for (int i = 0; i < source.width; i++)
				for (int j = 0; j < source.height; j++)
					texture.SetPixel(i, j, source.GetPixel(i, j));
			texture.Apply();
			return texture;
		}
		/// <summary>
		/// Creates a texture with the given color
		/// </summary>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <param name="color"></param>
		/// <returns></returns>
		public static Texture2D GenerateOneColorTexture2D(int width, int height, Color color)
		{
			Texture2D texture = new Texture2D(width, height);
			for (int i = 0; i < width; i++)
				for (int j = 0; j < height; j++)
					texture.SetPixel(i, j, color);
			texture.Apply();
			return texture;
		}

		/// <summary>
		/// Converts a float3[][] into a Texture2D
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public static Texture2D float3_2DToTexture2D(float3[][] input)
		{
			Texture2D texture = new Texture2D(input.Length, input[0].Length);
			for (int i = 0; i < input.Length; i++)
			{
				//	Debug.Log("\"" + input[i][0].x + "," + input[i][0].y + "," + input[i][0].z + "\"");
				for (int j = 0; j < input[i].Length; j++)
				{
					texture.SetPixel(i, j, new Color(input[i][j].x, input[i][j].y, input[i][j].z));
				}
			}
			texture.Apply();
			return texture;
		}
		/// <summary>
		/// Converts a Texture2D into a float3[][]
		/// </summary>
		/// <param name="texture"></param>
		/// <returns></returns>
		public static float3[][] Texture2D_ToFloat3_2D(Texture2D texture)
		{
			float3[][] data = new float3[texture.width][];
			for (int i = 0; i < data.Length; i++)
			{
				data[i] = new float3[texture.height];
				for (int j = 0; j < data[i].Length; j++)
				{
					Color color = texture.GetPixel(i, j);
					//		Debug.Log("Color: " + color.ToString());
					data[i][j] = new float3(color.r, color.g, color.b);
				}
			}
			return data;
		}


/*
		// Use this to generate the texture
			/// <summary>
		/// Generates a Color based on the given attributes. Please look at GenerateTexure for a better understanding on this works
		/// </summary>
		/// <param name="type"></param>
		/// <param name="minDimensions"></param>
		/// <param name="dimensions"></param>
		/// <param name="valueInterpertation"></param>
		/// <param name="Scale"></param>
		/// <param name="input"></param>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="customColor"></param>
		/// <param name="dimension"></param>
		/// <returns></returns>
		private static Color GenerateColor(NoiseType type, int4 minDimensions, int4 dimensions, NoiseValueInterpertation valueInterpertation, float Scale,
			float4 input, float x, float y, CustomColorAttributes customColor, int dimension = 2)
		{
			float z, w, value;
			float2 value2;
			float3 value3;
			Color color = new Color();

			if (dimension == 2)
			{
				switch (type)
				{
					case NoiseType.PerlinNoise:
						{
							// Classic Perlin noise, periodic variant
							value = GenerateNoise(type,new float2(x, y), new float2(input.x, input.y)).x;

							color = new Color(valueInterpertation.ColorInterpertation1D.x ? value : 0, valueInterpertation.ColorInterpertation1D.y ? value : 0, valueInterpertation.ColorInterpertation1D.z ? value : 0);
							color = HandleCustomColor(color, customColor);
							break;
						}

					case NoiseType.SRNoise2D:
						{
							// 2-D non-tiling simplex noise with rotating gradients,
							// without the analytical derivative.
							value = noise.srnoise(new float2(x, y), input.x);
							color = new Color(valueInterpertation.ColorInterpertation1D.x ? value : 0, valueInterpertation.ColorInterpertation1D.y ? value : 0, valueInterpertation.ColorInterpertation1D.z ? value : 0);
							color = HandleCustomColor(color, customColor);
							break;
						}
					case NoiseType.SRNoise:
						{
							value = noise.srnoise(new float2(x, y));
							color = new Color(valueInterpertation.ColorInterpertation1D.x ? value : 0, valueInterpertation.ColorInterpertation1D.y ? value : 0, valueInterpertation.ColorInterpertation1D.z ? value : 0);
							color = HandleCustomColor(color, customColor);
							break;
						}
					case NoiseType.SRDNoise2D:
						{
							// 2-D non-tiling simplex noise with rotating gradients and analytical derivative.
							// The first component of the 3-element return vector is the noise value,
							// and the second and third components are the x and y partial derivatives.
							value3 = noise.srdnoise(new float2(x, y), input.x);
							// so for the color we use just the x since y and z are the deriratives

							//color = new Color(value3.x, value3.x, value3.x);
							//  i like the way the derivatives work so i'll keep it like this for now
							//	color = new Color(value3.x, value3.y, value3.z);

							// new color system based on the value interperter
							color = new Color(
								valueInterpertation.ColorInterpertationA3D.x ? value3.x : valueInterpertation.ColorInterpertationB3D.x ? value3.y : valueInterpertation.ColorInterpertationC3D.x ? value3.z : 0,
								valueInterpertation.ColorInterpertationA3D.y ? value3.x : valueInterpertation.ColorInterpertationB3D.y ? value3.y : valueInterpertation.ColorInterpertationC3D.y ? value3.z : 0,
								valueInterpertation.ColorInterpertationA3D.z ? value3.x : valueInterpertation.ColorInterpertationB3D.z ? value3.y : valueInterpertation.ColorInterpertationC3D.z ? value3.z : 0
							);
							color = HandleCustomColor(color, customColor);


							break;
						}
					case NoiseType.SRDNoise:
						{
							// 2-D non-tiling simplex noise with rotating gradients,
							// without the analytical derivative.
							value3 = noise.srdnoise(new float2(x, y));
							// so for the color we use just the x since y and z are the deriratives

							//color = new Color(value3.x, value3.x, value3.x);
							//  i like the way the derivatives work so i'll keep it like this for now
							//	color = new Color(value3.x, value3.y, value3.z);
							color = new Color(
									valueInterpertation.ColorInterpertationA3D.x ? value3.x : valueInterpertation.ColorInterpertationB3D.x ? value3.y : valueInterpertation.ColorInterpertationC3D.x ? value3.z : 0,
									valueInterpertation.ColorInterpertationA3D.y ? value3.x : valueInterpertation.ColorInterpertationB3D.y ? value3.y : valueInterpertation.ColorInterpertationC3D.y ? value3.z : 0,
									valueInterpertation.ColorInterpertationA3D.z ? value3.x : valueInterpertation.ColorInterpertationB3D.z ? value3.y : valueInterpertation.ColorInterpertationC3D.z ? value3.z : 0
								);
							color = HandleCustomColor(color, customColor);

							break;
						}
					case NoiseType.ClassicPerlinNoise:
						{
							value = noise.cnoise(new float2(x, y));
							color = new Color(valueInterpertation.ColorInterpertation1D.x ? value : 0, valueInterpertation.ColorInterpertation1D.y ? value : 0, valueInterpertation.ColorInterpertation1D.z ? value : 0);
							color = HandleCustomColor(color, customColor);
							break;
						}
					case NoiseType.SimplexNoise:
						{
							value = noise.snoise(new float2(x, y));
							color = new Color(valueInterpertation.ColorInterpertation1D.x ? value : 0, valueInterpertation.ColorInterpertation1D.y ? value : 0, valueInterpertation.ColorInterpertation1D.z ? value : 0);
							color = HandleCustomColor(color, customColor);
							break;
						}
					case NoiseType.CellularNoise2x2:
						{
							value2 = noise.cellular2x2(new float2(x, y));
							//	color = new Color(value2.x, value2.x, value2.x);

							color = new Color(
								valueInterpertation.ColorInterpertationA2D.x ? value2.x : valueInterpertation.ColorInterpertationB2D.x ? value2.y : 0,
								valueInterpertation.ColorInterpertationA2D.y ? value2.x : valueInterpertation.ColorInterpertationB2D.y ? value2.y : 0,
								valueInterpertation.ColorInterpertationA2D.z ? value2.x : valueInterpertation.ColorInterpertationB2D.z ? value2.y : 0
							);
							color = HandleCustomColor(color, customColor);
							break;
						}
					case NoiseType.CellularNoise:
						{
							value2 = noise.cellular(new float2(x, y));
							//	color = new Color(value2.x, value2.x, value2.x);

							color = new Color(
								valueInterpertation.ColorInterpertationA2D.x ? value2.x : valueInterpertation.ColorInterpertationB2D.x ? value2.y : 0,
								valueInterpertation.ColorInterpertationA2D.y ? value2.x : valueInterpertation.ColorInterpertationB2D.y ? value2.y : 0,
								valueInterpertation.ColorInterpertationA2D.z ? value2.x : valueInterpertation.ColorInterpertationB2D.z ? value2.y : 0
							);
							color = HandleCustomColor(color, customColor);
							break;
						}


				}
			}
			else if (dimension == 3)
			{
				for (int k = minDimensions.z; k < dimensions.z; k++)
				{
					z = (float)k / dimensions.z * Scale;
					switch (type)
					{
						case NoiseType.PerlinNoise3x3x3:
							{
								// Classic Perlin noise, periodic variant
								value = noise.pnoise(new float3(x, y, z), new float3(input.x, input.y, input.z));

								color = new Color(valueInterpertation.ColorInterpertation1D.x ? value : 0, valueInterpertation.ColorInterpertation1D.y ? value : 0, valueInterpertation.ColorInterpertation1D.z ? value : 0);
								color = HandleCustomColor(color, customColor);

								break;
							}
						case NoiseType.ClassicPerlinNoise3x3x3:
							{
								// Classic Perlin noise
								value = noise.cnoise(new float3(x, y, z));

								color = new Color(valueInterpertation.ColorInterpertation1D.x ? value : 0, valueInterpertation.ColorInterpertation1D.y ? value : 0, valueInterpertation.ColorInterpertation1D.z ? value : 0);
								color = HandleCustomColor(color, customColor);
								break;
							}
						case NoiseType.SimplexNoise3x3x3:
							{
								value = noise.snoise(new float3(x, y, z));

								color = new Color(valueInterpertation.ColorInterpertation1D.x ? value : 0, valueInterpertation.ColorInterpertation1D.y ? value : 0, valueInterpertation.ColorInterpertation1D.z ? value : 0);
								color = HandleCustomColor(color, customColor);
								break;
							}
						case NoiseType.SimplexNoise3x3x3Gradient:
							{
								// idk what to do with the gradient
								float3 gradient = new float3();
								value = noise.snoise(new float3(x, y, z), out gradient);
								color = new Color(valueInterpertation.ColorInterpertation1D.x ? value : 0, valueInterpertation.ColorInterpertation1D.y ? value : 0, valueInterpertation.ColorInterpertation1D.z ? value : 0);
								color = HandleCustomColor(color, customColor);
								break;
							}
						case NoiseType.CellularNoise2x2x2:
							{
								// Cellular noise, returning F1 and F2 in a float2.
								// Speeded up by umath.sing 2x2x2 search window instead of 3x3x3,
								// at the expense of some pattern artifacts.
								// F2 is often wrong and has sharp discontinuities.
								// If you need a good F2, use the slower 3x3x3 version.
								value2 = noise.cellular2x2x2(new float3(x, y, z));


								//	color = new Color(value2.x, value2.x, value2.x);
								color = new Color(
									valueInterpertation.ColorInterpertationA2D.x ? value2.x : valueInterpertation.ColorInterpertationB2D.x ? value2.y : 0,
									valueInterpertation.ColorInterpertationA2D.y ? value2.x : valueInterpertation.ColorInterpertationB2D.y ? value2.y : 0,
									valueInterpertation.ColorInterpertationA2D.z ? value2.x : valueInterpertation.ColorInterpertationB2D.z ? value2.y : 0
								);
								color = HandleCustomColor(color, customColor);
								break;
							}
						case NoiseType.CellularNoise3x3x3:
							{
								// Cellular noise, returning F1 and F2 in a float2.
								// 3x3x3 search region for good F2 everywhere, but a lot
								// slower than the 2x2x2 version.
								// The code below is a bit scary even to its author,
								// but it has at least half decent performance on a
								// math.modern GPU. In any case, it beats any software
								// implementation of Worley noise hands down.
								value2 = noise.cellular(new float3(x, y, z));


								//	color = new Color(value2.x, value2.x, value2.x);
								color = new Color(
									valueInterpertation.ColorInterpertationA2D.x ? value2.x : valueInterpertation.ColorInterpertationB2D.x ? value2.y : 0,
									valueInterpertation.ColorInterpertationA2D.y ? value2.x : valueInterpertation.ColorInterpertationB2D.y ? value2.y : 0,
									valueInterpertation.ColorInterpertationA2D.z ? value2.x : valueInterpertation.ColorInterpertationB2D.z ? value2.y : 0
								);
								color = HandleCustomColor(color, customColor);
								break;
							}


					}
				}
			}
			else if (dimension == 4)
			{
				for (int k = minDimensions.z; k < dimensions.z; k++)
				{
					z = (float)k / dimensions.z * Scale;
					for (int l = minDimensions.w; l < dimensions.w; l++)
					{
						w = (float)l / dimensions.w * Scale;
						switch (type)
						{
							case NoiseType.PerlinNoise4x4x4x4:
								//		Debug.LogWarning("This Scene doesnto currently support 4D inputs because idk what they are");
								value = noise.pnoise(new float4(x, y, z, w), input);
								color = new Color(valueInterpertation.ColorInterpertation1D.x ? value : 0, valueInterpertation.ColorInterpertation1D.y ? value : 0, valueInterpertation.ColorInterpertation1D.z ? value : 0);
								color = HandleCustomColor(color, customColor);
								break;
							case NoiseType.ClassicPerlinNoise4x4x4x4:
								//		Debug.LogWarning("This Scene doesnto currently support 4D inputs because idk what they are");
								value = noise.cnoise(new float4(x, y, z, w));
								color = new Color(valueInterpertation.ColorInterpertation1D.x ? value : 0, valueInterpertation.ColorInterpertation1D.y ? value : 0, valueInterpertation.ColorInterpertation1D.z ? value : 0);
								color = HandleCustomColor(color, customColor);
								break;
							case NoiseType.SimplexNoise4x4x4x4:
								{
									//			Debug.LogWarning("This Scene doesnto currently support 4D inputs because idk what they are");
									value = noise.snoise(new float4(x, y, z, w));
									color = new Color(valueInterpertation.ColorInterpertation1D.x ? value : 0, valueInterpertation.ColorInterpertation1D.y ? value : 0, valueInterpertation.ColorInterpertation1D.z ? value : 0);
									color = HandleCustomColor(color, customColor);
									break;
								}
						}
					}
				}
			}
			else
				Debug.LogError("Unknown Dimension " + dimension + " detected!");
			return color;
		}
		
		/// <summary>
		/// Generates a Texture2D based on the given arguments
		/// </summary>
		/// <param name="type">type of noise to generate</param>
		/// <param name="width">width of texture</param>
		/// <param name="height">height of texture</param>
		/// <param name="length">used in 3D and 4D noise and is treated like another axis</param>
		/// <param name="depth">used in 4D noise and is treated like another axis</param>
		/// <param name="Scale">zoom in/out</param>
		/// <param name="input">the input is the gradient or extra values some noise may need (see the function for more information)</param>
		/// <param name="dimension">
		/// 2 = 2D noise
		/// 3 = 3D noise
		/// 4 = 4D noise
		/// </param>
		/// <returns></returns>
		public static Texture2D GenerateTexture(NoiseType type, int4 minDimensions, int4 dimensions, OffsetCropSection crop, ValueInterpertation valueInterpertation, float Scale, float4 input, int dimension = 2)
		{
			Texture2D texture = new Texture2D(dimensions.x, dimensions.y);
			float x, y;
			Color color = new Color();
			// get the crop section
			int4 cropSection = crop.GetCropSection();
			CustomColorAttributes cca = new CustomColorAttributes
			{
				Enabled = new bool2(valueInterpertation.UseCutsomColors, valueInterpertation.useGradientValue && valueInterpertation.GradientEnabled),
				Gradient = valueInterpertation.gradientData
			};
			for (int i = minDimensions.x; i < dimensions.x; i++)
			{
				x = (float)i / dimensions.x * Scale;
				for (int j = minDimensions.y; j < dimensions.y; j++)
				{
					y = (float)j / dimensions.y * Scale;
					cca.CustomColor = valueInterpertation.colors;
					if (crop.useCroppedSection)
					{
						if (i < cropSection.x || i > cropSection.y || j < cropSection.z || j > cropSection.w)
							color = new Color(0, 0, 0);
						else
						{
							color = GenerateColor(type, minDimensions, dimensions, valueInterpertation, Scale, input, x, y,
								cca
								, dimension);
						}
					}
					else
						color = GenerateColor(type, minDimensions, dimensions, valueInterpertation, Scale, input, x, y, cca, dimension);
					texture.SetPixel(i, j, color);
				}
			}

			texture.Apply();

			return texture;
		}
		*/
	}

}
