using Core.Extensions;
using System;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(Float3Slider))]
public class Float3SliderDrawer : PropertyDrawer
{

	SerializedProperty Value = null;
	SerializedProperty MinValue = null;
	SerializedProperty MaxValue = null;
	SerializedProperty SliderName = null;
	SerializedProperty MaxValueName = null;
	SerializedProperty MinValueName = null;
	SerializedProperty XLabel = null;
	SerializedProperty YLabel = null;
	SerializedProperty ZLabel = null;
	SerializedProperty Toggled = null;



	public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
	{
		return Toggled != null ? Toggled.boolValue ? 100 : 10 : 10;
	}
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		EditorGUI.BeginProperty(position,label,property);

		UpdateSerializedFields(property);
		if (Toggled.boolValue) position.y -= 45f; //-20f
	//	else position.y = 25f;
		Toggled.boolValue = EditorGUI.Foldout(position, Toggled.boolValue, SliderName.stringValue);
		if (Toggled.boolValue)
		{
			EditorGUI.indentLevel++;
			{
				position.y += 55f;//35f;
				// Display the Max Value
				MaxValue.vector3Value = EditorGUI.Vector3Field(position, MaxValueName.stringValue, MaxValue.vector3Value);
				position.y += EditorGUIUtility.singleLineHeight;
				//Display the Min Value
				MinValue.vector3Value = EditorGUI.Vector3Field(position, MinValueName.stringValue, MinValue.vector3Value);
				position.y += EditorGUIUtility.singleLineHeight;
				// Display the current values
				float tmpHeight = position.height;
				position.height = EditorGUIUtility.singleLineHeight;
				float3 value = Value.vector3Value;
				float3 minValue = MinValue.vector3Value;
				float3 maxValue = MaxValue.vector3Value;
				value.x = EditorGUI.Slider(position, XLabel.stringValue, value.x, minValue.x, maxValue.x);
				position.y += EditorGUIUtility.singleLineHeight;
				value.y = EditorGUI.Slider(position, YLabel.stringValue, value.y, minValue.y, maxValue.y);
				position.y += EditorGUIUtility.singleLineHeight;
				value.z = EditorGUI.Slider(position, ZLabel.stringValue, value.z, minValue.z, maxValue.z);
				Value.vector3Value = value;
				MinValue.vector3Value = minValue;
				MaxValue.vector3Value = maxValue;

				position.y -= 55f;
				position.height = tmpHeight;
			}
			EditorGUI.indentLevel--;
		}

	

		EditorGUI.EndProperty();
	}

	public void UpdateSerializedFields(SerializedProperty property)
	{
		Value = property.FindPropertyRelative("Value");
		MinValue = property.FindPropertyRelative("MinValue");
		MaxValue = property.FindPropertyRelative("MaxValue");
		XLabel = property.FindPropertyRelative("XLabel");
		YLabel = property.FindPropertyRelative("YLabel");
		ZLabel = property.FindPropertyRelative("ZLabel");
		SliderName = property.FindPropertyRelative("SliderName");
		MaxValueName = property.FindPropertyRelative("MaxValueName");
		MinValueName = property.FindPropertyRelative("MinValueName");
		Toggled = property.FindPropertyRelative("FoldoutToggle");

	}

}
