using UnityEngine;
using UnityEditor;	
using Core.MegaGradient;

namespace EditorCore.EditorMegaGradient
{
	[CustomPropertyDrawer(typeof(MegaGradient))]
	public class MegaGradientDrawer : PropertyDrawer
	{
		private const int LABEL_BUFFER = 5;

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			MegaGradient gradient = (MegaGradient)fieldInfo.GetValue(property.serializedObject.targetObject);
			if (gradient.hide)
			{
			//	GUI.Label(position, new GUIContent("Hidden"));
				return;
			}
			else
			{
				Event guiEvent = Event.current;

				float labelWidth = GUI.skin.label.CalcSize(label).x + LABEL_BUFFER;
				Rect textureRect = new Rect(position.x + labelWidth, position.y, position.width - labelWidth, position.height);

				// display gradient on inspector
				if (guiEvent.type == EventType.Repaint)
				{
					GUI.Label(position, label);
					GUI.DrawTexture(textureRect, gradient.GetTexture((int)position.width));
				}
				else
				{
					// if mouse down and left mouse button click
					if (guiEvent.type == EventType.MouseDown && guiEvent.button == 0)
					{
						// if mouse position inside the gradient box
						if (textureRect.Contains(guiEvent.mousePosition))
						{
							MegaGradientEditor window = EditorWindow.GetWindow<MegaGradientEditor>();
							window.SetGradient(gradient);
						}
					}
				}
			}

		}
	}

	[CustomPropertyDrawer(typeof(MegaGradientData))]
	public class MegaGradientDataDrawer : PropertyDrawer
	{
		private const int LABEL_BUFFER = 5;

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			Event guiEvent = Event.current;
			object[] objects = property.serializedObject.targetObjects;
			MegaGradientDataComponent dataComponent = null;
			for (int i = 0; i < objects.Length; i++)
			{
			//	Debug.Log(objects[i].GetType());
				if (objects[i].GetType() == typeof(MegaGradientDataComponent))
					dataComponent = (MegaGradientDataComponent)objects[i];
			}
			if (dataComponent != null)
			{
				if (Application.isPlaying)
				{
					MegaGradientData gradient = dataComponent.gradient;//(MegaGradientData)fieldInfo.GetValue(property.serializedObject.targetObject);
					float labelWidth = GUI.skin.label.CalcSize(label).x + LABEL_BUFFER;
					Rect textureRect = new Rect(position.x + labelWidth, position.y, position.width - labelWidth, position.height);
					// display gradient on inspector

					if (gradient.isCreated)
					{
						if (guiEvent.type == EventType.Repaint)
						{
							GUI.Label(position, label);
							GUI.DrawTexture(textureRect, gradient.GetTexture((int)position.width));
						
						}
						else
						{
							// if mouse down and left mouse button click
							if (guiEvent.type == EventType.MouseDown && guiEvent.button == 0)
							{
								// if mouse position inside the gradient box
								if (textureRect.Contains(guiEvent.mousePosition))
								{
									MegaGradientDataEditor window = EditorWindow.GetWindow<MegaGradientDataEditor>();
									window.SetGradient(gradient);
									window.SetRequestReceived(dataComponent.entityRequest.RequestReceived);
								}
							}
						}
					}
					else
					{
						if (guiEvent.type == EventType.Repaint)
						{
							GUI.Label(position, new GUIContent("MegaGradientData: Data is not created!"));
						}
					}
				}
				else
				{
					// edit mode

				}
			}
			else Debug.LogWarning("Failed to get MegaGradientDataComponent!");
		}
	}
}