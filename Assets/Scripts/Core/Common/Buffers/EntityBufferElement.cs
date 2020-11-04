using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Core.Common.Buffers
{
	public class EntityBufferElementComponent : MonoBehaviour, IConvertGameObjectToEntity
	{
		public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
		{
			dstManager.AddBuffer<EntityBufferElement>(entity);
		}
	}
	// Since Entities are created at run time System.Serializable is useless


	public struct DynamicBufferEntityBufferElement : IComponentData
	{
		// Entity where the buffer is located
		public Entity mEntity;
		
		/// <summary>
		/// returns the DynamicBuffer using the entity stoed in this struct
		/// </summary>
		/// <param name="entityManager"></param>
		/// <returns></returns>
		public DynamicBuffer<EntityBufferElement> GetValue(EntityManager entityManager)
		{
			return entityManager.GetBuffer<EntityBufferElement>(mEntity);
		}
		
		/// <summary>
		/// Use this if you want to get the DynamicBuffer<EntityBufferElement> from the given entity
		/// </summary>
		/// <param name="entityManager"></param>
		/// <param name="entity"></param>
		/// <returns></returns>
		public static DynamicBuffer<EntityBufferElement> GetValue(EntityManager entityManager,Entity entity)
		{
			return entityManager.GetBuffer<EntityBufferElement>(entity);
		}
		
		// Initializers

		/// <summary>
		/// Use this to initialize a dynamic buffer just to temporary use other wise use AddBuffer()
		/// </summary>
		/// <param name="entityManager"></param>
		public DynamicBufferEntityBufferElement(EntityManager entityManager,bool addBuffer = false)
		{
			mEntity = entityManager.CreateEntity();
			entityManager.SetName(mEntity, "DynamicBufferEntityBufferElement");
			if (addBuffer)
				entityManager.AddBuffer<EntityBufferElement>(mEntity);
		}
		/// <summary>
		/// Use this to initialize a dynamic buffer just to temporary use other wise use AddBuffer()
		/// </summary>
		/// <param name="ecb"></param>
		/// <param name="addBuffer"></param>
		public DynamicBufferEntityBufferElement(EntityCommandBuffer.ParallelWriter ecb,int jobIndex, bool addBuffer = false)
		{
			mEntity = ecb.CreateEntity(jobIndex);
			if (addBuffer)
				ecb.AddBuffer<EntityBufferElement>(jobIndex,mEntity);
		}

		// add Buffer

		/// <summary>
		/// Use this to add EntityBufferElements to the given entity NOTE: you can only have 1 per entity
		/// </summary>
		/// <param name="entityManager"></param>
		/// <param name="entity"></param>
		public void AddBuffer(EntityManager entityManager, Entity entity)
		{
			entityManager.AddBuffer<EntityBufferElement>(entity);
		}

		/// <summary>
		/// Adds the buffer to the eneity and returns the buffer
		/// </summary>
		/// <param name="entityManager"></param>
		/// <returns></returns>
		public DynamicBuffer<EntityBufferElement> AddBuffer(EntityManager entityManager)
		{
			return entityManager.AddBuffer<EntityBufferElement>(mEntity);
		}

		// To Array

		/// <summary>
		/// converts the DynamicBuffer<EntityBufferElement> to an EntityBufferElement rrary
		/// </summary>
		/// <returns></returns>
		public static EntityBufferElement[] ToArray(DynamicBuffer<EntityBufferElement> buffer)
		{
			EntityBufferElement[] arr = new EntityBufferElement[buffer.Length];
			for (int i = 0; i < buffer.Length; i++)
				arr[i] = buffer[i];
			return arr;
		}
		
		/// <summary>
		/// converts the DynamicBuffer<EntityBufferElement> to an Entity rrary
		/// </summary>
		/// <param name="Value"></param>
		/// <returns></returns>
		public Entity[] ToEntityArray(DynamicBuffer<EntityBufferElement> buffer)
		{
			Entity[] arr = new Entity[buffer.Length];
			for (int i = 0; i < buffer.Length; i++)
				arr[i] = buffer[i].Value;
			return arr;
		}

		// Overrides

		/// <summary>
		/// DynamicBufferEntityBufferElement instances are equal if they refer to the same Value.
		/// </summary>
		/// <param name="lhs">An DynamicBufferEntityBufferElement object.</param>
		/// <param name="rhs">Another DynamicBufferEntityBufferElement object.</param>
		/// <returns>True, if both values are identical.</returns>
		public static bool operator ==(DynamicBufferEntityBufferElement lhs, DynamicBufferEntityBufferElement rhs)
		{
			return lhs.mEntity == rhs.mEntity;
		}

		/// <summary>
		/// DynamicBufferEntityBufferElement instances are equal if they refer to the same Value.
		/// </summary>
		/// <param name="lhs">An DynamicBufferEntityBufferElement object.</param>
		/// <param name="rhs">Another DynamicBufferEntityBufferElement object.</param>
		/// <returns>True, if either value is different.</returns>
		public static bool operator !=(DynamicBufferEntityBufferElement lhs, DynamicBufferEntityBufferElement rhs)
		{
			return !(lhs == rhs);
		}

		/// <summary>
		/// DynamicBufferEntityBufferElement instances are equal if they refer to the same DynamicBufferEntityBufferElement.
		/// </summary>
		/// <param name="compare">The object to compare to this DynamicBufferEntityBufferElement.</param>
		/// <returns>True, if the compare parameter contains an DynamicBufferEntityBufferElement object having the same value
		/// as this DynamicBufferEntityBufferElement.</returns>
		public override bool Equals(object compare)
		{
			return this == (DynamicBufferEntityBufferElement)compare;
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
			if(mEntity != Entity.Null)
				entityManager.DestroyEntity(mEntity);
		}

		/// <summary>
		/// A "blank" DynamicBufferEntityBufferElement object that does not refer to an actual entity.
		/// </summary>
		public static DynamicBufferEntityBufferElement Null => new DynamicBufferEntityBufferElement();
	}


	public struct EntityBufferElement : IBufferElementData
	{
		public Entity Value;


		/// <summary>
		/// Get the buffers
		/// </summary>
		/// <param name="entityManager"></param>
		/// <param name="entity"></param>
		/// <returns></returns>
		public static DynamicBuffer<EntityBufferElement> GetBuffer(EntityManager entityManager, Entity entity)
		{
			return entityManager.GetBuffer<EntityBufferElement>(entity);
		}

		/// <summary>
		/// set the buffer elements of the given aBuffer
		/// </summary>
		/// <param name="entityManager"></param>
		/// <param name="entity"></param>
		/// <param name="bBuffer"></param>
		public static void SetBuffer(EntityManager entityManager, Entity entity, DynamicBuffer<EntityBufferElement> bBuffer)
		{
			DynamicBuffer<EntityBufferElement> aBuffer = GetBuffer(entityManager, entity);
			aBuffer.Clear();
			aBuffer.CopyFrom(bBuffer);
		}

		/// <summary>
		/// set the buffer elements of the given aBuffer
		/// </summary>
		/// <param name="ecb"></param>
		/// <param name="aBuffer"></param>
		/// <param name="bBuffer"></param>
		public static void SetBuffer(EntityCommandBuffer.ParallelWriter ecb, DynamicBuffer<EntityBufferElement> aBuffer, DynamicBuffer<EntityBufferElement> bBuffer)
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
		public static DynamicBuffer<EntityBufferElement> AddBuffer(EntityCommandBuffer.ParallelWriter ecb,int jobIndex, Entity entity)
		{
			return ecb.AddBuffer<EntityBufferElement>(jobIndex,entity);
		}

		/// <summary>
		/// Adds the buffer to the entity and returns the buffer
		/// </summary>
		/// <param name="entityManager"></param>
		/// <returns></returns>
		public static DynamicBuffer<EntityBufferElement> AddBuffer(EntityManager entityManager, Entity entity)
		{
			return entityManager.AddBuffer<EntityBufferElement>(entity);
		}




		public static Entity[] ToArray(DynamicBuffer<EntityBufferElement> buffer)
		{
			Entity[] array = new Entity[buffer.Capacity];
			int i = 0;
			foreach (EntityBufferElement element in buffer)
				array[i] = element.Value;
			return array;
		}

		/// <summary>
		/// EntityBufferElement instances are equal if they refer to the same Value.
		/// </summary>
		/// <param name="lhs">An EntityBufferElement object.</param>
		/// <param name="rhs">Another EntityBufferElement object.</param>
		/// <returns>True, if both values are identical.</returns>
		public static bool operator ==(EntityBufferElement lhs, EntityBufferElement rhs)
		{
			return lhs.Value == rhs.Value;
		}

		/// <summary>
		/// EntityBufferElement instances are equal if they refer to the same Value.
		/// </summary>
		/// <param name="lhs">An EntityBufferElement object.</param>
		/// <param name="rhs">Another EntityBufferElement object.</param>
		/// <returns>True, if either value is different.</returns>
		public static bool operator !=(EntityBufferElement lhs, EntityBufferElement rhs)
		{
			return !(lhs == rhs);
		}

		/// <summary>
		/// EntityBufferElement instances are equal if they refer to the same EntityBufferElement.
		/// </summary>
		/// <param name="compare">The object to compare to this EntityBufferElement.</param>
		/// <returns>True, if the compare parameter contains an EntityBufferElement object having the same value
		/// as this EntityBufferElement.</returns>
		public override bool Equals(object compare)
		{
			return this == (EntityBufferElement)compare;
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
		/// A "blank" EntityBufferElement object that does not refer to an actual entity.
		/// </summary>
		public static EntityBufferElement Null => new EntityBufferElement();

	}
}