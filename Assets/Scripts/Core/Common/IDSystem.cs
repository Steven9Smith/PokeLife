
namespace Core.Common
{
	public interface IIDSystem
	{
		void InitilizeIDArrays();

		void RemoveId(int id);

		int GenerateId();
	}

/*	public class IDSystemClass : IIDSystem
	{
		private static DynamicBufferIntBufferElement Ids;
		private static DynamicBufferIntBufferElement AvailibleIds;
		/// <summary>
		/// Removes an Id from the Ids Buffer
		/// </summary>
		/// <param name="entityManager"></param>
		/// <param name="id"></param>
		private static void RemoveId(EntityManager entityManager, int id)
		{
			DynamicBuffer<IntBufferElement> _Ids = Ids.GetValue(entityManager);
			DynamicBuffer<IntBufferElement> _AvailibleIds = AvailibleIds.GetValue(entityManager);
			bool match = false;
			for (int i = 0; i < _Ids.Length; i++)
			{
				if (id == _Ids[i].Value)
				{
					// if id is at the end then we just remove it
					if (i == _Ids.Length - 1)
					{
						_Ids.RemoveAt(i);
						// add it to availible ids so we can reuse ids
					}
					else
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

			if (Ids == DynamicBufferIntBufferElement.Null)
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
				return _Ids[_Ids.Length - 1].Value;
			}
			else
			{
				// create a new id and return it
				_Ids.Add(new IntBufferElement { Value = _Ids.Length });
				return _Ids.Length - 1;
			}
		}




		public void GetId()
		{

		}

		public void GenerateId(int id)
		{

		}

		// Use this for initialization
		void Start()
		{

		}

		// Update is called once per frame
		void Update()
		{

		}
	}
	*/


}