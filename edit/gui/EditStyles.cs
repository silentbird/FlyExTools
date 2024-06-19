using System.Collections.Generic;
using System.Reflection;
using edit.resource;
using UnityEditor;
using UnityEngine;

namespace edit.gui {
	public static class EditStyles {
		public static readonly Color lineDark = EditorGUIUtility.isProSkin ? Color.black : new Color(0.2f, 0.2f, 0.2f, 1f);
		public static readonly Color lineLight = EditorGUIUtility.isProSkin ? Color.gray : Color.white;
		public static GUIContent NoneContent = new GUIContent("");
		public static ErrorStyle Error;
		public static GUIStyle Border0;
		public static GUIStyle Border1;
		public static GUIStyle Border2;
		public static GUIStyle Selected0;
		public static GUIStyle SelectedRed;
		public static GUIStyle SelectedGreen;
		public static GUIStyle SelectedUnfocused;
		public static GUIStyle Hover;
		public static GUIStyle None;
		public static GUIStyle Foldout;
		public static GUIStyle ToolTip;
		public static GUIStyle DropOver;
		public static GUIStyle DropResult;

		public static GUIStyle GreenLabel;

		// public static readonly Color systemBlue = ColorUtils.StringToColor("4081EEFF");
		public static GUIStyle boldLabel;
		public static GUIStyle Label;
		public static GUIStyle DisableLabel;

		public static GUIStyle SelectLabel;

		// public static GUIStyle DropInsert = GUIStyle.op_Implicit("TV Insertion");
		public static GUIStyle DropDown;
		private static bool inited;
		private static Dictionary<string, GUIStyle> style_pool = new Dictionary<string, GUIStyle>();

		static EditStyles() {
			BuildBorder();
			build_selection_styles();
		}

		private static void BuildBorder() {
			Border0 = new GUIStyle {
				normal = {
					background = PGUIUtility.ColorToTex(1, "0xFF151515")
				}
			};
			Border1 = new GUIStyle {
				normal = {
					background = PGUIUtility.ColorToTex(1, "0xff606060")
				}
			};
			Border2 = new GUIStyle {
				normal = {
					background = PGUIUtility.ColorToTex(1, "0xffa0a0a0")
				}
			};
		}

		private static GUIStyle get_color_background(string color) {
			return new GUIStyle {
				normal = {
					background = PGUIUtility.ColorToTex(1, color)
				},
				hover = {
					background = PGUIUtility.ColorToTex(1, color)
				},
				alignment = (TextAnchor)3,
				clipping = (TextClipping)1,
				contentOffset = Vector2.zero
			};
		}

		private static void build_selection_styles() {
			Selected0 = new GUIStyle {
				normal = {
					background = PGUIUtility.ColorToTex(1, "0x664081EE"),
					textColor = Color.white
				},
				hover = {
					background = PGUIUtility.ColorToTex(1, "0xFF4081EE"),
					textColor = Color.white
				},
				alignment = (TextAnchor)3,
				clipping = (TextClipping)1,
				contentOffset = Vector2.zero
			};
			GreenLabel = new GUIStyle(EditorStyles.label) {
				normal = {
					textColor = Color.green
				},
				hover = {
					textColor = Color.green
				}
			};
			SelectedRed = get_color_background("0xffff0000");
			SelectedGreen = get_color_background("0xff00ff00");
			SelectedUnfocused = get_color_background("0x66909090");
			DropOver = new GUIStyle {
				normal = {
					background = PGUIUtility.ColorToTex(1, "0xFFAACAF6"),
					textColor = Color.white
				}
			};
			DropResult = new GUIStyle {
				// normal = {
				// 	background = PResourceManager.LoadTexture("icons/DropResultGrid.png")
				// },
				border = new RectOffset(10, 10, 0, 0),
				margin = new RectOffset(5, 5, 0, 0)
			};
			Hover = new GUIStyle {
				normal = {
					background = PGUIUtility.ColorToTex(1, "0xFFDDDDAA"),
					textColor = Color.white
				}
			};
			None = new GUIStyle {
				normal = {
					background = PGUIUtility.ColorToTex(1, "0x00000000")
				}
			};
			Foldout = new GUIStyle(EditorStyles.foldout) {
				contentOffset = new Vector2(0.0f, -1f),
				padding = new RectOffset(0, 0, 0, 0)
			};
			ToolTip = new GUIStyle {
				normal = {
					background = PGUIUtility.ColorToTex(1, "0xFFFFFFCC"),
					textColor = Color.black
				},
				padding = new RectOffset(5, 5, 5, 5)
			};
		}

		private static bool isInvalid {
			get { return Selected0 == null || !Selected0.normal.background; }
		}


		public static void Init() {
			if (inited && !isInvalid)
				return;
			Input.imeCompositionMode = IMECompositionMode.On;
			Label = new GUIStyle(GUI.skin.label) {
				alignment = TextAnchor.MiddleLeft,
				clipping = TextClipping.Clip,
				richText = true
			};
			SelectLabel = new GUIStyle(GUI.skin.label) {
				alignment = TextAnchor.MiddleLeft,
				normal = {
					textColor = Color.white
				},
				clipping = TextClipping.Clip,
				richText = true
			};
			DisableLabel = new GUIStyle(GUI.skin.label) {
				alignment = TextAnchor.MiddleLeft,
				normal = {
					textColor = Color.gray
				},
				clipping = TextClipping.Clip,
				richText = true
			};
			boldLabel = new GUIStyle(GUI.skin.label) {
				alignment = TextAnchor.MiddleLeft,
				clipping = TextClipping.Clip,
				fontStyle = FontStyle.Bold,
				richText = true
			};
			DropDown = new GUIStyle("PreDropDown") {
				fontStyle = FontStyle.Normal
			};
			BuildBorder();
			build_selection_styles();
			Error = new ErrorStyle();
			inited = true;
		}

		public static void DrawHorizontalSeperator(float upSpace, float downSpace, GUIStyle style) {
			Init();
			if (upSpace > 0.0)
				GUILayout.Space(upSpace);
			GUILayout.Box("", style, GUILayout.Height(1f));
			if (downSpace <= 0.0)
				return;
			GUILayout.Space(downSpace);
		}

		public static void DrawHorizontalSeperator() {
			DrawHorizontalSeperator(5f, 2f, Border1);
		}

		public static void DrawHorizontalSeperator(GUIStyle style) {
			DrawHorizontalSeperator(5f, 2f, style);
		}

		public static void DrawHorizontalSeperator(float upSpace, float downSpace) {
			DrawHorizontalSeperator(upSpace, downSpace, Border1);
		}

		public static void DrawVerticalSeperator(
			float height,
			float leftSpace,
			float rightSpace,
			GUIStyle style) {
			Init();
			if (leftSpace > 0.0)
				GUILayout.Space(leftSpace);
			GUILayout.Box("", style, GUILayout.Width(1f), GUILayout.Height(height));
			if (rightSpace <= 0.0)
				return;
			GUILayout.Space(rightSpace);
		}

		public static void DrawVerticalSeperator() {
			DrawVerticalSeperator(18f, 5f, 5f, Border1);
		}

		public static void DrawVerticalSeperator(GUIStyle style) {
			DrawVerticalSeperator(18f, 5f, 5f, style);
		}

		public static void DrawVerticalSeperator(float height) {
			DrawVerticalSeperator(height, 5f, 5f, Border1);
		}

		public static void DrawVerticalSeperator(float height, float leftSpace, float rightSpace) {
			DrawVerticalSeperator(height, leftSpace, rightSpace, Border1);
		}

		public static GUIStyle GetStyleFromUnityEnity(string styleName) {
			GUIStyle styleFromUnityEnity;
			if (!style_pool.TryGetValue(styleName, out styleFromUnityEnity)) {
				PropertyInfo property = typeof(EditorStyles).GetProperty(styleName, BindingFlags.Static | BindingFlags.NonPublic);
				Debug.Log(property);
				styleFromUnityEnity = (GUIStyle)property?.GetValue(null, null);
				if (styleFromUnityEnity != null)
					style_pool.Add(styleName, styleFromUnityEnity);
			}

			return styleFromUnityEnity;
		}

		public class ErrorStyle {
			internal Texture2D iconInfo;
			internal Texture2D iconWarn;
			internal Texture2D iconError;
			internal Texture2D iconWarnInactive;
			internal Texture2D iconErrorInactive;

			internal GUIStyle info = new GUIStyle("CN EntryInfoIcon") {
				border = new RectOffset(0, 0, 0, 0),
				margin = new RectOffset(0, 0, 0, 0)
			};

			internal GUIStyle error = new GUIStyle("CN EntryErrorIcon") {
				border = new RectOffset(0, 0, 0, 0),
				margin = new RectOffset(0, 0, 0, 0)
			};

			internal GUIStyle warn = new GUIStyle("CN EntryWarnIcon") {
				border = new RectOffset(0, 0, 0, 0),
				margin = new RectOffset(0, 0, 0, 0)
			};

			internal ErrorStyle() {
				MethodInfo method = typeof(EditorGUIUtility).GetMethod("LoadIcon", BindingFlags.Static | BindingFlags.NonPublic);
				if (!(method != null))
					return;
				iconInfo = (Texture2D)method.Invoke(null, new object[1] {
					"console.infoicon.sml"
				});
				iconWarn = (Texture2D)method.Invoke(null, new object[1] {
					"console.warnicon.sml"
				});
				iconWarnInactive = (Texture2D)method.Invoke(null, new object[1] {
					"console.warnicon.inactive.sml"
				});
				iconError = (Texture2D)method.Invoke(null, new object[1] {
					"console.erroricon.sml"
				});
				iconErrorInactive = (Texture2D)method.Invoke(null, new object[1] {
					"console.erroricon.inactive.sml"
				});
			}
		}
	}
}