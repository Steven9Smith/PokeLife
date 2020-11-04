using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Core.Extensions;
using UnityEngine;

// Make sure you use Convert And Inject if you want to be able to changes the values at run time
[RequiresEntityConversion]
public class ProceduralGenerationElementComponent : MonoBehaviour { 
	
	// Start is called before the first frame update
	void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

namespace Core.Procedural
{
	public struct ProceduralGenerationElement : IComponentData
	{
		public Entity BaseEntity;

		public ProceduralGenerationElement(EntityManager entityManager,Entity proceduralAreaEntity,Entity entity)
		{
			BaseEntity = entity;
			ParentExtensions.AddParent(entityManager, entity, proceduralAreaEntity);
		}

		public ProceduralGenerationElement(EntityCommandBuffer.ParallelWriter commandBuffer,int jobIndex, Entity proceduralAreaEntity, Entity entity)
		{

			BaseEntity = entity;
			ParentExtensions.AddParent(commandBuffer,jobIndex, entity, proceduralAreaEntity);
		}

		public void Destroy(EntityManager entityManager)
		{
			entityManager.DestroyEntity(BaseEntity);
		}

		public void Destroy(EntityCommandBuffer.ParallelWriter ecb,int jobIndex)
		{
			ecb.DestroyEntity(jobIndex, BaseEntity);
		}

		/// <summary>
		/// ProceduralGenerationElement instances are equal if they refer to the same Value.
		/// </summary>
		/// <param name="lhs">An ProceduralGenerationElement object.</param>
		/// <param name="rhs">Another ProceduralGenerationElement object.</param>
		/// <returns>True, if both values are identical.</returns>
		public static bool operator ==(ProceduralGenerationElement lhs, ProceduralGenerationElement rhs)
		{
			return lhs.BaseEntity == rhs.BaseEntity;
		}

		/// <summary>
		/// ProceduralGenerationElement instances are equal if they refer to the same Value.
		/// </summary>
		/// <param name="lhs">An ProceduralGenerationElement object.</param>
		/// <param name="rhs">Another ProceduralGenerationElement object.</param>
		/// <returns>True, if either value is different.</returns>
		public static bool operator !=(ProceduralGenerationElement lhs, ProceduralGenerationElement rhs)
		{
			return !(lhs == rhs);
		}

		/// <summary>
		/// ProceduralGenerationElement instances are equal if they refer to the same ProceduralGenerationElement.
		/// </summary>
		/// <param name="compare">The object to compare to this ProceduralGenerationElement.</param>
		/// <returns>True, if the compare parameter contains an ProceduralGenerationElement object having the same value
		/// as this ProceduralGenerationElement.</returns>
		public override bool Equals(object compare)
		{
			return this == (ProceduralGenerationElement)compare;
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
		public static ProceduralGenerationElement Null => new ProceduralGenerationElement();
	}

	public struct ProceduralGenerationElementBufferElement : IBufferElementData
	{
		public ProceduralGenerationElement Value;

		/// <summary>
		/// IntBufferElement instances are equal if they refer to the same Value.
		/// </summary>
		/// <param name="lhs">An IntBufferElement object.</param>
		/// <param name="rhs">Another IntBufferElement object.</param>
		/// <returns>True, if both values are identical.</returns>
		public static bool operator ==(ProceduralGenerationElementBufferElement lhs, ProceduralGenerationElementBufferElement rhs)
		{
			return lhs.Value == rhs.Value;
		}

		/// <summary>
		/// IntBufferElement instances are equal if they refer to the same Value.
		/// </summary>
		/// <param name="lhs">An IntBufferElement object.</param>
		/// <param name="rhs">Another IntBufferElement object.</param>
		/// <returns>True, if either value is different.</returns>
		public static bool operator !=(ProceduralGenerationElementBufferElement lhs, ProceduralGenerationElementBufferElement rhs)
		{
			return !(lhs == rhs);
		}

		/// <summary>
		/// ProceduralGenerationElementBufferElement instances are equal if they refer to the same ProceduralGenerationElementBufferElement.
		/// </summary>
		/// <param name="compare">The object to compare to this ProceduralGenerationElementBufferElement.</param>
		/// <returns>True, if the compare parameter contains an ProceduralGenerationElementBufferElement object having the same value
		/// as this ProceduralGenerationElementBufferElement.</returns>
		public override bool Equals(object compare)
		{
			return this == (ProceduralGenerationElementBufferElement)compare;
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
		public static ProceduralGenerationElementBufferElement Null => new ProceduralGenerationElementBufferElement();
	}

	public struct DynamicBufferProceduralGenerationElementBufferElement : IBufferElementData
	{

		// Entity where the buffer is located
		public Entity mEntity;

		/// <summary>
		/// returns the DynamicBuffer using the entity stoed in this struct
		/// </summary>
		/// <param name="entityManager"></param>
		/// <returns></returns>
		public DynamicBuffer<ProceduralGenerationElementBufferElement> GetValue(EntityManager entityManager)
		{
			return entityManager.GetBuffer<ProceduralGenerationElementBufferElement>(mEntity);
		}

		/// <summary>
		/// Use this if you want to get the DynamicBuffer<ProceduralGenerationElementBufferElement> from the given entity
		/// </summary>
		/// <param name="entityManager"></param>
		/// <param name="entity"></param>
		/// <returns></returns>
		public static DynamicBuffer<ProceduralGenerationElementBufferElement> GetValue(EntityManager entityManager, Entity entity)
		{
			return entityManager.GetBuffer<ProceduralGenerationElementBufferElement>(entity);
		}

		/// <summary>
		/// Use this to initialize a dynamic buffer just to temporary use other wise use AddBuffer()
		/// </summary>
		/// <param name="entityManager"></param>
		public DynamicBufferProceduralGenerationElementBufferElement(EntityManager entityManager,bool addBuffer = false)
		{
			mEntity = entityManager.CreateEntity();
			entityManager.SetName(mEntity, "ProceduralGenerationElementBufferElement");
			if(addBuffer)
				entityManager.AddBuffer<ProceduralGenerationElementBufferElement>(mEntity);
		}

		/// <summary>
		/// Use this to initialize a dynamic buffer just to temporary use other wise use AddBuffer()
		/// </summary>
		/// <param name="ecb"></param>
		/// <param name="addBuffer"></param>
		public DynamicBufferProceduralGenerationElementBufferElement(EntityCommandBuffer.ParallelWriter ecb,int jobIndex, bool addBuffer = false)
		{
			mEntity = ecb.CreateEntity(jobIndex);
			if (addBuffer)
				ecb.AddBuffer<ProceduralGenerationElementBufferElement>(jobIndex,mEntity);
		}


		/// <summary>
		/// Use this to add ProceduralGenerationElementBufferElements to the given entity NOTE: you can only have 1 per entity
		/// </summary>
		/// <param name="entityManager"></param>
		/// <param name="entity"></param>
		public void AddBuffer(EntityManager entityManager, Entity entity)
		{
			entityManager.AddBuffer<ProceduralGenerationElementBufferElement>(entity);
		}

		/// <summary>
		/// Adds the buffer to the eneity and returns the buffer
		/// </summary>
		/// <param name="entityManager"></param>
		/// <returns></returns>
		public DynamicBuffer<ProceduralGenerationElementBufferElement> AddBuffer(EntityManager entityManager)
		{
			return entityManager.AddBuffer<ProceduralGenerationElementBufferElement>(mEntity);
		}

		/// <summary>
		/// converts the DynamicBuffer<ProceduralGenerationElementBufferElement> to an ProceduralGenerationElementBufferElement rrary
		/// </summary>
		/// <returns></returns>
		public static ProceduralGenerationElementBufferElement[] ToArray(DynamicBuffer<ProceduralGenerationElementBufferElement> buffer)
		{
			ProceduralGenerationElementBufferElement[] arr = new ProceduralGenerationElementBufferElement[buffer.Length];
			for (int i = 0; i < buffer.Length; i++)
				arr[i] = buffer[i];
			return arr;
		}

		/// <summary>
		/// converts the DynamicBuffer<ProceduralGenerationElementBufferElement> to an Entity rrary
		/// </summary>
		/// <param name="Value"></param>
		/// <returns></returns>
		public ProceduralGenerationElement[] ToProeduralGenerationElementArray(DynamicBuffer<ProceduralGenerationElementBufferElement> buffer)
		{
			ProceduralGenerationElement[] arr = new ProceduralGenerationElement[buffer.Length];
			for (int i = 0; i < buffer.Length; i++)
				arr[i] = buffer[i].Value;
			return arr;
		}

		/// <summary>
		/// DynamicBufferProceduralGenerationElementBufferElement instances are equal if they refer to the same Value.
		/// </summary>
		/// <param name="lhs">An DynamicBufferProceduralGenerationElementBufferElement object.</param>
		/// <param name="rhs">Another DynamicBufferProceduralGenerationElementBufferElement object.</param>
		/// <returns>True, if both values are identical.</returns>
		public static bool operator ==(DynamicBufferProceduralGenerationElementBufferElement lhs, DynamicBufferProceduralGenerationElementBufferElement rhs)
		{
			return lhs.mEntity == rhs.mEntity;
		}

		/// <summary>
		/// DynamicBufferProceduralGenerationElementBufferElement instances are equal if they refer to the same Value.
		/// </summary>
		/// <param name="lhs">An DynamicBufferProceduralGenerationElementBufferElement object.</param>
		/// <param name="rhs">Another DynamicBufferProceduralGenerationElementBufferElement object.</param>
		/// <returns>True, if either value is different.</returns>
		public static bool operator !=(DynamicBufferProceduralGenerationElementBufferElement lhs, DynamicBufferProceduralGenerationElementBufferElement rhs)
		{
			return !(lhs == rhs);
		}

		/// <summary>
		/// DynamicBufferProceduralGenerationElementBufferElement instances are equal if they refer to the same DynamicBufferProceduralGenerationElementBufferElement.
		/// </summary>
		/// <param name="compare">The object to compare to this DynamicBufferProceduralGenerationElementBufferElement.</param>
		/// <returns>True, if the compare parameter contains an DynamicBufferProceduralGenerationElementBufferElement object having the same value
		/// as this DynamicBufferProceduralGenerationElementBufferElement.</returns>
		public override bool Equals(object compare)
		{
			return this == (DynamicBufferProceduralGenerationElementBufferElement)compare;
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
		/// A "blank" DynamicBufferProceduralGenerationElementBufferElement object that does not refer to an actual entity.
		/// </summary>
		public static DynamicBufferProceduralGenerationElementBufferElement Null => new DynamicBufferProceduralGenerationElementBufferElement();
	}

}
