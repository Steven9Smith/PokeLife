using UnityEngine;
using Unity.Entities;
using Core.Common.Systems.Request;
using Core.MegaGradient.Buffers;
using Unity.Mathematics;

namespace Core.MegaGradient
{
	public class MegaGradientDataComponent : MonoBehaviour, IConvertGameObjectToEntity
	{
		public RequestComponent entityRequest;

		public MegaGradientData gradient;

		public MegaGradient editorGradient = new MegaGradient()
		{
			hide = false,
			blendMode = MegaGradient.BlendMode.Linear,
			randomizeColor = false
		}; 


		public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
		{
			MegaGradientData data = new MegaGradientData(dstManager);
			data.blendMode = editorGradient.blendMode;
			data.randomizeColor = editorGradient.randomizeColor;
			data.keys.GetBuffer(dstManager);
			editorGradient.hide = true;
			for (int i = 0; i < editorGradient.NumKeys; i++)
			{
				if(i < data.NumKeys)
					data.keys.buffer[i] = new ColorKeyDataBufferElement { Value = ColorKeyData.ColorKeyToColorKeyData(editorGradient.GetKey(i)) };
				else
					data.keys.buffer.Add(new ColorKeyDataBufferElement { Value = ColorKeyData.ColorKeyToColorKeyData(editorGradient.GetKey(i)) });

			}
			dstManager.AddComponentData(entity, data);
		}

		public void ValidateKeys()
		{
			gradient.ValidateKeys(entityRequest.RequestReceived.entityManager);
		}

		// Use this for initialization
		void Start()
		{
			if (editorGradient == null)
				editorGradient = new MegaGradient();
			if(entityRequest == null)
			{
				// try and get the component from the GameObject
				entityRequest = this.GetComponent<RequestComponent>();
				if (entityRequest == null)
					Debug.LogError("Failed to get RequestComponent from GameObject.");
			}
		}

		// Update is called once per frame
		void Update()
		{
			if (entityRequest.RequestReceived.IsValid)
			{
			//	Debug.Log("Got Entity");
				if (!gradient.isCreated)
				{
					gradient = entityRequest.RequestReceived.entityManager.GetComponentData<MegaGradientData>(entityRequest.RequestReceived.entity);
				}
			}
		}
	}

	[System.Serializable]
	public struct MegaGradientData : IComponentData
	{
		public DynamicBufferColorKeyDataBufferElement keys;
		public MegaGradient.BlendMode blendMode;
		public bool randomizeColor;
		public bool isCreated;
		private EntityManager entityManager;

		public MegaGradientData(EntityManager entityManager, MegaGradient.BlendMode blendMode = MegaGradient.BlendMode.Linear, bool randomizeColor = false)
		{
			this.keys = new DynamicBufferColorKeyDataBufferElement(entityManager);
			this.blendMode = blendMode;
			this.randomizeColor = randomizeColor;
			this.isCreated = true;
			this.entityManager = entityManager;
		}

		public void ValidateKeys(EntityManager entityManager)
		{
			Debug.Log("validating keys");
			keys.GetBuffer(entityManager);
		}

		// Num Keys

		public int NumKeys
		{
			get { return keys.buffer.Length; }
		}

		// Get Key

		public ColorKeyData GetKey(int i)
		{
			return keys.buffer[i].Value;
		}

		public void RemoveKey(int index)
		{
			keys.buffer.RemoveAt(index);
		}

		// Add key

		public int AddKey(Color color, float time)
		{
			float3 fColor = new float3(color.r, color.g, color.b);
			return FinishAddition(fColor, time);
		}

		public int AddKey(float3 color, float time)
		{
			return FinishAddition(color, time);
		}

		private int FinishAddition(float3 color, float time)
		{
			ColorKeyData newKeys = new ColorKeyData(color, time);
			for (int i = 0; i < keys.buffer.Length; i++)
				if (newKeys.Time < keys.buffer[i].Value.Time)
				{
					keys.buffer.Insert(i, new ColorKeyDataBufferElement { Value = newKeys });
					return i;
				}
			keys.buffer.Add(new ColorKeyDataBufferElement { Value = newKeys });
			return keys.buffer.Length - 1;
		}

		// Update Key Color

		public void UpdateKeyColor(int index, Color color)
		{
			float3 fColor = new float3(color.r, color.g, color.b);
			FinishKeyColorUpdate(index, fColor);
		}

		public void UpdateKeyColor(int index, float3 color)
		{
			FinishKeyColorUpdate(index, color);
		}

		public void FinishKeyColorUpdate(int index, float3 color)
		{
			keys.buffer[index] = new ColorKeyDataBufferElement { Value = new ColorKeyData(color, keys.buffer[index].Value.Time) };
		}

		// Update Key Time

		public int UpdateKeyTime(int keyIndex, float newTime)
		{
			float3 color = keys.buffer[keyIndex].Value.ColorValue;
			RemoveKey(keyIndex);
			return AddKey(color, newTime);
		}

		public float3 Evaluate(float time)
		{
			keys.buffer = keys.GetBuffer(entityManager);
			ColorKeyData keyLeft = keys.buffer[0].Value;
			ColorKeyData keyRight = keys.buffer[keys.buffer.Length - 1].Value;

			for (int i = 0; i < keys.buffer.Length; i++)
			{
				if (keys.buffer[i].Value.Time <= time)
				{
					keyLeft = keys.buffer[i].Value;
				}
				if (keys.buffer[i].Value.Time >= time)
				{
					keyRight = keys.buffer[i].Value;
					break;
				}
			}

			if (blendMode == MegaGradient.BlendMode.Linear)
			{
				float blendTime = Mathf.InverseLerp(keyLeft.Time, keyRight.Time, time);
				return math.lerp(keyLeft.ColorValue, keyRight.ColorValue, blendTime);
				//	return Color.Lerp(keyLeft.Color, keyRight.Color, blendTime);
			}
			else
			{
				return keyRight.ColorValue;
			}
		}

		public Color EvaluateColor(float time)
		{
			float3 color = Evaluate(time);
			return new Color(color.x, color.y, color.z);
		}

		public Texture2D GetTexture(int width)
		{

			Texture2D texture = new Texture2D(width, 1);
			Color[] colors = new Color[width];
			for (int i = 0; i < width; i++)
			{
				colors[i] = EvaluateColor((float)i / (width - 1));
			}
			texture.SetPixels(colors);
			texture.Apply();
			return texture;
		}




		// overrides


		/// <summary>
		/// MegaGradientData instances are equal if they refer to the same Value.
		/// </summary>
		/// <param name="lhs">An MegaGradientData object.</param>
		/// <param name="rhs">Another MegaGradientData object.</param>
		/// <returns>True, if both values are identical.</returns>
		public static bool operator ==(MegaGradientData lhs, MegaGradientData rhs)
		{
			return lhs.blendMode == rhs.blendMode && lhs.randomizeColor == rhs.randomizeColor
				&& lhs.keys == rhs.keys;
		}

		/// <summary>
		/// MegaGradientData instances are equal if they refer to the same Value.
		/// </summary>
		/// <param name="lhs">An MegaGradientData object.</param>
		/// <param name="rhs">Another MegaGradientData object.</param>
		/// <returns>True, if either value is different.</returns>
		public static bool operator !=(MegaGradientData lhs, MegaGradientData rhs)
		{
			return !(lhs == rhs);
		}


		/// <summary>
		/// MegaGradientData instances are equal if they refer to the same MegaGradientData.
		/// </summary>
		/// <param name="compare">The object to compare to this MegaGradientData.</param>
		/// <returns>True, if the compare parameter contains an MegaGradientData object having the same value
		/// as this MegaGradientData.</returns>
		public override bool Equals(object compare)
		{
			return this == (MegaGradientData)compare;
		}

		/// <summary>
		/// A hash used for comparisons.
		/// </summary>
		/// <returns>A unique hash code.</returns>
		public override int GetHashCode()
		{
			return base.GetHashCode();
		}



		public static MegaGradientData Null => new MegaGradientData();
	}

}