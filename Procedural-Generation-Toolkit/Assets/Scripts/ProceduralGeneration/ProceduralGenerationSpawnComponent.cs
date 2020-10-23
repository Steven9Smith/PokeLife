using Core.Common;
using Core.Common.Buffers;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEditor;
using UnityEngine;
using Core.Procedural;

namespace Core.Procedural
{
	public class ProceduralGenerationSpawnComponent : MonoBehaviour
	{
		public float MinGenerationHeight;
		public float MaxSteepness;
		public int Amount;
		public bool IgnoreCollisions;
		public int Id;

		public string PrefabEntityName;

		public EntityPrefab[] prefabs = new EntityPrefab[0];

	/*	// Start is called before the first frame update
		void Start()
		{

		}

		// Update is called once per frame
		void Update()
		{

		}*/
	}

	[System.Serializable]
	public struct ProceduralGenerationSpawnData : IComponentData
	{
		public float MinGenerationHeight;
		public float MaxSteepness;
		public int Amount;
		public BlittableBool IgnoreCollisions;
		public int Id;

		public EntityPrefab EntityPrefab;
		
		// Id and Id handling

		private static DynamicBufferIntBufferElement Ids;
		private static DynamicBufferIntBufferElement AvailibleIds;
		/// <summary>
		/// Removes an Id from the Ids Buffer
		/// </summary>
		/// <param name="entityManager"></param>
		/// <param name="id"></param>
		private static void RemoveId(EntityManager entityManager,int id)
		{
			DynamicBuffer<IntBufferElement> _Ids = Ids.GetValue(entityManager);
			DynamicBuffer<IntBufferElement> _AvailibleIds = AvailibleIds.GetValue(entityManager);
			bool match = false;
			for(int i = 0; i < _Ids.Length; i++) { 
				if(id == _Ids[i].Value){
					// if id is at the end then we just remove it
					if (i == _Ids.Length - 1) {
						_Ids.RemoveAt(i);
						// add it to availible ids so we can reuse ids
					} else
					{
						_Ids.RemoveAt(i);
						_AvailibleIds.Add(new IntBufferElement { Value = id });
					}
					match = true;
				} 
			}
			if (!match)
				Debug.LogWarning("ProceduralgenerationSpawnData: Failed to remove id: id not found");
		}
		/// <summary>
		/// Gets
		/// </summary>
		/// <param name="entityManager"></param>
		/// <returns></returns>
		private static int GenerateId(EntityManager entityManager)
		{
			// some null checking to make sure entities are availible

			if(Ids == DynamicBufferIntBufferElement.Null)
				// create some new Ids
				Ids = new DynamicBufferIntBufferElement(entityManager);
			if (AvailibleIds == DynamicBufferIntBufferElement.Null)
				// create some new availible Ids
				AvailibleIds = new DynamicBufferIntBufferElement(entityManager);

			// we get the id down here

			DynamicBuffer<IntBufferElement> _Ids = Ids.GetValue(entityManager);
			DynamicBuffer<IntBufferElement> _AvailibleIds = AvailibleIds.GetValue(entityManager);

			if (_AvailibleIds.Length > 0)
			{
				// use the ids in availible ids
				_Ids.Add(_AvailibleIds[0]);
				_AvailibleIds.RemoveAt(0);
				return _Ids[_Ids.Length-1].Value;
			}
			else
			{
				// create a new id and return it
				_Ids.Add(new IntBufferElement { Value = _Ids.Length });
				return _Ids.Length-1;
			}
		}

		// Create new spawn data
		
		public static ProceduralGenerationSpawnData CreateSpawnData(EntityManager entityManager,EntityPrefab entityPrefab,float minGenerationHeight,int amount,int maxSteepness,bool ignoreCollisions)
		{
			return new ProceduralGenerationSpawnData
			{
				Amount = amount,
				IgnoreCollisions = ignoreCollisions,
				MaxSteepness = maxSteepness,
				MinGenerationHeight = minGenerationHeight,
				EntityPrefab = entityPrefab,
				Id = GenerateId(entityManager)
			};
		}
		public static ProceduralGenerationSpawnData CreateSpawnData(EntityManager entityManager,string EntityPrefabName,float minGenerationHeight,int amount,int maxSteepness,bool ignoreCollisions)
		{
			 
			return new ProceduralGenerationSpawnData
			{
				Amount = amount,
				IgnoreCollisions = ignoreCollisions,
				MaxSteepness = maxSteepness,
				MinGenerationHeight = minGenerationHeight,
				EntityPrefab = EntityPrefab.FindEntityPrefab(EntityPrefabName),
				Id = GenerateId(entityManager)
			};
		}


		// Spawn Data Generation

		public static NativeArray<Entity> GenerateEntities(EntityManager entityManager, Entity proceduralAreaEntity, ProceduralGenerationSpawnData spawnData,Allocator allocator) {
			NativeArray<Entity> entities = entityManager.Instantiate(spawnData.EntityPrefab.Entity,spawnData.Amount,allocator);
			foreach (Entity entity in entities)
				entityManager.AddComponentData<ProceduralGenerationElement>(entity, new ProceduralGenerationElement(entityManager,proceduralAreaEntity,spawnData.EntityPrefab.Entity));
			return entities;
		}

		public static NativeArray<Entity> GenerateEntities(EntityCommandBuffer.ParallelWriter ecb,int jobIndex,Entity proceduralAreaEntity, ProceduralGenerationSpawnData spawnData,Allocator allocator) {
			NativeArray<Entity> entities = new NativeArray<Entity>(spawnData.Amount,allocator);
			foreach (Entity entity in entities)
				ecb.AddComponent(jobIndex,entity, new ProceduralGenerationElement(ecb,jobIndex, proceduralAreaEntity, spawnData.EntityPrefab.Entity));
			return entities;
		}

		public static Entity GenerateEntity(EntityCommandBuffer.ParallelWriter ecb,int jobIndex, ProceduralGenerationSpawnData spawnData)
		{
			Entity entity = ecb.CreateEntity(jobIndex);
			// ecb.addComponent(entity,new ProceduralGenerationElement(ecb, proceduralAreaEntity, spawnData.EntityPrefab));
			return entity;
		}

		public static Entity GenerateEntity(EntityManager entityManager, ProceduralGenerationSpawnData spawnData)
		{
			Entity entity = entityManager.CreateEntity();
			// ecb.addComponent(entity,new ProceduralGenerationElement(ecb, proceduralAreaEntity, spawnData.EntityPrefab));
			return entity;
		}

		/// <summary>
		/// Generates Entities and returns them after creation using the given arguments
		/// </summary>
		/// <param name="entityManager"></param>
		/// <param name="proceduralAreaEntity">the entity that will house all the new;y created entities</param>
		/// <param name="spawnData"></param>
		/// <param name="entityPrefab"></param>
		/// <returns></returns>
		public static NativeArray<Entity> GenerateSpawnEntities(EntityManager entityManager, Entity proceduralAreaEntity,ref ProceduralGenerationSpawnData spawnData, GameObject entityPrefab,Allocator allocator)
		{
			NativeArray<Entity> entities = new NativeArray<Entity>(spawnData.Amount, allocator);
			entityManager.Instantiate(entityPrefab,entities);
			// since we used a gameobect we have to set the entity prefab
			spawnData.EntityPrefab = new EntityPrefab(entityManager,entityManager.Instantiate(entities[0]),new FixedString64(entityPrefab.name));
			foreach (Entity entity in entities)
				entityManager.AddComponentData<ProceduralGenerationElement>(entity, new ProceduralGenerationElement(entityManager,proceduralAreaEntity,spawnData.EntityPrefab.Entity));
			return entities;
		}

		// Overrides

		/// <summary>
		/// ProceduralGenerationSpawnDataBufferElement instances are equal if they refer to the same Value.
		/// </summary>
		/// <param name="lhs">An ProceduralGenerationSpawnDataBufferElement object.</param>
		/// <param name="rhs">Another ProceduralGenerationSpawnDataBufferElement object.</param>
		/// <returns>True, if both values are identical.</returns>
		public static bool operator ==(ProceduralGenerationSpawnData lhs, ProceduralGenerationSpawnData rhs)
		{
			return (
				lhs.MinGenerationHeight == rhs.MinGenerationHeight
				&&  lhs.MaxSteepness == rhs.MaxSteepness
				&& lhs.Amount == rhs.Amount
				&& lhs.IgnoreCollisions == rhs.IgnoreCollisions
				&& lhs.EntityPrefab == rhs.EntityPrefab
				&& lhs.Id == rhs.Id
				);
		}

		/// <summary>
		/// ProceduralGenerationSpawnDataBufferElement instances are equal if they refer to the same Value.
		/// </summary>
		/// <param name="lhs">An ProceduralGenerationSpawnDataBufferElement object.</param>
		/// <param name="rhs">Another ProceduralGenerationSpawnDataBufferElement object.</param>
		/// <returns>True, if either value is different.</returns>
		public static bool operator !=(ProceduralGenerationSpawnData lhs, ProceduralGenerationSpawnData rhs)
		{
			return !(lhs == rhs);
		}

		/// <summary>
		/// ProceduralGenerationSpawnDataBufferElement instances are equal if they refer to the same ProceduralGenerationSpawnDataBufferElement.
		/// </summary>
		/// <param name="compare">The object to compare to this ProceduralGenerationSpawnDataBufferElement.</param>
		/// <returns>True, if the compare parameter contains an ProceduralGenerationSpawnDataBufferElement object having the same value
		/// as this ProceduralGenerationSpawnDataBufferElement.</returns>
		public override bool Equals(object compare)
		{
			return this == (ProceduralGenerationSpawnData)compare;
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
		/// A "blank" ProceduralGenerationSpawnDataBufferElement object that does not refer to an actual entity.
		/// </summary>
		public static ProceduralGenerationSpawnData Null => new ProceduralGenerationSpawnData();
	}

	[System.Serializable]
	public struct ProceduralGenerationSpawnDataBufferElement : IBufferElementData
	{
		public ProceduralGenerationSpawnData Value;

		// Functionality

		/// <summary>
		/// Get the buffers
		/// </summary>
		/// <param name="entityManager"></param>
		/// <param name="entity"></param>
		/// <returns></returns>
		public static DynamicBuffer<ProceduralGenerationSpawnDataBufferElement> GetBuffer(EntityManager entityManager, Entity entity)
		{
			return entityManager.GetBuffer<ProceduralGenerationSpawnDataBufferElement>(entity);
		}

		/// <summary>
		/// set the buffer elements of the given aBuffer
		/// </summary>
		/// <param name="entityManager"></param>
		/// <param name="entity"></param>
		/// <param name="bBuffer"></param>
		public static void SetBuffer(EntityManager entityManager, Entity entity, DynamicBuffer<ProceduralGenerationSpawnDataBufferElement> bBuffer)
		{
			DynamicBuffer<ProceduralGenerationSpawnDataBufferElement> aBuffer = GetBuffer(entityManager, entity);
			aBuffer.Clear();
			aBuffer.CopyFrom(bBuffer);
		}

		/// <summary>
		/// set the buffer elements of the given aBuffer
		/// </summary>
		/// <param name="ecb"></param>
		/// <param name="aBuffer"></param>
		/// <param name="bBuffer"></param>
		public static void SetBuffer(EntityCommandBuffer.ParallelWriter ecb, DynamicBuffer<ProceduralGenerationSpawnDataBufferElement> aBuffer, DynamicBuffer<ProceduralGenerationSpawnDataBufferElement> bBuffer)
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
		public static DynamicBuffer<ProceduralGenerationSpawnDataBufferElement> AddBuffer(EntityCommandBuffer.ParallelWriter ecb,int indexJob, Entity entity)
		{
			return ecb.AddBuffer<ProceduralGenerationSpawnDataBufferElement>(indexJob,entity);
		}

		/// <summary>
		/// Adds the buffer to the entity and returns the buffer
		/// </summary>
		/// <param name="entityManager"></param>
		/// <returns></returns>
		public static DynamicBuffer<ProceduralGenerationSpawnDataBufferElement> AddBuffer(EntityManager entityManager, Entity entity)
		{
			return entityManager.AddBuffer<ProceduralGenerationSpawnDataBufferElement>(entity);
		}


		// Sorting

		public static void SortByMinHeight(ref DynamicBuffer<ProceduralGenerationSpawnDataBufferElement> buffer,Allocator allocator)
		{
			NativeArray<FloatBufferElement> tmp = ToFloatBufferElementNativeArray(buffer,allocator, 0);
			NativeArray<ProceduralGenerationSpawnDataBufferElement> datas = buffer.ToNativeArray(allocator);
			FloatBufferElement.Sort(ref tmp,allocator);

			for (int i = 0; i < tmp.Length; i++)
				buffer[i] = datas[tmp[i].Index];

			tmp.Dispose();
			datas.Dispose();
		}

		// Conversions

		/// <summary>
		/// returns a NativeArray<float> of a float value in the given buffer
		/// </summary>
		/// <param name="buffer">buffer to use in operation</param>
		/// <param name="mode">mode determines which value to return
		/// 0 = MinGenerationHeight
		/// 1 = Amoun
		/// 2 = MaxSteepness</param>
		/// <returns></returns>
		public static NativeArray<float> ToFloatNativeArray(DynamicBuffer<ProceduralGenerationSpawnDataBufferElement> buffer,Allocator allocator, int mode = 0)
		{
			NativeArray<float> tmp = new NativeArray<float>(buffer.Length, allocator);
			if (mode == 0)
				for (int i = 0; i < buffer.Length; i++)
					tmp[i] = buffer[i].Value.MinGenerationHeight;
			else if (mode == 1)
				for (int i = 0; i < buffer.Length; i++)
					tmp[i] = buffer[i].Value.Amount;
			else
				for (int i = 0; i < buffer.Length; i++)
					tmp[i] = buffer[i].Value.MaxSteepness;
			return tmp;
		}
		/// <summary>
		/// returns a NativeArray<FloatBufferElement> of a float value in the given buffer
		/// </summary>
		/// <param name="buffer">buffer to use in operation</param>
		/// <param name="mode">mode determines which value to return
		/// 0 = MinGenerationHeight
		/// 1 = Amoun
		/// 2 = MaxSteepness</param>
		/// <returns></returns>
		public static NativeArray<FloatBufferElement> ToFloatBufferElementNativeArray(DynamicBuffer<ProceduralGenerationSpawnDataBufferElement> buffer,Allocator allocator, int mode = 0)
		{
			NativeArray<FloatBufferElement> tmp = new NativeArray<FloatBufferElement>(buffer.Length, allocator);
			if (mode == 0)
				for (int i = 0; i < buffer.Length; i++)
					tmp[i] = new FloatBufferElement { Value = buffer[i].Value.MinGenerationHeight, Index = i };
			else if (mode == 1)
				for (int i = 0; i < buffer.Length; i++)
					tmp[i] = new FloatBufferElement { Value = buffer[i].Value.Amount, Index = i };
			else
				for (int i = 0; i < buffer.Length; i++)
					tmp[i] = new FloatBufferElement { Value = buffer[i].Value.MaxSteepness, Index = i };
			return tmp;
		}

		public ProceduralGenerationSpawnData[] ToArray(DynamicBuffer<ProceduralGenerationSpawnDataBufferElement> buffer)
		{
			ProceduralGenerationSpawnData[] array = new ProceduralGenerationSpawnData[buffer.Capacity];
			int i = 0;
			foreach (ProceduralGenerationSpawnDataBufferElement element in buffer)
				array[i] = element.Value;
			return array;
		}

		// Overrides

		/// <summary>
		/// ProceduralGenerationSpawnDataBufferElement instances are equal if they refer to the same Value.
		/// </summary>
		/// <param name="lhs">An ProceduralGenerationSpawnDataBufferElement object.</param>
		/// <param name="rhs">Another ProceduralGenerationSpawnDataBufferElement object.</param>
		/// <returns>True, if both values are identical.</returns>
		public static bool operator ==(ProceduralGenerationSpawnDataBufferElement lhs, ProceduralGenerationSpawnDataBufferElement rhs)
		{
			return lhs.Value == rhs.Value;
		}

		/// <summary>
		/// ProceduralGenerationSpawnDataBufferElement instances are equal if they refer to the same Value.
		/// </summary>
		/// <param name="lhs">An ProceduralGenerationSpawnDataBufferElement object.</param>
		/// <param name="rhs">Another ProceduralGenerationSpawnDataBufferElement object.</param>
		/// <returns>True, if either value is different.</returns>
		public static bool operator !=(ProceduralGenerationSpawnDataBufferElement lhs, ProceduralGenerationSpawnDataBufferElement rhs)
		{
			return !(lhs == rhs);
		}

		
		/// <summary>
		/// ProceduralGenerationSpawnDataBufferElement instances are equal if they refer to the same ProceduralGenerationSpawnDataBufferElement.
		/// </summary>
		/// <param name="compare">The object to compare to this ProceduralGenerationSpawnDataBufferElement.</param>
		/// <returns>True, if the compare parameter contains an ProceduralGenerationSpawnDataBufferElement object having the same value
		/// as this ProceduralGenerationSpawnDataBufferElement.</returns>
		public override bool Equals(object compare)
		{
			return this == (ProceduralGenerationSpawnDataBufferElement)compare;
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
		/// A "blank" ProceduralGenerationSpawnDataBufferElement object that does not refer to an actual entity.
		/// </summary>
		public static ProceduralGenerationSpawnDataBufferElement Null => new ProceduralGenerationSpawnDataBufferElement();
	}

	public struct DynamicBufferProceduralGenerationSpawnDataBufferElement : IComponentData
	{
		// Entity where the buffer is located
		public Entity mEntity;

		/// <summary>
		/// returns the DynamicBuffer using the entity stoed in this struct
		/// </summary>
		/// <param name="entityManager"></param>
		/// <returns></returns>
		public DynamicBuffer<ProceduralGenerationSpawnDataBufferElement> GetValue(EntityManager entityManager)
		{
			return entityManager.GetBuffer<ProceduralGenerationSpawnDataBufferElement>(mEntity);
		}

		/// <summary>
		/// Use this if you want to get the DynamicBuffer<ProceduralGenerationSpawnDataBufferElement> from the given entity
		/// </summary>
		/// <param name="entityManager"></param>
		/// <param name="entity"></param>
		/// <returns></returns>
		public static DynamicBuffer<ProceduralGenerationSpawnDataBufferElement> GetValue(EntityManager entityManager, Entity entity)
		{
			return entityManager.GetBuffer<ProceduralGenerationSpawnDataBufferElement>(entity);
		}

		/// <summary>
		/// Use this to initialize a dynamic buffer just to temporary use other wise use AddBuffer()
		/// </summary>
		/// <param name="entityManager"></param>
		public DynamicBufferProceduralGenerationSpawnDataBufferElement(EntityManager entityManager,bool addBuffer = false)
		{
			mEntity = entityManager.CreateEntity(typeof(ComputeEntity));
			entityManager.SetName(mEntity,"ProceduralGenerationSpawnDataBufferElement");
			if (addBuffer)
				entityManager.AddBuffer<ProceduralGenerationSpawnDataBufferElement>(mEntity);
		}

		/// <summary>
		/// Use this to initialize a dynamic buffer just to temporary use other wise use AddBuffer()
		/// </summary>
		/// <param name="ecb"></param>
		/// <param name="addBuffer"></param>
		public DynamicBufferProceduralGenerationSpawnDataBufferElement(EntityCommandBuffer.ParallelWriter ecb,int jobIndex, bool addBuffer = false)
		{
			mEntity = ecb.CreateEntity(jobIndex);
			if (addBuffer)
				ecb.AddBuffer<ProceduralGenerationSpawnDataBufferElement>(jobIndex,mEntity);
		}



		/// <summary>
		/// Use this to add ProceduralGenerationSpawnDataBufferElements to the given entity NOTE: you can only have 1 per entity
		/// </summary>
		/// <param name="entityManager"></param>
		/// <param name="entity"></param>
		public static DynamicBuffer<ProceduralGenerationSpawnDataBufferElement> AddBuffer(EntityManager entityManager, Entity entity)
		{
			return entityManager.AddBuffer<ProceduralGenerationSpawnDataBufferElement>(entity);
		}

		/// <summary>
		/// converts the DynamicBuffer<ProceduralGenerationSpawnDataBufferElement> to an ProceduralGenerationSpawnDataBufferElement rrary
		/// </summary>
		/// <returns></returns>
		public static ProceduralGenerationSpawnDataBufferElement[] ToArray(DynamicBuffer<ProceduralGenerationSpawnDataBufferElement> buffer)
		{
			ProceduralGenerationSpawnDataBufferElement[] arr = new ProceduralGenerationSpawnDataBufferElement[buffer.Length];
			for (int i = 0; i < buffer.Length; i++)
				arr[i] = buffer[i];
			return arr;
		}

		/// <summary>
		/// converts the DynamicBuffer<ProceduralGenerationSpawnDataBufferElement> to an Entity rrary
		/// </summary>
		/// <param name="Value"></param>
		/// <returns></returns>
		public ProceduralGenerationSpawnData[] ToProceduralGenerationSpawnDataArray(DynamicBuffer<ProceduralGenerationSpawnDataBufferElement> buffer)
		{
			ProceduralGenerationSpawnData[] arr = new ProceduralGenerationSpawnData[buffer.Length];
			for (int i = 0; i < buffer.Length; i++)
				arr[i] = buffer[i].Value;
			return arr;
		}

		/// <summary>
		/// DynamicBufferProceduralGenerationSpawnDataBufferElement instances are equal if they refer to the same Value.
		/// </summary>
		/// <param name="lhs">An DynamicBufferProceduralGenerationSpawnDataBufferElement object.</param>
		/// <param name="rhs">Another DynamicBufferProceduralGenerationSpawnDataBufferElement object.</param>
		/// <returns>True, if both values are identical.</returns>
		public static bool operator ==(DynamicBufferProceduralGenerationSpawnDataBufferElement lhs, DynamicBufferProceduralGenerationSpawnDataBufferElement rhs)
		{
			return lhs.mEntity == rhs.mEntity;
		}

		/// <summary>
		/// DynamicBufferProceduralGenerationSpawnDataBufferElement instances are equal if they refer to the same Value.
		/// </summary>
		/// <param name="lhs">An DynamicBufferProceduralGenerationSpawnDataBufferElement object.</param>
		/// <param name="rhs">Another DynamicBufferProceduralGenerationSpawnDataBufferElement object.</param>
		/// <returns>True, if either value is different.</returns>
		public static bool operator !=(DynamicBufferProceduralGenerationSpawnDataBufferElement lhs, DynamicBufferProceduralGenerationSpawnDataBufferElement rhs)
		{
			return !(lhs == rhs);
		}

		/// <summary>
		/// DynamicBufferProceduralGenerationSpawnDataBufferElement instances are equal if they refer to the same DynamicBufferProceduralGenerationSpawnDataBufferElement.
		/// </summary>
		/// <param name="compare">The object to compare to this DynamicBufferProceduralGenerationSpawnDataBufferElement.</param>
		/// <returns>True, if the compare parameter contains an DynamicBufferProceduralGenerationSpawnDataBufferElement object having the same value
		/// as this DynamicBufferProceduralGenerationSpawnDataBufferElement.</returns>
		public override bool Equals(object compare)
		{
			return this == (DynamicBufferProceduralGenerationSpawnDataBufferElement)compare;
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
		/// A "blank" DynamicBufferProceduralGenerationSpawnDataBufferElement object that does not refer to an actual entity.
		/// </summary>
		public static DynamicBufferProceduralGenerationSpawnDataBufferElement Null => new DynamicBufferProceduralGenerationSpawnDataBufferElement();
	}
}

[CustomEditor(typeof(ProceduralGenerationSpawnComponent))]
public class ProceduralGenerationSpawnComponentEditor : Editor
{
	public ProceduralGenerationSpawnComponent dataComponent;

	// Button Variables
	private const int BUTTON_WIDTH = 200;
	private const int BUTTON_HEIGHT = 20;

	private void Awake()
	{
		dataComponent = target as ProceduralGenerationSpawnComponent;
	}

	// Spawn Data
	bool EntityPrefabFoldout = false;

	// Entity prefabs
	bool[] PrefabEntityFoldouts = new bool[0];

	public override void OnInspectorGUI()
	{
		dataComponent.MinGenerationHeight = EditorGUILayout.FloatField("Minimum Generation Height", dataComponent.MinGenerationHeight);
		dataComponent.MaxSteepness = EditorGUILayout.FloatField("Max Steepness", dataComponent.MaxSteepness);
		dataComponent.Amount = EditorGUILayout.IntField("Spawn Amount", dataComponent.Amount);
		dataComponent.IgnoreCollisions = EditorGUILayout.Toggle("Ignore Collisions", dataComponent.IgnoreCollisions);

		// Entity Prefabs
		{
			EntityPrefabFoldout = EditorGUILayout.Foldout(EntityPrefabFoldout, "Spawn Data");
			if (EntityPrefabFoldout)
			{
				int length = EditorGUILayout.IntField("Length", dataComponent.prefabs.Length);
				if (PrefabEntityFoldouts.Length != length)
					PrefabEntityFoldouts = new bool[length];
				int i = 0;
				if (length > dataComponent.prefabs.Length)
				{
					// add 
					EntityPrefab[] tmp = dataComponent.prefabs;
					dataComponent.prefabs = new EntityPrefab[tmp.Length + 1];
					for (i = 0; i < tmp.Length; i++)
						dataComponent.prefabs[i] = tmp[i];
					bool[] tmpB = new bool[tmp.Length];
					PrefabEntityFoldouts = new bool[tmp.Length + 1];
					for (i = 0; i < tmpB.Length; i++)
						PrefabEntityFoldouts[i] = tmpB[i];
				}
				else if (length < dataComponent.prefabs.Length)
				{
					// remove
					EntityPrefab[] tmp = new EntityPrefab[dataComponent.prefabs.Length - 1];
					for (i = 0; i < tmp.Length; i++)
						tmp[i] = dataComponent.prefabs[i];
					dataComponent.prefabs = tmp;
					bool[] tmpB = new bool[tmp.Length-1];
					for (i = 0; i < tmpB.Length; i++)
						tmpB[i] = PrefabEntityFoldouts[i];
					PrefabEntityFoldouts = tmpB;
				}

				for (i = 0; i < dataComponent.prefabs.Length; i++)
				{
					PrefabEntityFoldouts[i] = EditorGUILayout.Foldout(PrefabEntityFoldouts[i], "Prefab Entity "+(i+1));
					EntityPrefabComponent.DisplayEntityPrefabInEditor(ref dataComponent.prefabs[i],PrefabEntityFoldouts[i]);
				}
			}
		}
	}
}