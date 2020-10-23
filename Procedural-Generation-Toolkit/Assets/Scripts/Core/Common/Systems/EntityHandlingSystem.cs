using UnityEngine;
using System.Collections;
using Unity.Entities;

namespace Core.Common.Systems
{
	public class EntityHandlingSystem : SystemBase
	{
		protected override void OnUpdate()
		{
			// Create Delete Entity Request System (requires ecb)
		}
	}

	public struct EntityDeleteRequest : IComponentData
	{
		public Entity entityToDelete;
	}
}