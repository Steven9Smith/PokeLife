using Unity.Mathematics;
using Unity.Entities;
using UnityEngine;
using Unity.Collections;
using Core.Common.Buffers;
using UnityEditor;

namespace Core.Common
{
	public class EntityPrefabComponent : MonoBehaviour,IConvertGameObjectToEntity
	{
		public string Name = "_blank";
		public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
		{
			dstManager.AddComponentData(entity, new EntityPrefab(dstManager,entity, new FixedString64(Name)));
		}

		public static void DisplayEntityPrefabInEditor(ref EntityPrefab entityPrefab,bool foldoutValue)
		{
			if (foldoutValue)
			{
				entityPrefab.Name = EditorGUILayout.TextField("Prefab Name", entityPrefab.Name.ToString());
				string entity = EditorGUILayout.TextField("Entity", entityPrefab.Entity.ToString());
			}
		}
	}

	// used to detect if an entity is a prefab
	public struct EntityPrefab : IComponentData{
		// name of the prefab up to 32 characters
		public FixedString64 Name;
		public Entity Entity;

		public static DynamicBufferEntityPrefab AllSpawnDatas;

		/// <summary>
		/// returns the first EntityPrefab that matches with the given lookFor argument. 
		/// </summary>
		/// <param name="prefabs">prefabs to look through</param>
		/// <param name="lookfor">name of the prefab to look for</param>
		/// <returns></returns>
		public static EntityPrefab FindEntityPrefab(EntityPrefab[] prefabs,FixedString64 lookfor)
		{
			for(int i = 0; i < prefabs.Length;i++)
			{
				if (prefabs[i].Name == lookfor)
					return prefabs[i];
			}
			return EntityPrefab.Null;
		}
		/// <summary>
		/// returns the first EntityPrefab that matches with the given lookFor argument. 
		/// </summary>
		/// <param name="prefabs">prefabs to look through</param>
		/// <param name="lookfor">name of the prefab to look for</param>
		/// <returns></returns>
		public static EntityPrefab FindEntityPrefab(NativeArray<EntityPrefab> prefabs, FixedString64 lookfor)
		{
			for (int i = 0; i < prefabs.Length; i++)
			{
				if (prefabs[i].Name == lookfor)
					return prefabs[i];
			}
			return EntityPrefab.Null;
		}
		/// <summary>
		/// returns the first EntityPrefab that matches with the given lookFor argument. 
		/// </summary>
		/// <param name="lookfor">name of the prefab to look for</param>
		/// <returns></returns>
		public static EntityPrefab FindEntityPrefab(FixedString64 lookfor)
		{
			for (int i = 0; i < AllSpawnDatas.buffer.Length; i++)
			{
				if (AllSpawnDatas.buffer[i].Value.Name == lookfor)
					return AllSpawnDatas.buffer[i].Value;
			}
			return EntityPrefab.Null;
		}

		public EntityPrefab(EntityManager entityManager,Entity entity,string name)
		{
			Name = new FixedString64(name);
			Entity = entity;
			entityManager.GetBuffer<EntityPrefabBufferElement>(EntityPrefab.AllSpawnDatas.dataEntity).Add(new EntityPrefabBufferElement { Value = this });
		}

		public EntityPrefab(EntityManager entityManager, Entity entity,FixedString64 name)
		{
			Name = name;
			Entity = entity;
			entityManager.GetBuffer<EntityPrefabBufferElement>(EntityPrefab.AllSpawnDatas.dataEntity).Add(new EntityPrefabBufferElement { Value = this});
		}

		/// <summary>
		/// sets up the entity and buffer for entity prefab
		/// </summary>
		/// <param name="entityManager"></param>
		/// <param name="entityManager"></param>
		/// <param name="entityManager"></param>
		public static void SetupPrefabs(EntityManager entityManager)
		{
			if (AllSpawnDatas.dataEntity == Entity.Null)
			{
				AllSpawnDatas.dataEntity = entityManager.CreateEntity();
				AllSpawnDatas.buffer = entityManager.AddBuffer<EntityPrefabBufferElement>(AllSpawnDatas.dataEntity);
				Debug.Log("Entity_Prefab: static entity and buffer is setup");
			}
			else Debug.LogWarning("Entity_Prefab: data are already setup.");
		}

		/// <summary>
		/// sets up the entity and buffer for entity prefab
		/// </summary>
		/// <param name="ecb"></param>
		/// <param name="jobIndex"></param>
		public static void SetupPrefabs(EntityCommandBuffer.ParallelWriter ecb,int jobIndex)
		{
			if (AllSpawnDatas.dataEntity == Entity.Null)
			{
				AllSpawnDatas.dataEntity = ecb.CreateEntity(jobIndex);
				AllSpawnDatas.buffer = ecb.AddBuffer<EntityPrefabBufferElement>(jobIndex, AllSpawnDatas.dataEntity);
				Debug.Log("Entity_Prefab: static entity and buffer is setup");
			}
			else Debug.LogWarning("Entity_Prefab: data are already setup.");
		}

		// Overrides

		/// <summary>
		/// EntityPrefab instances are equal if they refer to the same Value.
		/// </summary>
		/// <param name="lhs">An EntityPrefab object.</param>
		/// <param name="rhs">Another EntityPrefab object.</param>
		/// <returns>True, if both values are identical.</returns>
		public static bool operator ==(EntityPrefab lhs, EntityPrefab rhs)
		{
			return lhs.Entity == rhs.Entity && lhs.Name == rhs.Name;
		}

		/// <summary>
		/// EntityPrefab instances are equal if they refer to the same Value.
		/// </summary>
		/// <param name="lhs">An EntityPrefab object.</param>
		/// <param name="rhs">Another EntityPrefab object.</param>
		/// <returns>True, if either value is different.</returns>
		public static bool operator !=(EntityPrefab lhs, EntityPrefab rhs)
		{
			return !(lhs == rhs);
		}


		/// <summary>
		/// EntityPrefab instances are equal if they refer to the same EntityPrefab.
		/// </summary>
		/// <param name="compare">The object to compare to this EntityPrefab.</param>
		/// <returns>True, if the compare parameter contains an EntityPrefab object having the same value
		/// as this EntityPrefab.</returns>
		public override bool Equals(object compare)
		{
			return this == (EntityPrefab)compare;
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
		/// A "blank" EntityPrefab object that does not refer to an actual entity.
		/// </summary>
		public static EntityPrefab Null => new EntityPrefab();
	}

}