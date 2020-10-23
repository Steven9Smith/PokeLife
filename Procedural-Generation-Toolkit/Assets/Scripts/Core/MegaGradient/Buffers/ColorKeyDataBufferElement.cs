
using Unity.Entities;
using Unity.Mathematics;

namespace Core.MegaGradient.Buffers
{
	public struct ColorKeyDataBufferElement : IBufferElementData
	{
		public ColorKeyData Value;

		public static bool operator ==(ColorKeyDataBufferElement lhs, ColorKeyDataBufferElement rhs)
		{
			return lhs.Value == rhs.Value;
		}
		
		public static bool operator !=(ColorKeyDataBufferElement lhs, ColorKeyDataBufferElement rhs)
		{
			return !(lhs == rhs);
		}

		/// <summary>
		/// ColorKeyDataBufferElement instances are equal if they refer to the same ColorKeyDataBufferElement.
		/// </summary>
		/// <param name="compare">The object to compare to this ColorKeyDataBufferElement.</param>
		/// <returns>True, if the compare parameter contains an ColorKeyDataBufferElement object having the same value
		/// as this ColorKeyDataBufferElement.</returns>
		public override bool Equals(object compare)
		{
			return this == (ColorKeyDataBufferElement)compare;
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
		/// A "blank" ColorKeyDataBufferElement object that does not refer to an actual entity.
		/// </summary>
		public static ColorKeyDataBufferElement Null => new ColorKeyDataBufferElement();
	}

	public struct DynamicBufferColorKeyDataBufferElement
	{
		public Entity dataEntity;
		public DynamicBuffer<ColorKeyDataBufferElement> buffer;
		
		public DynamicBuffer<ColorKeyDataBufferElement> GetBuffer(EntityManager entityManager)
		{
			buffer = entityManager.GetBuffer<ColorKeyDataBufferElement>(dataEntity);
			return buffer;
		}

		public DynamicBufferColorKeyDataBufferElement(EntityManager entityManager)
		{
			this.dataEntity = entityManager.CreateEntity();
			entityManager.SetName(this.dataEntity, "DataEntity");
			this.buffer = entityManager.AddBuffer<ColorKeyDataBufferElement>(dataEntity);
			this.buffer.Add(new ColorKeyDataBufferElement { Value = new ColorKeyData(float3.zero, 0) });
			this.buffer.Add(new ColorKeyDataBufferElement { Value = new ColorKeyData(new float3(1f,1f,1f), 1) });
		}

		public DynamicBufferColorKeyDataBufferElement(EntityCommandBuffer.ParallelWriter ecb, int jobIndex)
		{
			this.dataEntity = ecb.CreateEntity(jobIndex);
			this.buffer = ecb.AddBuffer<ColorKeyDataBufferElement>(jobIndex, dataEntity);
			this.buffer.Add(new ColorKeyDataBufferElement { Value = new ColorKeyData(float3.zero, 0) });
			this.buffer.Add(new ColorKeyDataBufferElement { Value = new ColorKeyData(new float3(1f, 1f, 1f), 1) });
		}

		// Add Buffer to another entity

		public DynamicBuffer<ColorKeyDataBufferElement> AddBuffer(EntityManager entityManager, Entity entity)
		{
			return entityManager.AddBuffer<ColorKeyDataBufferElement>(entity);
		}

		public DynamicBuffer<ColorKeyDataBufferElement> AddBuffer(EntityCommandBuffer.ParallelWriter ecb, int jobIndex, Entity entity)
		{
			return ecb.AddBuffer<ColorKeyDataBufferElement>(jobIndex, entity);
		}

		// Overrides

		/// <summary>
		/// DynamicBufferColorKeyDataBufferElement instances are equal if they refer to the same Value.
		/// </summary>
		/// <param name="lhs">An DynamicBufferColorKeyDataBufferElement object.</param>
		/// <param name="rhs">Another DynamicBufferColorKeyDataBufferElement object.</param>
		/// <returns>True, if both values are identical.</returns>
		public static bool operator ==(DynamicBufferColorKeyDataBufferElement lhs, DynamicBufferColorKeyDataBufferElement rhs)
		{
			if (lhs.buffer.IsCreated != rhs.buffer.IsCreated)
				return false;
			if (lhs.buffer.IsCreated)
			{
				if (lhs.buffer.Length != rhs.buffer.Length)
					return false;
				for (int i = 0; i < lhs.buffer.Length; i++)
					if (lhs.buffer[i] != rhs.buffer[i])
						return false;
			}
			return true;
		}

		/// <summary>
		/// DynamicBufferColorKeyDataBufferElement instances are equal if they refer to the same Value.
		/// </summary>
		/// <param name="lhs">An DynamicBufferColorKeyDataBufferElement object.</param>
		/// <param name="rhs">Another DynamicBufferColorKeyDataBufferElement object.</param>
		/// <returns>True, if either value is different.</returns>
		public static bool operator !=(DynamicBufferColorKeyDataBufferElement lhs, DynamicBufferColorKeyDataBufferElement rhs)
		{
			return !(lhs == rhs);
		}

		/// <summary>
		/// DynamicBufferColorKeyDataBufferElement instances are equal if they refer to the same DynamicBufferColorKeyDataBufferElement.
		/// </summary>
		/// <param name="compare">The object to compare to this DynamicBufferColorKeyDataBufferElement.</param>
		/// <returns>True, if the compare parameter contains an DynamicBufferColorKeyDataBufferElement object having the same value
		/// as this DynamicBufferColorKeyDataBufferElement.</returns>
		public override bool Equals(object compare)
		{
			return this == (DynamicBufferColorKeyDataBufferElement)compare;
		}

		/// <summary>
		/// A hash used for comparisons.
		/// </summary>
		/// <returns>A unique hash code.</returns>
		public override int GetHashCode()
		{
			return base.GetHashCode();
		}



		public static DynamicBufferColorKeyDataBufferElement Null => new DynamicBufferColorKeyDataBufferElement();

	}

}