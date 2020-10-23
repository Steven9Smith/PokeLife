using Unity.Entities;
using UnityEngine;

namespace Core.Common.Systems.Request
{

	public class RequestSystems : SystemBase
	{
		bool debug = true;
		bool verbose = false;

		protected override void OnCreate()
		{
			Debug.Log("Request_System: System successfully created!");
		}
		protected override void OnUpdate()
		{
			Entities.
				WithoutBurst().
				WithStructuralChanges().
				ForEach((Entity entity,ref RequestData data)=>{
					PerformRequest(EntityManager,entity,data);
				}).Run();
		}

		public void PerformRequest(EntityManager entityManager,Entity entity,RequestData data)
		{
			if(data.Type == RequestData.RequestTypes.Entity)
			{
				// we need to find the game object with the given name
				GameObject go = GameObject.Find(data.Name.ToString());
				if(go != null)
				{
					if(verbose)Debug.Log("Found GameObject "+data.Name.ToString());
					var RRC = go.GetComponent<RequestComponent>();
					if(RRC != null)
					{
						RRC.RequestReceived.UpdateRequestReceivedComponent(entityManager, entity, true);
						entityManager.RemoveComponent<RequestData>(entity);
					}
					else
					{
						if (debug) Debug.LogWarning("Failed to the RequestReceivedComponent");
					}
				}
				else
				{
					if(debug)Debug.LogWarning("Failed to find GameObject \""+data.Name.ToString()+"\"");
				}
			}
		}
	}
}