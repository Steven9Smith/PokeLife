using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;


namespace Core.Common.Buffers
{
	public class Float3BufferElementComponent : MonoBehaviour, IConvertGameObjectToEntity
	{
		public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
		{
			dstManager.AddBuffer<Float3BufferElement>(entity);
		}
	}

	public struct DynamicBufferFloat3BufferElement : IComponentData
	{

		// Entity where the buffer is located
		//	public Entity mEntity;

		/// <summary>
		/// Get the buffers
		/// </summary>
		/// <param name="entityManager"></param>
		/// <param name="entity"></param>
		/// <returns></returns>
		public static DynamicBuffer<Float3BufferElement> GetBuffer(EntityManager entityManager, Entity entity)
		{
			return entityManager.GetBuffer<Float3BufferElement>(entity);
		}

		/// <summary>
		/// set the buffer elements of the given aBuffer
		/// </summary>
		/// <param name="entityManager"></param>
		/// <param name="entity"></param>
		/// <param name="bBuffer"></param>
		public static void SetBuffer(EntityManager entityManager, Entity entity, DynamicBuffer<Float3BufferElement> bBuffer)
		{
			DynamicBuffer<Float3BufferElement> aBuffer = GetBuffer(entityManager, entity);
			aBuffer.Clear();
			aBuffer.CopyFrom(bBuffer);
		}

		/// <summary>
		/// set the buffer elements of the given aBuffer
		/// </summary>
		/// <param name="ecb"></param>
		/// <param name="aBuffer"></param>
		/// <param name="bBuffer"></param>
		public static void SetBuffer(EntityCommandBuffer.ParallelWriter ecb, ref DynamicBuffer<Float3BufferElement> aBuffer, DynamicBuffer<Float3BufferElement> bBuffer)
		{
			aBuffer.Clear();
			aBuffer.CopyFrom(bBuffer);
		}

		/// <summary>
		/// Adds the buffer to the entity and returns the buffer
		/// </summary>
		/// <param name="ecb"></param>
		/// <param name="entity"></param>
		/// <returns></returns>
		public static DynamicBuffer<Float3BufferElement> AddBuffer(EntityCommandBuffer.ParallelWriter ecb, int jobIndex, Entity entity)
		{
			return ecb.AddBuffer<Float3BufferElement>(jobIndex, entity);
		}

		/// <summary>
		/// Adds the buffer to the entity and returns the buffer
		/// </summary>
		/// <param name="entityManager"></param>
		/// <returns></returns>
		public static DynamicBuffer<Float3BufferElement> AddBuffer(EntityManager entityManager, Entity entity)
		{
			return entityManager.AddBuffer<Float3BufferElement>(entity);
		}

		/// <summary>
		/// converts the DynamicBuffer<Float3BufferElement> to an Float3BufferElement Arrary
		/// </summary>
		/// <returns></returns>
		public static Float3BufferElement[] ToArray(DynamicBuffer<Float3BufferElement> buffer)
		{
			Float3BufferElement[] arr = new Float3BufferElement[buffer.Length];
			for (int i = 0; i < buffer.Length; i++)
				arr[i] = buffer[i];
			return arr;
		}

		/// <summary>
		/// converts the DynamicBuffer<Float3BufferElement> to an Entity rrary
		/// </summary>
		/// <param name="Value"></param>
		/// <returns></returns>
		public float3[] ToFloatArray(DynamicBuffer<Float3BufferElement> buffer)
		{
			float3[] arr = new float3[buffer.Length];
			for (int i = 0; i < buffer.Length; i++)
				arr[i] = buffer[i].Value;
			return arr;
		}
	}

	[System.Serializable]
	public struct Float3BufferElement : IBufferElementData
	{
		public float3 Value;
		public int Index;

		/// <summary>
		/// Initializes a FlaotBufferElement
		/// </summary>
		/// <param name="value"></param>
		/// <param name="index"></param>
		public Float3BufferElement(float3 value = new float3(), int index = -1)
		{
			Value = value;
			Index = index;
		}

		/// <summary>
		/// Converts a NativeArray to a dynamic buffer
		/// </summary>
		/// <param name="buf">A buffer that has been initialized by an Entity</param>
		/// <param name="arr">The NativeArray to convert (must be same size )</param>
		/// <returns>returns true of the operation was successful</returns>
		public static bool ToDyanamicBuffer(ref DynamicBuffer<Float3BufferElement> buf, NativeArray<Float3BufferElement> arr)
		{
			if (arr.Length != buf.Length)
			{
				Debug.LogWarning("given array and value size are not the same length");
				return false;
			}
			for (int i = 0; i < arr.Length; i++)
				buf[i] = arr[i];
			arr.Dispose();
			return true;
		}
		/// <summary>
		/// converts a float3[] into a Float3BufferElement[]
		/// </summary>
		/// <param name="arr"></param>
		/// <returns></returns>
		public static Float3BufferElement[] ToFloat3BufferElementArray(float3[] arr)
		{
			Float3BufferElement[] tmp = new Float3BufferElement[arr.Length];
			for (int i = 0; i < arr.Length; i++)
				tmp[i] = new Float3BufferElement { Value = arr[i] };
			return tmp;
		}

		// Dynamic Buffer stuff


		/// <summary>
		/// Get the buffers
		/// </summary>
		/// <param name="entityManager"></param>
		/// <param name="entity"></param>
		/// <returns></returns>
		public static DynamicBuffer<Float3BufferElement> GetBuffer(EntityManager entityManager, Entity entity)
		{
			return entityManager.GetBuffer<Float3BufferElement>(entity);
		}

		/// <summary>
		/// set the buffer elements of the given aBuffer
		/// </summary>
		/// <param name="entityManager"></param>
		/// <param name="entity"></param>
		/// <param name="bBuffer"></param>
		public static void SetBuffer(EntityManager entityManager, Entity entity, DynamicBuffer<Float3BufferElement> bBuffer)
		{
			DynamicBuffer<Float3BufferElement> aBuffer = GetBuffer(entityManager, entity);
			aBuffer.Clear();
			aBuffer.CopyFrom(bBuffer);
		}

		/// <summary>
		/// set the buffer elements of the given aBuffer
		/// </summary>
		/// <param name="ecb"></param>
		/// <param name="aBuffer"></param>
		/// <param name="bBuffer"></param>
		public static void SetBuffer(EntityCommandBuffer.ParallelWriter ecb, DynamicBuffer<Float3BufferElement> aBuffer, DynamicBuffer<Float3BufferElement> bBuffer)
		{
			aBuffer.Clear();
			aBuffer.CopyFrom(bBuffer);
		}

		/// <summary>
		/// Adds the buffer to the entity and returns the buffer
		/// </summary>
		/// <param name="ecb"></param>
		/// <param name="entity"></param>
		/// <returns></returns>
		public static DynamicBuffer<Float3BufferElement> AddBuffer(EntityCommandBuffer.ParallelWriter ecb, int jobIndex, Entity entity)
		{
			return ecb.AddBuffer<Float3BufferElement>(jobIndex, entity);
		}

		/// <summary>
		/// Adds the buffer to the entity and returns the buffer
		/// </summary>
		/// <param name="entityManager"></param>
		/// <returns></returns>
		public static DynamicBuffer<Float3BufferElement> AddBuffer(EntityManager entityManager, Entity entity)
		{
			return entityManager.AddBuffer<Float3BufferElement>(entity);
		}

		/// <summary>
		/// converts the DynamicBuffer<Float3BufferElement> to an Float3BufferElement Arrary
		/// </summary>
		/// <returns></returns>
		public static Float3BufferElement[] ToArray(DynamicBuffer<Float3BufferElement> buffer)
		{
			Float3BufferElement[] arr = new Float3BufferElement[buffer.Length];
			for (int i = 0; i < buffer.Length; i++)
				arr[i] = buffer[i];
			return arr;
		}

		/// <summary>
		/// converts the DynamicBuffer<Float3BufferElement> to an Entity rrary
		/// </summary>
		/// <param name="Value"></param>
		/// <returns></returns>
		public float3[] ToFloatArray(DynamicBuffer<Float3BufferElement> buffer)
		{
			float3[] arr = new float3[buffer.Length];
			for (int i = 0; i < buffer.Length; i++)
				arr[i] = buffer[i].Value;
			return arr;
		}

		// Float3BufferElement on float3

		public static bool operator ==(Float3BufferElement lhs, float3 rhs)
		{
			return lhs.Value.Equals(rhs);
		}

		public static bool operator !=(Float3BufferElement lhs, float3 rhs)
		{
			return !(lhs == rhs);
		}

		// float3 on Float3BufferElement

		public static bool operator ==(float3 lhs, Float3BufferElement rhs)
		{
			return lhs.Equals(rhs.Value);
		}

		public static bool operator !=(float3 lhs, Float3BufferElement rhs)
		{
			return !(lhs == rhs);
		}

		// Float3BufferElement on Float3BufferElement

		/// <summary>
		/// Float3BufferElement instances are equal if they refer to the same Value.
		/// </summary>
		/// <param name="lhs">An Float3BufferElement object.</param>
		/// <param name="rhs">Another Float3BufferElement object.</param>
		/// <returns>True, if both values are identical.</returns>
		public static bool operator ==(Float3BufferElement lhs, Float3BufferElement rhs)
		{
			return lhs.Value.Equals(rhs.Value);
		}

		/// <summary>
		/// Float3BufferElement instances are equal if they refer to the same Value.
		/// </summary>
		/// <param name="lhs">An Float3BufferElement object.</param>
		/// <param name="rhs">Another Float3BufferElement object.</param>
		/// <returns>True, if either value is different.</returns>
		public static bool operator !=(Float3BufferElement lhs, Float3BufferElement rhs)
		{
			return !(lhs == rhs);
		}

		// other

		/// <summary>
		/// Float3BufferElement instances are equal if they refer to the same Float3BufferElement.
		/// </summary>
		/// <param name="compare">The object to compare to this Float3BufferElement.</param>
		/// <returns>True, if the compare parameter contains an Float3BufferElement object having the same value
		/// as this Float3BufferElement.</returns>
		public override bool Equals(object compare)
		{
			return this == (Float3BufferElement)compare;
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
		/// A "blank" Float3BufferElement object that does not refer to an actual entity.
		/// </summary>
		public static Float3BufferElement Null => new Float3BufferElement();

	}
}
