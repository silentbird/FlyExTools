using UnityEditor;
using UnityEngine;

namespace FlyExTools.edit.tool {
	[InitializeOnLoad]
	public class MoveSelectedObject : Editor {
		static float moveSpeed = 1f;

		static MoveSelectedObject() {
			SceneView.duringSceneGui += DuringSceneGUI;
		}

		static void DuringSceneGUI(SceneView sceneView) {
			if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.UpArrow) {
				Vector3 movement = new Vector3(0, 0, 1);
				foreach (Transform transform in Selection.transforms) {
					if (transform.GetComponent<RectTransform>() != null) {
						movement = new Vector3(0, 1, 0);
					}

					transform.position += movement * moveSpeed;
				}

				Event.current.Use();
			}
			else if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.DownArrow) {
				Vector3 movement = new Vector3(0, 0, -1);
				foreach (Transform transform in Selection.transforms) {
					if (transform.GetComponent<RectTransform>() != null) {
						movement = new Vector3(0, -1, 0);
					}

					transform.position += movement * moveSpeed;
				}

				Event.current.Use();
			}
			else if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.LeftArrow) {
				Vector3 movement = new Vector3(-1, 0, 0);
				foreach (Transform transform in Selection.transforms) {
					transform.position += movement * moveSpeed;
				}

				Event.current.Use();
			}
			else if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.RightArrow) {
				Vector3 movement = new Vector3(1, 0, 0);
				foreach (Transform transform in Selection.transforms) {
					transform.position += movement * moveSpeed;
				}

				Event.current.Use();
			}
		}
	}
}