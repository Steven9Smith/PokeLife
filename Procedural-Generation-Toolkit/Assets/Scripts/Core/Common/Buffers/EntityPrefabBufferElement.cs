using UnityEngine;
using System.Collections;
using Unity.Entities;

namespace Core.Common.Buffers
{
	public struct EntityPrefabBufferElement : IBufferElementData {
		public EntityPrefab Value;
		
		/// <summary>
		/// Get the buffers
		/// </summary>
		/// <param name="entityManager"></param>
		/// <param name="entity"></param>
		/// <returns></returns>
		public static DynamicBuffer<EntityPrefabBufferElement> GetBuffer(EntityManager entityManager, Entity entity)
		{
			return entityManager.GetBuffer<EntityPrefabBufferElement>(entity);
		}

		/// <summary>
		/// set the buffer elements of the given aBuffer
		/// </summary>
		/// <param name="entityManager"></param>
		/// <param name="entity"></param>
		/// <param name="bBuffer"></param>
		public static void SetBuffer(EntityManager entityManager, Entity entity, DynamicBuffer<EntityPrefabBufferElement> bBuffer)
		{
			DynamicBuffer<EntityPrefabBufferElement> aBuffer = GetBuffer(entityManager, entity);
			aBuffer.Clear();
			aBuffer.CopyFrom(bBuffer);
		}

		/// <summary>
		/// set the buffer elements of the given aBuffer
		/// </summary>
		/// <param name="ecb"></param>
		/// <param name="aBuffer"></param>
		/// <param name="bBuffer"></param>
		public static void SetBuffer(EntityCommandBuffer.ParallelWriter ecb, DynamicBuffer<EntityPrefabBufferElement> aBuffer, DynamicBuffer<EntityPrefabBufferElement> bBuffer)
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
		public static DynamicBuffer<EntityPrefabBufferElement> AddBuffer(EntityCommandBuffer.ParallelWriter ecb, int jobIndex, Entity entity)
		{
			return ecb.AddBuffer<EntityPrefabBufferElement>(jobIndex, entity);
		}

		/// <summary>
		/// Adds the buffer to the entity and returns the buffer
		/// </summary>
		/// <param name="entityManager"></param>
		/// <returns></returns>
		public static DynamicBuffer<EntityPrefabBufferElement> AddBuffer(EntityManager entityManager, Entity entity)
		{
			return entityManager.AddBuffer<EntityPrefabBufferElement>(entity);
		}

	}

	public struct DynamicBufferEntityPrefab
	{
		public Entity dataEntity;
		public DynamicBuffer<EntityPrefabBufferElement> buffer;

		public DynamicBufferEntityPrefab(EntityManager entityManager)
		{
			dataEntity = entityManager.CreateEntity();
			buffer = entityManager.AddBuffer<EntityPrefabBufferElement>(dataEntity);
		}

		public DynamicBufferEntityPrefab(EntityCommandBuffer.ParallelWriter ecb,int jobIndex)
		{
			dataEntity = ecb.CreateEntity(jobIndex);
			buffer = ecb.AddBuffer<EntityPrefabBufferElement>(jobIndex,dataEntity);
		}

		// Add Buffer to another entity

		public DynamicBuffer<EntityPrefabBufferElement> AddBuffer(EntityManager entityManager,Entity entity)
		{
			return entityManager.AddBuffer<EntityPrefabBufferElement>(entity);
		}

		public DynamicBuffer<EntityPrefabBufferElement> AddBuffer(EntityCommandBuffer.ParallelWriter ecb,int jobIndex,Entity entity)
		{
			return ecb.AddBuffer<EntityPrefabBufferElement>(jobIndex,entity);
		}
	}

}