/*
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
namespace Core
{
//	public class ECSBoundsSystem : SystemBase
//	{
//		protected override void OnUpdate()
//		{
//		}
//	}
	public class ECSBoundsClass
	{
		public enum BoundsType
		{
			Cube,
			Sphere,
			Capsule,
			Cone,
			Cylinder
		}


		// 3D functions
		public bool Contains(ECSBounds bounds, float3 point)
		{
			switch (bounds.type)
			{
				case BoundsType.Cylinder:
					return false;
				case BoundsType.Cone:
					return false;
				case BoundsType.Capsule:
					return false;
				case BoundsType.Sphere:
					return InSphere(bounds.center, bounds.GetLargestExtent(), point);
				default:
					return bounds.Contains(point);
			}
		}
		public bool InSphere(float3 center, float radius, float3 point)
		{
			return math.sqrt((point.x - center.x)) + math.sqrt(point.y - center.y) + math.sqrt(point.z - center.z) < (radius * radius);
		}

		public bool InCapsule()
		{
			return false;
		}

		public bool Contains(BoxCollider collider, float3 point)
		{
			Aabb a = collider.CalculateAabb();
			return a.Contains(point);
		}
	}

	/// <summary>
	/// A struct that mimics parts of a normal 2D Bounds
	/// </summary>
	[System.Serializable]
	public struct ACubeBounds2D
	{
		private float2 m_center;
		private float2 m_size;
		private float2 m_min;
		private float2 m_max;
		private float2 m_extents;
		/// <summary>
		/// Creates A Bounds that works in ECS
		/// </summary>
		/// <param name="center"></param>
		/// <param name="size"></param>
		public ACubeBounds2D(float2 center, float2 size)
		{
			// forced to initialize these first
			m_center = new float2();
			m_size = new float2();
			m_min = new float2();
			m_max = new float2();
			m_extents = new float2();
			this.size = size;
			this.center = center;
		}
		/// <summary>
		/// center of the bounds
		/// </summary>
		public float2 center
		{
			get { return m_center; }
			set
			{
				m_center = value;
				m_min = m_center - m_extents;
				m_max = m_center + m_extents;
			}
		}
		/// <summary>
		/// size of the bounds
		/// </summary>
		public float2 size
		{
			get { return m_size; }
			set
			{
				m_size = value;
				m_extents = m_size / 2;
			}
		}
		/// <summary>
		/// extents of the bounds. This will always be half the size
		/// </summary>
		public float2 extents
		{
			get { return m_extents; }
			set
			{
				m_extents = value;
				m_size = m_extents.x * 2;
			}
		}
		/// <summary>
		/// min point of the boundns
		/// </summary>
		public float2 min
		{
			get { return m_min; }
			set
			{
				m_min = value;
				m_extents = -m_min + m_center;
			}
		}
		/// <summary>
		/// max point of the bounds
		/// </summary>
		public float2 max
		{
			get { return m_max; }
			set
			{
				m_max = value;
				m_extents = m_max - m_center;
			}
		}
		/// <summary>
		/// returns true if the given point is within the bounds
		/// </summary>
		/// <param name="point"></param>
		/// <returns></returns>
		public bool Contains(float3 point)
		{
			return
				min.x < point.x &&
				min.y < point.y &&
				max.x > point.x &&
				max.y > point.y;
		}

		public float GetLargestExtent()
		{
			return extents.x > extents.y ? extents.x : extents.y;
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
				return false;
			else if (obj.GetType() != typeof(ECSBounds))
				return false;
			else
				return Equals((ECSBounds)obj);
		}

		public bool Equals(ECSBounds bounds)
		{
			return center.Equals(bounds.center) && size.Equals(bounds.size);
		}

		public override string ToString()
		{
			return "center: " + center + ", size: " + size;
		}
		/// <summary>
		/// returns the bounds as a  string
		/// </summary>
		/// <param name="returnExtraData"></param>
		/// <returns></returns>
		public string ToString(bool returnExtraData)
		{
			return returnExtraData ? this.ToString() + ", extents: " + extents + ", min: " + min + "max: " + max : this.ToString();
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
	}
	
	/// <summary>
	/// A struct that mimics parts of a normal 2D Bounds
	/// </summary>
	[System.Serializable]
	public struct CubeBounds2D
	{
		public float2 center;
		public float2 size;
		public float2 min;
		public float2 max;
		public float2 extents;

		public CubeBounds2D(float2 _center,float2 _extents)
		{
			center = _center;
			extents = _extents;
			min = new float2(_center.x - _extents.x, _center.y - _extents.y);
			max = new float2(_center.x + _extents.x, _center.y + _extents.y);
			size = extents * 2;
		}

		public void SetCenter(float2 _center)
		{
			center = _center;
			min = new float2(_center.x - extents.x, _center.y - extents.y);
			max = new float2(_center.x + extents.x, _center.y + extents.y);
			size = extents * 2;
		}

		public void SetExtents(float2 _extents)
		{
			extents = _extents;
			min = new float2(center.x - _extents.x, center.y - _extents.y);
			max = new float2(center.x + _extents.x, center.y + _extents.y);
			size = extents * 2;
		}
		
		public bool Contains(float2 point)
		{
			return point.x > min.x && point.x < max.x && point.y > min.y && point.y < max.y; 
		}

		public bool Intersects(CubeBounds2D other)
		{
			return Contains(other.min) || Contains(other.max) ||
				Contains(new float2(min.x, min.y + size.y)) ||
				Contains(new float2(max.x,max.y-size.y));
		}

		public static int Intersects(CubeBounds2D box1,CubeBounds2D box2)
		{
			if (box1.Intersects(box2))
				return 1;
			else if(box2.Intersects(box1))
				return -1;
			return 0;
		}
	}

	/// <summary>
	/// The ComponentData version of CubeBounds2D
	/// </summary>
	[System.Serializable]
	public struct ECSCubeBounds2D : IComponentData
	{
		public CubeBounds2D Value;
	}


	[System.Serializable]
	public struct CircleBounds2D
	{
		public float2 center;
		public float radius;
		public float diameter() { return radius * 2; }

		public CircleBounds2D(float2 _center, float _radius)
		{
			center = _center;
			radius = _radius;
		}

		public static bool Intersects(CircleBounds2D circle1,CircleBounds2D circle2)
		{
			float a = (circle1.radius + circle2.radius);
			return DistanceBetweenPointsSquared(circle1.center, circle2.center) > (a * a);
		}

		public static bool Contains(CircleBounds2D circle1,float2 point)
		{
			return DistanceBetweenPointsSquared(circle1.center, point) > (circle1.radius * circle1.radius);
		}

		public static float DistanceBetweenPointsSquared(float2 point1,float2 point2)
		{
			float a = (point2.x - point1.x);
			float b = (point2.y - point1.y);
			return a * a + b * b;
		}
	}

	public struct ECSCircleBounds2D : IComponentData
	{
		public CircleBounds2D Value;
		Unity.Physics.CylinderCollider a;
		public void b()
		{
		}
	}



	/// <summary>
	/// A struct that mimics parts of a normal Bounds
	/// </summary>
	[System.Serializable]
	public struct ECSBounds : IComponentData
	{
		public ECSBoundsClass.BoundsType type;

		private float3 m_center;
		private float3 m_size;
		private float3 m_min;
		private float3 m_max;
		private float3 m_extents;
		/// <summary>
		/// Creates A Bounds that works in ECS
		/// </summary>
		/// <param name="center"></param>
		/// <param name="size"></param>
		/// <param name="type"></param>
		public ECSBounds(float3 center, float3 size, ECSBoundsClass.BoundsType type)
		{
			this.type = type;
			// forced to initialize these first
			m_center = new float3();
			m_size = new float3();
			m_min = new float3();
			m_max = new float3();
			m_extents = new float3();
			this.size = size;
			this.center = center;
		}
		/// <summary>
		/// center of the bounds
		/// </summary>
		public float3 center
		{
			get { return m_center; }
			set
			{
				m_center = value;
				m_min = m_center - m_extents;
				m_max = m_center + m_extents;
			}
		}
		/// <summary>
		/// size of the bounds
		/// </summary>
		public float3 size
		{
			get { return m_size; }
			set
			{
				m_size = value;
				m_extents = m_size / 2;
			}
		}
		/// <summary>
		/// extents of the bounds. This will always be half the size
		/// </summary>
		public float3 extents
		{
			get { return m_extents; }
			set
			{
				m_extents = value;
				m_size = m_extents.x * 2;
			}
		}
		/// <summary>
		/// min point of the boundns
		/// </summary>
		public float3 min
		{
			get { return m_min; }
			set
			{
				m_min = value;
				m_extents = -m_min + m_center;
			}
		}
		/// <summary>
		/// max point of the bounds
		/// </summary>
		public float3 max
		{
			get { return m_max; }
			set
			{
				m_max = value;
				m_extents = m_max - m_center;
			}
		}
		/// <summary>
		/// returns true if the given point is within the bounds
		/// </summary>
		/// <param name="point"></param>
		/// <returns></returns>
		public bool Contains(float3 point)
		{
			return
				min.x < point.x &&
				min.y < point.y &&
				min.z < point.z &&
				max.x > point.x &&
				max.y > point.y &&
				max.z > point.z;
		}

		public float GetLargestExtent()
		{
			return extents.x > extents.y && extents.x > extents.z ? extents.x : extents.y > extents.x && extents.y > extents.z ? extents.y : extents.z;
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
				return false;
			else if (obj.GetType() != typeof(ECSBounds))
				return false;
			else
				return Equals((ECSBounds)obj);
		}

		public bool Equals(ECSBounds bounds)
		{
			return center.Equals(bounds.center) && size.Equals(bounds.size);
		}

		public override string ToString()
		{
			return "center: " + center + ", size: " + size;
		}
		/// <summary>
		/// returns the bounds as a  string
		/// </summary>
		/// <param name="returnExtraData"></param>
		/// <returns></returns>
		public string ToString(bool returnExtraData)
		{
			return returnExtraData ? this.ToString() + ", extents: " + extents + ", min: " + min + "max: " + max : this.ToString();
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
	}

}
*/
