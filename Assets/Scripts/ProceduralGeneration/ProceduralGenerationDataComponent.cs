using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Core.Common;
using Unity.Physics.Stateful;
using Unity.Physics.Systems;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Physics;
using Core.Extensions;
using Unity.Collections;
using Core.Common.Buffers;
using UnityEditor;
using Core.Procedural;

namespace Core.Procedural
{
	// convert and inject for editor functionality

	public class ProceduralGenerationUpdateSystem : SystemBase
	{
		private ExportPhysicsWorld m_ExportPhysicsWorld;
		private TriggerEventConversionSystem m_TriggerSystem;
		private EndFramePhysicsSystem m_EndFramePhysicsSystem;
		private EntityQueryMask m_NonTriggerDynamicBodyMask;
		EndSimulationEntityCommandBufferSystem m_EndSimulationEcbSystem;


		protected override void OnCreate()
		{
			m_ExportPhysicsWorld = World.GetOrCreateSystem<ExportPhysicsWorld>();
			m_TriggerSystem = World.GetOrCreateSystem<TriggerEventConversionSystem>();
			m_EndFramePhysicsSystem = World.GetOrCreateSystem<EndFramePhysicsSystem>();

			m_EndSimulationEcbSystem = World
				.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

			m_NonTriggerDynamicBodyMask = EntityManager.GetEntityQueryMask(
				   GetEntityQuery(new EntityQueryDesc
				   {
					   All = new ComponentType[]
					   {
						typeof(Translation),
						typeof(Rotation),
						typeof(PhysicsVelocity)
					   },
					   None = new ComponentType[]
					   {
						typeof(StatefulTriggerEvent)
					   }
				   })
			   );
			//tests
			{
				/*	
					Entity a = EntityManager.CreateEntity();
				//	Entity b = EntityManager.CreateEntity();
				//	Entity c = EntityManager.CreateEntity();

					// Test stuff
					Unity.Mathematics.Random random = new Unity.Mathematics.Random(1);
					DynamicBuffer<FloatBufferElement> test = EntityManager.AddBuffer<FloatBufferElement>(a);
					for (int i = 0; i < 10; i++)
					{
						test.Add(new FloatBufferElement { Value = random.NextFloat(0, 10) });
					}
						random = new Unity.Mathematics.Random(2);
						DynamicBuffer<FloatBufferElement> testA = EntityManager.AddBuffer<FloatBufferElement>(b);
						for (int i = 0; i < 10; i++)
						{
							testA.Add(new FloatBufferElement { Value = random.NextFloat(0, 10) });
						}
						DynamicBuffer<DynamicBufferFloatBufferElement> testB = EntityManager.AddBuffer<DynamicBufferFloatBufferElement>(c);
						test = EntityManager.GetBuffer<FloatBufferElement>(a);
						testA = EntityManager.GetBuffer<FloatBufferElement>(b);
						testB.Add(new DynamicBufferFloatBufferElement {
							Value = test,
							mEntity = a
						});
						testB.Add(new DynamicBufferFloatBufferElement {
							Value = testA,
							mEntity = b
						});
						Debug.Log("before");
						DynamicBuffer<FloatBufferElement> _a ;
						for (int i = 0; i < testB.Length; i++)
						{
							for (int j = 0; j < testB[i].Value.Length; j++)
							{
								//Debug.Log("i = " + i + ": " + testB[i].Value[j].Value);
								_a = testB[i].Value;
								FloatBufferElement.Sort(ref _a);
								testB[i] = new DynamicBufferFloatBufferElement
								{
									Value = _a,
									mEntity = testB[i].mEntity
								};
							}

						}*/
			}
			// other tests
			{
				/*		Unity.Mathematics.Random random = new Unity.Mathematics.Random(1);
						Entity a = EntityManager.CreateEntity();
						DynamicBuffer<ProceduralGenerationSpawnDataBufferElement> tmp = EntityManager.AddBuffer<ProceduralGenerationSpawnDataBufferElement>(a);
						for (int i = 0; i < 10; i++)
							tmp.Add(new ProceduralGenerationSpawnDataBufferElement {
								Value = new ProceduralGenerationSpawnData
								{
									Amount = 1,
									IgnoreCollisions = false,
									MaxSteepness = 1.0f,
									MinGenerationHeight = random.NextFloat(0,10)
								}
							});
						for (int i = 0; i < 10; i++)
							Debug.Log(tmp[i].Value.MinGenerationHeight);
						ProceduralGenerationSpawnDataBufferElement.SortByMinHeight(ref tmp);
						Debug.Log("After");
						for (int i = 0; i < 10; i++)
							Debug.Log(tmp[i].Value.MinGenerationHeight);
					*/

			}
		}

		protected override void OnUpdate()
		{
			Dependency = JobHandle.CombineDependencies(m_ExportPhysicsWorld.GetOutputDependency(), Dependency);
			Dependency = JobHandle.CombineDependencies(m_TriggerSystem.OutDependency, Dependency);

			var ecb = m_EndSimulationEcbSystem.CreateCommandBuffer().AsParallelWriter();

			NativeList<Entity> entitiesToUpdate = new NativeList<Entity>(0,Allocator.TempJob);
			NativeList<Translation> newPositions = new NativeList<Translation>(0,Allocator.TempJob);
			Entities
				.WithName("ProceduralGenerationUpdateSystem")
				.WithBurst()
				.ForEach((Entity entity,int entityInQueryIndex,ref DynamicBuffer<FloatBufferElement> HeightMapBuffer,ref DynamicBuffer<ProceduralGenerationSpawnDataBufferElement> SpawnElementsBuffer,
				ref ProceduralGenerationData proceduralGenerationData) => {
					Debug.Log(proceduralGenerationData.ToString());
					if (!proceduralGenerationData.Scale.Equals(new float3(0, 0, 0)))
						if (proceduralGenerationData.ForceUpdate)
						{
							NativeArray<Translation> positions;
							NativeArray<Entity> entities = proceduralGenerationData.UpdateAllEntities(ecb, entityInQueryIndex,HeightMapBuffer, SpawnElementsBuffer,out positions,Allocator.Temp,false);
							proceduralGenerationData.ForceUpdate = false;

							for(int i = 0; i < entities.Length; i++)
							{
								entitiesToUpdate.Add(entities[i]);
								newPositions.Add(positions[i]);
							}

							entities.Dispose();
							positions.Dispose();
						}
				}).Schedule(Dependency).Complete();

			if (entitiesToUpdate.Length > 0)
			{
				// update the positions
				for (int i = 0; i < entitiesToUpdate.Length; i++)
				{
					SetComponent(entitiesToUpdate[i], newPositions[i]);
				}
			}

			entitiesToUpdate.Dispose();
			newPositions.Dispose();

			// all Entities outside thier area will be deleted
			Entities
				.WithName("ProceduralGenerationElementSystem")
				.WithoutBurst()
				.ForEach((Entity entity, in ProceduralGenerationElement element, in Parent parent, in DynamicBuffer<StatefulTriggerEvent> triggerBuffer) =>
				{
					for (int i = 0; i < triggerBuffer.Length; i++)
					{
						StatefulTriggerEvent triggerEvent = triggerBuffer[i];
						Entity other = triggerEvent.GetOtherEntity(entity);
						if (other == parent.Value)
							Debug.Log("Found a match");
						if (!m_NonTriggerDynamicBodyMask.Matches(other) && triggerEvent.State == EventOverlapState.Exit)
						{
							if (HasComponent<ProceduralGenerationData>(parent.Value))
							{
								ProceduralGenerationData data = GetComponent<ProceduralGenerationData>(parent.Value);
					//			ProceduralGenerationData.GenerateElement(EntityManager, data,);
							}
							else Debug.LogError("ProceduralAreaEntity does not have type ProceduralGenerationData!");
						}
					}
				}).Run();
			m_EndFramePhysicsSystem.AddInputDependency(Dependency);


		}
	}

	public struct PGDS : IComponentData{}

	public struct ProceduralGenerationData : IComponentData
	{
		public int3 MaxSize;
		public float3 Scale;
		public BlittableBool ForceUpdate;


		/// <summary>
		/// Name of the area (remeber 64 bytes = 30 characters)
		/// </summary>
		public FixedString64 Name;
		/// <summary>
		/// Entity of the area
		/// </summary>
		public Entity mEntity;

		// EntityManager Initialization

		/// <summary>
		/// Initializes the struct and adds the given aruments
		/// </summary>
		/// <param name="maxSize">max size of the area</param>
		/// <param name="scale">scale of the area</param>
		/// <param name="forceUpdate">set this to true to force an update</param>
		/// <param name="name">name of the area</param>
		/// <param name="entity">associated entity</param>
		/// <param name="entityManager">EntitYManager</param>
		public ProceduralGenerationData(int3 maxSize,float3 scale,bool forceUpdate,string name,Entity entity,EntityManager entityManager)
		{
			this = new ProceduralGenerationData { };
			InitializeDefaultVariables(maxSize, scale, forceUpdate, name, entity);
			AddHeightMap(entityManager);
			AddEntitiesInArea(entityManager);
			AddProceduralGenerationSpawnData(entityManager);
		}
		/// <summary>
		/// Initializes the struct and adds the given aruments
		/// </summary>
		/// <param name="maxSize">max size of the area</param>
		/// <param name="scale">scale of the area</param>
		/// <param name="forceUpdate">set this to true to force an update</param>
		/// <param name="name">name of the area</param>
		/// <param name="entity">associated entity</param>
		/// <param name="entityManager">EntitYManager</param>
		/// <param name="heightMap">height map to be set</param>
		public ProceduralGenerationData(int3 maxSize, float3 scale, bool forceUpdate, string name, Entity entity, EntityManager entityManager,DynamicBuffer<FloatBufferElement> heightMap)
		{
			this = new ProceduralGenerationData { };
			InitializeDefaultVariables(maxSize, scale, forceUpdate, name, entity);
			DynamicBuffer<FloatBufferElement> HeightMap = AddHeightMap(entityManager);
			HeightMap.CopyFrom(heightMap);
			AddEntitiesInArea(entityManager);
			AddProceduralGenerationSpawnData(entityManager);
		}
		/// <summary>
		/// Initializes the struct and adds the given aruments
		/// </summary>
		/// <param name="maxSize">max size of the area</param>
		/// <param name="scale">scale of the area</param>
		/// <param name="forceUpdate">set this to true to force an update</param>
		/// <param name="name">name of the area</param>
		/// <param name="entity">associated entity</param>
		/// <param name="entityManager">EntitYManager</param>
		/// <param name="heightMap">height map to be set</param>
		public ProceduralGenerationData(int3 maxSize, float3 scale, bool forceUpdate, string name, Entity entity, EntityManager entityManager, NativeArray<FloatBufferElement> heightMap)
		{
			this = new ProceduralGenerationData { };
			InitializeDefaultVariables(maxSize, scale, forceUpdate, name, entity);
			DynamicBuffer<FloatBufferElement> HeightMap = AddHeightMap(entityManager);
			HeightMap.CopyFrom(heightMap);
			AddEntitiesInArea(entityManager);
			AddProceduralGenerationSpawnData(entityManager);
		}
		/// <summary>
		/// Initializes the struct and adds the given aruments
		/// </summary>
		/// <param name="maxSize">max size of the area</param>
		/// <param name="scale">scale of the area</param>
		/// <param name="forceUpdate">set this to true to force an update</param>
		/// <param name="name">name of the area</param>
		/// <param name="entity">associated entity</param>
		/// <param name="entityManager">EntitYManager</param>
		/// <param name="heightMap">height map to be set</param>
		/// <param name="entitiesInArea">entities within the area</param>
		public ProceduralGenerationData(int3 maxSize, float3 scale, bool forceUpdate, string name, Entity entity, EntityManager entityManager,DynamicBuffer<FloatBufferElement> heightMap,
			DynamicBuffer<EntityBufferElement> entitiesInArea)
		{
			this = new ProceduralGenerationData { };
			InitializeDefaultVariables(maxSize, scale, forceUpdate, name, entity);
			DynamicBuffer<FloatBufferElement> HeightMap = AddHeightMap(entityManager);
			HeightMap.CopyFrom(heightMap);
			DynamicBuffer<EntityBufferElement> entities = AddEntitiesInArea(entityManager);
			entities.CopyFrom(entitiesInArea);
			AddProceduralGenerationSpawnData(entityManager);
		}
		/// <summary>
		/// Initializes the struct and adds the given aruments
		/// </summary>
		/// <param name="maxSize">max size of the area</param>
		/// <param name="scale">scale of the area</param>
		/// <param name="forceUpdate">set this to true to force an update</param>
		/// <param name="name">name of the area</param>
		/// <param name="entity">associated entity</param>
		/// <param name="entityManager">EntitYManager</param>
		/// <param name="heightMap">height map to be set</param>
		/// <param name="entitiesInArea">entities within the area</param>
		public ProceduralGenerationData(int3 maxSize, float3 scale, bool forceUpdate, string name, Entity entity, EntityManager entityManager,NativeArray<FloatBufferElement> heightMap,
			NativeArray<EntityBufferElement> entitiesInArea)
		{
			this = new ProceduralGenerationData { };
			InitializeDefaultVariables(maxSize, scale, forceUpdate, name, entity);
			DynamicBuffer<FloatBufferElement> HeightMap = AddHeightMap(entityManager);
			HeightMap.CopyFrom(heightMap);
			DynamicBuffer<EntityBufferElement> entities = AddEntitiesInArea(entityManager);
			entities.CopyFrom(entitiesInArea);
			AddProceduralGenerationSpawnData(entityManager);
		}
		/// <summary>
		/// Initializes the struct and adds the given aruments
		/// </summary>
		/// <param name="maxSize">max size of the area</param>
		/// <param name="scale">scale of the area</param>
		/// <param name="forceUpdate">set this to true to force an update</param>
		/// <param name="name">name of the area</param>
		/// <param name="entity">associated entity</param>
		/// <param name="entityManager">EntitYManager</param>
		/// <param name="heightMap">height map to be set</param>
		/// <param name="entitiesInArea">entities within the area</param>
		/// <param name="spawnDatas">spawn data elements</param>
		public ProceduralGenerationData(int3 maxSize, float3 scale, bool forceUpdate, string name, Entity entity, EntityManager entityManager, DynamicBuffer<FloatBufferElement> heightMap,
			DynamicBuffer<EntityBufferElement> entitiesInArea, DynamicBuffer<ProceduralGenerationSpawnDataBufferElement> spawnDatas)
		{
			this = new ProceduralGenerationData { };
			InitializeDefaultVariables(maxSize, scale, forceUpdate, name, entity);
			DynamicBuffer<FloatBufferElement> HeightMap = AddHeightMap(entityManager);
			HeightMap.CopyFrom(heightMap);
			DynamicBuffer<EntityBufferElement> entities = AddEntitiesInArea(entityManager);
			entities.CopyFrom(entitiesInArea);
			DynamicBuffer<ProceduralGenerationSpawnDataBufferElement> SpawnDatas = AddProceduralGenerationSpawnData(entityManager);
			SpawnDatas.CopyFrom(spawnDatas);
		}
		/// <summary>
		/// Initializes the struct and adds the given aruments
		/// </summary>
		/// <param name="maxSize">max size of the area</param>
		/// <param name="scale">scale of the area</param>
		/// <param name="forceUpdate">set this to true to force an update</param>
		/// <param name="name">name of the area</param>
		/// <param name="entity">associated entity</param>
		/// <param name="entityManager">EntitYManager</param>
		/// <param name="heightMap">height map to be set</param>
		/// <param name="entitiesInArea">entities within the area</param>
		/// <param name="spawnDatas">spawn data elements</param>
		public ProceduralGenerationData(int3 maxSize, float3 scale, bool forceUpdate, string name, Entity entity, EntityManager entityManager, NativeArray<FloatBufferElement> heightMap,
			NativeArray<EntityBufferElement> entitiesInArea, NativeArray<ProceduralGenerationSpawnDataBufferElement> spawnDatas)
		{
			this = new ProceduralGenerationData { };
			InitializeDefaultVariables(maxSize, scale, forceUpdate, name, entity);
			DynamicBuffer<FloatBufferElement> HeightMap = AddHeightMap(entityManager);
			HeightMap.CopyFrom(heightMap);
			DynamicBuffer<EntityBufferElement> entities = AddEntitiesInArea(entityManager);
			entities.CopyFrom(entitiesInArea);
			DynamicBuffer<ProceduralGenerationSpawnDataBufferElement> SpawnDatas = AddProceduralGenerationSpawnData(entityManager);
			SpawnDatas.CopyFrom(spawnDatas);
		}
		
		/// <summary>
		/// Initializes the struct and adds the given aruments
		/// </summary>
		/// <param name="maxSize">max size of the area</param>
		/// <param name="scale">scale of the area</param>
		/// <param name="forceUpdate">set this to true to force an update</param>
		/// <param name="name">name of the area</param>
		/// <param name="entity">associated entity</param>
		/// <param name="ecb">EntityCommandBuffer</param>
		/// <param name="heightMap">height map to be set</param>
		/// <param name="entitiesInArea">entities within the area</param>
		/// <param name="outHeightMap">the DynamicBuffer<FloatBufferElement> associated with the entity</param>
		/// <param name="outEntities">the DynamicBuffer<EntityBufferElement> associated with the entity</param>
		/// <param name="outSpawnDatas">the DynamicBuffer<ProceduralGenerationSpawnDataBufferElement> associated with the entity</param>
		public ProceduralGenerationData(int3 maxSize, float3 scale, bool forceUpdate, string name, Entity entity, EntityManager entityManager,
			out DynamicBuffer<FloatBufferElement> outHeightMap, out DynamicBuffer<EntityBufferElement> outEntities, out DynamicBuffer<ProceduralGenerationSpawnDataBufferElement> outSpawnDatas)
		{
			this = new ProceduralGenerationData { };
			InitializeDefaultVariables(maxSize, scale, forceUpdate, name, entity);
			outHeightMap = AddHeightMap(entityManager);
			outEntities = AddEntitiesInArea(entityManager);
			outSpawnDatas = AddProceduralGenerationSpawnData(entityManager);
			// the first 2 dynamic buffers are invalid so we have to grab them again
			outHeightMap = GetHeightMap(entityManager);
			outEntities = GetEntitiesInArea(entityManager);
		}

		/// <summary>
		/// Initializes the struct and adds the given aruments
		/// </summary>
		/// <param name="maxSize">max size of the area</param>
		/// <param name="scale">scale of the area</param>
		/// <param name="forceUpdate">set this to true to force an update</param>
		/// <param name="name">name of the area</param>
		/// <param name="entity">associated entity</param>
		/// <param name="ecb">EntityCommandBuffer</param>
		/// <param name="heightMap">height map to be set</param>
		/// <param name="outHeightMap">the DynamicBuffer<FloatBufferElement> associated with the entity</param>
		/// <param name="outEntities">the DynamicBuffer<EntityBufferElement> associated with the entity</param>
		/// <param name="outSpawnDatas">the DynamicBuffer<ProceduralGenerationSpawnDataBufferElement> associated with the entity</param>
		public ProceduralGenerationData(int3 maxSize, float3 scale, bool forceUpdate, string name, Entity entity, EntityManager entityManager, DynamicBuffer<FloatBufferElement> heightMap,
			out DynamicBuffer<FloatBufferElement> outHeightMap, out DynamicBuffer<EntityBufferElement> outEntities, out DynamicBuffer<ProceduralGenerationSpawnDataBufferElement> outSpawnDatas)
		{
			this = new ProceduralGenerationData { };
			InitializeDefaultVariables(maxSize, scale, forceUpdate, name, entity);
			outHeightMap = AddHeightMap(entityManager);
			outHeightMap.CopyFrom(heightMap);
			outEntities = AddEntitiesInArea(entityManager);
			outSpawnDatas = AddProceduralGenerationSpawnData(entityManager);
			// the first 2 dynamic buffers are invalid so we have to grab them again
			outHeightMap = GetHeightMap(entityManager);
			outEntities = GetEntitiesInArea(entityManager);
		}
		/// <summary>
		/// Initializes the struct and adds the given aruments
		/// </summary>
		/// <param name="maxSize">max size of the area</param>
		/// <param name="scale">scale of the area</param>
		/// <param name="forceUpdate">set this to true to force an update</param>
		/// <param name="name">name of the area</param>
		/// <param name="entity">associated entity</param>
		/// <param name="ecb">EntityCommandBuffer</param>
		/// <param name="heightMap">height map to be set</param>
		/// <param name="outHeightMap">the DynamicBuffer<FloatBufferElement> associated with the entity</param>
		/// <param name="outEntities">the DynamicBuffer<EntityBufferElement> associated with the entity</param>
		/// <param name="outSpawnDatas">the DynamicBuffer<ProceduralGenerationSpawnDataBufferElement> associated with the entity</param>
		public ProceduralGenerationData(int3 maxSize, float3 scale, bool forceUpdate, string name, Entity entity, EntityManager entityManager, NativeArray<FloatBufferElement> heightMap,
			out DynamicBuffer<FloatBufferElement> outHeightMap, out DynamicBuffer<EntityBufferElement> outEntities, out DynamicBuffer<ProceduralGenerationSpawnDataBufferElement> outSpawnDatas)
		{
			this = new ProceduralGenerationData { };
			InitializeDefaultVariables(maxSize, scale, forceUpdate, name, entity);
			outHeightMap = AddHeightMap(entityManager);
			outHeightMap.CopyFrom(heightMap);
			outEntities = AddEntitiesInArea(entityManager);
			outSpawnDatas = AddProceduralGenerationSpawnData(entityManager);
			// the first 2 dynamic buffers are invalid so we have to grab them again
			outHeightMap = GetHeightMap(entityManager);
			outEntities = GetEntitiesInArea(entityManager);
		}


		/// <summary>
		/// Initializes the struct and adds the given aruments
		/// </summary>
		/// <param name="maxSize">max size of the area</param>
		/// <param name="scale">scale of the area</param>
		/// <param name="forceUpdate">set this to true to force an update</param>
		/// <param name="name">name of the area</param>
		/// <param name="entity">associated entity</param>
		/// <param name="ecb">EntityCommandBuffer</param>
		/// <param name="heightMap">height map to be set</param>
		/// <param name="entitiesInArea">entities within the area</param>
		/// <param name="outHeightMap">the DynamicBuffer<FloatBufferElement> associated with the entity</param>
		/// <param name="outEntities">the DynamicBuffer<EntityBufferElement> associated with the entity</param>
		/// <param name="outSpawnDatas">the DynamicBuffer<ProceduralGenerationSpawnDataBufferElement> associated with the entity</param>
		public ProceduralGenerationData(int3 maxSize, float3 scale, bool forceUpdate, string name, Entity entity, EntityManager entityManager, DynamicBuffer<FloatBufferElement> heightMap,
			DynamicBuffer<EntityBufferElement> entitiesInArea, out DynamicBuffer<FloatBufferElement> outHeightMap, out DynamicBuffer<EntityBufferElement> outEntities,
			out DynamicBuffer<ProceduralGenerationSpawnDataBufferElement> outSpawnDatas)
		{
			this = new ProceduralGenerationData { };
			InitializeDefaultVariables(maxSize, scale, forceUpdate, name, entity);
			outHeightMap = AddHeightMap(entityManager);
			outHeightMap.CopyFrom(heightMap);
			outEntities = AddEntitiesInArea(entityManager);
			outEntities.CopyFrom(entitiesInArea);
			outSpawnDatas = AddProceduralGenerationSpawnData(entityManager);
			// the first 2 dynamic buffers are invalid so we have to grab them again
			outHeightMap = GetHeightMap(entityManager);
			outEntities = GetEntitiesInArea(entityManager);
		}
		
		/// <summary>
		/// Initializes the struct and adds the given aruments
		/// </summary>
		/// <param name="maxSize">max size of the area</param>
		/// <param name="scale">scale of the area</param>
		/// <param name="forceUpdate">set this to true to force an update</param>
		/// <param name="name">name of the area</param>
		/// <param name="entity">associated entity</param>
		/// <param name="ecb">EntityCommandBuffer</param>
		/// <param name="heightMap">height map to be set</param>
		/// <param name="entitiesInArea">entities within the area</param>
		/// <param name="outHeightMap">the DynamicBuffer<FloatBufferElement> associated with the entity</param>
		/// <param name="outEntities">the DynamicBuffer<EntityBufferElement> associated with the entity</param>
		/// <param name="outSpawnDatas">the DynamicBuffer<ProceduralGenerationSpawnDataBufferElement> associated with the entity</param>
		public ProceduralGenerationData(int3 maxSize, float3 scale, bool forceUpdate, string name, Entity entity, EntityManager entityManager, NativeArray<FloatBufferElement> heightMap,
			NativeArray<EntityBufferElement> entitiesInArea, out DynamicBuffer<FloatBufferElement> outHeightMap, out DynamicBuffer<EntityBufferElement> outEntities,
			out DynamicBuffer<ProceduralGenerationSpawnDataBufferElement> outSpawnDatas)
		{
			this = new ProceduralGenerationData { };
			InitializeDefaultVariables(maxSize, scale, forceUpdate, name, entity);
			outHeightMap = AddHeightMap(entityManager);
			outHeightMap.CopyFrom(heightMap);
			outEntities = AddEntitiesInArea(entityManager);
			outEntities.CopyFrom(entitiesInArea);
			outSpawnDatas = AddProceduralGenerationSpawnData(entityManager);
			// the first 2 dynamic buffers are invalid so we have to grab them again
			outHeightMap = GetHeightMap(entityManager);
			outEntities = GetEntitiesInArea(entityManager);
		}

		/// <summary>
		/// Initializes the struct and adds the given aruments
		/// </summary>
		/// <param name="maxSize">max size of the area</param>
		/// <param name="scale">scale of the area</param>
		/// <param name="forceUpdate">set this to true to force an update</param>
		/// <param name="name">name of the area</param>
		/// <param name="entity">associated entity</param>
		/// <param name="entityManager">EntitYManager</param>
		/// <param name="heightMap">height map to be set</param>
		/// <param name="entitiesInArea">entities within the area</param>
		/// <param name="spawnDatas">spawn data elements</param>
		/// <param name="outHeightMap">the DynamicBuffer<FloatBufferElement> associated with the entity</param>
		/// <param name="outEntities">the DynamicBuffer<EntityBufferElement> associated with the entity</param>
		/// <param name="outSpawnDatas">the DynamicBuffer<ProceduralGenerationSpawnDataBufferElement> associated with the entity</param>
		public ProceduralGenerationData(int3 maxSize, float3 scale, bool forceUpdate, string name, Entity entity, EntityManager entityManager, DynamicBuffer<FloatBufferElement> heightMap,
			DynamicBuffer<EntityBufferElement> entitiesInArea, DynamicBuffer<ProceduralGenerationSpawnDataBufferElement> spawnDatas, 
			out DynamicBuffer<FloatBufferElement> outHeightMap,out DynamicBuffer<EntityBufferElement> outEntities,out DynamicBuffer<ProceduralGenerationSpawnDataBufferElement> outSpawnDatas)
		{
			this = new ProceduralGenerationData { };
			InitializeDefaultVariables(maxSize, scale, forceUpdate, name, entity);
			outHeightMap = AddHeightMap(entityManager);
			outHeightMap.CopyFrom(heightMap);
			outEntities = AddEntitiesInArea(entityManager);
			outEntities.CopyFrom(entitiesInArea);
			outSpawnDatas = AddProceduralGenerationSpawnData(entityManager);
			outSpawnDatas.CopyFrom(spawnDatas);
			// the first 2 dynamic buffers are invalid so we have to grab them again
			outHeightMap = GetHeightMap(entityManager);
			outEntities = GetEntitiesInArea(entityManager);
		}
		/// <summary>
		/// Initializes the struct and adds the given aruments
		/// </summary>
		/// <param name="maxSize">max size of the area</param>
		/// <param name="scale">scale of the area</param>
		/// <param name="forceUpdate">set this to true to force an update</param>
		/// <param name="name">name of the area</param>
		/// <param name="entity">associated entity</param>
		/// <param name="entityManager">EntitYManager</param>
		/// <param name="heightMap">height map to be set</param>
		/// <param name="entitiesInArea">entities within the area</param>
		/// <param name="spawnDatas">spawn data elements</param>
		/// <param name="outHeightMap">the DynamicBuffer<FloatBufferElement> associated with the entity</param>
		/// <param name="outEntities">the DynamicBuffer<EntityBufferElement> associated with the entity</param>
		/// <param name="outSpawnDatas">the DynamicBuffer<ProceduralGenerationSpawnDataBufferElement> associated with the entity</param>
		public ProceduralGenerationData(int3 maxSize, float3 scale, bool forceUpdate, string name, Entity entity, EntityManager entityManager, NativeArray<FloatBufferElement> heightMap,
			NativeArray<EntityBufferElement> entitiesInArea, NativeArray<ProceduralGenerationSpawnDataBufferElement> spawnDatas, 
			out DynamicBuffer<FloatBufferElement> outHeightMap,out DynamicBuffer<EntityBufferElement> outEntities,out DynamicBuffer<ProceduralGenerationSpawnDataBufferElement> outSpawnDatas)
		{
			this = new ProceduralGenerationData { };
			InitializeDefaultVariables(maxSize, scale, forceUpdate, name, entity);
			outHeightMap = AddHeightMap(entityManager);
			outHeightMap.CopyFrom(heightMap);
			outEntities = AddEntitiesInArea(entityManager);
			outEntities.CopyFrom(entitiesInArea);
			outSpawnDatas = AddProceduralGenerationSpawnData(entityManager);
			outSpawnDatas.CopyFrom(spawnDatas);
			// the first 2 dynamic buffers are invalid so we have to grab them again
			outHeightMap = GetHeightMap(entityManager);
			outEntities = GetEntitiesInArea(entityManager);
		}

		// EntityCommandBuffer Initialization

		/// <summary>
		/// Initializes the struct and adds the given aruments
		/// </summary>
		/// <param name="maxSize">max size of the area</param>
		/// <param name="scale">scale of the area</param>
		/// <param name="forceUpdate">set this to true to force an update</param>
		/// <param name="name">name of the area</param>
		/// <param name="entity">associated entity</param>
		/// <param name="ecb">EntityCommandBuffer</param>
		public ProceduralGenerationData(int3 maxSize,float3 scale,bool forceUpdate,string name,Entity entity,EntityCommandBuffer.ParallelWriter ecb,int jobIndex)
		{
			this = new ProceduralGenerationData { };
			InitializeDefaultVariables(maxSize, scale, forceUpdate, name, entity);
			AddHeightMap(ecb, jobIndex);
			AddEntitiesInArea(ecb, jobIndex);
			AddProceduralGenerationSpawnData(ecb, jobIndex);
		}
		/// <summary>
		/// Initializes the struct and adds the given aruments
		/// </summary>
		/// <param name="maxSize">max size of the area</param>
		/// <param name="scale">scale of the area</param>
		/// <param name="forceUpdate">set this to true to force an update</param>
		/// <param name="name">name of the area</param>
		/// <param name="entity">associated entity</param>
		/// <param name="ecb">EntityCommandBuffer.Concurrent</param>
		/// <param name="heightMap">height map to be set</param>
		public ProceduralGenerationData(int3 maxSize, float3 scale, bool forceUpdate, string name, Entity entity, EntityCommandBuffer.ParallelWriter ecb,int jobIndex, DynamicBuffer<FloatBufferElement> heightMap)
		{
			this = new ProceduralGenerationData { };
			InitializeDefaultVariables(maxSize, scale, forceUpdate, name, entity);
			DynamicBuffer<FloatBufferElement> HeightMap = AddHeightMap(ecb, jobIndex);
			HeightMap.CopyFrom(heightMap);
			AddEntitiesInArea(ecb, jobIndex);
			AddProceduralGenerationSpawnData(ecb, jobIndex);
		}
		/// <summary>
		/// Initializes the struct and adds the given aruments
		/// </summary>
		/// <param name="maxSize">max size of the area</param>
		/// <param name="scale">scale of the area</param>
		/// <param name="forceUpdate">set this to true to force an update</param>
		/// <param name="name">name of the area</param>
		/// <param name="entity">associated entity</param>
		/// <param name="ecb">EntityCommandBuffer.Concurrent</param>
		/// <param name="heightMap">height map to be set</param>
		/// <param name="entitiesInArea">entities within the area</param>
		public ProceduralGenerationData(int3 maxSize, float3 scale, bool forceUpdate, string name, Entity entity, EntityCommandBuffer.ParallelWriter ecb,int jobIndex, DynamicBuffer<FloatBufferElement> heightMap,
			DynamicBuffer<EntityBufferElement> entitiesInArea)
		{
			this = new ProceduralGenerationData { };
			InitializeDefaultVariables(maxSize, scale, forceUpdate, name, entity);
			DynamicBuffer<FloatBufferElement> HeightMap = AddHeightMap(ecb, jobIndex);
			HeightMap.CopyFrom(heightMap);
			DynamicBuffer<EntityBufferElement> entities = AddEntitiesInArea(ecb, jobIndex);
			entities.CopyFrom(entitiesInArea);
			AddProceduralGenerationSpawnData(ecb, jobIndex);
		}
		/// <summary>
		/// Initializes the struct and adds the given aruments
		/// </summary>
		/// <param name="maxSize">max size of the area</param>
		/// <param name="scale">scale of the area</param>
		/// <param name="forceUpdate">set this to true to force an update</param>
		/// <param name="name">name of the area</param>
		/// <param name="entity">associated entity</param>
		/// <param name="ecb">EntityCommandBuffer.Concurrent</param>
		/// <param name="heightMap">height map to be set</param>
		/// <param name="entitiesInArea">entities within the area</param>
		/// <param name="spawnDatas">spawn data elements</param>
		public ProceduralGenerationData(int3 maxSize, float3 scale, bool forceUpdate, string name, Entity entity, EntityCommandBuffer.ParallelWriter ecb,int jobIndex, DynamicBuffer<FloatBufferElement> heightMap,
			DynamicBuffer<EntityBufferElement> entitiesInArea, DynamicBuffer<ProceduralGenerationSpawnDataBufferElement> spawnDatas)
		{
			this = new ProceduralGenerationData { };
			InitializeDefaultVariables(maxSize, scale, forceUpdate, name, entity);
			DynamicBuffer<FloatBufferElement> HeightMap = AddHeightMap(ecb, jobIndex);
			HeightMap.CopyFrom(heightMap);
			DynamicBuffer<EntityBufferElement> entities = AddEntitiesInArea(ecb, jobIndex);
			entities.CopyFrom(entitiesInArea);
			DynamicBuffer<ProceduralGenerationSpawnDataBufferElement> SpawnDatas = AddProceduralGenerationSpawnData(ecb, jobIndex);
			SpawnDatas.CopyFrom(spawnDatas);
		}
		
		/// <summary>
		/// Initializes the struct and adds the given aruments
		/// </summary>
		/// <param name="maxSize">max size of the area</param>
		/// <param name="scale">scale of the area</param>
		/// <param name="forceUpdate">set this to true to force an update</param>
		/// <param name="name">name of the area</param>
		/// <param name="entity">associated entity</param>
		/// <param name="ecb">EntityCommandBuffer.Concurrent</param>
		/// <param name="heightMap">height map to be set</param>
		/// <param name="entitiesInArea">entities within the area</param>
		/// <param name="outHeightMap">the DynamicBuffer<FloatBufferElement> associated with the entity</param>
		/// <param name="outEntities">the DynamicBuffer<EntityBufferElement> associated with the entity</param>
		/// <param name="outSpawnDatas">the DynamicBuffer<ProceduralGenerationSpawnDataBufferElement> associated with the entity</param>
		public ProceduralGenerationData(int3 maxSize, float3 scale, bool forceUpdate, string name, Entity entity, EntityCommandBuffer.ParallelWriter ecb,int jobIndex,
			out DynamicBuffer<FloatBufferElement> outHeightMap, out DynamicBuffer<EntityBufferElement> outEntities, out DynamicBuffer<ProceduralGenerationSpawnDataBufferElement> outSpawnDatas)
		{
			this = new ProceduralGenerationData { };
			InitializeDefaultVariables(maxSize, scale, forceUpdate, name, entity);
			outHeightMap = AddHeightMap(ecb,jobIndex);
			outEntities = AddEntitiesInArea(ecb,jobIndex);
			outSpawnDatas = AddProceduralGenerationSpawnData(ecb,jobIndex);
		}

		/// <summary>
		/// Initializes the struct and adds the given aruments
		/// </summary>
		/// <param name="maxSize">max size of the area</param>
		/// <param name="scale">scale of the area</param>
		/// <param name="forceUpdate">set this to true to force an update</param>
		/// <param name="name">name of the area</param>
		/// <param name="entity">associated entity</param>
		/// <param name="ecb">EntityCommandBuffer.Concurrent</param>
		/// <param name="heightMap">height map to be set</param>
		/// <param name="outHeightMap">the DynamicBuffer<FloatBufferElement> associated with the entity</param>
		/// <param name="outEntities">the DynamicBuffer<EntityBufferElement> associated with the entity</param>
		/// <param name="outSpawnDatas">the DynamicBuffer<ProceduralGenerationSpawnDataBufferElement> associated with the entity</param>
		public ProceduralGenerationData(int3 maxSize, float3 scale, bool forceUpdate, string name, Entity entity, EntityCommandBuffer.ParallelWriter ecb,int jobIndex, DynamicBuffer<FloatBufferElement> heightMap,
			out DynamicBuffer<FloatBufferElement> outHeightMap, out DynamicBuffer<EntityBufferElement> outEntities,out DynamicBuffer<ProceduralGenerationSpawnDataBufferElement> outSpawnDatas)
		{
			this = new ProceduralGenerationData { };
			InitializeDefaultVariables(maxSize, scale, forceUpdate, name, entity);
			outHeightMap = AddHeightMap(ecb,jobIndex);
			outHeightMap.CopyFrom(heightMap);
			outEntities = AddEntitiesInArea(ecb,jobIndex);
			outSpawnDatas = AddProceduralGenerationSpawnData(ecb,jobIndex);
		}


		/// <summary>
		/// Initializes the struct and adds the given aruments
		/// </summary>
		/// <param name="maxSize">max size of the area</param>
		/// <param name="scale">scale of the area</param>
		/// <param name="forceUpdate">set this to true to force an update</param>
		/// <param name="name">name of the area</param>
		/// <param name="entity">associated entity</param>
		/// <param name="ecb">EntityCommandBuffer.Concurrent</param>
		/// <param name="heightMap">height map to be set</param>
		/// <param name="entitiesInArea">entities within the area</param>
		/// <param name="outHeightMap">the DynamicBuffer<FloatBufferElement> associated with the entity</param>
		/// <param name="outEntities">the DynamicBuffer<EntityBufferElement> associated with the entity</param>
		/// <param name="outSpawnDatas">the DynamicBuffer<ProceduralGenerationSpawnDataBufferElement> associated with the entity</param>
		public ProceduralGenerationData(int3 maxSize, float3 scale, bool forceUpdate, string name, Entity entity, EntityCommandBuffer.ParallelWriter ecb,int jobIndex, DynamicBuffer<FloatBufferElement> heightMap,
			DynamicBuffer<EntityBufferElement> entitiesInArea,out DynamicBuffer<FloatBufferElement> outHeightMap,out DynamicBuffer<EntityBufferElement> outEntities, 
			out DynamicBuffer<ProceduralGenerationSpawnDataBufferElement> outSpawnDatas)
		{
			this = new ProceduralGenerationData { };
			InitializeDefaultVariables(maxSize, scale, forceUpdate, name, entity);
			outHeightMap = AddHeightMap(ecb,jobIndex);
			outHeightMap.CopyFrom(heightMap);
			outEntities = AddEntitiesInArea(ecb,jobIndex);
			outEntities.CopyFrom(entitiesInArea);
			outSpawnDatas = AddProceduralGenerationSpawnData(ecb,jobIndex);
		}

		/// <summary>
		/// Initializes the struct and adds the given aruments
		/// </summary>
		/// <param name="maxSize">max size of the area</param>
		/// <param name="scale">scale of the area</param>
		/// <param name="forceUpdate">set this to true to force an update</param>
		/// <param name="name">name of the area</param>
		/// <param name="entity">associated entity</param>
		/// <param name="ecb">EntityCommandBuffer.Concurrent</param>
		/// <param name="heightMap">height map to be set</param>
		/// <param name="entitiesInArea">entities within the area</param>
		/// <param name="spawnDatas">spawn data elements</param>
		/// <param name="outHeightMap">the DynamicBuffer<FloatBufferElement> associated with the entity</param>
		/// <param name="outEntities">the DynamicBuffer<EntityBufferElement> associated with the entity</param>
		/// <param name="outSpawnDatas">the DynamicBuffer<ProceduralGenerationSpawnDataBufferElement> associated with the entity</param>
		public ProceduralGenerationData(int3 maxSize, float3 scale, bool forceUpdate, string name, Entity entity, EntityCommandBuffer.ParallelWriter ecb,int jobIndex, DynamicBuffer<FloatBufferElement> heightMap,
			DynamicBuffer<EntityBufferElement> entitiesInArea, DynamicBuffer<ProceduralGenerationSpawnDataBufferElement> spawnDatas,
			out DynamicBuffer<FloatBufferElement> outHeightMap, out DynamicBuffer<EntityBufferElement> outEntities, out DynamicBuffer<ProceduralGenerationSpawnDataBufferElement> outSpawnDatas)
		{
			this = new ProceduralGenerationData { };
			InitializeDefaultVariables(maxSize, scale, forceUpdate, name, entity);
			outHeightMap = AddHeightMap(ecb,jobIndex);
			outHeightMap.CopyFrom(heightMap);
			outEntities = AddEntitiesInArea(ecb,jobIndex);
			outEntities.CopyFrom(entitiesInArea);
			outSpawnDatas = AddProceduralGenerationSpawnData(ecb,jobIndex);
			outSpawnDatas.CopyFrom(spawnDatas);
		}


		private void InitializeDefaultVariables(int3 maxSize, float3 scale, bool forceUpdate, string name, Entity entity)
		{
			MaxSize = maxSize;
			Scale = scale;
			ForceUpdate = forceUpdate;
			Name = name;
			mEntity = entity;
		}

		// Height Map


		private DynamicBuffer<FloatBufferElement> AddHeightMap(EntityManager entityManager)
		{
			return FloatBufferElement.AddBuffer(entityManager, mEntity);
		}

		private DynamicBuffer<FloatBufferElement> AddHeightMap(EntityCommandBuffer.ParallelWriter ecb,int jobIndex)
		{
			return FloatBufferElement.AddBuffer(ecb,jobIndex, mEntity);
		}

		public DynamicBuffer<FloatBufferElement> GetHeightMap(EntityManager entityManager)
		{
			return FloatBufferElement.GetBuffer(entityManager, mEntity);
		}

		public void SetHeightMap(EntityManager entityManager, DynamicBuffer<FloatBufferElement> buffer)
		{
			FloatBufferElement.SetBuffer(entityManager, mEntity, buffer);
		}

		public void SetHeightMap(EntityCommandBuffer.ParallelWriter ecb, DynamicBuffer<FloatBufferElement> aBuffer, DynamicBuffer<FloatBufferElement> bBuffer)
		{
			FloatBufferElement.SetBuffer(ecb, aBuffer, bBuffer);
		}

		// All Elements In Area

		private DynamicBuffer<EntityBufferElement> AddEntitiesInArea(EntityManager entityManager)
		{
			return EntityBufferElement.AddBuffer(entityManager, mEntity);
		}

		private DynamicBuffer<EntityBufferElement> AddEntitiesInArea(EntityCommandBuffer.ParallelWriter ecb,int jobIndex)
		{
			return EntityBufferElement.AddBuffer(ecb,jobIndex, mEntity);
		}

		public DynamicBuffer<EntityBufferElement> GetEntitiesInArea(EntityManager entityManager)
		{
			return EntityBufferElement.GetBuffer(entityManager, mEntity);
		}

		public void SetEntitiesInArea(EntityManager entityManager, DynamicBuffer<EntityBufferElement> buffer)
		{
			EntityBufferElement.SetBuffer(entityManager, mEntity, buffer);
		}

		public void SetEntitiesInArea(EntityCommandBuffer.ParallelWriter ecb, DynamicBuffer<EntityBufferElement> aBuffer, DynamicBuffer<EntityBufferElement> bBuffer)
		{
			EntityBufferElement.SetBuffer(ecb, aBuffer, bBuffer);
		}

		//	public DynamicBuffer<EntityBufferElement> AllElementsInArea;

		// Spawn Datas

		private DynamicBuffer<ProceduralGenerationSpawnDataBufferElement> AddProceduralGenerationSpawnData(EntityManager entityManager)
		{
			return ProceduralGenerationSpawnDataBufferElement.AddBuffer(entityManager, mEntity);
		}

		private DynamicBuffer<ProceduralGenerationSpawnDataBufferElement> AddProceduralGenerationSpawnData(EntityCommandBuffer.ParallelWriter ecb,int jobIndex)
		{
			return ProceduralGenerationSpawnDataBufferElement.AddBuffer(ecb,jobIndex, mEntity);
		}

		public DynamicBuffer<ProceduralGenerationSpawnDataBufferElement> GetProceduralGenerationSpawnData(EntityManager entityManager)
		{
			return ProceduralGenerationSpawnDataBufferElement.GetBuffer(entityManager, mEntity);
		}

		public void SetProceduralGenerationSpawnData(EntityManager entityManager, DynamicBuffer<ProceduralGenerationSpawnDataBufferElement> buffer)
		{
			ProceduralGenerationSpawnDataBufferElement.SetBuffer(entityManager, mEntity, buffer);
		}

		public void SetProceduralGenerationSpawnData(EntityCommandBuffer.ParallelWriter ecb, DynamicBuffer<ProceduralGenerationSpawnDataBufferElement> aBuffer, DynamicBuffer<ProceduralGenerationSpawnDataBufferElement> bBuffer)
		{
			ProceduralGenerationSpawnDataBufferElement.SetBuffer(ecb, aBuffer, bBuffer);
		}


		//	public DynamicBuffer<ProceduralGenerationSpawnDataBufferElement> SpawnElements;

		// Update Function
		/// <summary>
		/// this updates the Entities and the SpawnElements data with the given arguments
		/// </summary>
		/// <param name="entityManager"></param>
		/// <param name="spawnElements"></param>
		public void UpdateAllEntities(EntityManager entityManager, DynamicBuffer<ProceduralGenerationSpawnDataBufferElement> spawnElements,Allocator allocator,bool sorted = false)
		{
			Debug.Log("Performing update using entitymanager...");
			DynamicBuffer<ProceduralGenerationSpawnDataBufferElement> SpawnElements = GetProceduralGenerationSpawnData(entityManager);
			SpawnElements.Clear();
			SpawnElements.CopyFrom(spawnElements);
			NativeArray<Entity> newEntities = GenerateAllEntitiesInArea(entityManager, this,allocator, sorted);
			DynamicBuffer<EntityBufferElement> oldEntities = entityManager.GetBuffer<EntityBufferElement>(mEntity);
			oldEntities.Clear();
			for (int i = 0; i < newEntities.Length; i++)
				oldEntities.Add(new EntityBufferElement { Value = newEntities[i] });
		}



		public void UpdateAllEntities(EntityManager entityManager,Allocator allocator,bool sorted = false)
		{
			Debug.Log("Performing update using entitymanager...");
			NativeArray<Entity> newEntities = GenerateAllEntitiesInArea(entityManager, this,allocator, sorted);
			DynamicBuffer<EntityBufferElement> oldEntities = entityManager.GetBuffer<EntityBufferElement>(mEntity);
			oldEntities.Clear();
			for (int i = 0; i < newEntities.Length; i++)
				oldEntities.Add(new EntityBufferElement { Value = newEntities[i] });
		}

		public NativeArray<Entity> UpdateAllEntities(EntityCommandBuffer.ParallelWriter ecb,int jobIndex,DynamicBuffer<FloatBufferElement> HeightMapBuffer, DynamicBuffer<ProceduralGenerationSpawnDataBufferElement> SpawnElements
			,out NativeArray<Translation> positions,Allocator allocator,bool sorted = false)
		{
			Debug.Log("Performing update using ecb...");
			return GenerateAllEntitiesInArea(ecb,jobIndex, this,HeightMapBuffer, SpawnElements,out positions,allocator,sorted);
		}



		public void Destroy(EntityManager entityManager)
		{
			// just some cleanup
			entityManager.DestroyEntity(mEntity);
		}

		public void Destroy(EntityCommandBuffer.ParallelWriter ecb,int jobIndex)
		{
			// just some cleanup
			ecb.DestroyEntity(jobIndex,mEntity);
		}

		// Generation Functions

		public static Entity GenerateEntity(EntityCommandBuffer.ParallelWriter ecb,int jobIndex,ProceduralGenerationData data,DynamicBuffer<FloatBufferElement> heightMap, ProceduralGenerationSpawnData element,
			out Translation position)
		{
			position = GetRandomPositionFromHeightMapBuffer(data,heightMap);
			return ProceduralGenerationSpawnData.GenerateEntity(ecb,jobIndex, element);
		}

		public static Entity GenerateEntity(EntityManager entityManager, ProceduralGenerationData data, DynamicBuffer<FloatBufferElement> heightMap, ProceduralGenerationSpawnData element,
		out Translation position)
		{
			position = GetRandomPositionFromHeightMapBuffer(data, heightMap);
			return ProceduralGenerationSpawnData.GenerateEntity(entityManager, element);
		}

		public static Translation GetRandomPositionFromHeightMapBuffer(ProceduralGenerationData data, DynamicBuffer<FloatBufferElement> heightMap)
		{
			// create random index from tmpHeights size
			Unity.Mathematics.Random random = new Unity.Mathematics.Random((uint)math.clamp(RandomExtensions.GenerateRandomInt(), 0, int.MaxValue));
			// choose a height randomly from the tmpHeightMap
			FloatBufferElement height = heightMap[random.NextInt(0, heightMap.Length + 1)];
			// calculate the new position
			int zIndex = (int)math.floor(height.Index / data.MaxSize.z);
			int xIndex = data.MaxSize.x - (((zIndex + 1) * data.MaxSize.x) - height.Index);
			// randomly choose locations
			return new Translation { Value = new float3(xIndex, height.Value, zIndex) };
		}

		public static NativeArray<Entity> GenerateEntitiesInArea(EntityCommandBuffer.ParallelWriter ecb, int jobIndex,ProceduralGenerationData data,DynamicBuffer<FloatBufferElement> HeightMapBuffer, 
			DynamicBuffer<FloatBufferElement> tmpHeightMap, ProceduralGenerationSpawnData element,out NativeArray<Translation> newTranslations,Allocator allocator,bool sorted = false)
		{
			GenerateValidHeights(HeightMapBuffer, ref tmpHeightMap, element.MinGenerationHeight);

			NativeArray<Entity> entityElements = ProceduralGenerationSpawnData.GenerateEntities(ecb,jobIndex,data.mEntity, element,allocator);

			newTranslations = new NativeList<Translation>(entityElements.Length,allocator);

			for (int j = 0; j < entityElements.Length; j++)
			{
				// the tmpHeight map has the data we want
				newTranslations[j] = GetRandomPositionFromHeightMapBuffer(data, tmpHeightMap);
			}
			return entityElements;
		}

		public static NativeArray<Entity> GenerateEntitiesInArea(EntityManager entityManager, ProceduralGenerationData data, ProceduralGenerationSpawnDataBufferElement element,Allocator allocator)
		{
			// create tmpHeightMap
			Entity computeEntity = entityManager.CreateEntity();
			DynamicBuffer<FloatBufferElement> tmpHeightMap = FloatBufferElement.AddBuffer(entityManager,computeEntity);

			GenerateValidHeights(FloatBufferElement.GetBuffer(entityManager,data.mEntity),ref tmpHeightMap, element.Value.MinGenerationHeight);

			NativeArray<Entity> entityElements = ProceduralGenerationSpawnData.GenerateEntities(entityManager, element.Value.EntityPrefab.Entity, element.Value,allocator);

			for (int j = 0; j < entityElements.Length; j++)
			{
				entityManager.SetComponentData(entityElements[j], GetRandomPositionFromHeightMapBuffer(data, tmpHeightMap));
			}

			// Cleanup
			entityManager.DestroyEntity(computeEntity);

			return entityElements;
		}

		public static NativeArray<Entity> GenerateAllEntitiesInArea(EntityCommandBuffer.ParallelWriter ecb,int jobIndex,ProceduralGenerationData data,DynamicBuffer<FloatBufferElement> HeightMapBuffer,
			DynamicBuffer<ProceduralGenerationSpawnDataBufferElement> SpawnElements,out NativeArray<Translation> translations,Allocator allocator, bool sorted = false)
		{
			// version 1.0 using a spawn point


			if (!sorted)
				ProceduralGenerationSpawnDataBufferElement.SortByMinHeight(ref SpawnElements,allocator);
			// now we generate the entities within the area
			float currentHeight = -1;

			Entity computeEntity = ecb.CreateEntity(jobIndex);
			DynamicBuffer<FloatBufferElement> tmpHeightMap = FloatBufferElement.AddBuffer(ecb,jobIndex, computeEntity);
			

			int finalEntitySize = 0;
			for (int i = 0; i < SpawnElements.Length; i++)
				finalEntitySize += SpawnElements[i].Value.Amount;

			NativeArray<Entity> entitiesInArea = new NativeArray<Entity>(finalEntitySize, allocator);
			translations = new NativeArray<Translation>(finalEntitySize, allocator);
			int _index = 0;

			for (int i = 0; i < SpawnElements.Length; i++)
			{
				// check to see if we entered a new height and generate crop those heights out of the overall height map and store it in a new buffer
				if (SpawnElements[i].Value.MinGenerationHeight != currentHeight)
				{
					// set the new height
					currentHeight = SpawnElements[i].Value.MinGenerationHeight;
					// just clear to "empty" the array
					tmpHeightMap.Clear();
					GenerateValidHeights(HeightMapBuffer, ref tmpHeightMap, SpawnElements[i].Value.MinGenerationHeight);
				}
				NativeArray<Translation> tmpPositions = new NativeArray<Translation>(SpawnElements[i].Value.Amount,allocator);
				NativeArray<Entity> entityElements = GenerateEntitiesInArea(ecb,jobIndex,data,HeightMapBuffer,tmpHeightMap, SpawnElements[i].Value,out translations,allocator,true);
				
				for (int j = 0; j < entityElements.Length; j++)
				{
					entitiesInArea[_index] = entityElements[j];
					translations[_index] = tmpPositions[j];
					_index++;
				}

				tmpPositions.Dispose();
				entityElements.Dispose();

			}
			ecb.DestroyEntity(jobIndex,computeEntity);

			return entitiesInArea;
		}

		public static NativeArray<Entity> GenerateAllEntitiesInArea(EntityManager entityManager,ProceduralGenerationData data,Allocator allocator, bool sorted = false)
		{
			// version 1.0 using a spawn point

			DynamicBuffer<ProceduralGenerationSpawnDataBufferElement> SpawnElements = ProceduralGenerationSpawnDataBufferElement.GetBuffer(entityManager, data.mEntity);
			DynamicBuffer<FloatBufferElement> HeightMapBuffer = FloatBufferElement.GetBuffer(entityManager, data.mEntity);

			if (!sorted)
				ProceduralGenerationSpawnDataBufferElement.SortByMinHeight(ref SpawnElements,allocator);
			// now we generate the entities within the area
			float currentHeight = -1;

			Entity computeEntity = entityManager.CreateEntity();
			DynamicBuffer<FloatBufferElement> tmpHeightMap = FloatBufferElement.AddBuffer(entityManager, computeEntity);
			

			int finalEntitySize = 0;
			for (int i = 0; i < SpawnElements.Length; i++)
				finalEntitySize += SpawnElements[i].Value.Amount;

			NativeArray<Entity> entitiesInArea = new NativeArray<Entity>(finalEntitySize, allocator);
			int _index = 0;

			for (int i = 0; i < SpawnElements.Length; i++)
			{
				// check to see if we entered a new height and generate crop those heights out of the overall height map and store it in a new buffer
				if (SpawnElements[i].Value.MinGenerationHeight != currentHeight)
				{
					// set the new height
					currentHeight = SpawnElements[i].Value.MinGenerationHeight;
					// just clear to "empty" the array
					tmpHeightMap.Clear();
					GenerateValidHeights(HeightMapBuffer, ref tmpHeightMap, SpawnElements[i].Value.MinGenerationHeight);
				}
				NativeArray<Translation> tmpPositions = new NativeArray<Translation>(SpawnElements[i].Value.Amount,allocator);
				NativeArray<Entity> entityElements = GenerateEntitiesInArea(entityManager,data, SpawnElements[i],allocator);
				
				for (int j = 0; j < entityElements.Length; j++)
				{
					entitiesInArea[_index] = entityElements[j];
					_index++;
				}

				tmpPositions.Dispose();
				entityElements.Dispose();

			}
			entityManager.DestroyEntity(computeEntity);

			return entitiesInArea;
		}

		private static void GenerateValidHeights(DynamicBuffer<FloatBufferElement> heightMap, ref DynamicBuffer<FloatBufferElement> buffer,float minHeight)
		{
			for(int i = 0; i < heightMap.Length; i++)
				if (heightMap[i].Value >= minHeight)
					buffer.Add(heightMap[i]);
		}

		public override bool Equals(object obj)
		{
			return Equals((ProceduralGenerationData)obj);
		}

		public bool Equals(ProceduralGenerationData otherData)
		{
			return (
				Name == otherData.Name &&
				MaxSize.Equals(otherData.MaxSize) &&
				Scale.Equals(otherData.Scale) &&
				ForceUpdate == otherData.ForceUpdate &&
				mEntity == otherData.mEntity
				);
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		public override string ToString()
		{
			return MaxSize.ToString()+"|"+Scale.ToString()+","+ForceUpdate.ToString()+","+Name.ToString()+","+
				mEntity.ToString();
		}

		public static ProceduralGenerationData Null => new ProceduralGenerationData();

	}

}

//	[RequiresEntityConversion]
public class ProceduralGenerationDataComponent : MonoBehaviour//, IConvertGameObjectToEntity
{
	public bool EditMode = false;
	public int3 MaxSize = new int3(3, 3, 3);
	public float3 Scale = new float3(1, 1, 1);

	public ProceduralGenerationSpawnComponent[] SpawnComponents;

	public float[] heights;

	/// <summary>
	/// Name of the area (remeber 64 bytes = 30 characters)
	/// </summary>
	public string Name = "Default";
	/*
			public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
			{
				DynamicBuffer<FloatBufferElement> HeightMapBuffer = new DynamicBuffer<FloatBufferElement>();
				DynamicBuffer<EntityBufferElement> EntitiesWithInArea = new DynamicBuffer<EntityBufferElement>();
				DynamicBuffer<ProceduralGenerationSpawnDataBufferElement> SpawnDatas = new DynamicBuffer<ProceduralGenerationSpawnDataBufferElement>();

				NativeArray<FloatBufferElement> heightmap = new NativeArray<FloatBufferElement>(FloatBufferElement.ToFloatBufferElementArray(heights), Allocator.TempJob);

				ProceduralGenerationData data = new ProceduralGenerationData(MaxSize, Scale, true, Name, 
					entity, dstManager,heightmap, out HeightMapBuffer, out EntitiesWithInArea, out SpawnDatas);

				dstManager.AddComponentData(entity, data);
				// this links the eneity to a gameobject
				dstManager.AddComponentData(entity, new PGDS { });
				heightmap.Dispose();

			}
			*/
	// Start is called before the first frame update
	void Start()
	{
		//		entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
	}

	// Update is called once per frame
	void Update()
	{

	}
}


[CustomEditor(typeof(ProceduralGenerationDataComponent))]
public class ProceduralGenerationDataComponentEditor : Editor
{
	public ProceduralGenerationDataComponent dataComponent;

	// Button Variables
	private const int BUTTON_WIDTH = 200;
	private const int BUTTON_HEIGHT = 20;

	private void Awake()
	{
		dataComponent = target as ProceduralGenerationDataComponent;
	}

	// Spawn Data
	bool SpawnDataFoldout = false;
	// heighs
	bool HeightFoldout = false;

	public override void OnInspectorGUI()
	{
		// Edit Mode
		dataComponent.EditMode = EditorGUILayout.Toggle("Edit Mode", dataComponent.EditMode);

		// Max Size
		{
			Vector3Int tmp = EditorGUILayout.Vector3IntField("Bounds", new Vector3Int(dataComponent.MaxSize.x, dataComponent.MaxSize.y, dataComponent.MaxSize.z));
			dataComponent.MaxSize = new int3(tmp.x, tmp.y, tmp.z);
		}
		// Scale
		dataComponent.Scale = EditorGUILayout.Vector3Field("Scale", dataComponent.Scale);

		// Spawn Data
		{
			SpawnDataFoldout = EditorGUILayout.Foldout(SpawnDataFoldout, "Spawn Data");
			if (SpawnDataFoldout)
			{
				int length = EditorGUILayout.IntField("Length", dataComponent.SpawnComponents.Length);
				int i = 0;
				if (length > dataComponent.SpawnComponents.Length)
				{
					// add 
					ProceduralGenerationSpawnComponent[] tmp = dataComponent.SpawnComponents;
					dataComponent.SpawnComponents = new ProceduralGenerationSpawnComponent[tmp.Length+1];
					for (i = 0; i < tmp.Length; i++)
						dataComponent.SpawnComponents[i] = tmp[i];
				}
				else if (length < dataComponent.SpawnComponents.Length)
				{
					// remove
					ProceduralGenerationSpawnComponent[] tmp = new ProceduralGenerationSpawnComponent[dataComponent.SpawnComponents.Length - 1];
					for (i = 0; i < tmp.Length; i++)
						tmp[i] = dataComponent.SpawnComponents[i];
					dataComponent.SpawnComponents = tmp;
				}

				for (i = 0; i < dataComponent.SpawnComponents.Length; i++)
				{
					dataComponent.SpawnComponents[i] = (ProceduralGenerationSpawnComponent)EditorGUILayout.ObjectField("Spawn Component "+(i+1), dataComponent.SpawnComponents[i], typeof(ProceduralGenerationSpawnComponent), true);
				}
			}
		}

		// heights
		{
			HeightFoldout = EditorGUILayout.Foldout(HeightFoldout, "Heights");
			if (HeightFoldout)
			{
				int length = dataComponent.MaxSize.x * dataComponent.MaxSize.z;
				if (length != dataComponent.heights.Length)
				{
					// resize the height array to match the size
					if (length > dataComponent.heights.Length)
					{
						float[] tmp = dataComponent.heights;
						dataComponent.heights = new float[length];
						for (int i = 0; i < tmp.Length; i++)
							dataComponent.heights[i] = tmp[i];
					}
					else if (length < dataComponent.heights.Length)
					{
						Debug.Log("AAA");
						float[] tmp = new float[length];
						for (int i = 0; i < tmp.Length; i++)
							tmp[i] = dataComponent.heights[i];
						dataComponent.heights = tmp;
					}
				}
				for (int i = 0; i < dataComponent.heights.Length; i++)
					EditorGUILayout.FloatField("height " + (i + 1), dataComponent.heights[i]);
			}
		}

	}
}


