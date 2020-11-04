using UnityEngine;
using System.Collections;
using Unity.Entities;
using Unity.Transforms;

namespace Core.Extensions
{
	public struct ParentExtensions
	{
		/// <summary>
		/// Adds a Parent and LocalToParent to the given child entity
		/// </summary>
		/// <param name="entityManager"></param>
		/// <param name="child"></param>
		/// <param name="parent"></param>
		public static void AddParent(EntityManager entityManager, Entity child, Entity parent)
		{
			entityManager.AddComponentData(child, new Parent  { Value = parent });
			entityManager.AddComponentData(child, new LocalToParent());
		}
		
		/// <summary>
		/// Removes the Parent Elements from the given child entity
		/// </summary>
		/// <param name="entityManager"></param>
		/// <param name="child"></param>
		public static void RemoveParent(EntityManager entityManager, Entity child)
		{
			entityManager.RemoveComponent<Parent>(child);
			entityManager.RemoveComponent<LocalToParent>(child);
		}

		// Command Buffer Variants

		/// <summary>
		/// Adds a Parent and LocalToParent to the given child entity
		/// </summary>
		/// <param name="entityManager"></param>
		/// <param name="child"></param>
		/// <param name="parent"></param>
		public static void AddParent(EntityCommandBuffer.ParallelWriter commandBuffer,int jobIndex, Entity child, Entity parent)
		{
			commandBuffer.AddComponent(jobIndex,child, new Parent { Value = parent });
			commandBuffer.AddComponent(jobIndex,child, new LocalToParent());
		}

		/// <summary>
		/// Removes the Parent Elements from the given child entity
		/// </summary>
		/// <param name="entityManager"></param>
		/// <param name="child"></param>
		public static void RemoveParent(EntityCommandBuffer.ParallelWriter commandBuffer,int jobIndex, Entity child)
		{
			commandBuffer.RemoveComponent<Parent>(jobIndex,child);
			commandBuffer.RemoveComponent<LocalToParent>(jobIndex,child);
		}
	}
}