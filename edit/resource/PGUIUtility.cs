using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace edit.resource {
	public static class PGUIUtility {
		public static float labelWidth = 150f;
		public static float fieldWidth = 50f;
		public static float indent = 0.0f;
		private static GUIStyle seperator;
		private static Stack<bool> changeStack = new Stack<bool>();
		private static int activeFloatField = -1;
		private static float activeFloatFieldLastValue;
		private static string activeFloatFieldString = "";
		private static Material texVizMat;
		private static Material lineMaterial;
		private static Texture2D lineTexture;
		private static FieldInfo field_scrollposition;
		private static MethodInfo delegate_get_scrollview;
		private static Func<Vector2, Vector2> unityIntervalUnclip;
		private static Func<Vector2, Vector2> unityInternalClip;
		private static FieldInfo unityEditorScreenPointOffset;


		private static float textFieldHeight => GUI.skin.textField.CalcHeight(new GUIContent("i"), 10f);


		private static float getLabelWidth() => EditorGUIUtility.labelWidth;

		private static float getFieldWidth() => EditorGUIUtility.fieldWidth;


		public static void Space() => Space(6f);

		public static void Space(float pixels) => GUILayoutUtility.GetRect(pixels, pixels);

		private static void setupSeperator() {
			if (seperator != null)
				return;
			seperator = new GUIStyle {
				normal = {
					background = ColorToTex(1, new Color(0.6f, 0.6f, 0.6f))
				},
				stretchWidth = true,
				margin = new RectOffset(0, 0, 7, 7)
			};
		}

		public static void BeginChangeCheck() {
			changeStack.Push(GUI.changed);
			GUI.changed = false;
		}

		public static bool EndChangeCheck() {
			bool changed = GUI.changed;
			if (changeStack.Count > 0) {
				GUI.changed = changeStack.Pop();
				if (changed && changeStack.Count > 0 && !changeStack.Peek()) {
					changeStack.Pop();
					changeStack.Push(true);
				}
			}
			else
				Debug.LogWarning("Requesting more EndChangeChecks than issuing BeginChangeChecks!");

			return changed;
		}

		public static bool Foldout(bool foldout, string content, params GUILayoutOption[] options) {
			return Foldout(foldout, new GUIContent(content), options);
		}

		public static bool Foldout(
			bool foldout,
			string content,
			GUIStyle style,
			params GUILayoutOption[] options) {
			return Foldout(foldout, new GUIContent(content), style, options);
		}

		public static bool Foldout(bool foldout, GUIContent content, params GUILayoutOption[] options) {
			return Application.isPlaying ? Foldout(foldout, content, GUI.skin.toggle, options) : EditorGUILayout.Foldout(foldout, content);
		}

		public static bool Foldout(
			bool foldout,
			GUIContent content,
			GUIStyle style,
			params GUILayoutOption[] options) {
			return Application.isPlaying ? GUILayout.Toggle(foldout, content, style, options) : EditorGUILayout.Foldout(foldout, content, style);
		}

		public static bool Toggle(bool toggle, string content, params GUILayoutOption[] options) {
			return Toggle(toggle, new GUIContent(content), options);
		}

		public static bool Toggle(
			bool toggle,
			string content,
			GUIStyle style,
			params GUILayoutOption[] options) {
			return Toggle(toggle, new GUIContent(content), style, options);
		}

		public static bool Toggle(bool toggle, GUIContent content, params GUILayoutOption[] options) {
			return Application.isPlaying ? Toggle(toggle, content, GUI.skin.toggle, options) : EditorGUILayout.ToggleLeft(content, toggle, options);
		}

		public static bool Toggle(
			bool toggle,
			GUIContent content,
			GUIStyle style,
			params GUILayoutOption[] options) {
			return Application.isPlaying ? GUILayout.Toggle(toggle, content, style, options) : EditorGUILayout.ToggleLeft(content, toggle, style, options);
		}


		public static Texture2D ColorToTex(int pxSize, Color col) {
			Texture2D tex = new Texture2D(pxSize, pxSize, (TextureFormat)4, false);
			for (int index1 = 0; index1 < pxSize; ++index1) {
				for (int index2 = 0; index2 < pxSize; ++index2)
					tex.SetPixel(index1, index2, col);
			}

			tex.Apply();
			tex.wrapMode = 0;
			return tex;
		}

		public static Texture2D ColorToTex(int pxSize, string colorName) {
			return ColorToTex(pxSize, ToColor(colorName));
		}

		public static Color ToColor(string colorName) {
			if (colorName.StartsWith("#"))
				colorName = colorName.Replace("#", string.Empty);
			if (colorName.StartsWith("0x"))
				colorName = colorName.Replace("0x", string.Empty);
			uint num = uint.Parse(colorName, NumberStyles.HexNumber);
			return new Color {
				a = (num >> 24 & byte.MaxValue) / (float)byte.MaxValue,
				r = (num >> 16 & byte.MaxValue) / (float)byte.MaxValue,
				g = (num >> 8 & byte.MaxValue) / (float)byte.MaxValue,
				b = (num & byte.MaxValue) / (float)byte.MaxValue
			};
		}
	}
}