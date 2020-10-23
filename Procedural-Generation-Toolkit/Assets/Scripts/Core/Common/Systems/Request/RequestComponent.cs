using UnityEngine;
using System.Collections;
using Unity.Entities;
using Unity.Collections;

namespace Core.Common.Systems.Request
{
	public class RequestComponent : MonoBehaviour,IConvertGameObjectToEntity
	{
		public RequestReceivedClass RequestReceived;
		public RequestData.RequestTypes RequestType;
		public string Name = "";
		public int Id = -1;

		public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
		{
			if (Name == "")
				Name = this.name;
			if (Id < 0)
				Id = 9999;

			dstManager.AddComponentData(entity, new RequestData
			{
				Id = Id,
				Name = new FixedString64(Name),
				Type = RequestType
			});
		}

		// Use this for initialization
		void Start()
		{
			RequestReceived = new RequestReceivedClass();
		}

		// Update is called once per frame
		void Update()
		{

		}
	}

	public struct RequestData : IComponentData
	{
		public enum RequestTypes
		{
			Entity
		}

		public RequestTypes Type;
		public FixedString64 Name;
		public int Id;
	}
}