using UnityEngine;
using System.Collections;
using Unity.Mathematics;
using Unity.Entities;
using System.Collections.Generic;
using Unity.Collections;

namespace Core.Procedural
{

	public class ProceduralGenerationComponent : MonoBehaviour
	{

		public bool EditMode = false;
		public int3 MaxSize = new int3(5, 5, 5);
		public float3 Scale = new float3(1, 1, 1);
		public bool ForceUpdate = false;
		public bool VariableUpdate = false;

		/// <summary>
		/// Name of the area (remeber 64 bytes = 30 characters)
		/// </summary>
		public string Name = "Default";
		/// <summary>
		/// Entity of the area
		/// </summary>
		public Entity mEntity = Entity.Null;

		private EntityManager entityManager;

		private ProceduralGenerationData proceduralGenerationData;

		public void SetupComponent(EntityManager e, Entity entity)
		{
			entityManager = e;
			mEntity = entity;
			GetProceduralGenerationData(entityManager);
		}


		// Use this for initialization
		void Start()
		{

		}

		// Update is called once per frame
		void Update()
		{
			if (mEntity == Entity.Null)
			{
				Debug.LogWarning("Gameobject's Entity is not valid yet");
			}
			else if (!EditMode)
			{
				if (HasChanged())
				{
					PerformUpdate();
				}
				if (VariableUpdate)
				{
					// Update the Procedural generation data
					GetProceduralGenerationData(entityManager);

					VariableUpdate = false;
				}
			}
		}

		/// <summary>
		/// Attempts to get ans set the GameObject's ProceduralGenerationData variable
		/// </summary>
		/// <param name="entityManager"></param>
		private void GetProceduralGenerationData(EntityManager entityManager)
		{
			if (entityManager.HasComponent<ProceduralGenerationData>(mEntity))
			{
				Debug.Log("updaing component data...");
				proceduralGenerationData = entityManager.GetComponentData<ProceduralGenerationData>(mEntity);
				MaxSize = proceduralGenerationData.MaxSize;
				Scale = proceduralGenerationData.Scale;
				Name = proceduralGenerationData.Name.ToString();
				OldMaxSize = MaxSize;
				OldScale = Scale;
				OldName = Name;
			}
			else
				Debug.LogWarning("This GameObject doesn't have an entity with a valid ProceduralGenerationData Component!");
		}

		// these hold the last value of a varible
		private int3 OldMaxSize = int3.zero;
		private float3 OldScale = float3.zero;
		private string OldName = "";

		void PerformUpdate()
		{
			Debug.Log("ProceduralGenerationComponent: Performing Update...");
			if (Scale.x > 0 && Scale.y > 0 && Scale.z > 0)
				proceduralGenerationData.Scale = Scale;
			//	proceduralGenerationData.mEntity = mEntity;
			if (Name != "")
				proceduralGenerationData.Name = Name;
			if (MaxSize.x > 0 && MaxSize.y > 0 && MaxSize.z > 0)
				proceduralGenerationData.MaxSize = MaxSize;
			proceduralGenerationData.ForceUpdate = true;
			entityManager.SetComponentData(mEntity, proceduralGenerationData);
			proceduralGenerationData.ForceUpdate = false;

			// set the old varibles
			OldMaxSize = MaxSize;
			OldScale = Scale;
			OldName = Name;
			ForceUpdate = false;
		}

		public bool HasChanged()
		{
			return !(OldMaxSize.Equals(MaxSize) && OldScale.Equals(Scale) && Name == OldName) || ForceUpdate;
		}


	}

	public class ProceduralGenerationSystem : SystemBase, Common.IIDSystem
	{
		EndSimulationEntityCommandBufferSystem m_EndSimulationEcbSystem;

		private EntityQuery spawnQuery;

		public GameObject prefab = null;

		public List<GameObject> ProceduralGenerationObjects = new List<GameObject>();


		// id system

		private static NativeList<int> ActiveIds;

		private static NativeList<int> AvailibleIds;
		/// <summary>
		/// removes and id when its no longer needed
		/// </summary>
		/// <param name="id"></param>
		public void RemoveId(int id)
		{
			bool match = false;
			for (int i = 0; i < ActiveIds.Length; i++)
			{
				if (id == ActiveIds[i])
				{
					// if id is at the end then we just remove it
					if (i == ActiveIds.Length - 1)
					{
						ActiveIds.RemoveAt(i);
					}
					else
					{
						ActiveIds.RemoveAt(i);
						AvailibleIds.Add(id);
					}
					match = true;
				}
			}
			if (!match)
				Debug.LogWarning("ProceduralgenerationSpawnData: Failed to remove id: id not found");
		}
		/// <summary>
		/// Initializes the id arrays
		/// </summary>
		public void InitilizeIDArrays()
		{
			ActiveIds = new NativeList<int>(Allocator.Persistent);
			AvailibleIds = new NativeList<int>(Allocator.Persistent);
		}
		/// <summary>
		/// Generates and returns a new ActiveId
		/// </summary>
		/// <returns></returns>
		public int GenerateId()
		{
			if (AvailibleIds.Length > 0)
			{
				// use the ids in availible ids
				ActiveIds.Add(AvailibleIds[0]);
				AvailibleIds.RemoveAt(0);
				return ActiveIds[ActiveIds.Length - 1];
			}
			else
			{
				// create a new id and return it
				ActiveIds.Add(ActiveIds.Length );
				return ActiveIds.Length - 1;
			}
		}

		protected override void OnCreate()
		{
			base.OnCreate();

			prefab = Resources.Load("Prefabs/ProceduralGeneration/ProceduralGenerationPrefab") as GameObject;
			if (prefab == null)
				Debug.LogError("Failed to load Prefab!");

			// Find the ECB system once and store it for later usage
			m_EndSimulationEcbSystem = World
				.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

			spawnQuery = EntityManager.CreateEntityQuery(typeof(PGDS));

			InitilizeIDArrays();
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();

			// cleanup id system

			ActiveIds.Dispose();
			AvailibleIds.Dispose();
		}

		protected override void OnUpdate()
		{
			int entityCount = spawnQuery.CalculateEntityCount();
			if (entityCount != 0)
			{
				var ecb = m_EndSimulationEcbSystem.CreateCommandBuffer().AsParallelWriter();

				GameObject tmpPrefab = prefab;
				List<GameObject> gameObjects = this.ProceduralGenerationObjects;

				for(int i = 0; i < entityCount; i++)
				{
					tmpPrefab.name = "ProceduralGeneration:" + GenerateId().ToString();
					ProceduralGenerationObjects.Add(GameObject.Instantiate(tmpPrefab));
				}

				// Now set the entities to the gameobjects
				Entities.WithoutBurst()
					.ForEach((Entity entity,PGDS a)=> {
						bool match = false;
						for(int i = 0; i < gameObjects.Count; i++)
						{
							ProceduralGenerationComponent component = gameObjects[i].GetComponent<ProceduralGenerationComponent>();
							if (component.mEntity == Entity.Null)
							{
								component.SetupComponent(EntityManager, entity);
								match = true;
								break;
							}
						}
						if (!match)
							Debug.LogWarning("Failed to find empty GameObject for entity");
					}).Run();

				// remove the component
				Entities.WithBurst()
					.ForEach((Entity entity, int entityInQueryIndex, PGDS a) =>
					{
						ecb.RemoveComponent<PGDS>(entityInQueryIndex, entity);
					}).Schedule();


				// Make sure that the ECB system knows about our job
				m_EndSimulationEcbSystem.AddJobHandleForProducer(this.Dependency);
			}
		}

		
	}

}
