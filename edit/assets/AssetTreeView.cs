using System;
using System.Collections.Generic;
using edit.gui;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using Object = UnityEngine.Object;


namespace edit.assets {
	public class AssetTreeView : TreeView {
		private const float ICON_SIZE = 18f;
		private const float ROW_HEIGHT = 20f;
		public AssetViewItem assetRoot;
		public string filter;

		public AssetTreeView(TreeViewState state, MultiColumnHeader multicolumnHeader)
			: base(state, multicolumnHeader) {
			rowHeight = 20f;
			columnIndexForTreeFoldouts = 0;
			showAlternatingRowBackgrounds = true;
			showBorder = false;
			customFoldoutYOffset = (float)((20.0 - EditorGUIUtility.singleLineHeight) * 0.5);
			extraSpaceBeforeIconAndLabel = 18f;
		}

		protected override void SelectionChanged(IList<int> selectedIds) {
			if (selectedIds.Count != 1)
				return;
			AssetViewItem assetViewItem = (AssetViewItem)FindItem(selectedIds[0], rootItem);
			if (assetViewItem == null)
				return;
			EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath(assetViewItem.data.path, typeof(Object)));
		}


		protected override void DoubleClickedItem(int id) {
			AssetViewItem assetViewItem = (AssetViewItem)FindItem(id, rootItem);
			if (assetViewItem == null)
				return;
			Object _object = AssetDatabase.LoadAssetAtPath(assetViewItem.data.path, typeof(Object));
			EditorUtility.FocusProjectWindow();
			Selection.activeObject = _object;
			EditorGUIUtility.PingObject(_object);
		}

		public static MultiColumnHeaderState CreateMultiColumnHeaderState(float treeViewWidth) {
			return new MultiColumnHeaderState(new MultiColumnHeaderState.Column[3] {
				new MultiColumnHeaderState.Column {
					headerContent = new GUIContent("Name"),
					headerTextAlignment = (TextAlignment)1,
					sortedAscending = false,
					width = 200f,
					minWidth = 60f,
					autoResize = false,
					allowToggleVisibility = false,
					canSort = false
				},
				new MultiColumnHeaderState.Column {
					headerContent = new GUIContent("Path"),
					headerTextAlignment = (TextAlignment)1,
					sortedAscending = false,
					width = 360f,
					minWidth = 60f,
					autoResize = false,
					allowToggleVisibility = false,
					canSort = false
				},
				new MultiColumnHeaderState.Column {
					headerContent = new GUIContent("Qty"),
					headerTextAlignment = (TextAlignment)1,
					sortedAscending = false,
					width = 60f,
					minWidth = 60f,
					autoResize = false,
					allowToggleVisibility = true,
					canSort = false
				}
			});
		}

		protected override TreeViewItem BuildRoot() => assetRoot;

		protected override void RowGUI(RowGUIArgs args) {
			AssetViewItem assetViewItem = (AssetViewItem)args.item;
			int numVisibleColumns = args.GetNumVisibleColumns();
			for (int index = 0; index < numVisibleColumns; ++index) {
				draw_cell(args.GetCellRect(index), assetViewItem, (MyColumns)args.GetColumn(index), ref args);
			}
		}

		private void draw_cell(
			Rect rect,
			AssetViewItem item,
			MyColumns column,
			ref RowGUIArgs args) {
			CenterRectUsingSingleLineHeight(ref rect);
			if (item.displayName == "__EMPTY_NODE__")
				return;
			switch (column) {
				case MyColumns.Name:
					Rect rect1 = rect;
					ref Rect local = ref rect1;
					local.x += GetContentIndent(item);
					rect1.width = 18f;
					if (local.x < rect.xMax) {
						Texture2D icon = get_icon(item.data.path);
						if (icon != null)
							GUI.DrawTexture(rect1, icon, (ScaleMode)2);
					}

					args.rowRect = rect;
					base.RowGUI(args);
					break;
				case MyColumns.Path:
					GUIStyle guiStyle1 = args.selected ? EditStyles.SelectLabel : EditStyles.Label;
					string str = item.data.path;
					if (!string.IsNullOrEmpty(filter))
						str = str.Replace(filter, "<color=red>" + filter + "</color>");
					GUI.Label(rect, str, guiStyle1);
					break;
				case MyColumns.Quantity:
					int num = item.mode == AssetDisplayMode.Dependency ? item.data.dependencies.Count : item.data.references.Count;
					GUIStyle guiStyle2 = args.selected ? EditStyles.SelectLabel : EditStyles.Label;
					GUI.Label(rect, num.ToString(), guiStyle2);
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(column), column, null);
			}
		}

		private static Texture2D get_icon(string path) {
			Object _object = AssetDatabase.LoadAssetAtPath(path, typeof(Object));
			if (_object == null)
				return null;
			Texture2D icon = AssetPreview.GetMiniThumbnail(_object);
			if (icon == null)
				icon = AssetPreview.GetMiniTypeThumbnail(_object.GetType());
			return icon;
		}

		private enum MyColumns {
			Name,
			Path,
			Quantity,
		}
	}
}