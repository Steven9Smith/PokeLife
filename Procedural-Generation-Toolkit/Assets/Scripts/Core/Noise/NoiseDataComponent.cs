using UnityEngine;
using System.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections;

public class NoiseDataComponent : MonoBehaviour
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
namespace Core.Noise
{
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
		//
		// 2-D tiling simplex noise with rotating gradients and analytical derivative.
		// The first component of the 3-element return vector is the noise value,
		// and the second and third components are the x and y partial derivatives.
		//
		PSRDNoise3,
		//
		// 2-D tiling simplex noise with fixed gradients
		// and analytical derivative.
		// This function is implemented as a wrapper to "psrdnoise",
		// at the math.minimal math.cost of three extra additions.
		//
		PSRDNoise,
		//
		// 2-D tiling simplex noise with rotating gradients,
		// but without the analytical derivative.
		//
		PSRNoise3,
		//
		// 2-D tiling simplex noise with fixed gradients,
		// without the analytical derivative.
		// This function is implemented as a wrapper to "psrnoise",
		// at the math.minimal math.cost of three extra additions.
		//
		PSRNoise,
		SRNoise,
		SRDNoise,
		SRDNoise2D,
		SRNoise2D,
		SimplexNoise3x3x3Gradient,

		// Used when noise is merged with other using arithmetic
		Mixed

	}

	public class NoiseDataClass
	{
		/// <summary>
		/// Type of Noise
		/// </summary>
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
	}

	public struct NoiseProfileData : IComponentData
	{
		NoiseData noiseData;
		NoiseDataHistory noiseDataHistory;

		public NoiseProfileData(NoiseData nData)
		{
			noiseData = new NoiseData();
			noiseDataHistory = new NoiseDataHistory();
		}

		public float[] GetNoiseValuesWithRange(float4 maxRange,float4 minRange = new float4(),
			float4 noiseOptions = new float4(),int decimalPercision = 0)
		{
			float[] data = maxRange.x != minRange.x ? maxRange.y != minRange.y ? maxRange.z != minRange.z ? maxRange.w != minRange.w ?
				new float[(int)((maxRange.x - minRange.x) * (maxRange.y - minRange.y) * (maxRange.z - minRange.z) * (maxRange.w-minRange.z))]
				: new float[(int)((maxRange.x - minRange.x) * (maxRange.y - minRange.y) * (maxRange.z - minRange.z))]
				: new float[(int)((maxRange.x-minRange.x)*(maxRange.y-minRange.y))] 
				: new float[(int)(maxRange.x-minRange.x)] :  new float[0];
			if(decimalPercision == 0)
			{
				int4 min = (int4)minRange;
				int4 max = (int4)maxRange;
				noiseData.NoiseOptionalParameters = noiseOptions;

				for(int i = min.x; i < max.x; i++)
				{
					if (max.y == min.y)
					{
						noiseData.NoiseParameters = new float4(i, 0,0,0);

					}
					else
					{
						for (int j = min.y; j < max.y; j++)
						{
							for (int k = min.z; k < max.z; k++)
							{
								for (int l = min.w; l < max.w; l++)
								{

								}
							}
						}
					}
				}
			}
			else
			{

			}

			return data;
		}
		public bool HasChanged()
		{
			return false;
		//	bool a = noiseData == noiseDataHistory.data;
		//	noiseDataHistory.data = noiseData;
		//	return !a;
		}
	}



	public struct NoiseData : IComponentData
	{
		public float4 NoiseParameters;
		public float4 NoiseOptionalParameters;
		public float3 NoiseGradient;
		public NoiseType NoiseType;

		public int GetDimension()
		{
			switch (NoiseType)
			{
				case NoiseType.PerlinNoise:
				case NoiseType.SRNoise2D:
				case NoiseType.SRNoise:
				case NoiseType.SRDNoise2D:
				case NoiseType.SRDNoise:
				case NoiseType.ClassicPerlinNoise:
				case NoiseType.SimplexNoise:
				case NoiseType.CellularNoise2x2:
				case NoiseType.CellularNoise:
					return 2;
				case NoiseType.PerlinNoise3x3x3:
				case NoiseType.ClassicPerlinNoise3x3x3:
				case NoiseType.SimplexNoise3x3x3:
				case NoiseType.CellularNoise2x2x2:
				case NoiseType.CellularNoise3x3x3:
				case NoiseType.SimplexNoise3x3x3Gradient:
					return 3;
				case NoiseType.PerlinNoise4x4x4x4:
				case NoiseType.ClassicPerlinNoise4x4x4x4:
				case NoiseType.SimplexNoise4x4x4x4:
					return 4;
				default:
					return 0;
			}
		}

		/// <summary>
		/// gets the value of the noise but returns 1 value instead of all possible values
		/// </summary>
		/// <param name="axis">type 'x' for x axis, 'y', and 'z' for respected axises. (NOTE: not all path return all 3 axis)</param>
		/// <returns></returns>
		public float GetValue1(char axis = 'x')
		{
			float3 tmp;
			switch (NoiseType)
			{

				case NoiseType.PSRNoise:
					return noise.psrnoise(new float2(NoiseParameters.x, NoiseParameters.y), new float2(NoiseOptionalParameters.x, NoiseOptionalParameters.y));
				case NoiseType.PSRNoise3:
					return noise.psrnoise(new float2(NoiseParameters.x, NoiseParameters.y), new float2(NoiseOptionalParameters.x, NoiseOptionalParameters.y), NoiseOptionalParameters.z);
				case NoiseType.PSRDNoise:
					tmp = noise.psrdnoise(new float2(NoiseParameters.x, NoiseParameters.y), new float2(NoiseOptionalParameters.x, NoiseOptionalParameters.y));
					return axis == 'x' ? tmp.x : axis == 'y' ? tmp.y : tmp.z;
				case NoiseType.PSRDNoise3:
					tmp =  noise.psrdnoise(new float2(NoiseParameters.x, NoiseParameters.y), new float2(NoiseOptionalParameters.x, NoiseOptionalParameters.y), NoiseOptionalParameters.z).x;
					return axis == 'x' ? tmp.x : axis == 'y' ? tmp.y : tmp.z;
				case NoiseType.PerlinNoise:
					return noise.pnoise(new float2(NoiseParameters.x, NoiseParameters.y), new float2(NoiseOptionalParameters.x, NoiseOptionalParameters.y));
				case NoiseType.SRNoise2D:
					return noise.srnoise(new float2(NoiseParameters.x, NoiseParameters.y), NoiseOptionalParameters.x);
				case NoiseType.SRNoise:
					return noise.srnoise(new float2(NoiseParameters.x, NoiseParameters.y));
				case NoiseType.SRDNoise2D:
					tmp = noise.srdnoise(new float2(NoiseParameters.x, NoiseParameters.y), NoiseOptionalParameters.x);
					return axis == 'x' ? tmp.x : axis == 'y' ? tmp.y : tmp.z;
				case NoiseType.SRDNoise:
					tmp = noise.srdnoise(new float2(NoiseParameters.x, NoiseParameters.y));
					return axis == 'x' ? tmp.x : axis == 'y' ? tmp.y : tmp.z;
				case NoiseType.ClassicPerlinNoise:
					return noise.cnoise(new float2(NoiseParameters.x, NoiseParameters.y));
				case NoiseType.SimplexNoise:
					return noise.snoise(new float2(NoiseParameters.x, NoiseParameters.y));
				case NoiseType.CellularNoise2x2:
					tmp = new float3(noise.cellular2x2(new float2(NoiseParameters.x, NoiseParameters.y)), 0);
					return axis == 'x' ? tmp.x : tmp.y;
				case NoiseType.CellularNoise:
					tmp = new float3(noise.cellular(new float2(NoiseParameters.x, NoiseParameters.y)), 0);
					return axis == 'x' ? tmp.x : tmp.y;
				case NoiseType.CellularNoise3x3x3:
					tmp =  new float3(noise.cellular(new float3(NoiseParameters.x, NoiseParameters.y, NoiseParameters.z)), 0);
					return axis == 'x' ? tmp.x : tmp.y;
				case NoiseType.PerlinNoise3x3x3:
					return noise.pnoise(new float3(NoiseParameters.x, NoiseParameters.y, NoiseParameters.z), new float3(NoiseOptionalParameters.x, NoiseOptionalParameters.y, NoiseOptionalParameters.z));
				case NoiseType.ClassicPerlinNoise3x3x3:
					return noise.cnoise(new float3(NoiseParameters.x, NoiseParameters.y, NoiseParameters.z));
				case NoiseType.SimplexNoise3x3x3:
					return noise.snoise(new float3(NoiseParameters.x, NoiseParameters.y, NoiseParameters.z));
				case NoiseType.CellularNoise2x2x2:
					tmp = new float3(noise.cellular2x2x2(new float3(NoiseParameters.x, NoiseParameters.y, NoiseParameters.z)), 0);
					return axis == 'x' ? tmp.x : tmp.y;
				case NoiseType.SimplexNoise3x3x3Gradient:
					return noise.snoise(new float3(NoiseParameters.x, NoiseParameters.y, NoiseParameters.z), out NoiseGradient);
				case NoiseType.PerlinNoise4x4x4x4:
					return noise.pnoise(NoiseParameters, NoiseOptionalParameters);
				case NoiseType.ClassicPerlinNoise4x4x4x4:
					return noise.cnoise(NoiseParameters);
				case NoiseType.SimplexNoise4x4x4x4:
					return noise.snoise(NoiseParameters);
				default:
					Debug.LogError("NoiseData::GetValue1::NoiseType does not have a valid float return value");
					return 0;
			}
		}

		public float3 GetValue()
		{
			switch (NoiseType) {
				case NoiseType.PSRNoise:
					return new float3(noise.psrnoise(new float2(NoiseParameters.x, NoiseParameters.y), new float2(NoiseOptionalParameters.x, NoiseOptionalParameters.y)), 0, 0);
				case NoiseType.PSRNoise3:
					return new float3(noise.psrnoise(new float2(NoiseParameters.x, NoiseParameters.y), new float2(NoiseOptionalParameters.x, NoiseOptionalParameters.y),NoiseOptionalParameters.z), 0, 0);
				case NoiseType.PSRDNoise:
					return noise.psrdnoise(new float2(NoiseParameters.x,NoiseParameters.y),new float2(NoiseOptionalParameters.x,NoiseOptionalParameters.y));
				case NoiseType.PSRDNoise3:
					return noise.psrdnoise(new float2(NoiseParameters.x, NoiseParameters.y), new float2(NoiseOptionalParameters.x, NoiseOptionalParameters.y), NoiseOptionalParameters.z);
				case NoiseType.PerlinNoise:
					return new float3(noise.pnoise(new float2(NoiseParameters.x, NoiseParameters.y),new float2(NoiseOptionalParameters.x,NoiseOptionalParameters.y)),0,0);
				case NoiseType.SRNoise2D:
					return new float3(noise.srnoise(new float2(NoiseParameters.x, NoiseParameters.y), NoiseOptionalParameters.x), 0,0);
				case NoiseType.SRNoise:
					return new float3(noise.srnoise(new float2(NoiseParameters.x, NoiseParameters.y)), 0,0);
				case NoiseType.SRDNoise2D:
					return noise.srdnoise(new float2(NoiseParameters.x, NoiseParameters.y), NoiseOptionalParameters.x);
				case NoiseType.SRDNoise:
					return noise.srdnoise(new float2(NoiseParameters.x, NoiseParameters.y));
				case NoiseType.ClassicPerlinNoise:
					return new float3(noise.cnoise(new float2(NoiseParameters.x, NoiseParameters.y)), 0, 0);
				case NoiseType.SimplexNoise:
					return new float3(noise.snoise(new float2(NoiseParameters.x, NoiseParameters.y)),0,0);
				case NoiseType.CellularNoise2x2:
					return new float3(noise.cellular2x2(new float2(NoiseParameters.x, NoiseParameters.y)), 0);
				case NoiseType.CellularNoise:
					return new float3(noise.cellular(new float2(NoiseParameters.x, NoiseParameters.y)), 0);
				case NoiseType.CellularNoise3x3x3:
					return new float3(noise.cellular(new float3(NoiseParameters.x, NoiseParameters.y, NoiseParameters.z)),0);
				case NoiseType.PerlinNoise3x3x3:
					return new float3(noise.pnoise(new float3(NoiseParameters.x, NoiseParameters.y, NoiseParameters.z), new float3(NoiseOptionalParameters.x, NoiseOptionalParameters.y, NoiseOptionalParameters.z)),0,0);
				case NoiseType.ClassicPerlinNoise3x3x3:
					return new float3(noise.cnoise(new float3(NoiseParameters.x, NoiseParameters.y, NoiseParameters.z)),0,0);
				case NoiseType.SimplexNoise3x3x3:
					return new float3(noise.snoise(new float3(NoiseParameters.x, NoiseParameters.y, NoiseParameters.z)),0,0);
				case NoiseType.CellularNoise2x2x2:
					return new float3(noise.cellular2x2x2(new float3(NoiseParameters.x, NoiseParameters.y, NoiseParameters.z)), 0);
				case NoiseType.SimplexNoise3x3x3Gradient:
					return new float3(noise.snoise(new float3(NoiseParameters.x,NoiseParameters.y,NoiseParameters.z),out NoiseGradient), 0, 0);
			//	case NoiseType.CellularNoise3x3x3:
			//		noise.cel
				case NoiseType.PerlinNoise4x4x4x4:
					return new float3(noise.pnoise(NoiseParameters, NoiseOptionalParameters),0,0);
				case NoiseType.ClassicPerlinNoise4x4x4x4:
					return new float3(noise.cnoise(NoiseParameters),0,0);
				case NoiseType.SimplexNoise4x4x4x4:
					return new float3(noise.snoise(NoiseParameters),0,0);
				default:
					Debug.LogError("NoiseData::GetValue:: Failed to get type of Noise, cannot return a valid value \""+NoiseType+"\"");
					return float3.zero;
			}

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

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		public override string ToString()
		{
			return Type2String(NoiseType) + ", parameters: " + NoiseParameters;
		}

		public bool Equals(NoiseData other)
		{
			return other.NoiseParameters.Equals(NoiseParameters) && NoiseType == other.NoiseType;
		}

		public override bool Equals(object obj)
		{
			return this.Equals((NoiseData)obj);
		}
	}

	public struct NoiseDataHistory : IComponentData
	{
		public NoiseData data;
	}

}