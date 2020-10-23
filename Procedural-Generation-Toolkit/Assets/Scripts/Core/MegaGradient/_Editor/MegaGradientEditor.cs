using UnityEngine;
using UnityEditor;
using Core.MegaGradient;

namespace EditorCore.EditorMegaGradient
{
	public class MegaGradientEditor : EditorWindow
	{
		MegaGradient gradient;

		const int borderSize = 10;
		const float keyWidth = 10;
		const float keyHeight = 20;


		Rect[] keyRects;
		bool mouseIsDownOverKey;
		int selectedKeyIndex;
		bool forceRepaint;

		Rect gradientPreveiwRect;
		Event guiEvent;

		private void OnGUI()
		{
			
				guiEvent = Event.current;
				Draw();

				HandleInput();

				if (forceRepaint)
				{
					Repaint();
					forceRepaint = false;
				}
		
		}

		void Draw()	
		{

			gradientPreveiwRect = new Rect(borderSize, borderSize, position.width - (borderSize * 2), 25);

			GUI.DrawTexture(gradientPreveiwRect, gradient.GetTexture((int)gradientPreveiwRect.width));
			keyRects = new Rect[gradient.NumKeys];
			for (int i = 0; i < gradient.NumKeys; i++)
			{
				ColorKey key = gradient.GetKey(i);
				
				Rect keyRect = new Rect(gradientPreveiwRect.x + gradientPreveiwRect.width * key.Time - (keyWidth / 2f), gradientPreveiwRect.yMax + borderSize, keyWidth, keyHeight);
				if (i == selectedKeyIndex)
				{
					EditorGUI.DrawRect(new Rect(keyRect.x - 2, keyRect.y - 2, keyRect.width + 4, keyRect.height + 4), Color.black);
				}

				EditorGUI.DrawRect(keyRect, key.Color);
				keyRects[i] = keyRect;
			}

			Rect settingsRect = new Rect(borderSize, keyRects[0].y + borderSize + keyHeight, position.width - borderSize * 2, position.height - (borderSize + keyHeight * 4));
			GUILayout.BeginArea(settingsRect);
			EditorGUI.BeginChangeCheck();
			Color newcolor = EditorGUILayout.ColorField(gradient.GetKey(selectedKeyIndex).Color);
			if (EditorGUI.EndChangeCheck())
			{
				gradient.UpdateKeyColor(selectedKeyIndex, newcolor);
			}
			// display gradient
			gradient.blendMode = (MegaGradient.BlendMode) EditorGUILayout.EnumPopup("Blend Mode", gradient.blendMode);
			gradient.randomizeColor = EditorGUILayout.Toggle("Randomize New Color", gradient.randomizeColor);
			GUILayout.EndArea();

		}

		void HandleInput()
		{	
			// mouse drag
			if (IsLeftClickEvent(guiEvent, EventType.MouseDrag) && mouseIsDownOverKey)
			{
				float keyTime = Mathf.InverseLerp(gradientPreveiwRect.x, gradientPreveiwRect.xMax, guiEvent.mousePosition.x);
				selectedKeyIndex = gradient.UpdateKeyTime(selectedKeyIndex, keyTime);
				forceRepaint = true;
			}
			// mouse down
			if (IsLeftClickEvent(guiEvent, EventType.MouseDown))
			{
				for (int i = 0; i < keyRects.Length; i++)
				{
					if (keyRects[i].Contains(guiEvent.mousePosition))
					{
						mouseIsDownOverKey = true;
						selectedKeyIndex = i;
						forceRepaint = true;
						break;
					}
				}
				if (!mouseIsDownOverKey)
				{
					float keyTime = Mathf.InverseLerp(gradientPreveiwRect.x, gradientPreveiwRect.xMax, guiEvent.mousePosition.x);
					Color interpolatedColor = gradient.Evaluate(keyTime);
					Color randomColor = new Color(Random.value, Random.value, Random.value);
					selectedKeyIndex = gradient.AddKey(gradient.randomizeColor ? randomColor : interpolatedColor, keyTime);
					// if the gradient is not updating whn you click on it, then uncomment this.
					forceRepaint = true;
					mouseIsDownOverKey = true;
				}
			}
			// mouse up
			if (IsLeftClickEvent(guiEvent, EventType.MouseUp))
			{
				mouseIsDownOverKey = false;
			}
			// backspace down
			if ((guiEvent.keyCode == KeyCode.Backspace || guiEvent.keyCode == KeyCode.Delete) && guiEvent.type == EventType.KeyDown)
			{
				gradient.RemoveKey(selectedKeyIndex);
				if (selectedKeyIndex >= gradient.NumKeys)
				{
					selectedKeyIndex--;
				}
				forceRepaint = true;
			}
		}

		public bool IsLeftClickEvent(Event _event,EventType otherType)
		{
			return _event.type == otherType && _event.button == 0; 
		}

		public void SetGradient(MegaGradient gradient)
		{
			this.gradient = gradient;
		}

		private void OnEnable()
		{
			titleContent.text = "MegaGradientEditor";
			position.Set(position.x, position.y, 400, 150);
			minSize = new Vector2(200,150);
		}

		private void OnDisable()
		{
			UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
		}
	}

	public class MegaGradientDataEditor: EditorWindow
	{
		MegaGradientData gradient;
		RequestReceivedClass RequestReceived;

		const int borderSize = 10;
		const float keyWidth = 10;
		const float keyHeight = 20;


		Rect[] keyRects;
		bool mouseIsDownOverKey;
		int selectedKeyIndex;
		bool forceRepaint;

		Rect gradientPreveiwRect;
		Event guiEvent;

		public bool isValid = false;

		private void OnGUI()
		{
			if (isValid && RequestReceived.IsValid)
			{
				gradient.ValidateKeys(RequestReceived.entityManager);
				guiEvent = Event.current;
				
				Draw();

				HandleInput();

				if (forceRepaint)
				{
					Repaint();
					forceRepaint = false;
				}
			}
			else return;
		}

		void Draw()
		{

			gradientPreveiwRect = new Rect(borderSize, borderSize, position.width - (borderSize * 2), 25);

			GUI.DrawTexture(gradientPreveiwRect, gradient.GetTexture((int)gradientPreveiwRect.width));
			keyRects = new Rect[gradient.NumKeys];
			for (int i = 0; i < gradient.NumKeys; i++)
			{
				ColorKeyData key = gradient.GetKey(i);
				Rect keyRect = new Rect(gradientPreveiwRect.x + gradientPreveiwRect.width * key.Time - (keyWidth / 2f), gradientPreveiwRect.yMax + borderSize, keyWidth, keyHeight);
				if (i == selectedKeyIndex)
				{
					EditorGUI.DrawRect(new Rect(keyRect.x - 2, keyRect.y - 2, keyRect.width + 4, keyRect.height + 4), Color.black);
				}

				EditorGUI.DrawRect(keyRect, key.Color);
				keyRects[i] = keyRect;
			}

			Rect settingsRect = new Rect(borderSize, keyRects[0].y + borderSize + keyHeight, position.width - borderSize * 2, position.height - (borderSize + keyHeight * 4));
			GUILayout.BeginArea(settingsRect);
			EditorGUI.BeginChangeCheck();
			Color newcolor = EditorGUILayout.ColorField(gradient.GetKey(selectedKeyIndex).Color);
			if (EditorGUI.EndChangeCheck())
			{
				gradient.UpdateKeyColor(selectedKeyIndex, newcolor);
			}
			// display gradient
			gradient.blendMode = (MegaGradient.BlendMode)EditorGUILayout.EnumPopup("Blend Mode", gradient.blendMode);
			gradient.randomizeColor = EditorGUILayout.Toggle("Randomize New Color", gradient.randomizeColor);
			GUILayout.EndArea();

		}

		void HandleInput()
		{
			// mouse drag
			if (IsLeftClickEvent(guiEvent, EventType.MouseDrag) && mouseIsDownOverKey)
			{
				float keyTime = Mathf.InverseLerp(gradientPreveiwRect.x, gradientPreveiwRect.xMax, guiEvent.mousePosition.x);
				selectedKeyIndex = gradient.UpdateKeyTime(selectedKeyIndex, keyTime);
				forceRepaint = true;
			}
			// mouse down
			if (IsLeftClickEvent(guiEvent, EventType.MouseDown))
			{
				for (int i = 0; i < keyRects.Length; i++)
				{
					if (keyRects[i].Contains(guiEvent.mousePosition))
					{
						mouseIsDownOverKey = true;
						selectedKeyIndex = i;
						forceRepaint = true;
						break;
					}
				}
				if (!mouseIsDownOverKey)
				{
					float keyTime = Mathf.InverseLerp(gradientPreveiwRect.x, gradientPreveiwRect.xMax, guiEvent.mousePosition.x);
					Color interpolatedColor = gradient.EvaluateColor(keyTime);
					Color randomColor = new Color(Random.value, Random.value, Random.value);
					selectedKeyIndex = gradient.AddKey(gradient.randomizeColor ? randomColor : interpolatedColor, keyTime);
					// if the gradient is not updating whn you click on it, then uncomment this.
					forceRepaint = true;
					mouseIsDownOverKey = true;
				}
			}
			// mouse up
			if (IsLeftClickEvent(guiEvent, EventType.MouseUp))
			{
				mouseIsDownOverKey = false;
			}
			// backspace down
			if ((guiEvent.keyCode == KeyCode.Backspace || guiEvent.keyCode == KeyCode.Delete) && guiEvent.type == EventType.KeyDown)
			{
				gradient.RemoveKey(selectedKeyIndex);
				if (selectedKeyIndex >= gradient.NumKeys)
				{
					selectedKeyIndex--;
				}
				forceRepaint = true;
			}
		}

		public bool IsLeftClickEvent(Event _event, EventType otherType)
		{
			return _event.type == otherType && _event.button == 0;
		}

		public void SetGradient(MegaGradientData gradient)
		{
			this.gradient = gradient;
		}

		public void SetRequestReceived(RequestReceivedClass requestReceived)
		{
			this.RequestReceived = requestReceived;
		}

		private void OnEnable()
		{
			titleContent.text = "MegaGradientDataEditor";
			position.Set(position.x, position.y, 400, 150);
			minSize = new Vector2(200, 150);
		}

		private void OnInspectorUpdate()
		{
			isValid = gradient != MegaGradientData.Null;

			if (!isValid)
			{
				this.Close();
				return;
			}
		}

		private void OnDisable()
		{
			if(!Application.isPlaying)
				UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
		}
	}
}
