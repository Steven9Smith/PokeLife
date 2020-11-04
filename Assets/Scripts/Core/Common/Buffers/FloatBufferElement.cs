using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;


namespace Core.Common.Buffers
{
	public class FloatBufferElementComponent : MonoBehaviour, IConvertGameObjectToEntity
	{
		public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
		{
			dstManager.AddBuffer<FloatBufferElement>(entity);
		}
	}

	public struct DynamicBufferFloatBufferElement : IComponentData{

		// Entity where the buffer is located
		//	public Entity mEntity;

		/// <summary>
		/// Get the buffers
		/// </summary>
		/// <param name="entityManager"></param>
		/// <param name="entity"></param>
		/// <returns></returns>
		public static DynamicBuffer<FloatBufferElement> GetBuffer(EntityManager entityManager,Entity entity)
		{
			return entityManager.GetBuffer<FloatBufferElement>(entity);
		}

		/// <summary>
		/// set the buffer elements of the given aBuffer
		/// </summary>
		/// <param name="entityManager"></param>
		/// <param name="entity"></param>
		/// <param name="bBuffer"></param>
		public static void SetBuffer(EntityManager entityManager,Entity entity, DynamicBuffer<FloatBufferElement> bBuffer)
		{
			DynamicBuffer<FloatBufferElement> aBuffer = GetBuffer(entityManager, entity);
			aBuffer.Clear();
			aBuffer.CopyFrom(bBuffer);
		}

		/// <summary>
		/// set the buffer elements of the given aBuffer
		/// </summary>
		/// <param name="ecb"></param>
		/// <param name="aBuffer"></param>
		/// <param name="bBuffer"></param>
		public static void SetBuffer(EntityCommandBuffer.ParallelWriter ecb, ref DynamicBuffer<FloatBufferElement> aBuffer, DynamicBuffer<FloatBufferElement> bBuffer)
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
		public static DynamicBuffer<FloatBufferElement> AddBuffer(EntityCommandBuffer.ParallelWriter ecb,int jobIndex, Entity entity)
		{
			return ecb.AddBuffer<FloatBufferElement>(jobIndex,entity);
		}
		
		/// <summary>
		/// Adds the buffer to the entity and returns the buffer
		/// </summary>
		/// <param name="entityManager"></param>
		/// <returns></returns>
		public static DynamicBuffer<FloatBufferElement> AddBuffer(EntityManager entityManager, Entity entity)
		{
			return entityManager.AddBuffer<FloatBufferElement>(entity);
		}

		/// <summary>
		/// converts the DynamicBuffer<FloatBufferElement> to an FloatBufferElement Arrary
		/// </summary>
		/// <returns></returns>
		public static FloatBufferElement[] ToArray(DynamicBuffer<FloatBufferElement> buffer)
		{
			FloatBufferElement[] arr = new FloatBufferElement[buffer.Length];
			for (int i = 0; i < buffer.Length; i++)
				arr[i] = buffer[i];
			return arr;
		}

		/// <summary>
		/// converts the DynamicBuffer<FloatBufferElement> to an Entity rrary
		/// </summary>
		/// <param name="Value"></param>
		/// <returns></returns>
		public float[] ToFloatArray(DynamicBuffer<FloatBufferElement> buffer)
		{
			float[] arr = new float[buffer.Length];
			for (int i = 0; i < buffer.Length; i++)
				arr[i] = buffer[i].Value;
			return arr;
		}
/*
		/// <summary>
		/// DynamicBufferFloatBufferElement instances are equal if they refer to the same Value.
		/// </summary>
		/// <param name="lhs">An DynamicBufferFloatBufferElement object.</param>
		/// <param name="rhs">Another DynamicBufferFloatBufferElement object.</param>
		/// <returns>True, if both values are identical.</returns>
		public static bool operator ==(DynamicBufferFloatBufferElement lhs, DynamicBufferFloatBufferElement rhs)
		{
			return lhs.mEntity == rhs.mEntity;
		}

		/// <summary>
		/// DynamicBufferFloatBufferElement instances are equal if they refer to the same Value.
		/// </summary>
		/// <param name="lhs">An DynamicBufferFloatBufferElement object.</param>
		/// <param name="rhs">Another DynamicBufferFloatBufferElement object.</param>
		/// <returns>True, if either value is different.</returns>
		public static bool operator !=(DynamicBufferFloatBufferElement lhs, DynamicBufferFloatBufferElement rhs)
		{
			return !(lhs == rhs);
		}

		/// <summary>
		/// DynamicBufferFloatBufferElement instances are equal if they refer to the same DynamicBufferFloatBufferElement.
		/// </summary>
		/// <param name="compare">The object to compare to this DynamicBufferFloatBufferElement.</param>
		/// <returns>True, if the compare parameter contains an DynamicBufferFloatBufferElement object having the same value
		/// as this DynamicBufferFloatBufferElement.</returns>
		public override bool Equals(object compare)
		{
			return this == (DynamicBufferFloatBufferElement)compare;
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
		/// Destroys mEntity if it is not null
		/// </summary>
		/// <param name="entityManager"></param>
		public void Destroy(EntityManager entityManager)
		{
			if (mEntity != Entity.Null)
				entityManager.DestroyEntity(mEntity);
		}

		/// <summary>
		/// A "blank" DynamicBufferFloatBufferElement object that does not refer to an actual entity.
		/// </summary>
		public static DynamicBufferFloatBufferElement Null => new DynamicBufferFloatBufferElement();
		*/
	}

	[System.Serializable]
	public struct FloatBufferElement : IBufferElementData
	{
		public float Value;
		public int Index;

		/// <summary>
		/// Initializes a FlaotBufferElement
		/// </summary>
		/// <param name="value"></param>
		/// <param name="index"></param>
		public FloatBufferElement(float value = 0f, int index = -1)
		{
			Value = value;
			Index = index;
		}

		/// <summary>
		/// Sorts the Dynamic Buffer by value
		/// </summary>
		public static void Sort(ref DynamicBuffer<FloatBufferElement> buffer,Allocator allocator)
		{
			NativeArray<FloatBufferElement> tmp = buffer.ToNativeArray(allocator);
			Sort(ref tmp,allocator);
			ToDyanamicBuffer(ref buffer,tmp);
		}

		public static void Sort(ref NativeArray<FloatBufferElement> arr,Allocator allocator)
		{
			MergeSort(ref arr, 0, arr.Length - 1,allocator);
		}

		/// <summary>
		/// performs a merge of the given arrays using the start, middle, and end indexcies
		/// Merges two subarrays of arr[]. 
		/// First subarray is arr[l..m] 
		/// Second subarray is arr[m+1..r] 
		/// </summary>
		/// <param name="arr">array to preform operation on</param>
		/// <param name="start">start index</param>
		/// <param name="middle">middle index</param>
		/// <param name="end">end index</param>
		private static void Merge(ref NativeArray<FloatBufferElement> arr, int start, int middle, int end,Allocator allocator)
		{

			int i, j, k;
			int n1 = middle - start + 1;
			int n2 = end - middle;

			/* create temp arrays */
			NativeArray<FloatBufferElement> L = new NativeArray<FloatBufferElement>(n1, allocator);
			NativeArray<FloatBufferElement> R = new NativeArray<FloatBufferElement>(n2, allocator);

			//	Debug.Log(n1 + "," + n2);

			/* Copy data to temp arrays L[] and R[] */
			for (i = 0; i < n1; i++)
				L[i] = arr[start + i];
			for (j = 0; j < n2; j++)
				R[j] = arr[middle + 1 + j];


			/* Merge the temp arrays back into arr[l..r]*/
			i = 0; // Initial index of first subarray 
			j = 0; // Initial index of second subarray 
			k = start; // Initial index of merged subarray 
			while (i < n1 && j < n2)
			{
				if (L[i] <= R[j])
				{
					arr[k] = L[i];
					i++;
				}
				else
				{
					arr[k] = R[j];
					j++;
				}
				k++;
			}

			/* Copy the remaining elements of L[], if there 
			   are any */
			while (i < n1)
			{
				arr[k] = L[i];
				i++;
				k++;
			}

			/* Copy the remaining elements of R[], if there 
			   are any */
			while (j < n2)
			{
				arr[k] = R[j];
				j++;
				k++;
			}
			L.Dispose();
			R.Dispose();
		}

		/* l is for left index and r is right index of the 
		   sub-array of arr to be sorted */
		private static void MergeSort(ref NativeArray<FloatBufferElement> arr, int l, int r,Allocator allocator)
		{
			if (l < r)
			{
				// Same as (l+r)/2, but avoids overflow for 
				// large l and h 
				int m = l + (r - l) / 2;

				// Sort first and second halves 
				MergeSort(ref arr, l, m,allocator);
				MergeSort(ref arr, m + 1, r,allocator);

				Merge(ref arr, l, m, r,allocator);
			}
		}
		
		/// <summary>
		/// Converts a NativeArray to a dynamic buffer
		/// </summary>
		/// <param name="buf">A buffer that has been initialized by an Entity</param>
		/// <param name="arr">The NativeArray to convert (must be same size )</param>
		/// <returns>returns true of the operation was successful</returns>
		public static bool ToDyanamicBuffer(ref DynamicBuffer<FloatBufferElement> buf, NativeArray<FloatBufferElement> arr)
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
		/// converts a float[] into a FloatBufferElement[]
		/// </summary>
		/// <param name="arr"></param>
		/// <returns></returns>
		public static FloatBufferElement[] ToFloatBufferElementArray(float[] arr)
		{
			FloatBufferElement[] tmp = new FloatBufferElement[arr.Length];
			for (int i = 0; i < arr.Length; i++)
				tmp[i] = new FloatBufferElement { Value = arr[i] };
			return tmp;
		}

		// Dynamic Buffer stuff


		/// <summary>
		/// Get the buffers
		/// </summary>
		/// <param name="entityManager"></param>
		/// <param name="entity"></param>
		/// <returns></returns>
		public static DynamicBuffer<FloatBufferElement> GetBuffer(EntityManager entityManager, Entity entity)
		{
			return entityManager.GetBuffer<FloatBufferElement>(entity);
		}

		/// <summary>
		/// set the buffer elements of the given aBuffer
		/// </summary>
		/// <param name="entityManager"></param>
		/// <param name="entity"></param>
		/// <param name="bBuffer"></param>
		public static void SetBuffer(EntityManager entityManager, Entity entity, DynamicBuffer<FloatBufferElement> bBuffer)
		{
			DynamicBuffer<FloatBufferElement> aBuffer = GetBuffer(entityManager, entity);
			aBuffer.Clear();
			aBuffer.CopyFrom(bBuffer);
		}

		/// <summary>
		/// set the buffer elements of the given aBuffer
		/// </summary>
		/// <param name="ecb"></param>
		/// <param name="aBuffer"></param>
		/// <param name="bBuffer"></param>
		public static void SetBuffer(EntityCommandBuffer.ParallelWriter ecb,DynamicBuffer<FloatBufferElement> aBuffer, DynamicBuffer<FloatBufferElement> bBuffer)
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
		public static DynamicBuffer<FloatBufferElement> AddBuffer(EntityCommandBuffer.ParallelWriter ecb,int jobIndex, Entity entity)
		{
			return ecb.AddBuffer<FloatBufferElement>(jobIndex,entity);
		}

		/// <summary>
		/// Adds the buffer to the entity and returns the buffer
		/// </summary>
		/// <param name="entityManager"></param>
		/// <returns></returns>
		public static DynamicBuffer<FloatBufferElement> AddBuffer(EntityManager entityManager, Entity entity)
		{
			return entityManager.AddBuffer<FloatBufferElement>(entity);
		}

		/// <summary>
		/// converts the DynamicBuffer<FloatBufferElement> to an FloatBufferElement Arrary
		/// </summary>
		/// <returns></returns>
		public static FloatBufferElement[] ToArray(DynamicBuffer<FloatBufferElement> buffer)
		{
			FloatBufferElement[] arr = new FloatBufferElement[buffer.Length];
			for (int i = 0; i < buffer.Length; i++)
				arr[i] = buffer[i];
			return arr;
		}

		/// <summary>
		/// converts the DynamicBuffer<FloatBufferElement> to an Entity rrary
		/// </summary>
		/// <param name="Value"></param>
		/// <returns></returns>
		public float[] ToFloatArray(DynamicBuffer<FloatBufferElement> buffer)
		{
			float[] arr = new float[buffer.Length];
			for (int i = 0; i < buffer.Length; i++)
				arr[i] = buffer[i].Value;
			return arr;
		}

		// FloatBufferElement on float

		public static bool operator ==(FloatBufferElement lhs,float rhs)
		{
			return lhs.Value == rhs;
		}

		public static bool operator !=(FloatBufferElement lhs,float rhs)
		{
			return !(lhs == rhs);
		}

		public static bool operator <=(FloatBufferElement lhs, float rhs)
		{
			return lhs.Value == rhs || lhs.Value < rhs;
		}

		public static bool operator >=(FloatBufferElement lhs, float rhs)
		{
			return lhs.Value == rhs || lhs.Value > rhs;
		}

		public static bool operator <(FloatBufferElement lhs, float rhs)
		{
			return lhs.Value < rhs;
		}

		public static bool operator >(FloatBufferElement lhs, float rhs)
		{
			return lhs.Value > rhs;
		}

		public int CompareTo(float other)
		{
			float tmp = Value - other;
			if (tmp < 0) return -1;
			else if (tmp == 0) return 0;
			else return 1;
		}


		// float on FloatBufferElement

		public static bool operator ==(float lhs, FloatBufferElement rhs)
		{
			return lhs == rhs.Value;
		}

		public static bool operator !=(float lhs, FloatBufferElement rhs)
		{
			return !(lhs == rhs);
		}

		public static bool operator <=(float lhs, FloatBufferElement rhs)
		{
			return lhs == rhs.Value || lhs < rhs.Value;
		}

		public static bool operator >=(float lhs, FloatBufferElement rhs)
		{
			return lhs == rhs.Value || lhs > rhs.Value;
		}

		public static bool operator <(float lhs, FloatBufferElement rhs)
		{
			return lhs < rhs.Value;
		}

		public static bool operator >(float lhs, FloatBufferElement rhs)
		{
			return lhs > rhs.Value;
		}


		// FloatBufferElement on FloatBufferElement

		/// <summary>
		/// FloatBufferElement instances are equal if they refer to the same Value.
		/// </summary>
		/// <param name="lhs">An FloatBufferElement object.</param>
		/// <param name="rhs">Another FloatBufferElement object.</param>
		/// <returns>True, if both values are identical.</returns>
		public static bool operator ==(FloatBufferElement lhs, FloatBufferElement rhs)
		{
			return lhs.Value == rhs.Value;
		}

		/// <summary>
		/// FloatBufferElement instances are equal if they refer to the same Value.
		/// </summary>
		/// <param name="lhs">An FloatBufferElement object.</param>
		/// <param name="rhs">Another FloatBufferElement object.</param>
		/// <returns>True, if either value is different.</returns>
		public static bool operator !=(FloatBufferElement lhs, FloatBufferElement rhs)
		{
			return !(lhs == rhs);
		}

		/// <summary>
		/// FloatBufferElement instances are less than or equal if the lhs value is less than or equal to the rhs value.
		/// </summary>
		/// <param name="lhs">An FloatBufferElement object.</param>
		/// <param name="rhs">Another FloatBufferElement object.</param>
		/// <returns>True, if the lhs value is less than or equal to the rhs value.</returns>
		public static bool operator <=(FloatBufferElement lhs, FloatBufferElement rhs)
		{
			return lhs.Value == rhs.Value || lhs.Value < rhs.Value;
		}

		/// <summary>
		/// FloatBufferElement instances are greater than or equal if the lhs value is greater than or equal to the rhs value.
		/// </summary>
		/// <param name="lhs">An FloatBufferElement object.</param>
		/// <param name="rhs">Another FloatBufferElement object.</param>
		/// <returns>True, if the lhs value is greater than or equal to the rhs value.</returns>
		public static bool operator >=(FloatBufferElement lhs, FloatBufferElement rhs)
		{
			return lhs.Value == rhs.Value || lhs.Value > rhs.Value;
		}

		/// <summary>
		/// FloatBufferElement instances are less than if the lhs value is less than the rhs value.
		/// </summary>
		/// <param name="lhs">An FloatBufferElement object.</param>
		/// <param name="rhs">Another FloatBufferElement object.</param>
		/// <returns>True, if the lhs value is less than the rhs value.</returns>
		public static bool operator <(FloatBufferElement lhs, FloatBufferElement rhs)
		{
			return lhs.Value < rhs.Value;
		}

		/// <summary>
		/// FloatBufferElement instances are greater than if the lhs value is greater than the rhs value.
		/// </summary>
		/// <param name="lhs">An FloatBufferElement object.</param>
		/// <param name="rhs">Another FloatBufferElement object.</param>
		/// <returns>True, if the lhs value is greater than the rhs value.</returns>
		public static bool operator >(FloatBufferElement lhs, FloatBufferElement rhs)
		{
			return lhs.Value > rhs.Value;
		}

		/// <summary>
		/// Compare this FloatBufferElement against a given one
		/// </summary>
		/// <param name="other">The other FloatBufferElement to compare to</param>
		/// <returns>Difference based on the FloatBufferElement value</returns>
		public int CompareTo(FloatBufferElement other)
		{
			float tmp = Value - other.Value;
			if (tmp < 0) return -1;
			else if (tmp == 0) return 0;
			else return 1;
		}

		// other

		/// <summary>
		/// FloatBufferElement instances are equal if they refer to the same FloatBufferElement.
		/// </summary>
		/// <param name="compare">The object to compare to this FloatBufferElement.</param>
		/// <returns>True, if the compare parameter contains an FloatBufferElement object having the same value
		/// as this FloatBufferElement.</returns>
		public override bool Equals(object compare)
		{
			return this == (FloatBufferElement)compare;
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
		/// A "blank" FloatBufferElement object that does not refer to an actual entity.
		/// </summary>
		public static FloatBufferElement Null => new FloatBufferElement();

	}
}
