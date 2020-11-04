using Unity.Entities;
using Unity.Mathematics;
using Core.Extensions.ACollider;
using Core.Extensions;
using UnityEngine;
using Unity.Physics.Authoring;
using UnityEditor;

namespace Core.CropSection
{	
	[System.Serializable]
	public class CropSection3DClass
	{
		[SerializeField]
		public AColliderClass Bounds = new AColliderClass();
		[SerializeField]
		public AColliderClass CropSectionBounds = new AColliderClass();
		[HideInInspector]
		public CropSection3D CropSection = new CropSection3D();
		[HideInInspector]
		public bool isUpdating = false;

		private bool debug = false;

		public CropSection3DClass(bool debug = false) {
			InitializeDefault(debug);
		}

		public void InitializeDefault(bool debug = false)
		{
			Bounds = null;
			CropSectionBounds = null;
			CropSection = CropSection3D.Null;
			this.debug = debug;
		}

		public CropSection3DClass(PhysicsShapeAuthoring shapeA,PhysicsShapeAuthoring shapeB,bool debug = false)
		{
			Bounds = new AColliderClass(shapeA);
			CropSectionBounds = new AColliderClass(shapeB);
			CropSection = new CropSection3D(Bounds.collider,CropSectionBounds.collider,float3.zero,float3.zero);
			TranslationOffsetSlider = new Float3Slider(CropSection.TranslationOffsetMin, CropSection.TranslationOffsetMax,
				default, "Translation Offset", "Max Offset", "Min Offset");
			RotationOffsetSlider = new Float3Slider(CropSection.RotationOffsetMin, CropSection.RotationOffsetMax, default,
				"Rotation Offset", "Max Offset", "Min Offset");
			this.debug = debug;
		}

		// displaying 
		public float3 OriginalTranslation;
		public float3 OriginalRotation;
		public float3 CurrentTranslation;
		public float3 CurrentRotation;
		[SerializeField]
		public Float3Slider TranslationOffsetSlider;
		[SerializeField]
		public Float3Slider RotationOffsetSlider;

		public void Update()
		{
			if (!isUpdating)
			{
				isUpdating = true;
				bool BoundsShapeChange = Bounds.Update();
				bool CropBoundsShapeChange = CropSectionBounds.Update();
				if (!Bounds.IsValid() || !CropSectionBounds.IsValid())
				{
					Debug.LogWarning("Bounds: " + Bounds.IsValid() + ",,," + Bounds.PhysicsShapeAuthoring + " collider: " + Bounds.collider.ToString());
					Debug.LogWarning("CropSectionBounds: " + CropSectionBounds.IsValid() + ",,," + CropSectionBounds.PhysicsShapeAuthoring + " collider:" + CropSectionBounds.collider.ToString());
					CropSection = CropSection3D.Null;
					TranslationOffsetSlider = null;
					RotationOffsetSlider = null;
				}
				// check for crop section being null
				if (CropSection == CropSection3D.Null)
				{
					if (Bounds.IsValid() && CropSectionBounds.IsValid())
					{
						Debug.Log("Creating a new crop section");
						CropSection = new CropSection3D(Bounds.collider, CropSectionBounds.collider);
						TranslationOffsetSlider = new Float3Slider(CropSection.TranslationOffsetMin, CropSection.TranslationOffsetMax, float3.zero,
							"Translation Offset", "Max Offset", "Min Offset");
						RotationOffsetSlider = new Float3Slider(CropSection.RotationOffsetMin, CropSection.RotationOffsetMax, float3.zero,
							"Rotation Offset", "Max Offset", "Min Offset");
						Debug.Log("CropSection3DClass: Successfully created a new crop section!");
					}
				}
				if (CropSection != CropSection3D.Null)
				{
					if (BoundsShapeChange)
					{
						//		Debug.Log("Detected a bounds change!");
						CropSection.ExecuteBoundsChange(Bounds.collider, true, true);
						TranslationOffsetSlider.MaxValue = CropSection.TranslationOffsetMax;
						TranslationOffsetSlider.MinValue = CropSection.TranslationOffsetMin;
					}
					if (CropBoundsShapeChange)
					{
						//		Debug.Log("Detected a crop bounds change");
						CropSection.ExecuteBoundsChange(CropSectionBounds.collider, false, true);
						TranslationOffsetSlider.MaxValue = CropSection.TranslationOffsetMax;
						TranslationOffsetSlider.MinValue = CropSection.TranslationOffsetMin;
					}
					// verify that the offset can be used
					if (CropSection.UpdateCropSection(TranslationOffsetSlider.Value, RotationOffsetSlider.Value))
					{
						//		Debug.Log("Updaing crop section stuff: "+CropSection.CropBounds.transform);
						// set the slider values to the result

						TranslationOffsetSlider.Value = CropSection.TranslationOffset;
						RotationOffsetSlider.Value = CropSection.RotationOffset;

						// now we update the other stuff
						// NOTE: this updates both when only 1 is changed, can be improved
						CropSectionBounds.collider.UpdateAabb(CropSection.CropBounds.transform);
						// offset doesn't affect parent shape so there's no need to update the bounds when an offset is changed
					//	Bounds.collider.UpdateAabb(CropSection.Bounds.transform);
					}

				}
				//	Debug.Log(CropSection.ToString());
				isUpdating = false;
			}
		}
	}
	

	[System.Serializable]
	public struct CropSection3D
	{
		[HideInInspector]
		public ACollider Bounds;
		[HideInInspector]
		public ACollider CropBounds;

		[HideInInspector]
		public bool3 SubtractionMode;
		private float3 SubModeMaxOffset;
		private float3 SubModeMaxROffset;

		
		public CropSection3D(ACollider bounds, ACollider cropBounds,float3 translationOffset = new float3(),float3 rotationOffset = new float3()){	
			Bounds = bounds;
			CropBounds = cropBounds;

			OriginalTranslation = bounds.transform.pos;
			OriginalRotation = bounds.transform.rot;

			TranslationOffset = translationOffset;
			TranslationOffsetMin = new float3();
			TranslationOffsetMax = new float3();
			RotationOffset = rotationOffset;
			RotationOffsetMin = new float3();
			RotationOffsetMax = new float3();
		
			CurrentRotation = cropBounds.transform.rot;
			CurrentTranslation = cropBounds.transform.pos;

			SubtractionMode = false;
			SubModeMaxOffset = 0f;
			SubModeMaxROffset = 0f;

			CalculateMinMaxA();

			ValidateMinMax();

			UpdateCropSection(TranslationOffset,RotationOffset);
		}

		/// <summary>
		/// Call this to calulate and set the new Min and Max values of the of offsets
		/// </summary>
		public void CalculateMinMaxA()
		{
			Debug.Log("Recalculating...");
			if (Bounds != ACollider.Null && CropBounds != ACollider.Null)
			{
				unsafe
				{
					if (CropBounds.GetCollider_P() != null && Bounds.GetCollider_P() != null)
					{
						switch (Bounds.GetCollider_P()->Type)
						{
							case Unity.Physics.ColliderType.Box:
							case Unity.Physics.ColliderType.Sphere:
							case Unity.Physics.ColliderType.Capsule:
							case Unity.Physics.ColliderType.Cylinder:
							case Unity.Physics.ColliderType.Mesh:
								//	CalculateMinMaxB(Bounds.aabb.Extents, Bounds.GetCollider_P()->Type);
								float3 s = AColliderProperties.GetSize(Bounds.properties);
								if (s.x == 0) s.x = 0.01f;
								if (s.y == 0) s.y = 0.01f;
								if (s.z == 0) s.z = 0.01f;
								CalculateMinMaxB(s, Bounds.GetCollider_P()->Type);
								break;
							default:
								Debug.LogWarning("CalculateMinMax: Failed to calculate the min max with the given collider type!");
								break;
						}
					}
					else
					{
						Debug.LogWarning("CalculateMinMax: Either Bounds collider or CropBounds collider is null\n"+Bounds.aabb.Extents+"\t "+CropBounds.aabb.Extents);
					}
				}
			}
			else Debug.LogWarning("CalculateMinMax: Either Bounds or Crop Bounds is Null!\n"+Bounds.ToString()+"\n"+CropBounds.ToString());
		}
		/// <summary>
		/// This is the second part of the Min Max calulates and should really only be called by CalculateMinMaxA
		/// </summary>
		/// <param name="size"></param>
		private void CalculateMinMaxB(float3 size,Unity.Physics.ColliderType type)
		{
			// lets try this with the up vector
			float3 offsetRange = new float3();
			unsafe
			{
				float3 oSize = CropBounds.aabb.Extents+0.01f;//AColliderProperties.GetSize(CropBounds.GetCollider_P()->Type, CropBounds.properties);
				if (oSize.x == 0) oSize.x = 0.01f;
				if (oSize.y == 0) oSize.y = 0.01f;
				if (oSize.z == 0) oSize.z = 0.01f;
				//		Debug.Log("CropBounds size:" + oSize+",,,"+CropBounds.aabb.Min+"");
				if (type == Unity.Physics.ColliderType.Box)
				{
					// for a cool effect, use CropBounds.aabb.Extents
					offsetRange = (size - oSize - 0.0001f) / 2;
					SubtractionMode = false;
				
				}
				else
				{
					//		Debug.Log("Getting B " + size + ",," + type.ToString() + ".." + CropBounds.GetCollider_P()->Type);
					switch (CropBounds.GetCollider_P()->Type)
					{
						case Unity.Physics.ColliderType.Box:
							if (type == Unity.Physics.ColliderType.Sphere)
							{
								offsetRange = (size - oSize) / 5;
								SubtractionMode = true;
							}
							else if (type == Unity.Physics.ColliderType.Cylinder)
							{
								offsetRange = (size - oSize) / 2.8f;
								offsetRange.y = (size.y - oSize.y - 0.001f) / 2;
								SubtractionMode = new bool3(true, false, true);
							}
							else if (type == Unity.Physics.ColliderType.Capsule)
							{
								offsetRange = (size - oSize) / 2.8f;
								SubtractionMode = true;
							}
							else Debug.LogError("Havent coded for this yet");
							//		Debug.Log("offsetRange = "+offsetRange);
							break;
						case Unity.Physics.ColliderType.Sphere:
							// due to the calculations testing for sizes. evey shape is treated as a box so round object may move in box-like patterns
							if (type == Unity.Physics.ColliderType.Capsule || type == Unity.Physics.ColliderType.Sphere || type == Unity.Physics.ColliderType.Capsule)
							{
								offsetRange = (size - oSize) / 2;
								SubtractionMode = true;
							}
							else Debug.LogError("Havent coded for this yet");
							break;
						case Unity.Physics.ColliderType.Capsule:
							if(type == Unity.Physics.ColliderType.Capsule || type == Unity.Physics.ColliderType.Sphere || type == Unity.Physics.ColliderType.Capsule)
							{
								offsetRange = (size - oSize) / 2.8f;
								SubtractionMode = true;
							}
							break;
						case Unity.Physics.ColliderType.Cylinder:
							if (type == Unity.Physics.ColliderType.Sphere)
							{
								// NOTE: this isn't a perfect calculation on how far it could go because we are using
								// a max offset in 1 dimension (x axis) which means the shape offset is limited by its radius
								offsetRange = (size - oSize) / 3.5f;
								SubtractionMode = true;
							}
							else if (type == Unity.Physics.ColliderType.Cylinder)
							{
								offsetRange = (size - oSize) / 2;
								SubtractionMode = new bool3(true,false,true);
							}else if(type == Unity.Physics.ColliderType.Capsule)
							{
								offsetRange = (size - oSize) / 2;
								offsetRange.y *= 2 / 2.3f;
								SubtractionMode = true;
							}
							break;
						case Unity.Physics.ColliderType.Mesh:
						
						default:
							Debug.LogWarning("CalculateMinMaxB: Cannot calulate min max with given collider type...please stop this...");
							break;
					}
				}

		//		Debug.Log(size + ":" + oSize+":"+(size-oSize));
			}

			// round the offsetRange value to 3rd decimal point
			offsetRange.x = (float)System.Math.Round(offsetRange.x, 3);
			offsetRange.y = (float)System.Math.Round(offsetRange.y, 3);
			offsetRange.z = (float)System.Math.Round(offsetRange.z, 3);


			TranslationOffsetMin = -offsetRange;
			TranslationOffsetMax = offsetRange;
			SubModeMaxOffset = offsetRange;


		//		Debug.Log("TMax: " + TranslationOffsetMax + ": TMin" + TranslationOffsetMin);

			ValidateMinMax();

			// for now we will let the Contains test limit the rotation

			RotationOffsetMax = 360f;
			RotationOffsetMin = 0f;

			TranslationOffset = float3.zero;
			RotationOffset = float3.zero;
		}

		public void ValidateMinMax()
		{
			if (TranslationOffsetMax.Equals(TranslationOffsetMin))
				TranslationOffsetMax = new float3(TranslationOffsetMin.x + 1f, TranslationOffsetMin.y + 1f, TranslationOffsetMin.z + 1f);
			if (RotationOffsetMax.Equals(RotationOffsetMin))
				RotationOffsetMax = new float3(RotationOffsetMin.x + 1f, RotationOffsetMin.y + 1f, RotationOffsetMin.z + 1f);
		}
		/// <summary>
		/// this neat function determines the final offset based off the values of the others
		/// </summary>
		/// <param name="changed">if any bool of changed is true then the function is executed on it until all changed values are false</param>
		/// <param name="total">total offset left</param>
		/// <param name="translationOffset">the translation offset used</param>
		/// <returns></returns>
		float SubtractFromOffset(bool3 changed,float total,ref float3 translationOffset)
		{
		//	Debug.Log("Subtracting from offset...");
			if (total > 0)
			{
				float tmp = 0f;
				// current x is prioritized, in a future version i should add some randomization or priority choosing
				if (changed.x)
				{
					
					changed.x = false;
					// get the absolute value of the offset.x
					tmp = math.abs(TranslationOffset.x);
					// remove from total
					total -= tmp;
					// if total is  < 0 this means that the offset is too high and has to be reduced to make the total non-negative
					if (total < 0)
						translationOffset.x += translationOffset.x < 0 ? -total : total;
					// just some clean up on floating point numbers
					if (math.abs(translationOffset.x) < 0.002f) translationOffset.x = 0;
				}
				else if (changed.y)
				{
					changed.y = false;
					tmp = math.abs(TranslationOffset.y);
					total -= tmp;
					if (total < 0)
						translationOffset.y += translationOffset.y < 0 ? -total : total;
					if (math.abs(translationOffset.y) < 0.002f) translationOffset.y = 0;
				}
				else
				{
					changed.z = false;
					tmp = math.abs(TranslationOffset.z);
					total -= tmp;
					if (total < 0)
						translationOffset.z += translationOffset.z < 0 ? -total : total;
					if (math.abs(translationOffset.z) < 0.002f) translationOffset.z = 0;
				}
				if (changed.x || changed.y || changed.z)
					return SubtractFromOffset(changed, total,ref translationOffset);
			}
			return total;
		}

		private float3 SubtractFromOffsetB(bool3 changed,float3 total,ref float3 translationOffset)
		{
			bool3 iszero = total == float3.zero;
			if (!iszero.x && !iszero.y && !iszero.z)
			{
				float tmp;
				float bigDiff = 0;
				//finds which axis has the greatest difference
				float GetBigDiff(float3 mTotal)
				{
					float t = 0;
					if (mTotal.x < 0 && math.abs(mTotal.x) > t)
						t = mTotal.x;
					if (mTotal.y < 0 && math.abs(mTotal.y) > t)
						t = mTotal.y;
					if (mTotal.z < 0 && math.abs(mTotal.z) > t)
						t = mTotal.z;
					return t;
				}
				if (changed.x && SubtractionMode.x)
				{
					tmp = math.abs(translationOffset.x);
					total -= tmp;
					if(total.x < 0 || total.y < 0 || total.z < 0)
						bigDiff = GetBigDiff(total);
			//		Debug.Log(translationOffset.x+",,"+bigDiff);
					if (bigDiff != 0)
						translationOffset.x += translationOffset.x < 0 ? -bigDiff : bigDiff; // don't forget that bigDiff is negative
					 // make float values are suposed to be zero when they're low wnough
					translationOffset.x = math.abs(translationOffset.x) < 0.0009f ? 0f : translationOffset.x;
				}
				if (changed.y && SubtractionMode.y)
				{
					tmp = math.abs(translationOffset.y);
					total -= tmp;
					if (total.x < 0 || total.y < 0 || total.z < 0)
						bigDiff = GetBigDiff(total);
					if (bigDiff != 0)
						translationOffset.y += translationOffset.y < 0 ? -bigDiff : bigDiff; // don't forget that bigDiff is negative
					translationOffset.y = math.abs(translationOffset.y) < 0.0009f ? 0f : translationOffset.y;
				}
				if (changed.z && SubtractionMode.z)
				{
					tmp = math.abs(translationOffset.z);
					total -= tmp;
					if (total.x < 0 || total.y < 0 || total.z < 0)
						bigDiff = GetBigDiff(total);
					if (bigDiff != 0)
						translationOffset.z += translationOffset.z < 0 ? -bigDiff : bigDiff; // don't forget that bigDiff is negative
					translationOffset.z = math.abs(translationOffset.z) < 0.0009f ? 0f : translationOffset.z;
				}
			}
			return total;
		}

		private bool3 OffsetExceedsBounds(float3 offset,float3 minOffset,float3 maxOffset,out bool3 positiveDirection,out float3 newOffset)
		{
			bool3 exceeds = false;
			newOffset = maxOffset - 0.01f;
			positiveDirection = true;
			if(offset.x > maxOffset.x) exceeds.x = true;
			else if (offset.x < minOffset.x)
			{
				exceeds.x = true;
				positiveDirection.x = false;
				newOffset.x = minOffset.x + 0.01f;
			}
			if (offset.y > maxOffset.y) exceeds.y = true;
			else if (offset.y < minOffset.y)
			{
				exceeds.y = true;
				positiveDirection.y = false;
				newOffset.y = minOffset.y + 0.01f;
			}
			if (offset.z > maxOffset.z) exceeds.z = true;
			else if (offset.z < minOffset.z)
			{
				exceeds.z = true;
				positiveDirection.z = false;
				newOffset.z = minOffset.z + 0.01f;
			}
			return exceeds;
		}
		/// <summary>
		/// updates the cropSection's values and returns true if an update occured
		/// </summary>
		public bool UpdateCropSection(float3 translationOffset, float3 rotationOffset)
		{
			//	Debug.Log(TranslationOffset+"::"+translationOffset);
			// test for new offset
			bool TranslationChanged = !translationOffset.Equals(TranslationOffset);
			bool RotationChanged = !rotationOffset.Equals(RotationOffset);
	//		Debug.Log(rotationOffset+",,"+RotationOffset);
			if (TranslationChanged || RotationChanged)
			{
		//		Debug.Log(TranslationChanged + "." + RotationChanged);

				// verify that the change wsas made inside bounds

		//		OffsetExceedsBounds(translationOffset,TranslationOffsetMin,TranslationOffsetMax,out bool3 positiveDirection,out translationOffset);

				if (SubtractionMode.x || SubtractionMode.y || SubtractionMode.z)
				{
					// Debug.Log("Detected Subtraction Mode");
					// last changed value takes priority
					bool3 changed = translationOffset != TranslationOffset;

					float3 total = SubModeMaxOffset;

					total = SubtractFromOffsetB(changed, total, ref translationOffset);
					total = SubtractFromOffsetB(!changed, total, ref translationOffset);
					//		Debug.Log(translationOffset+"::"+TranslationOffset);
					TranslationOffset = translationOffset;
				}
				ACollider tmp = CropBounds;
				//		Debug.Log(OriginalTranslation + "::" + new RigidTransform(OriginalRotation.value + quaternion.Euler(rotationOffset).value, OriginalTranslation + translationOffset).ToString());
				RigidTransform trans = new RigidTransform(OriginalRotation, OriginalTranslation);
				trans.pos += translationOffset;
				// this is not calculated correctly and will be fixed later
		//		Debug.Log("test old rot = "+RotationOffset+",,"+rotationOffset+",,,,"+ quaternion.EulerXYZ(math.radians(rotationOffset))+"radians: "+math.radians(rotationOffset)+":::");
				trans.rot = math.mul(trans.rot,quaternion.EulerXYZ(math.radians(rotationOffset.x),math.radians(rotationOffset.y),math.radians(rotationOffset.z)));
			//	Debug.Log("Offset Match Test: "+RotationOffset+",,"+trans.rot.value+",,"+new Quaternion(trans.rot.value.x, trans.rot.value.y, trans.rot.value.z, trans.rot.value.w).eulerAngles);
				unsafe
				{
					tmp.UpdateAabb(trans);
				}
				Debug.Log(CropBounds.AabbToString() +"\n"+CropBounds.properties.height+"\n"+AColliderProperties.GetSize(CropBounds.properties)+"\n"+CropBounds.properties.ToString());
				
				// verify that the CropBounds is within the Bounds
				if (Bounds.Contains(tmp))
				{
					//update Bounds
					CropBounds.UpdateAabb(tmp.transform);

					CurrentTranslation = CropBounds.transform.pos;
					CurrentRotation = CropBounds.transform.rot;

					TranslationOffset = translationOffset;
					RotationOffset = rotationOffset;

					return true;
				}
				else
				{
					// do nothing but you shouldn't get to this if the max was done right.
					Debug.LogWarning("Detected you somehow exceeded the bounds!");
					return false;
				}

			//	Debug.Log("Bounds Properties::: " + Bounds.ToString() + "\ntmp Properties::: \n" + tmp.ToString());
			//	Debug.Log("CropBounds props::: "+CropBounds.ToString());
			}
			return false;
		}

		public void ExecuteBoundsChange(ACollider collider,bool IsBounds,bool forceUpdate = false)
		{
			Debug.Log("ExecuteBoundsChange: changing...");
			unsafe
			{
				if (IsBounds)
				{
		//			Debug.Log("ExecuteBoundsChange: changing bounds...");
					if (!Bounds.Update(collider.transform, collider.GetCollider_P(),forceUpdate))
						Debug.LogWarning("ExecuteBoundsChange: No update was performed");
				}
				else
				{
		//			Debug.Log("ExecuteBoundsChange: changing cropbounds...");
					if (!CropBounds.Update(collider.transform, collider.GetCollider_P(),forceUpdate))
						Debug.LogWarning("ExecuteBoundsChange: No update was performed");
				}
			}
			CalculateMinMaxA();
			TranslationOffset = float3.zero;
			RotationOffset = float3.zero;

		}
		//////////////////////////
		// Original Translation //
		//////////////////////////

		public quaternion OriginalRotation;

		public float3 EulerOriginalRotation()
		{
			return new UnityEngine.Quaternion(
					OriginalRotation.value.x,
					OriginalRotation.value.y,
					OriginalRotation.value.z,
					OriginalRotation.value.w
				).eulerAngles;
		}
	
		//////////////////////////
		// Original Translation //
		//////////////////////////

		public float3 OriginalTranslation;

		////////////////////////
		// Translation Offset //
		////////////////////////
		
		public float3 TranslationOffset;
//		private float3 OldTranslationOffset;
		public float3 TranslationOffsetMax;
		public float3 TranslationOffsetMin;
		

		/////////////////////////
		// Rotation Offset	   //
		/////////////////////////
		public float3 RotationOffset;
	//	private float3 OldRotationOffset;
		public float3 RotationOffsetMax;
		public float3 RotationOffsetMin;

		public quaternion QuaternionRotationOffset()
		{
			return quaternion.Euler(RotationOffset);
		}

		/////////////////////////
		// Current Translation //
		/////////////////////////
		public float3 CurrentTranslation;

		/////////////////////////
		// Current Rotation    //
		/////////////////////////
		public quaternion CurrentRotation;

		public float3 EulerCurrentRotation()
		{
			return new UnityEngine.Quaternion(
					CurrentRotation.value.x,
					CurrentRotation.value.y,
					CurrentRotation.value.z,
					CurrentRotation.value.w
					).eulerAngles;
		}



		#region overrides
		public static bool operator ==(CropSection3D a,CropSection3D b)
		{
			return a.CurrentTranslation.Equals(b.CurrentTranslation) && 
				a.CurrentRotation.Equals(b.CurrentRotation) && 
				a.CropBounds == b.CropBounds &&
				a.Bounds == b.Bounds &&
				a.OriginalRotation.Equals(b.OriginalRotation) &&
				a.OriginalTranslation.Equals(b.OriginalTranslation) &&
				a.RotationOffset.Equals(b.RotationOffset) &&
				a.TranslationOffset.Equals(b.TranslationOffset);
		}
		public static bool operator !=(CropSection3D a, CropSection3D b)
		{
			return !(a==b);
		}
		public bool Equals(CropSection3D a)
		{
			return this == a;
		}
		public override bool Equals(object obj)
		{
			return this.Equals((CropSection3D)obj);
		}
		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		public static CropSection3D Null => new CropSection3D();

		public override string ToString()
		{
			return "Original Translation: " + OriginalTranslation + " Original Rotation: " + OriginalRotation + " Current Translation: " + CurrentTranslation + " Current Rotation: " +
				CurrentRotation + " Translation Offset"+TranslationOffset+" Translation Offset Max:"+ TranslationOffsetMax+" Trnaslation Offset Min: "+TranslationOffsetMin+
				" Rotation Offset: "+RotationOffset+" Rotation Offset Max: "+RotationOffsetMax+" Rotation Offset Min: "+RotationOffsetMin;
		}
		#endregion
	}

	[System.Obsolete]
	/// <summary>
	/// Crop Section
	/// </summary>
	[System.Serializable]
	public struct OldCropSection
	{
		public enum CropSectionType
		{
			Box,
			Circle,
			Triangle
		}
		public bool useCroppedSection;
		public CropSectionType type;
		private float4 cropSection;
		public float offsetY;
		public float offsetX;

		/// <summary>
		/// calculates how much offset you can have based on the given inputs. (the negative and positive int slide values)
		/// </summary>
		/// <param name="maxWidth">non-cropped width of </param>
		/// <param name="maxHeight"></param>
		/// <returns>an int 4 of the format (-maxOffsetX/2,maxOddsetX/2,-maxOffsetY/2,maxOddsetY/2)</returns>
		public float4 CalculateOffsetBoundsSliderValues(float maxWidth, float maxHeight)
		{
			switch (type)
			{
				case CropSectionType.Circle:
					return new float4();
				case CropSectionType.Triangle:
					return new float4();
				default:
					return new float4(-cropSection.x, maxWidth - cropSection.y, -cropSection.z, maxHeight - cropSection.w);
			}
		}
		/// <summary>
		/// sets the crop section
		/// </summary>
		/// <param name="newSection"></param>
		public void SetCropSection(int4 newSection)
		{
			cropSection = newSection;
		}
		/// <summary>
		/// returns the crop section + the offset
		/// </summary>
		/// <returns></returns>
		public float4 GetCropSection()
		{
			return new float4(
				cropSection.x + offsetX,
				cropSection.y + offsetX,
				cropSection.z + offsetY,
				cropSection.w + offsetY
				);
		}
		/// <summary>
		/// returns the cropSection without the offset
		/// </summary>
		/// <returns></returns>
		public float4 GetOriginalCropSection()
		{
			return cropSection;
		}

		public override bool Equals(object obj)
		{
			OldCropSection cs = (OldCropSection)obj;
			return false;
	//		return useCroppedSection == cs.useCroppedSection && cropSection.Equals(cs.cropSection) && offsetY == cs.offsetY && offsetX == cs.offsetX;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
	}

}
 