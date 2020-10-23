using Unity.Collections;
using Unity.Entities;
using UnityEngine;


namespace Core.Common.Buffers{

	public class IntBufferElementComponent : MonoBehaviour, IConvertGameObjectToEntity
	{
		public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
		{
			dstManager.AddBuffer<IntBufferElement>(entity);
		}
	}
	[System.Serializable]
	public struct IntBufferElement : IBufferElementData
	{
		public int Value;


		/// <summary>
		/// Get the buffers
		/// </summary>
		/// <param name="entityManager"></param>
		/// <param name="entity"></param>
		/// <returns></returns>
		public static DynamicBuffer<IntBufferElement> GetBuffer(EntityManager entityManager, Entity entity)
		{
			return entityManager.GetBuffer<IntBufferElement>(entity);
		}

		/// <summary>
		/// set the buffer elements of the given aBuffer
		/// </summary>
		/// <param name="entityManager"></param>
		/// <param name="entity"></param>
		/// <param name="bBuffer"></param>
		public static void SetBuffer(EntityManager entityManager, Entity entity, DynamicBuffer<IntBufferElement> bBuffer)
		{
			DynamicBuffer<IntBufferElement> aBuffer = GetBuffer(entityManager, entity);
			aBuffer.Clear();
			aBuffer.CopyFrom(bBuffer);
		}

		/// <summary>
		/// set the buffer elements of the given aBuffer
		/// </summary>
		/// <param name="ecb"></param>
		/// <param name="aBuffer"></param>
		/// <param name="bBuffer"></param>
		public static void SetBuffer(EntityCommandBuffer ecb, DynamicBuffer<IntBufferElement> aBuffer, DynamicBuffer<IntBufferElement> bBuffer)
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
		public static DynamicBuffer<IntBufferElement> AddBuffer(EntityCommandBuffer ecb, Entity entity)
		{
			return ecb.AddBuffer<IntBufferElement>(entity);
		}

		/// <summary>
		/// Adds the buffer to the entity and returns the buffer
		/// </summary>
		/// <param name="entityManager"></param>
		/// <returns></returns>
		public static DynamicBuffer<IntBufferElement> AddBuffer(EntityManager entityManager, Entity entity)
		{
			return entityManager.AddBuffer<IntBufferElement>(entity);
		}


		public int[] ToArray(DynamicBuffer<IntBufferElement> buffer)
		{
			int[] array = new int[buffer.Capacity];
			int i = 0;
			foreach (IntBufferElement element in buffer)
				array[i] = element.Value;
			return array;
		}

		/// <summary>
		/// IntBufferElement instances are equal if they refer to the same Value.
		/// </summary>
		/// <param name="lhs">An IntBufferElement object.</param>
		/// <param name="rhs">Another IntBufferElement object.</param>
		/// <returns>True, if both values are identical.</returns>
		public static bool operator ==(IntBufferElement lhs, IntBufferElement rhs)
		{
			return lhs.Value == rhs.Value;
		}

		/// <summary>
		/// IntBufferElement instances are equal if they refer to the same Value.
		/// </summary>
		/// <param name="lhs">An IntBufferElement object.</param>
		/// <param name="rhs">Another IntBufferElement object.</param>
		/// <returns>True, if either value is different.</returns>
		public static bool operator !=(IntBufferElement lhs, IntBufferElement rhs)
		{
			return !(lhs == rhs);
		}

		/// <summary>
		/// IntBufferElement instances are less than or equal if the lhs value is less than or equal to the rhs value.
		/// </summary>
		/// <param name="lhs">An IntBufferElement object.</param>
		/// <param name="rhs">Another IntBufferElement object.</param>
		/// <returns>True, if the lhs value is less than or equal to the rhs value.</returns>
		public static bool operator <=(IntBufferElement lhs, IntBufferElement rhs)
		{
			return lhs.Value == rhs.Value || lhs.Value < rhs.Value;
		}

		/// <summary>
		/// IntBufferElement instances are greater than or equal if the lhs value is greater than or equal to the rhs value.
		/// </summary>
		/// <param name="lhs">An IntBufferElement object.</param>
		/// <param name="rhs">Another IntBufferElement object.</param>
		/// <returns>True, if the lhs value is greater than or equal to the rhs value.</returns>
		public static bool operator >=(IntBufferElement lhs, IntBufferElement rhs)
		{
			return lhs.Value == rhs.Value || lhs.Value > rhs.Value;
		}

		/// <summary>
		/// IntBufferElement instances are less than if the lhs value is less than the rhs value.
		/// </summary>
		/// <param name="lhs">An IntBufferElement object.</param>
		/// <param name="rhs">Another IntBufferElement object.</param>
		/// <returns>True, if the lhs value is less than the rhs value.</returns>
		public static bool operator <(IntBufferElement lhs, IntBufferElement rhs)
		{
			return lhs.Value < rhs.Value;
		}

		/// <summary>
		/// IntBufferElement instances are greater than if the lhs value is greater than the rhs value.
		/// </summary>
		/// <param name="lhs">An IntBufferElement object.</param>
		/// <param name="rhs">Another IntBufferElement object.</param>
		/// <returns>True, if the lhs value is greater than the rhs value.</returns>
		public static bool operator >(IntBufferElement lhs, IntBufferElement rhs)
		{
			return lhs.Value > rhs.Value;
		}

		/// <summary>
		/// Compare this IntBufferElement against a given one
		/// </summary>
		/// <param name="other">The other IntBufferElement to compare to</param>
		/// <returns>Difference based on the IntBufferElement value</returns>
		public int CompareTo(IntBufferElement other)
		{
			return Value - other.Value;
		}

		/// <summary>
		/// IntBufferElement instances are equal if they refer to the same IntBufferElement.
		/// </summary>
		/// <param name="compare">The object to compare to this IntBufferElement.</param>
		/// <returns>True, if the compare parameter contains an IntBufferElement object having the same value
		/// as this IntBufferElement.</returns>
		public override bool Equals(object compare)
		{
			return this == (IntBufferElement)compare;
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
		/// A "blank" IntBufferElement object that does not refer to an actual entity.
		/// </summary>
		public static IntBufferElement Null => new IntBufferElement();

	}


	public struct DynamicBufferIntBufferElement : IBufferElementData
	{

		// Entity where the buffer is located
		public Entity mEntity;

		/// <summary>
		/// returns the DynamicBuffer using the entity stoed in this struct
		/// </summary>
		/// <param name="entityManager"></param>
		/// <returns></returns>
		public DynamicBuffer<IntBufferElement> GetValue(EntityManager entityManager)
		{
			return entityManager.GetBuffer<IntBufferElement>(mEntity);
		}

		/// <summary>
		/// Use this if you want to get the DynamicBuffer<IntBufferElement> from the given entity
		/// </summary>
		/// <param name="entityManager"></param>
		/// <param name="entity"></param>
		/// <returns></returns>
		public static DynamicBuffer<IntBufferElement> GetValue(EntityManager entityManager, Entity entity)
		{
			return entityManager.GetBuffer<IntBufferElement>(entity);
		}

		/// <summary>
		/// Use this to initialize a dynamic buffer just to temporary use other wise use AddBuffer()
		/// </summary>
		/// <param name="entityManager"></param>
		public DynamicBufferIntBufferElement(EntityManager entityManager,bool addBuffer = false)
		{
			mEntity = entityManager.CreateEntity();
			entityManager.SetName(mEntity,"IntBufferElement_ComputeEntity");
			if (addBuffer)
				entityManager.AddBuffer<IntBufferElement>(mEntity);
		}

		/// <summary>
		/// Use this to initialize a dynamic buffer just to temporary use other wise use AddBuffer()
		/// </summary>
		/// <param name="ecb"></param>
		/// <param name="addBuffer"></param>
		public DynamicBufferIntBufferElement(EntityCommandBuffer ecb, bool addBuffer = false)
		{
			mEntity = ecb.CreateEntity();
			if (addBuffer)
				ecb.AddBuffer<IntBufferElement>(mEntity);
		}


		/// <summary>
		/// Use this to add IntBufferElements to the given entity NOTE: you can only have 1 per entity
		/// </summary>
		/// <param name="entityManager"></param>
		/// <param name="entity"></param>
		public void AddBuffer(EntityManager entityManager, Entity entity)
		{
			entityManager.AddBuffer<IntBufferElement>(entity);
		}

		/// <summary>
		/// Adds the buffer to the eneity and returns the buffer
		/// </summary>
		/// <param name="entityManager"></param>
		/// <returns></returns>
		public DynamicBuffer<IntBufferElement> AddBuffer(EntityManager entityManager)
		{
			return entityManager.AddBuffer<IntBufferElement>(mEntity);
		}

		/// <summary>
		/// converts the DynamicBuffer<IntBufferElement> to an IntBufferElement rrary
		/// </summary>
		/// <returns></returns>
		public static IntBufferElement[] ToArray(DynamicBuffer<IntBufferElement> buffer)
		{
			IntBufferElement[] arr = new IntBufferElement[buffer.Length];
			for (int i = 0; i < buffer.Length; i++)
				arr[i] = buffer[i];
			return arr;
		}

		/// <summary>
		/// converts the DynamicBuffer<IntBufferElement> to an Entity rrary
		/// </summary>
		/// <param name="Value"></param>
		/// <returns></returns>
		public int[] ToIntArray(DynamicBuffer<IntBufferElement> buffer)
		{
			int[] arr = new int[buffer.Length];
			for (int i = 0; i < buffer.Length; i++)
				arr[i] = buffer[i].Value;
			return arr;
		}

		/// <summary>
		/// DynamicBufferIntBufferElement instances are equal if they refer to the same Value.
		/// </summary>
		/// <param name="lhs">An DynamicBufferIntBufferElement object.</param>
		/// <param name="rhs">Another DynamicBufferIntBufferElement object.</param>
		/// <returns>True, if both values are identical.</returns>
		public static bool operator ==(DynamicBufferIntBufferElement lhs, DynamicBufferIntBufferElement rhs)
		{
			return lhs.mEntity == rhs.mEntity;
		}

		/// <summary>
		/// DynamicBufferIntBufferElement instances are equal if they refer to the same Value.
		/// </summary>
		/// <param name="lhs">An DynamicBufferIntBufferElement object.</param>
		/// <param name="rhs">Another DynamicBufferIntBufferElement object.</param>
		/// <returns>True, if either value is different.</returns>
		public static bool operator !=(DynamicBufferIntBufferElement lhs, DynamicBufferIntBufferElement rhs)
		{
			return !(lhs == rhs);
		}

		/// <summary>
		/// DynamicBufferIntBufferElement instances are equal if they refer to the same DynamicBufferIntBufferElement.
		/// </summary>
		/// <param name="compare">The object to compare to this DynamicBufferIntBufferElement.</param>
		/// <returns>True, if the compare parameter contains an DynamicBufferIntBufferElement object having the same value
		/// as this DynamicBufferIntBufferElement.</returns>
		public override bool Equals(object compare)
		{
			return this == (DynamicBufferIntBufferElement)compare;
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
		/// A "blank" DynamicBufferIntBufferElement object that does not refer to an actual entity.
		/// </summary>
		public static DynamicBufferIntBufferElement Null => new DynamicBufferIntBufferElement();
	}


}
