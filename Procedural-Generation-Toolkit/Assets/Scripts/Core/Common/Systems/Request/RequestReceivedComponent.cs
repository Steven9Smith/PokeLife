using UnityEngine;
using System.Collections;
using Unity.Entities;
[System.Serializable]
public class RequestReceivedClass
{
	private bool isValid = false;
	[SerializeField]
	public bool IsValid { get { return isValid; } }
	public Entity entity;
	public EntityManager entityManager;

	public void UpdateRequestReceivedComponent(EntityManager entityManager, Entity entity, bool isValid = true)
	{
		this.isValid = true;
		this.entity = entity;
		this.entityManager = entityManager;
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
