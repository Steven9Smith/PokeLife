using UnityEngine;
using System.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Mathematics;
using Unity.Physics.Authoring;
using Unity.Collections;
using Core.Extensions.ACollider;

public class AColliderComponent : MonoBehaviour
{
	public AColliderClass aColliderClass = new AColliderClass();
	private void OnEnable()
	{

	}
	void Start()
	{

	}

	// Update is called once per frame
	void Update()
	{

	}
}

namespace Core.Extensions.ACollider
{
	[System.Serializable]
	public class AColliderClass
	{
		[SerializeField]
		public PhysicsShapeAuthoring PhysicsShapeAuthoring;

		[HideInInspector]
		public ACollider collider;

		private AColliderClassOld history;

		#region Initalizers

		public AColliderClass(bool debug = false, bool verbose = false) {
			if(debug)Debug.LogWarning("AColliderClass: Setting up a Collider Class with no Physics Shape Authoring will produce errors and warnings");
			collider = ACollider.Null;
			PhysicsShapeAuthoring = null;
		}
		public AColliderClass(PhysicsShapeAuthoring shape,Transform transform = null)
		{
			if (shape == null) Debug.LogWarning("AColliderClass: Given PhysicsShapeAuthoring is null");
			PhysicsShapeAuthoring = shape;
			Update();
		}
		#endregion

		#region Destructor

		~AColliderClass()
		{
		//	if (PhysicsShapeAuthoring != null)
		//		if (Application.isPlaying)
		//			GameObject.Destroy(PhysicsShapeAuthoring.gameObject);
		//		else
		//			GameObject.DestroyImmediate(PhysicsShapeAuthoring.gameObject);
		}

		#endregion

		public bool IsValid()
		{
			return PhysicsShapeAuthoring != null && collider != ACollider.Null;
		}

		public void GenerateMeshEquivalent(ref Mesh m)
		{
			if (collider != ACollider.Null)
			{
				GameObject go = null;
				Mesh tmp = new Mesh();
				if (m == null) m = new Mesh();
				float3 scale;
				float3 size = float3.zero;
				unsafe
				{
					
					if (collider.GetCollider_P() == null)
					{
						Debug.LogWarning("GenerateMeshE: Cannot generate a mesh with a null collider.");
						return;
					}
					size = AColliderProperties.GetSize(collider.GetCollider_P()->Type, collider.properties);
				//	Debug.Log(collider.aabb.Min);
					switch (collider.GetCollider_P()->Type)
					{
						case ColliderType.Box:
							go = GameObject.CreatePrimitive(PrimitiveType.Cube);
							tmp = go.GetComponent<MeshFilter>().sharedMesh;
							m = CopyMeshWithModification(tmp, size);
							break;
						case ColliderType.Capsule:
							go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
							tmp = go.GetComponent<MeshFilter>().sharedMesh;
					//		Debug.Log(collider.properties.height);
					//		Debug.Log("tmp::" + tmp.bounds.ToString("F4")+",,,,\n"+collider.AabbToString());
							scale = (collider.aabb.Extents * 2) / tmp.bounds.size;

					//		Debug.Log(collider.aabb.Extents + "," + scale);
							m = CopyMeshWithModification(tmp, scale / 2);
					//				Debug.Log("mesh::" + m.bounds.ToString("F4"));

							break;
						case ColliderType.Cylinder:
							go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
							tmp = go.GetComponent<MeshFilter>().sharedMesh;
							scale = (collider.aabb.Extents * 2) / tmp.bounds.size;
							m = CopyMeshWithModification(tmp,  scale / 2);
							break;
						case ColliderType.Sphere:
							go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
							tmp = go.GetComponent<MeshFilter>().sharedMesh;
							m = CopyMeshWithModification(tmp, size);
							break;
						default:
							Debug.LogWarning("Failed to get mesh for this type....");
							break;
					}
				}
				if(!Application.isPlaying)
					GameObject.DestroyImmediate(go);
				else
					GameObject.Destroy(go);

			//	Vector3[] verticies = m.vertices;
			//	for (int i = 0; i < verticies.Length; i++)
			//		verticies[i] = new Vector3(verticies[i].x * extents.x, verticies[i].y * extents.y, verticies[i].z * extents.z);
			//	m.vertices = verticies;




			}
			else Debug.LogWarning("GenerateMeshE: cannot generate a mesh with an invalid ACollider.");
		}

		private Mesh CopyMeshWithModification(Mesh org,float3 scale)
		{
			Mesh m = new Mesh();
			m.vertices = org.vertices;
			m.uv = org.uv;
			m.triangles = org.triangles;
			m.normals = org.normals;
			m.tangents = org.tangents;

			Vector3[] verticies = m.vertices;
		//	string a = "";
			for (int i = 0; i < verticies.Length; i++)
			{
				verticies[i] = new Vector3(verticies[i].x * scale.x, verticies[i].y * scale.y, verticies[i].z * scale.z);
		//		a += "\n" + verticies[i].ToString();
			}
			m.vertices = verticies;

		//	Debug.Log(a);

			m.RecalculateBounds();
			m.RecalculateNormals();
			m.RecalculateTangents();

			return m;
		}
		#region Update_Related

		/// <summary>
		/// Updates the collider when nessacary and returns false is the PhysicsShapeAuthoring value has change
		/// </summary>
		/// <returns></returns>
		public bool Update(PhysicsShapeAuthoring shape, RigidTransform transform)
		{
			if (!HasChanged())
			{
				if (shape == null)
					Debug.LogWarning("Update: Cannot update with a null shape!");
				else
					collider = GenerateACollider(shape, transform);
				return true;
			}
			return false;
		}
		
		/// <summary>
		/// Updates the collider when nessacary and returns false is the PhysicsShapeAuthoring value has change
		/// </summary>
		/// <returns></returns>
		public bool Update()
		{
			/*	if (!HasChanged())
				{
					UpdateTransform();
					return true;
				}
				return false;*/
			return HasChanged();
		}

		public void UpdateTransform()
		{
			if (PhysicsShapeAuthoring != null && collider != ACollider.Null)
			{
				collider.UpdateAabb(new RigidTransform(PhysicsShapeAuthoring.transform.rotation,PhysicsShapeAuthoring.transform.position));

				if (!collider.transform.Equals(new RigidTransform()))
				{
					PhysicsShapeAuthoring.transform.position = collider.transform.pos;
					PhysicsShapeAuthoring.transform.rotation = collider.transform.rot;
				}
				collider.ValidateBounds(PhysicsShapeAuthoring);
			}
		}

		public bool HasChanged()
		{
			bool ShapePropertyChanged(ShapeType shape)
			{
				bool value = false;
				switch (shape)
				{
					case ShapeType.Box:
						BoxGeometry box = PhysicsShapeAuthoring.GetBoxProperties();
						value = !box.Equals(history.boxGeometry);
						if (value) Debug.Log("HasChanged: Detected Box Properties Changed");
						history.boxGeometry = box;
						break;
					case ShapeType.Capsule:
						CapsuleGeometryAuthoring cap = PhysicsShapeAuthoring.GetCapsuleProperties();
						value = !cap.Equals(history.capsuleGeometry);
						if (value) Debug.Log("HasChanged: Detected Capsule Properties Changed");
						history.capsuleGeometry = cap;
						break;
					case ShapeType.Cylinder:
						CylinderGeometry cyl = PhysicsShapeAuthoring.GetCylinderProperties();
						value = !cyl.Equals(history.cylinderGeometry);
						if (value) Debug.Log("HasChanged: Detected Cylinder Properties Changed");
						history.cylinderGeometry = cyl;
						break;
					case ShapeType.Sphere:
						SphereGeometry sph = PhysicsShapeAuthoring.GetSphereProperties(out quaternion rot);
						value = !sph.Equals(history.sphereGeometry);
						if (value) Debug.Log("HasChanged: Detected Shpere Properties Changed");
						history.sphereGeometry = sph;
						break;
					case ShapeType.Mesh:
					case ShapeType.Plane:
					case ShapeType.ConvexHull:
					default:
						Debug.Log("cannot handle shapes of type Mesh, Plane, and Convex Hull...");
						break;
				}
				return value;
			}

			if (PhysicsShapeAuthoring != null)
			{
				RigidTransform trans = new RigidTransform(PhysicsShapeAuthoring.transform.rotation, PhysicsShapeAuthoring.transform.position);
				if (PhysicsShapeAuthoring.ShapeType != history.ShapeType || collider == ACollider.Null)
				{
					Debug.Log("HasChanged: Detected shape type change");
					history.Clear();
					history.ShapeType = PhysicsShapeAuthoring.ShapeType;
					ShapePropertyChanged(PhysicsShapeAuthoring.ShapeType);
					collider = GenerateACollider(PhysicsShapeAuthoring, trans);
				//	UpdateTransform();
					return true;
				}
				else if (!collider.transform.Equals(trans))
				{
			//		Debug.Log("HasChanged: Detected a transform change!\n"+collider.transform.ToString()+",,"+
			//			trans.ToString()+
			//			",,"+PhysicsShapeAuthoring.transform.rotation);
					PhysicsShapeAuthoring.transform.position = collider.transform.pos;
					PhysicsShapeAuthoring.transform.rotation = collider.transform.rot;


				//	UpdateTransform();
					return true;
				}
				else
				{
					bool value = ShapePropertyChanged(PhysicsShapeAuthoring.ShapeType);		
					if (value)
					{
						//change collider
						collider = GenerateACollider(PhysicsShapeAuthoring, trans);
					}
					return value;
				}
			}
			return false;
		}

		#endregion

		#region Generate_Colliders
		// NOTE in future use ColliderType!
		/// <summary>
		/// Updates the collider based on the given PhysicsShapeAuthoring
		/// </summary>
		/// <param name="PhysicsShapeAuthoring"></param>
		/// <param name="collider"></param>
		/// <param name="position"></param>
		/// <param name="rotation"></param>
		/// <returns></returns>
		public static ACollider GenerateACollider(PhysicsShapeAuthoring PhysicsShapeAuthoring, RigidTransform transform)
		{
			Debug.Log("GenerateACollider::"+transform.ToString());
			if (PhysicsShapeAuthoring != null)
			{
			//	Debug.Log("Generating Collider...");
				BlobAssetReference<Unity.Physics.Collider> m_collider = new BlobAssetReference<Unity.Physics.Collider>();
				switch (PhysicsShapeAuthoring.ShapeType)
				{
					case ShapeType.Box:
				//		if(verbose)Debug.Log("Detected Box!");
						BoxGeometry box = PhysicsShapeAuthoring.GetBoxProperties();
						m_collider = Unity.Physics.BoxCollider.Create(box);
						break;
					case ShapeType.Capsule:
				//		if (verbose) Debug.Log("Detected Capsule!");
						CapsuleGeometryAuthoring capsule = PhysicsShapeAuthoring.GetCapsuleProperties();
						// Orientation is lost in this conversion but ACollider uses a RigidTransform so 
						// we should be okay.
						CapsuleGeometry m_capsule = new CapsuleGeometry
						{
							Radius = capsule.Radius,
							Vertex0 = new float3(capsule.Center.x, capsule.Center.y + capsule.Height, capsule.Center.z),
							Vertex1 = new float3(capsule.Center.x, capsule.Center.y - capsule.Height, capsule.Center.z)
						};
						m_collider = Unity.Physics.CapsuleCollider.Create(m_capsule);
						break;
					case ShapeType.ConvexHull:
			//			if (verbose) Debug.Log("Detected Convex Hull!");
						NativeList<float3> p_cloud = new NativeList<float3>(0, Allocator.TempJob);
						// add the offset to each vertex
						PhysicsShapeAuthoring.GetConvexHullProperties(p_cloud);
						m_collider = Unity.Physics.ConvexCollider.Create(p_cloud,
							PhysicsShapeAuthoring.ConvexHullGenerationParameters);
						break;
					case ShapeType.Cylinder:
			//			if (verbose) Debug.Log("Detected Cylinder!");
						CylinderGeometry cylinder = PhysicsShapeAuthoring.GetCylinderProperties();
						m_collider = Unity.Physics.CylinderCollider.Create(cylinder);
						break;
					case ShapeType.Mesh:
			//			if (verbose) Debug.Log("Detected Mesh!");
						NativeList<float3> verticies = new NativeList<float3>(0, Allocator.TempJob);
						NativeList<int3> triangles = new NativeList<int3>(0, Allocator.TempJob);
						PhysicsShapeAuthoring.GetMeshProperties(verticies, triangles);
						m_collider = Unity.Physics.MeshCollider.Create(verticies, triangles);
						break;
					case ShapeType.Plane:
			//			if (verbose) Debug.Log("Detected Plane!");
						PhysicsShapeAuthoring.GetPlaneProperties(out float3 center, out float2 size, out quaternion orientaiton);
						// in this instance we convert a plane to a box collider since the collision queries will execut faster and i can't find a plane collider
						m_collider = Unity.Physics.BoxCollider.Create(new BoxGeometry
						{
							BevelRadius = 0,
							Center = center,
							Size = new float3(size.x, 0.1f, size.y),
							Orientation = orientaiton
						});
						break;
					case ShapeType.Sphere:
			//			if (verbose) Debug.Log("Detected Sphere!");
						SphereGeometry sphere = PhysicsShapeAuthoring.GetSphereProperties(out quaternion orientation);
						m_collider = Unity.Physics.SphereCollider.Create(sphere);
						break;
					default:
						Debug.LogError("Failed to Detected PhysicsShapeAuthoring type");
						break;
				}
				if (m_collider.IsCreated)
					unsafe
					{
						fixed (Unity.Physics.Collider* c = &m_collider.Value)
						{
							return new ACollider(transform,c, true);
						}
					}
				else
					Debug.LogError("Failed to create Collider");

			}
			else Debug.LogWarning("GenerateACollider: Given PhysicsShape is null or invalid!");
			return ACollider.Null;
		}

	
		#endregion

		#region overrides

		public bool Equals(AColliderClass a)
		{
			return this.collider == a.collider && this.PhysicsShapeAuthoring == a.PhysicsShapeAuthoring;
		}
		public override bool Equals(object obj)
		{
			return this.Equals((AColliderClass)obj);
		}
		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
		#endregion

		private struct AColliderClassOld
		{
			public ShapeType ShapeType;
			public BoxGeometry boxGeometry;
			public CapsuleGeometryAuthoring capsuleGeometry;
			public CylinderGeometry cylinderGeometry;
			public SphereGeometry sphereGeometry;

			public void Clear()
			{
				boxGeometry = new BoxGeometry();
				capsuleGeometry = new CapsuleGeometryAuthoring();
				cylinderGeometry = new CylinderGeometry();
				sphereGeometry = new SphereGeometry();
			}
		}
	}

	

	/// <summary>
	/// This is ACollider which makes it a little easier to perform Aabb collision test
	/// without the need of a SystemBase or job.
	/// </summary>
	[System.Serializable]
	public struct ACollider : IComponentData
	{
		// The acutal Collider Data (BoxCollider,SphereCollider,etc) is actually lost in the internal Data so converting
		// Collider->Collider*->BoxCollider gives curropted Data. This only way to get that data is to store the Collider*
		private unsafe Unity.Physics.Collider* Bounds_p;

		[HideInInspector]
		// the Aabb of the collider. this is stored to prevent multiple calls to 
		// Bounds.calculate Aabb
		public Aabb aabb;
		[HideInInspector]
		// store the colliders position and rotation
		public RigidTransform transform;
		[HideInInspector]
		public AColliderProperties properties;

		public void ValidateBounds(PhysicsShapeAuthoring shape)
		{
			unsafe {
				if (Bounds_p == null)
					this = AColliderClass.GenerateACollider(shape, new RigidTransform(shape.transform.rotation,shape.transform.position));
			}
		}

		#region Initializers
		/// <summary>
		/// Creates ACollider
		/// </summary>
		/// <param name="transform"></param>
		/// <param name="collider"></param>
		/// <param name="debug">set to ture to output any warngings or errors</param>
		public unsafe ACollider(RigidTransform transform, Unity.Physics.Collider* collider, bool debug = false)
		{
			this.transform = transform;
			Bounds_p = collider;
			aabb = Bounds_p->CalculateAabb(this.transform);
			if (aabb.Equals(Aabb.Empty) && debug)
				UnityEngine.Debug.LogError("Failed to create Aabb!");
			properties = new AColliderProperties();
			GenerateProperties();
			UpdateAabb(this.transform);
		}
		/// <summary>
		/// Create ACollider
		/// </summary>
		/// <param name="collider"></param>
		/// <param name="position"></param>
		/// <param name="rotation"></param>
		/// <param name="debug">set to ture to output any warngings or errors</param>
		public unsafe ACollider(Unity.Physics.Collider* collider, float3 position, quaternion rotation, bool debug = false)
		{
			Debug.Log("Creating ACollider...");
			this.transform = new RigidTransform(rotation, position);
			Bounds_p = collider;
			aabb = new Aabb();
			aabb = Bounds_p->CalculateAabb(this.transform);
			if (aabb.Equals(Aabb.Empty) && debug)
				UnityEngine.Debug.LogError("Failed to create AABB");
			properties = new AColliderProperties();
			GenerateProperties();
			UpdateAabb(this.transform);
		}
		#endregion

		#region Updates

		/// <summary>
		/// Generates and stores useful easy to get information about the collider in the ACollider. this prevent 
		/// massive amounts of converting the Bounds_p variable into its original collider thus saving time.
		/// this version currently can't get information on every type of collider but can handle the primitives
		/// </summary>
		private unsafe void GenerateProperties()
		{
			Debug.Log("Generating Properties");
			switch (Bounds_p->Type)
			{
				case Unity.Physics.ColliderType.Box:
					Unity.Physics.BoxCollider Box = GetCollider<Unity.Physics.BoxCollider>();
					properties = new AColliderProperties
					{
						translation = Box.Center,
						orientation = Box.Orientation,
						valueA = Box.Size,
						radius = Box.BevelRadius
					};
					break;
				case Unity.Physics.ColliderType.Capsule:
					Unity.Physics.CapsuleCollider Capsule = GetCollider<Unity.Physics.CapsuleCollider>();

					// calulate the center using midpoint formula

					float3 center = new float3(
							(Capsule.Vertex1.x + Capsule.Vertex0.x) / 2,
							(Capsule.Vertex1.y + Capsule.Vertex0.y) / 2,
							(Capsule.Vertex1.z + Capsule.Vertex0.z) / 2
						);

					// we need to calculate the orientation...why do you do this to me unity!

					float3 up = (Capsule.Vertex1 - Capsule.Vertex0) * Vector3.up;
					float3 right = Vector3Extensions.Rotate90CCW(up);
					float3 forward = Vector3.Cross(up, right).normalized;

					float3x3 matrix = new float3x3(forward, right, up);

					quaternion orientation = new quaternion(matrix);

					float x = Capsule.Vertex1.x - Capsule.Vertex0.x;
					float y = Capsule.Vertex1.y - Capsule.Vertex0.y;
					float z = Capsule.Vertex1.z - Capsule.Vertex0.z;


					properties = new AColliderProperties
					{
						translation = center,
						orientation = orientation,
						valueA = Capsule.Vertex0,
						valueB = Capsule.Vertex1,
						radius = Capsule.Radius,
						height = math.sqrt((x * x) + (y * y) + (z * z))/2
					};
					break;
				case Unity.Physics.ColliderType.Cylinder:
					Unity.Physics.CylinderCollider Cylinder = GetCollider<Unity.Physics.CylinderCollider>();
					properties = new AColliderProperties
					{
						height = Cylinder.Height,
						orientation = Cylinder.Orientation,
						translation = Cylinder.Center,
						radius = Cylinder.Radius,
						valueA = new float3(Cylinder.SideCount),
						valueB = new float3(Cylinder.BevelRadius)
					};
					break;
				case Unity.Physics.ColliderType.Sphere:
					Unity.Physics.SphereCollider Sphere = GetCollider<Unity.Physics.SphereCollider>();
					float sphereSize = Sphere.Radius * 2;
					properties = new AColliderProperties
					{
						radius = Sphere.Radius,
						translation = Sphere.Center,
						// so um... a sphere has an orientation but it doen't matter with the collider calculations so it itsn't stored.
						orientation = quaternion.identity,
						valueA = new float3(sphereSize,sphereSize,sphereSize) // size of Sphere
					};
					break;
				default:
					Debug.LogWarning("Any other collider cannot be processed in this current Version");
					break;
			}
		}
	
		/// <summary>
		/// Updates the ACollider using the given parameters. if the parameters are invalid then internal ones are used.
		/// If an update was performed then the function returns true, false otherwise.
		/// </summary>
		/// <param name="transform"></param>
		/// <param name="collider"></param>
		public unsafe bool Update(RigidTransform transform, Unity.Physics.Collider* collider = null,bool forceUpdate = false)
		{
		//	Debug.Log("Updating Bounds..."+transform.ToString()+",,,"+this.transform.ToString()+"."+collider->ToString()+",,"+forceUpdate+","+(Bounds_p == collider)+",,"+(transform.Equals(this.transform)));

			//	Debug.Log("old properties = "+properties.ToString());


			bool performedUpdate = false;
			if (forceUpdate)
			{
				Bounds_p = collider;
				this.transform = transform;
				GenerateProperties();
				Debug.Log("new properties = " + properties.ToString());
				UpdateAabb(this.transform);
				performedUpdate = true;
			}
			if (collider != null && collider != Bounds_p)
			{
				Debug.Log("Update: Changing collider");
				Bounds_p = collider;
				GenerateProperties();
				performedUpdate = true;
			}
		//	else if (Bounds_p == null) Debug.LogError("the collider is null");
			if (!transform.Equals(new RigidTransform()) && !this.transform.Equals(transform))
			{
				Debug.Log("Update: Changing transform");
				this.transform = transform;
				UpdateAabb(this.transform);
				performedUpdate = true;
			}
		//	else Debug.LogWarning("transform is wierd,"+transform+",,,"+this.transform);
			return performedUpdate;
		}

		#endregion

		#region Getter_&_Setters
		/// <summary>
		/// Returns the collider based on the given struct type parameter
		/// </summary>
		/// <typeparam name="T">Type parameters must be of type Unity.Physics.[Type of Collider (Not Collider)]</typeparam>
		/// <returns></returns>
		public unsafe T GetCollider<T>() where T : struct
		{
			object obj = null;
			switch (Bounds_p->Type)
			{
				case ColliderType.Box:
					Unity.Physics.BoxCollider* Box_p = (Unity.Physics.BoxCollider*)Bounds_p;
					Unity.Physics.BoxCollider Box = *Box_p;
					obj = Box;
					break;
				case ColliderType.Capsule:
					Unity.Physics.CapsuleCollider* Capsule_p = (Unity.Physics.CapsuleCollider*)Bounds_p;
					Unity.Physics.CapsuleCollider Capsule = *Capsule_p;
					obj = Capsule;
					break;
				case ColliderType.Compound:
					Unity.Physics.CompoundCollider* Compound_p = (Unity.Physics.CompoundCollider*)Bounds_p;
					Unity.Physics.CompoundCollider Compound = *Compound_p;
					obj = Compound;
					break;
				case ColliderType.Convex:
					Unity.Physics.ConvexCollider* Convex_p = (Unity.Physics.ConvexCollider*)Bounds_p;
					Unity.Physics.ConvexCollider Convex = *Convex_p;
					obj = Convex;
					break;
				case ColliderType.Cylinder:
					Unity.Physics.CylinderCollider* Cylinder_p = (Unity.Physics.CylinderCollider*)Bounds_p;
					Unity.Physics.CylinderCollider Cylinder = *Cylinder_p;
					obj = Cylinder;
					break;
				case ColliderType.Mesh:
					Unity.Physics.MeshCollider* Mesh_p = (Unity.Physics.MeshCollider*)Bounds_p;
					Unity.Physics.MeshCollider Mesh = *Mesh_p;
					obj = Mesh;
					break;
				case ColliderType.Sphere:
					Unity.Physics.SphereCollider* Sphere_p = (Unity.Physics.SphereCollider*)Bounds_p;
					Unity.Physics.SphereCollider Sphere = *Sphere_p;
					obj = Sphere;
					break;
				case ColliderType.Terrain:
					Unity.Physics.TerrainCollider* Terrain_p = (Unity.Physics.TerrainCollider*)Bounds_p;
					Unity.Physics.TerrainCollider Terrain = *Terrain_p;
					obj = Terrain;
					break;
				case ColliderType.Triangle:Debug.LogWarning("GetCollider: cannot create a triganle collider");break;
				case ColliderType.Quad:Debug.LogWarning("GetCollider: cannot create a quad collider");break;
			}
			if (obj != null)
				return (T)obj;
			else
			{
				Debug.LogWarning("GetCollider:: Failed to get Collider with matching T type");
				return new T();
			}
		}
		/// <summary>
		/// returns the Unity.Physics.Collider version of the pointer
		/// </summary>
		/// <returns></returns>
		public Unity.Physics.Collider GetCollider()
		{
			unsafe
			{
				return *Bounds_p;
			}
		}
		/// <summary>
		/// returns the collider pointer
		/// </summary>
		/// <returns></returns>
		public unsafe Unity.Physics.Collider* GetCollider_P()
		{
			return Bounds_p;
		}
		/// <summary>
		/// sets the collider
		/// </summary>
		/// <param name="collider"></param>
		public unsafe void SetCollider(Unity.Physics.Collider* collider)
		{
			Bounds_p = collider;
			UpdateAabb(this.transform);
		//	GenerateProperties();
		}
		#endregion

		#region Aabb_Functions

		/// <summary>
		/// checks to see if the given Aabb overlaps the internal one
		/// </summary>
		/// <param name="other">other Aabb</param>
		/// <returns>true if overlap, false otherwise</returns>
		public bool Overlaps(Aabb other)
		{
			return aabb.Overlaps(other);
		}
		/// <summary>
		/// checks to see if the given point is contained within the bounds
		/// </summary>
		/// <param name="point">point to test</param>
		/// <returns>true if point is within bounds, false otherwise</returns>
		public bool Contains(float3 point)
		{
			return aabb.Contains(point);
		}
		/// <summary>
		/// checks to see if the given Aabb is contained within the internal Aabb
		/// </summary>
		/// <param name="_aabb"></param>
		/// <returns></returns>
		public bool Contains(Aabb _aabb)
		{
			return aabb.Contains(_aabb);
		}
		/// <summary>
		/// checks to see if the given ACollider's Aabb is contained within the internal Aabb
		/// </summary>
		/// <param name="_aabb"></param>
		/// <returns></returns>
		public bool Contains(ACollider other)
		{
			return aabb.Contains(other.aabb);
		}
		/// <summary>
		/// returns the closet
		/// </summary>
		/// <param name="position"></param>
		/// <returns></returns>
		public float3 ClosestPoint(float3 position)
		{
			return aabb.ClosestPoint(position);
		}

		

		/// <summary>
		/// updates the Aabb
		/// </summary>
		public void UpdateAabb()
		{
			unsafe
			{
				if (Bounds_p == null)
					return;
				else if(this.transform.Equals(new RigidTransform()))
					aabb = Bounds_p->CalculateAabb();
				else
					aabb = Bounds_p->CalculateAabb(this.transform);
				FinalizeAabb();
			}
		}
		/// <summary>
		/// Updates the Aabb
		/// </summary>
		/// <param name="rigidTransform"></param>
		public void UpdateAabb(RigidTransform rigidTransform)
		{
		//	Debug.Log("Updaing Aabb..."+AabbToString());
			unsafe
			{
				if (Bounds_p == null)
					return;
				if (!this.transform.Equals(rigidTransform) && !rigidTransform.Equals(new RigidTransform()))
				{
			//		Debug.Log(!this.transform.Equals(rigidTransform) + ",,new: " + rigidTransform + ",,old: " + transform);
					this.transform = rigidTransform;
				}
				
				aabb = Bounds_p->CalculateAabb(this.transform);

				properties.translation = transform.pos;
				properties.orientation = transform.rot;
				FinalizeAabb();
			}
		//	Debug.Log("new Aabb:" + AabbToString());
		}

		private unsafe void FinalizeAabb()
		{
			switch (Bounds_p->Type)
			{
				case ColliderType.Box:
				case ColliderType.Sphere:
					// do nothing
					break;
				case ColliderType.Cylinder:
					Debug.Log("aabb:::::"+aabb.Min+",,"+aabb.Max);
					aabb.Min = new float3(aabb.Min.x , -properties.height / 2, aabb.Min.z);
					aabb.Max = new float3(aabb.Max.x, properties.height / 2, aabb.Max.z);
					break;
				case ColliderType.Capsule:
					aabb.Min = new float3(-properties.radius, -(properties.height / 2), -properties.radius);
					aabb.Max = new float3(properties.radius, (properties.height / 2), properties.radius);
					break;
				default:

					Debug.LogWarning("FinalizeAabb: cannot create aabb with given collider \"" + Bounds_p->Type.ToString());
					break;
			}

		}

		#endregion

		#region Overrides

		public static bool operator ==(ACollider a, ACollider b)
		{
			unsafe
			{
				if (a.Bounds_p != b.Bounds_p)
					return false;
			}
			return a.transform.Equals(b.transform);
		}

		public static bool operator !=(ACollider a, ACollider b)
		{
			return !(a == b);
		}

		public bool Equals(ACollider a)
		{
			return this == a;
		}

		public override bool Equals(object obj)
		{
			return this.Equals((ACollider)obj);
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		public static ACollider Null => new ACollider();

		/// <summary>
		/// returns all easily availible properties realted to the given geometry
		/// </summary>
		/// <typeparam name="T">a struct of type Unity.Physics.[Type of Geometry]Geometry</typeparam>
		/// <param name="geometry">same as T</param>
		/// <param name="type">collider type</param>
		/// <returns></returns>
		public static string GeometryToString<T>(T geometry, ColliderType type) where T : struct
		{
			object obj = null;
			switch (type)
			{
				case ColliderType.Box:
					obj = geometry;
					BoxGeometry bg = (BoxGeometry)obj;
					return "GeometryType: Box\n\tBevel Radius: "+bg.BevelRadius+"\n\tCenter: "+bg.Center + "\n\tOrientation: " + bg.Orientation + "\n\tSize: " + bg.Size;
				case ColliderType.Capsule:
					obj = geometry;
					CapsuleGeometry cg = (CapsuleGeometry)obj;
					return "GeometryType: Box\n\tRadius: " + cg.Radius + "\n\tVertext0: " + cg.Vertex0 + "\n\tVertex1: " + cg.Vertex1;
				case ColliderType.Cylinder:
					obj = geometry;
					CylinderGeometry cyg = (CylinderGeometry)obj;
					return "GeometryType: Box\n\tBevel Radius: " + cyg.BevelRadius + "\n\tHeight: " + cyg.Height + "\n\tCenter: " + cyg.Center+"\n\tRadius: "+cyg.Radius+"\n\tOrientation: "+cyg.Orientation;
				case ColliderType.Sphere:
					obj = geometry;
					SphereGeometry sg = (SphereGeometry)obj;
					return "GeometryType: Box\n\t Radius: " + "\n\tCenter: " + sg.Center + "\n\tRadius: " + sg.Radius;
				default:
					return "Failed to get geometry";
					
			}
		}

		public string AabbToString()
		{
			return "Aabb: center("+aabb.Center+")\n\tExtents("+aabb.Extents+")\n\t surfaceArea("+aabb.SurfaceArea+")\n\t Min("+aabb.Min+")\n\t Max("+aabb.Max+")";;
		}

		public override string ToString()
		{
			//	return properties.ToString();
			return "Transform: " + transform.ToString() + "\n" + AabbToString();
				/*
			unsafe
			{
				switch (Bounds_p->Type)
				{
					case ColliderType.Box:
						Unity.Physics.BoxCollider* Box_p = (Unity.Physics.BoxCollider*)Bounds_p;
						return "Collider Data: \nType: BoxCollider\n" +	GeometryToString<BoxGeometry>(Box_p->Geometry, Bounds_p->Type)+"\nProperties: "+properties.ToString();
					case ColliderType.Capsule:
						Unity.Physics.CapsuleCollider* Capsule_p = (Unity.Physics.CapsuleCollider*)Bounds_p;
						return "Collider Data: \nType: CapsuleCollider\n" + GeometryToString<CapsuleGeometry>(Capsule_p->Geometry, Bounds_p->Type) + "\nProperties: " + properties.ToString();
					case ColliderType.Compound:
						Unity.Physics.CompoundCollider* Compound_p = (Unity.Physics.CompoundCollider*)Bounds_p;
						return "Collider Data: \nType: CompoundCollider\n# of Children: " + Compound_p->NumChildren + "\nProperties: " + properties.ToString();
					case ColliderType.Convex:
						return "Collider Data: \nType: ConvexCollider\n" + "\nProperties: " + properties.ToString();
					case ColliderType.Cylinder:
						Unity.Physics.CylinderCollider* Cylinder_p = (Unity.Physics.CylinderCollider*)Bounds_p;
						return "Collider Data: \nType: CapsuleCollider\n" + GeometryToString<CylinderGeometry>(Cylinder_p->Geometry, Bounds_p->Type) + "\nProperties: " + properties.ToString();
					case ColliderType.Mesh:
						return "Collider Data: \nType: MeshCollider\n" + "\nProperties: " + properties.ToString();
					case ColliderType.Sphere:
						Unity.Physics.SphereCollider* Sphere_p = (Unity.Physics.SphereCollider*)Bounds_p;
						return "Collider Data: \nType: SphereCollider\n" + GeometryToString<SphereGeometry>(Sphere_p->Geometry, Bounds_p->Type) + "\nProperties: " + properties.ToString();
					case ColliderType.Terrain:
						return "Collider Data: \nType: Terrain\n" + "\nProperties: " + ;
					case ColliderType.Triangle:
						return "Collider Data: \nType: TriangleCollider\n" + "\nProperties: " + properties.ToString();
					case ColliderType.Quad:
						return "Collider Data: \nType: QuadCollider\n" + "\nProperties: " + properties.ToString();
				}
			}
			return "No Collider Data Found!";*/
		}


		#endregion
	}

	public struct AColliderProperties : IComponentData
	{
		public float3 translation;
		public quaternion orientation;
		public float radius;
		public float height;
		public float3 valueA;
		public float3 valueB;

		public static float3 GetSize(ColliderType type,AColliderProperties properties)
		{
			switch (type) {
				case ColliderType.Box:
					return properties.valueA;
				case ColliderType.Sphere:
					return properties.valueA;
				case ColliderType.Cylinder:
					return new float3(properties.radius * 2,properties.height, properties.radius * 2);
				case ColliderType.Capsule:
					return new float3(properties.radius * 2, properties.radius * 2 + properties.height, properties.radius * 2);
				default:
					Debug.LogWarning("AColliderProperties::GetSize:: ColliderType is not supported");
					return float3.zero;
			}

		}

		public static float3 GetSize(Aabb aabb,quaternion rotation)
		{


			quaternion tmpA = new quaternion(-rotation.value.x, -rotation.value.y, -rotation.value.z, -rotation.value.w);
			quaternion rA = math.normalize(math.mul(tmpA, rotation));
			float3 remaining = new float3(rA.value.x,rA.value.y,rA.value.z); 
			float3 min = remaining * aabb.Min;
			float3 max = remaining * aabb.Max;
			Debug.Log("rotation test: "+rA+","+min+"..."+max);




			return float3.zero;
		}

		#region overrides

		public static bool operator ==(AColliderProperties a,AColliderProperties b)
		{
			return a.translation.Equals(b.translation) && a.orientation.Equals(b.orientation) && a.radius == b.radius && a.height == b.height && a.valueA.Equals(b.valueA) && a.valueB.Equals(b.valueB);
		}

		public static bool operator !=(AColliderProperties a,AColliderProperties b)
		{
			return !(a == b);
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
				return false;
			return this.Equals((AColliderProperties)obj);
		}

		public bool Equals(AColliderProperties a)
		{
			return this == a;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		public override string ToString()
		{
			return "Trasnlation: " + translation + "\nOrientation: " + orientation + "\nradius: "+radius+"\nheight: "+height+"\nvalueA: "+valueA+"\nvalueB:"+valueB;
		}

		public AColliderProperties Null => new AColliderProperties();
		#endregion
	}

}