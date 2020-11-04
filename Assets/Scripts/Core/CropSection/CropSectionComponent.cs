using UnityEngine;
using Unity.Physics;
using Unity.Mathematics;
using Core.Extensions;
using Unity.Physics.Authoring;
using Core.Extensions.ACollider;

namespace Core.CropSection
{
	public class CropSectionComponent : MonoBehaviour
	{
		public CropSection3DClass cropSection = new CropSection3DClass(true);
		private Mesh BoundsMesh = null;
		private Mesh CropBoundsMesh = null;
		private void OnDrawGizmos()
		{
			if (cropSection != null)
			{
				if (cropSection.Bounds.IsValid() && cropSection.CropSectionBounds.IsValid())
				{
					Gizmos.color = Color.green;
					cropSection.Bounds.GenerateMeshEquivalent(ref BoundsMesh);
					Gizmos.DrawWireMesh(BoundsMesh, cropSection.CropSection.Bounds.transform.pos, math.normalize(cropSection.CropSection.Bounds.transform.rot));
				//	Gizmos.color = Color.black;
				//	cropSection.Bounds.GenerateMeshEquivalent(ref BoundsMesh,cropSection.Bounds.collider.aabb.Extents);
				//	Gizmos.DrawWireMesh(BoundsMesh, cropSection.CropSection.Bounds.transform.pos, math.normalize(cropSection.CropSection.Bounds.transform.rot));
					Gizmos.color = Color.blue;
			//		Debug.Log("aa:"+cropSection.CropSection.CropBounds.transform.ToString()+",,,"+cropSection.CropSectionBounds.collider.transform.ToString());
					cropSection.CropSectionBounds.GenerateMeshEquivalent(ref CropBoundsMesh);
					Gizmos.DrawWireMesh(CropBoundsMesh, cropSection.CropSection.CropBounds.transform.pos, math.normalize(cropSection.CropSection.CropBounds.transform.rot));


					
					// uncomment this to see what the extents of the shape looks like. 
					// NOTE: Capsule and cylinder will look funnky because it's extends are calculated relative to rotation
					cropSection.CropSectionBounds.GenerateMeshEquivalent(ref CropBoundsMesh, cropSection.CropSectionBounds.collider.aabb.Extents);
					Gizmos.color = Color.red;
					Gizmos.DrawWireMesh(CropBoundsMesh, cropSection.CropSection.CropBounds.transform.pos, quaternion.identity);
				//	*/
				}
			}
		//	else Debug.LogWarning("can't draw gizmo");
		}

		private void Reset()
		{
			cropSection = new CropSection3DClass();
			BoundsMesh = null;
			CropBoundsMesh = null;
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
}
