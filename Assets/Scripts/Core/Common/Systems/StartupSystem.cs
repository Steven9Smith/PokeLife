using UnityEngine;
using System.Collections;
using Unity.Entities;

namespace Core.Common.Systems
{
	public class StartupSystem : SystemBase
	{
		protected override void OnCreate()
		{
			EntityPrefab.SetupPrefabs(EntityManager);
		}

		protected override void OnUpdate()
		{

		}
	}
}
