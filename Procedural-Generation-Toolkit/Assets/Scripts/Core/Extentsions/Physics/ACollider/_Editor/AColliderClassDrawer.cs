using UnityEngine;
using UnityEditor;
using Core.Extensions.ACollider;
using Unity.Physics.Authoring;
using Unity.Mathematics;
using System.Reflection;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System;

[CustomPropertyDrawer(typeof(AColliderClass))]
public class AColliderClassDrawer : PropertyDrawer
{
	public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
	{
		return EditorGUI.GetPropertyHeight(property, true);
	}
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		// First we grab the value of the object and the GUI Event
		//	Event guiEvent = Event.current;
		// this display all properties, save for later
		/*		
					SerializedObject childObj = new UnityEditor.SerializedObject(property.serializedObject.targetObject);

					SerializedProperty ite = childObj.GetIterator();
					float prevHeight = 0;// = EditorGUI.GetPropertyHeight(property, label, true);

					while (ite.NextVisible(true))
					{
						if ( NeededField(ite.name))
						{
							Debug.Log("Child is " + ite.displayName + "\n" + ite.name);
							Rect newRect = new Rect(position.x, position.y + prevHeight + EditorGUIUtility.standardVerticalSpacing, position.width, EditorGUI.GetPropertyHeight(ite, label, true));
							prevHeight += newRect.height + EditorGUIUtility.standardVerticalSpacing;
							EditorGUI.PropertyField(newRect, ite,true);
						}
					}*/
		AColliderClass aCollider = null;
		try
		{
			// error happens when i call this. idk why it's calling the CropSection3D struct but whatever i'll just use a workaround for now
			aCollider = fieldInfo.GetValue(property.serializedObject.targetObject) as AColliderClass;
		}
		catch (ArgumentException e)
		{
			///	Debug.LogWarning("AColliderClass: that wierd error is happing again please make sure you update this in the future and try to fix it!");
		}
		if (aCollider != null)
			aCollider.Update();
		
	//	if (aCollider.collider.GetCollider().Type == Unity.Physics.ColliderType.Box)
	//	{
	//		Unity.Physics.BoxCollider box = aCollider.collider.GetCollider<Unity.Physics.BoxCollider>();
	//	}
		
		EditorGUI.PropertyField(position, property, true);

	}
}