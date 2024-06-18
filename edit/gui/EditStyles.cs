using edit.resource;
using pure.utils.color;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;


namespace edit.gui {
	public static class EditStyles {
		public static readonly Color lineDark = EditorGUIUtility.isProSkin ? Color.black : new Color(0.2f, 0.2f, 0.2f, 1f);
		public static readonly Color lineLight = EditorGUIUtility.isProSkin ? Color.gray : Color.white;
		public static GUIContent NoneContent = new GUIContent("");
		public static EditStyles.ErrorStyle Error;
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
			EditStyles.BuildBorder();
			EditStyles.build_selection_styles();
		}

		private static void BuildBorder() {
			EditStyles.Border0 = new GUIStyle() {
				normal = {
					background = PGUIUtility.ColorToTex(1, "0xFF151515")
				}
			};
			EditStyles.Border1 = new GUIStyle() {
				normal = {
					background = PGUIUtility.ColorToTex(1, "0xff606060")
				}
			};
			EditStyles.Border2 = new GUIStyle() {
				normal = {
					background = PGUIUtility.ColorToTex(1, "0xffa0a0a0")
				}
			};
		}

		private static GUIStyle get_color_background(string color) {
			return new GUIStyle() {
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
			EditStyles.Selected0 = new GUIStyle() {
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
			EditStyles.GreenLabel = new GUIStyle(EditorStyles.label) {
				normal = {
					textColor = Color.green
				},
				hover = {
					textColor = Color.green
				}
			};
			EditStyles.SelectedRed = EditStyles.get_color_background("0xffff0000");
			EditStyles.SelectedGreen = EditStyles.get_color_background("0xff00ff00");
			EditStyles.SelectedUnfocused = EditStyles.get_color_background("0x66909090");
			EditStyles.DropOver = new GUIStyle() {
				normal = {
					background = PGUIUtility.ColorToTex(1, "0xFFAACAF6"),
					textColor = Color.white
				}
			};
			EditStyles.DropResult = new GUIStyle() {
				normal = {
					background = PResourceManager.LoadTexture("Icons/DropResultGrid.png")
				},
				border = new RectOffset(10, 10, 0, 0),
				margin = new RectOffset(5, 5, 0, 0)
			};
			EditStyles.Hover = new GUIStyle() {
				normal = {
					background = PGUIUtility.ColorToTex(1, "0xFFDDDDAA"),
					textColor = Color.white
				}
			};
			EditStyles.None = new GUIStyle() {
				normal = {
					background = PGUIUtility.ColorToTex(1, "0x00000000")
				}
			};
			EditStyles.Foldout = new GUIStyle(EditorStyles.foldout) {
				contentOffset = new Vector2(0.0f, -1f),
				padding = new RectOffset(0, 0, 0, 0)
			};
			EditStyles.ToolTip = new GUIStyle() {
				normal = {
					background = PGUIUtility.ColorToTex(1, "0xFFFFFFCC"),
					textColor = Color.black
				},
				padding = new RectOffset(5, 5, 5, 5)
			};
		}

		private static bool isInvalid {
			get { return EditStyles.Selected0 == null || !EditStyles.Selected0.normal.background; }
		}

		public static void Init() {
			if (EditStyles.inited && !EditStyles.isInvalid)
				return;
			Input.imeCompositionMode = (IMECompositionMode)1;
			EditStyles.Label = new GUIStyle(GUI.skin.label) {
				alignment = (TextAnchor)3,
				clipping = (TextClipping)1,
				richText = true
			};
			EditStyles.SelectLabel = new GUIStyle(GUI.skin.label) {
				alignment = (TextAnchor)3,
				normal = {
					textColor = Color.white
				},
				clipping = (TextClipping)1,
				richText = true
			};
			EditStyles.DisableLabel = new GUIStyle(GUI.skin.label) {
				alignment = (TextAnchor)3,
				normal = {
					textColor = Color.gray
				},
				clipping = (TextClipping)1,
				richText = true
			};
			EditStyles.boldLabel = new GUIStyle(GUI.skin.label) {
				alignment = (TextAnchor)3,
				clipping = (TextClipping)1,
				fontStyle = (FontStyle)1,
				richText = true
			};
			EditStyles.DropDown = new GUIStyle(EditorStyles.foldoutPreDrop) {
				fontStyle = (FontStyle)0
			};
			EditStyles.BuildBorder();
			EditStyles.build_selection_styles();
			EditStyles.Error = new EditStyles.ErrorStyle();
			EditStyles.inited = true;
		}

		public static void DrawHorizontalSeperator(float upSpace, float downSpace, GUIStyle style) {
			EditStyles.Init();
			if ((double)upSpace > 0.0)
				GUILayout.Space(upSpace);
			GUILayout.Box("", style, new GUILayoutOption[1] {
				GUILayout.Height(1f)
			});
			if ((double)downSpace <= 0.0)
				return;
			GUILayout.Space(downSpace);
		}

		public static void DrawHorizontalSeperator() {
			EditStyles.DrawHorizontalSeperator(5f, 2f, EditStyles.Border1);
		}

		public static void DrawHorizontalSeperator(GUIStyle style) {
			EditStyles.DrawHorizontalSeperator(5f, 2f, style);
		}

		public static void DrawHorizontalSeperator(float upSpace, float downSpace) {
			EditStyles.DrawHorizontalSeperator(upSpace, downSpace, EditStyles.Border1);
		}

		public static void DrawVerticalSeperator(
			float height,
			float leftSpace,
			float rightSpace,
			GUIStyle style) {
			EditStyles.Init();
			if ((double)leftSpace > 0.0)
				GUILayout.Space(leftSpace);
			GUILayout.Box("", style, new GUILayoutOption[2] {
				GUILayout.Width(1f),
				GUILayout.Height(height)
			});
			if ((double)rightSpace <= 0.0)
				return;
			GUILayout.Space(rightSpace);
		}

		public static void DrawVerticalSeperator() {
			EditStyles.DrawVerticalSeperator(18f, 5f, 5f, EditStyles.Border1);
		}

		public static void DrawVerticalSeperator(GUIStyle style) {
			EditStyles.DrawVerticalSeperator(18f, 5f, 5f, style);
		}

		public static void DrawVerticalSeperator(float height) {
			EditStyles.DrawVerticalSeperator(height, 5f, 5f, EditStyles.Border1);
		}

		public static void DrawVerticalSeperator(float height, float leftSpace, float rightSpace) {
			EditStyles.DrawVerticalSeperator(height, leftSpace, rightSpace, EditStyles.Border1);
		}

		public static GUIStyle GetStyleFromUnityEnity(string styleName) {
			GUIStyle styleFromUnityEnity;
			if (!EditStyles.style_pool.TryGetValue(styleName, out styleFromUnityEnity)) {
				PropertyInfo property = typeof(EditorStyles).GetProperty(styleName, BindingFlags.Static | BindingFlags.NonPublic);
				Debug.Log((object)property);
				styleFromUnityEnity = (GUIStyle)property?.GetValue((object)null, (object[])null);
				if (styleFromUnityEnity != null)
					EditStyles.style_pool.Add(styleName, styleFromUnityEnity);
			}

			return styleFromUnityEnity;
		}

		public class ErrorStyle {
			internal Texture2D iconInfo;
			internal Texture2D iconWarn;
			internal Texture2D iconError;
			internal Texture2D iconWarnInactive;

			internal Texture2D iconErrorInactive;
			// internal GUIStyle info = new GUIStyle(GUIStyle.op_Implicit("CN EntryInfoIcon"))
			// {
			//   border = new RectOffset(0, 0, 0, 0),
			//   margin = new RectOffset(0, 0, 0, 0)
			// };
			// internal GUIStyle error = new GUIStyle(GUIStyle.op_Implicit("CN EntryErrorIcon"))
			// {
			//   border = new RectOffset(0, 0, 0, 0),
			//   margin = new RectOffset(0, 0, 0, 0)
			// };
			// internal GUIStyle warn = new GUIStyle(GUIStyle.op_Implicit("CN EntryWarnIcon"))
			// {
			//   border = new RectOffset(0, 0, 0, 0),
			//   margin = new RectOffset(0, 0, 0, 0)
			// };

			internal ErrorStyle() {
				MethodInfo method = typeof(EditorGUIUtility).GetMethod("LoadIcon", BindingFlags.Static | BindingFlags.NonPublic);
				if (!(method != (MethodInfo)null))
					return;
				this.iconInfo = (Texture2D)method.Invoke((object)null, new object[1] {
					(object)"console.infoicon.sml"
				});
				this.iconWarn = (Texture2D)method.Invoke((object)null, new object[1] {
					(object)"console.warnicon.sml"
				});
				this.iconWarnInactive = (Texture2D)method.Invoke((object)null, new object[1] {
					(object)"console.warnicon.inactive.sml"
				});
				this.iconError = (Texture2D)method.Invoke((object)null, new object[1] {
					(object)"console.erroricon.sml"
				});
				this.iconErrorInactive = (Texture2D)method.Invoke((object)null, new object[1] {
					(object)"console.erroricon.inactive.sml"
				});
			}
		}
	}
}