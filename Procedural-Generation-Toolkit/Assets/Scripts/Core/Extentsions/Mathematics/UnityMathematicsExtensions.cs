using Unity.Entities;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace Core.Extensions
{
	public class Vector3Extensions
	{ 
		// clockwise
		public static Vector3 Rotate90CW(Vector3 aDir)
		{
			return new Vector3(aDir.z, 0, -aDir.x);
		}
		// counter clockwise
		public static Vector3 Rotate90CCW(Vector3 aDir)
		{
			return new Vector3(-aDir.z, 0, aDir.x);
		}
	}


	[System.Serializable]
	public class Float3Slider : PropertyAttribute
	{
		[SerializeField]
		public Vector3 Value;
		[SerializeField]
		public Vector3 MaxValue;
		[SerializeField]
		public Vector3 MinValue;
		[HideInInspector]
		[SerializeField]
		public bool FoldoutToggle = true;
		[HideInInspector]
		[SerializeField]
		public string SliderName = "Float3Slider";
		[HideInInspector]
		[SerializeField]
		public string MaxValueName = "Max";
		[HideInInspector]
		[SerializeField]
		public string MinValueName = "Min";
		[HideInInspector]
		[SerializeField]
		public string XLabel = "X Value";
		[HideInInspector]
		[SerializeField]
		public string YLabel = "Y Value";
		[HideInInspector]
		[SerializeField]
		public string ZLabel = "Z Value";


		#region Initializers
		/// <summary>
		/// creates a Float3Slider with the given arguments
		/// </summary>
		/// <param name="maxValue"></param>
		/// <param name="minValue"></param>
		public Float3Slider(float3 _minValue = new float3(), float3 _maxValue = new float3(), float3 value = new float3(),
			string sliderName = "Float3Slider",string maxValueName = "Max",string minValueName = "Min",
			string xLabel = "X Value",string yLabel = "Y Value",string zLabel = "Z Value")
		{
			MaxValue = _maxValue.Equals(_minValue) ? new float3(_minValue.x + 1f, _minValue.y + 1f, _minValue.z + 1f) : _maxValue;
			MinValue = _minValue;
			Value = value;
			SliderName = sliderName;
			MaxValueName = maxValueName;
			MinValueName = minValueName;
			XLabel = xLabel;
			YLabel = yLabel;
			ZLabel = zLabel;
		}
		#endregion

	

		/// <summary>
		/// returns the value as a float2. 2 out of the 3 booleans return values must be true otherwise an invalid value is returned
		/// </summary>
		/// <param name="returns"></param>
		/// <param name="flipValues">set to true to flip to positition of the 2 returning values</param>
		/// <returns></returns>
		public float2 GetValueAsFloat2(bool3 returns,bool flipvalues = false)
		{
			return GetValueAsFloat2(returns.x, returns.y, returns.z, flipvalues);
		}
		/// <summary>
		/// returns the value as a float2. 2 out of the 3 boolean return values must be true otherwise an invalid value is returned
		/// </summary>
		/// <param name="returnX">set to true to return the x value</param>
		/// <param name="returnY">set to true to return the y value</param>
		/// <param name="returnZ">set to true to return the z value</param>
		/// <param name="flipValues">set to true to flip to positition of the 2 returning values</param>
		/// <returns></returns>
		public float2 GetValueAsFloat2(bool returnX = true, bool returnY = true, bool returnZ = false, bool flipValues = false)
		{
			if (returnX && returnY)
			{
				if (!flipValues)
					return new float2(Value.x, Value.y);
				else
					return new float2(Value.y, Value.x);
			}
			else if (returnX && returnZ)
			{
				if (!flipValues)
					return new float2(Value.x, Value.z);
				else
					return new float2(Value.z, Value.x);
			}
			else if (returnY && returnZ)
			{
				if (!flipValues)
					return new float2(Value.y, Value.z);
				else
					return new float2(Value.z, Value.y);
			}
			return Float2Slider.Invalid;
		}

		/// <summary>
		/// returns the x,y, or z value
		/// </summary>
		/// <param name="returns"></param>
		/// <returns></returns>
		public float GetValueAsFloat(bool2 returns)
		{
			return GetValueAsFloat(returns.x, returns.y);
		}
		/// <summary>
		/// returns the x,y, or z value
		/// </summary>
		/// <param name="returnX">set to true to return the x value</param>
		/// <param name="returnY">set to true to return the y value</param>
		/// <returns></returns>
		public float GetValueAsFloat(bool returnX,bool returnY)
		{
			if (returnX)
				return Value.x;
			else if (returnY)
				return Value.y;
			return Value.z;
		}
		// Overrides

		public bool Equals(Float3Slider b)
		{
			return Value == b.Value && MaxValue == b.MaxValue && MinValue == b.MinValue;
		}
		public override bool Equals(object obj)
		{
			return this.Equals((Float3Slider)obj);
		}
		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
		public override string ToString()
		{
			return "Value: " + Value + "\t Min:" + MinValue + "\t Max: " + MaxValue;
		}
	}

	// Float2Slider and Float1 Slider have not been updated!

	[System.Serializable]
	public struct Float2Slider : IComponentData
	{
		private float2 Value;
		private float2 MaxValue;
		private float2 MinValue;
		/// <summary>
		/// creates a Float2Slider with the given arguments
		/// </summary>
		/// <param name="maxValue"></param>
		/// <param name="minValue"></param>
		public Float2Slider(float2 maxValue, float2 minValue)
		{
			MaxValue = maxValue;
			MinValue = minValue;
			Value = minValue;
		}
		/// <summary>
		/// creates a Float2Slider with the given arguments
		/// </summary>
		/// <param name="_maxValue"></param>
		/// <param name="_minValue"></param>
		/// <param name="_value"></param>
		public Float2Slider(float2 _maxValue, float2 _minValue, float2 _value)
		{
			MaxValue = _maxValue;
			MinValue = _minValue;
			Value = float2.zero;
			value = _value;
		}
		// the current slider value between the min and max values
		public float2 value
		{
			get { return Value; }
			set
			{
				bool2 isValid = value <= MaxValue;
				if (!isValid.x)
					value.x = MaxValue.x;
				if (!isValid.y)
					value.y = MaxValue.y;
				isValid = value >= MinValue;
				if (!isValid.x)
					value.x = MinValue.x;
				if (!isValid.y)
					value.y = MinValue.y;
				Value = value;
			}
		}
		// the max value of the slider
		public float2 maxValue
		{
			get { return MaxValue; }
			set
			{
				bool2 isMax = value > MinValue;
				if (!isMax.x)
					value.x = MinValue.x + 1;
				if (!isMax.y)
					value.y = MinValue.y + 1;
				MaxValue = value;
				// update the value
				this.value = this.value;
			}
		}
		// the min value of the slider
		public float2 minValue
		{
			get { return MinValue; }
			set
			{
				bool2 isMin = value < MaxValue;
				if (!isMin.x)
					value.x = MaxValue.x - 1;
				if (!isMin.y)
					value.y = MaxValue.y - 1;
				MinValue = value;
				// update the value
				this.value = this.value;
			}
		}
		/// <summary>
		/// returns the value as a float3 with z  being 0
		/// </summary>
		/// <returns></returns>
		public float3 GetValueAsFloat3()
		{
			return new float3(Value, 0);
		}

		/// <summary>
		/// returns the x or y value
		/// </summary>
		/// <param name="returnX">set to true to return the x value</param>
		/// <returns></returns>
		public float GetValueAsFloat(bool returnX)
		{
			if (returnX)
				return Value.x;
			return Value.y;
		}

		public static bool operator ==(Float2Slider a, Float2Slider b)
		{
			return a.value.Equals(b.value) && a.maxValue.Equals(b.maxValue) && a.minValue.Equals(b.minValue);
		}

		public static bool operator !=(Float2Slider a, Float2Slider b)
		{
			return !(a == b);
		}


		public bool Equals(Float2Slider a)
		{
			return this == a;
		}
		public override bool Equals(object obj)
		{
			return this.Equals((Float2Slider)obj);
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		public static float2 Invalid => new float2(-1.193f, -1.854f);

		public static Float2Slider Null => new Float2Slider();
	}
	[System.Serializable]
	public struct FloatSlider : IComponentData
	{
		private float Value;
		private float MaxValue;
		private float MinValue;
		/// <summary>
		/// creates a FloatSlider with the given arguments
		/// </summary>
		/// <param name="maxValue"></param>
		/// <param name="minValue"></param>
		public FloatSlider(float maxValue, float minValue)
		{
			MaxValue = maxValue;
			MinValue = minValue;
			Value = minValue;
		}
		/// <summary>
		/// creates a FloatSlider with the given arguments
		/// </summary>
		/// <param name="_maxValue"></param>
		/// <param name="_minValue"></param>
		/// <param name="_value"></param>
		public FloatSlider(float _maxValue, float _minValue, float _value)
		{
			MaxValue = _maxValue;
			MinValue = _minValue;
			Value = 0;
			value = _value;
		}
		// the current slider value between the min and max values
		public float value
		{
			get { return Value; }
			set
			{
				if (value > MaxValue)
					Value = MaxValue;
				else if (value < MinValue)
					Value = MinValue;
				else
					Value = value;
			}
		}
		// the max value of the slider
		public float maxValue
		{
			get { return MaxValue; }
			set
			{
				if (value > MinValue)
					MaxValue = value;
				else
					MaxValue = MinValue + 1;
				// update the value
				this.value = this.value;
			}
		}
		// the min value of the slider
		public float minValue
		{
			get { return MinValue; }
			set
			{
				if (value < MaxValue)
					MinValue = value;
				else
					MinValue = MaxValue - 1;
				// update the value
				this.value = this.value;
			}
		}
		/// <summary>
		/// returns the value as a float3 with z and y being 0
		/// </summary>
		/// <returns></returns>
		public float3 GetValueAsFloat3()
		{
			return new float3(Value, 0,0);
		}
		/// <summary>
		/// returns the value as a float3 with y being 0
		/// </summary>
		/// <returns></returns>
		public float2 GetValueAsFloat2()
		{
			return new float2(Value, 0);
		}

		public static bool operator ==(FloatSlider a, FloatSlider b)
		{
			return a.value == b.value && a.maxValue == b.maxValue && a.minValue == b.minValue;
		}

		public static bool operator !=(FloatSlider a, FloatSlider b)
		{
			return !(a == b);
		}

		public bool Equals(FloatSlider a)
		{
			return this == a;
		}
		public override bool Equals(object obj)
		{
			return this.Equals((FloatSlider)obj);
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		public static float Invalid => -1.193f;

		public static FloatSlider Null => new FloatSlider();
	}


}