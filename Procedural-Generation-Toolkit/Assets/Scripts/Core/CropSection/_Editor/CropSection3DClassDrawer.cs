using UnityEngine;
using System.Collections;
using UnityEditor;
using System;
using Core.Extensions;
using Unity.Mathematics;

namespace Core.CropSection
{
	[CustomPropertyDrawer(typeof(CropSection3DClass))]
	public class CropSection3DClassDrawer : PropertyDrawer
	{
		const int Y_OFFSET = 5;
		const float HEIGHT = 20;
		public float startingPosition = -150f;


		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return EditorGUI.GetPropertyHeight(property,label, true);
		}
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			// get the data
			CropSection3DClass cropsection = fieldInfo.GetValue(property.serializedObject.targetObject) as CropSection3DClass;
			// update it

			cropsection.Update();
			EditorGUI.PropertyField(position, property, true);

			/*			// display the results
						//	Debug.Log("A="+property.name+":"+position);
				//		position.y = startingPosition; 
				//		Debug.Log("B=" + property.name + ":" + position+"a"+ EditorGUI.GetPropertyHeight(property, label, false));
				//		CropSectionPropertiesFoldoutValue = EditorGUI.Foldout(position, CropSectionPropertiesFoldoutValue, "Crop Section 3D Properties");
						// get height of a property;
						if (CropSectionPropertiesFoldoutValue)
						// Crop Section Properties
						{
							EditorGUI.indentLevel++;
							{
								float tmp = 0;
								//	Debug.Log("C=" + position);
								position.y = (EditorGUI.GetPropertyHeight(property, label, true) + HEIGHT * 3);
								//	Debug.Log("D=" + position);
								EditorGUI.Vector3Field(position, "Current Translation", cropsection.CropSection.GetCurrentTranslation());
								position.y += HEIGHT;
								EditorGUI.Vector3Field(position, "Current Rotation", cropsection.CropSection.GetEulerCurrentRotation());
								tmp = position.y;
								position.y = startingPosition + (HEIGHT * 3);
								CSPRotationOffsetFoldoutValue = EditorGUI.Foldout(position, CSPRotationOffsetFoldoutValue, "Rotation Offset");
								position.y = tmp + HEIGHT;
								Debug.Log("A=" + position);

								if (CSPRotationOffsetFoldoutValue)
								{
									Rect tmpRect = position;
									tmpRect.height = 20;
									EditorGUI.indentLevel++;
									tmpRect.y += HEIGHT;
									tmpRect.height = EditorGUI.GetPropertyHeight(property, label, false);
									cropsection.CropSection.RotationOffset.DisplayValue(tmpRect, Y_OFFSET);
									EditorGUI.indentLevel--;
									position.y = startingPosition + (HEIGHT * 7);
								}
								else position.y = startingPosition + (HEIGHT * 4);
								Debug.Log("B=" + position);

								CSPTranslationOffsetFoldoutValue = EditorGUI.Foldout(position, CSPTranslationOffsetFoldoutValue, "Translation Offset");
								if (CSPTranslationOffsetFoldoutValue)
								{
									Rect tmpRect = position;
									EditorGUI.indentLevel++;
									tmpRect.y = HEIGHT * 15;
									tmpRect.height = EditorGUI.GetPropertyHeight(property, label, false);
									cropsection.CropSection.TranslationOffset.DisplayValue(tmpRect, Y_OFFSET);
									EditorGUI.indentLevel--;
									position.y = startingPosition + (HEIGHT * 26);

								}
								else position.y += HEIGHT;
								Debug.Log("C=" + position);
								Debug.Log("ww=" + CSPRotationOffsetFoldoutValue + "," + CSPTranslationOffsetFoldoutValue);

								EditorGUI.Vector3Field(position, "Original Rotation", cropsection.CropSection.GetEulerOriginalRotation());
								position.y += HEIGHT;
								EditorGUI.Vector3Field(position, "Original Transition", cropsection.CropSection.GetOriginalTranslation());
								position.y += HEIGHT;
							}
							EditorGUI.indentLevel--;
						}
						*/
		}
	}
}
