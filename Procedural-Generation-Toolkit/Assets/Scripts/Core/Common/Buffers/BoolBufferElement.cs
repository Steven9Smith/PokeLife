using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;


namespace Core.Common.Buffers
{

	public class BoolBufferElementComponent : MonoBehaviour, IConvertGameObjectToEntity
	{
		public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
		{
			dstManager.AddBuffer<BoolBufferElement>(entity);
		}
	}
	[System.Serializable]
	public struct BoolBufferElement : IBufferElementData
	{
		public BlittableBool Value;

		public DynamicBuffer<BoolBufferElement> ArrayToDynamicBuffer(BlittableBool[] array)
		{
			if (array.Length == 0)
				return new DynamicBuffer<BoolBufferElement> { };
			DynamicBuffer<BoolBufferElement> buffer = new DynamicBuffer<BoolBufferElement> { };
			for (int i = 0; i < array.Length; i++)
				buffer.Add(new BoolBufferElement { Value = array[i] });
			return buffer;
		}

		public DynamicBuffer<BoolBufferElement> ListToDynamicBuffer(List<BlittableBool> list)
		{
			if (list.Count == 0)
				return new DynamicBuffer<BoolBufferElement> { };
			DynamicBuffer<BoolBufferElement> buffer = new DynamicBuffer<BoolBufferElement> { };
			for (int i = 0; i < list.Count; i++)
				buffer.Add(new BoolBufferElement { Value = list[i] });
			return buffer;

		}

		public DynamicBuffer<BoolBufferElement> NativeArrayToDynamicBuffer(NativeArray<BlittableBool> array, bool DeallocateOnFinish = true)
		{
			if (array.Length == 0)
				return new DynamicBuffer<BoolBufferElement> { };
			DynamicBuffer<BoolBufferElement> buffer = new DynamicBuffer<BoolBufferElement> { };
			for (int i = 0; i < array.Length; i++)
				buffer.Add(new BoolBufferElement { Value = array[i] });
			if (DeallocateOnFinish)
				array.Dispose();
			return buffer;
		}

		public BlittableBool[] ToArray(DynamicBuffer<BoolBufferElement> buffer)
		{
			BlittableBool[] array = new BlittableBool[buffer.Capacity];
			int i = 0;
			foreach (BoolBufferElement element in buffer)
				array[i] = element.Value;
			return array;
		}

		public List<BlittableBool> ToList(DynamicBuffer<BoolBufferElement> buffer)
		{
			List<BlittableBool> list = new List<BlittableBool>(buffer.Capacity);
			int i = 0;
			foreach (BoolBufferElement element in buffer)
				list[i] = element.Value;
			return list;
		}

		public NativeArray<BlittableBool> ToNativeArray(DynamicBuffer<BoolBufferElement> buffer, Allocator allocator = Allocator.TempJob)
		{
			NativeArray<BlittableBool> array = new NativeArray<BlittableBool>(buffer.Capacity, allocator);
			int i = 0;
			foreach (BoolBufferElement element in buffer)
				array[i] = element.Value;
			return array;
		}


		/// <summary>
		/// BoolBufferElement instances are equal if they refer to the same Value.
		/// </summary>
		/// <param name="lhs">An BoolBufferElement object.</param>
		/// <param name="rhs">Another BoolBufferElement object.</param>
		/// <returns>True, if both values are identical.</returns>
		public static bool operator ==(BoolBufferElement lhs, BoolBufferElement rhs)
		{
			return lhs.Value == rhs.Value;
		}

		/// <summary>
		/// BoolBufferElement instances are equal if they refer to the same Value.
		/// </summary>
		/// <param name="lhs">An BoolBufferElement object.</param>
		/// <param name="rhs">Another BoolBufferElement object.</param>
		/// <returns>True, if either value is different.</returns>
		public static bool operator !=(BoolBufferElement lhs, BoolBufferElement rhs)
		{
			return !(lhs == rhs);
		}


		/// <summary>
		/// BoolBufferElement instances are equal if they refer to the same BoolBufferElement.
		/// </summary>
		/// <param name="compare">The object to compare to this BoolBufferElement.</param>
		/// <returns>True, if the compare parameter contains an BoolBufferElement object having the same value
		/// as this BoolBufferElement.</returns>
		public override bool Equals(object compare)
		{
			return this == (BoolBufferElement)compare;
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
		/// A "blank" BoolBufferElement object that does not refer to an actual entity.
		/// </summary>
		public static BoolBufferElement Null => new BoolBufferElement();

	}

	[System.Serializable]
	public struct DynamicBufferBoolBufferElement : IBufferElementData
	{
		public DynamicBuffer<BoolBufferElement> Value;

		/// <summary>
		/// DynamicBufferBoolBufferElement instances are equal if they refer to the same Value.
		/// </summary>
		/// <param name="lhs">An DynamicBufferBoolBufferElement object.</param>
		/// <param name="rhs">Another DynamicBufferBoolBufferElement object.</param>
		/// <returns>True, if both values are identical.</returns>
		public static bool operator ==(DynamicBufferBoolBufferElement lhs, DynamicBufferBoolBufferElement rhs)
		{
			for (int i = 0; i < lhs.Value.Length; i++)
				if (lhs.Value[i].Value != rhs.Value[i].Value)
					return false;
			return true;
		}

		/// <summary>
		/// DynamicBufferBoolBufferElement instances are equal if they refer to the same Value.
		/// </summary>
		/// <param name="lhs">An DynamicBufferBoolBufferElement object.</param>
		/// <param name="rhs">Another DynamicBufferBoolBufferElement object.</param>
		/// <returns>True, if either value is different.</returns>
		public static bool operator !=(DynamicBufferBoolBufferElement lhs, DynamicBufferBoolBufferElement rhs)
		{
			return !(lhs == rhs);
		}

		/// <summary>
		/// DynamicBufferBoolBufferElement instances are equal if they refer to the same DynamicBufferBoolBufferElement.
		/// </summary>
		/// <param name="compare">The object to compare to this DynamicBufferBoolBufferElement.</param>
		/// <returns>True, if the compare parameter contains an DynamicBufferBoolBufferElement object having the same value
		/// as this DynamicBufferBoolBufferElement.</returns>
		public override bool Equals(object compare)
		{
			return this == (DynamicBufferBoolBufferElement)compare;
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
		/// A "blank" DynamicBufferBoolBufferElement object that does not refer to an actual entity.
		/// </summary>
		public static DynamicBufferBoolBufferElement Null => new DynamicBufferBoolBufferElement();
	}

}
