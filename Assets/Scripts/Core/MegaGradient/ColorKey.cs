using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
namespace Core.MegaGradient
{

	[System.Serializable]
	public struct ColorKey
	{
		[SerializeField]
		Color color;
		[SerializeField]
		float time;

		public ColorKey(Color color, float time)
		{
			this.color = color;
			this.time = time;
		}

		public Color Color
		{
			get { return color; }
		}

		public float Time
		{
			get { return time; }
		}



	}

	[System.Serializable]
	public struct ColorKeyData : IComponentData
	{
		[SerializeField]
		float3 color;
		[SerializeField]
		float time;

		public static ColorKeyData ColorKeyToColorKeyData(ColorKey key)
		{
			return new ColorKeyData(new float3(key.Color.r, key.Color.g, key.Color.b), key.Time);
		}

		public ColorKeyData(float3 color, float time)
		{
			this.color = color;
			this.time = time;
		}

		public Color Color
		{
			get { return new Color(color.x, color.y, color.z); }
		}

		public float3 ColorValue
		{
			get { return color; }
		}
		public float Time
		{
			get { return time; }
		}


		public static bool operator ==(ColorKeyData lhs, ColorKeyData rhs)
		{
			return lhs.color.Equals(rhs.color) && lhs.time == rhs.time;
		}

		public static bool operator !=(ColorKeyData lhs, ColorKeyData rhs)
		{
			return !(lhs == rhs);
		}

		/// <summary>
		/// ColorKeyData instances are equal if they refer to the same ColorKeyData.
		/// </summary>
		/// <param name="compare">The object to compare to this ColorKeyData.</param>
		/// <returns>True, if the compare parameter contains an ColorKeyData object having the same value
		/// as this ColorKeyData.</returns>
		public override bool Equals(object compare)
		{
			return this == (ColorKeyData)compare;
		}

		/// <summary>
		/// A hash used for comparisons.
		/// </summary>
		/// <returns>A unique hash code.</returns>
		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		/// <summary>
		/// A "blank" ColorKeyData object that does not refer to an actual entity.
		/// </summary>
		public static ColorKeyData Null => new ColorKeyData();

	}
}