using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using Unity.Mathematics;
using Core.MegaGradient.Buffers;

namespace Core.MegaGradient
{
	public class MegaGradientComponent : MonoBehaviour
	{
		public MegaGradient gradient; 
		// Use this for initialization
		void Start()
		{

		}

		// Update is called once per frame
		void Update()
		{

		}
	}
	


	[System.Serializable]
	public class MegaGradient
	{
		[SerializeField]
		List<ColorKey> keys = new List<ColorKey>();
		public enum BlendMode { Linear, Discrete };
		public BlendMode blendMode;
		public bool randomizeColor;
		public bool hide = false;

		public MegaGradient()
		{
			AddKey(Color.black, 0);
			AddKey(Color.white, 1);
		}

		public int NumKeys
		{
			get { return keys.Count; }
		}

		public ColorKey GetKey(int i)
		{
			return keys[i];
		}

		public void RemoveKey(int index)
		{
			keys.RemoveAt(index);
		}

		public int AddKey(Color color, float time)
		{
			ColorKey newKeys = new ColorKey(color, time);
			for (int i = 0; i < keys.Count; i++)
				if (newKeys.Time < keys[i].Time)
				{
					keys.Insert(i, newKeys);
					return i;
				}
			keys.Add(newKeys);
			return keys.Count - 1;
		}

		public void UpdateKeyColor(int index, Color color)
		{
			keys[index] = new ColorKey(color, keys[index].Time);
		}

		public int UpdateKeyTime(int keyIndex, float newTime)
		{
			Color color = keys[keyIndex].Color;
			RemoveKey(keyIndex);
			return AddKey(color, newTime);
		}

		public Color Evaluate(float time)
		{
				ColorKey keyLeft = keys[0];
			ColorKey keyRight = keys[keys.Count - 1];

			for (int i = 0; i < keys.Count; i++)
			{
				if (keys[i].Time <= time)
				{
					keyLeft = keys[i];
				}
				if (keys[i].Time >= time)
				{
					keyRight = keys[i];
					break;
				}
			}

			if (blendMode == BlendMode.Linear)
			{
				float blendTime = Mathf.InverseLerp(keyLeft.Time, keyRight.Time, time);
				return Color.Lerp(keyLeft.Color, keyRight.Color, blendTime);
			}
			else
			{
				return keyRight.Color;
			}
		}

		public Texture2D GetTexture(int width)
		{
			Texture2D texture = new Texture2D(width, 1);
			Color[] colors = new Color[width];
			for (int i = 0; i < width; i++)
			{
				colors[i] = Evaluate((float)i / (width - 1));
			}
			texture.SetPixels(colors);
			texture.Apply();
			return texture;
		}

		public static MegaGradient NULL => new MegaGradient();
	}


	
}
