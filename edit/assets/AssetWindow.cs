// Decompiled with JetBrains decompiler
// Type: edit.assets.AssetWindow
// Assembly: edit, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3ADECF35-F68B-48ED-B268-19248EA3422D
// Assembly location: D:\W\UnityProj\PluginsProj\Assets\Plugins\Win\csharp\edit.dll

using edit.etui.utils;
using edit.gui;
using pure.utils.mathTools;
using pure.utils.task;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace edit.assets {
	public class AssetWindow : EditorWindow {
		private const string MODE_PREF_KEY = "__ASSET_REF_MODE_KEY__";
		private const string FILTER_PREF_KEY = "__ASSET_REF_FILTER_KEY__";
		private const string AUTO_LOCATE_KEY = "__ASSET_REF_AUTO_LOCATE_KEY__";
		private AssetDisplayMode _displayMode;
		private AssetWindow.DisplayFilter _displayFilter;
		private bool _autoLocate;
		private InvalidateCache _valid;
		private string _filter = string.Empty;
		private AssetWindow.Style _style;
		private List<string> _selectedAssetGuid = new List<string>();
		private AssetTreeView _view;

		[SerializeField]
		private TreeViewState _state;

		private readonly AssetInfoCollection _collection = new AssetInfoCollection();

		[MenuItem("Assets/查找引用关系")]
		protected static void FindRef() {
			AssetWindow.OpenWindow();
			EditorWindow.GetWindow<AssetWindow>().update_selection();
		}

		[MenuItem("工具/资源/资源关系", false, 2500)]
		protected static void OpenWindow() {
			AssetWindow window = EditorWindow.GetWindow<AssetWindow>();
			window.wantsMouseMove = false;
			window.titleContent = new GUIContent("资源关系");
			window.Show();
			window.Focus();
		}

		private void update_selection() {
			this._selectedAssetGuid.Clear();
			foreach (var _object in Selection.objects) {
				string assetPath = AssetDatabase.GetAssetPath(_object is GameObject gameObject ? EditPrefabUtility.GetPrefab(gameObject) : _object);
				if (Directory.Exists(assetPath)) {
					string[] strArray = new string[1] { assetPath };
					foreach (string asset in AssetDatabase.FindAssets((string)null, strArray)) {
						if (!this._selectedAssetGuid.Contains(asset) && !Directory.Exists(AssetDatabase.GUIDToAssetPath(asset)))
							this._selectedAssetGuid.Add(asset);
					}
				}
				else {
					string guid = AssetDatabase.AssetPathToGUID(assetPath);
					if (!string.IsNullOrEmpty(guid))
						this._selectedAssetGuid.Add(guid);
				}
			}

			this._valid.Add(InvalidateType.Data);
		}

		private void purge_filter(AssetViewItem x) {
			if (x.data == null)
				return;
			if (x.data.path.Contains(this._filter))
				x.ok = true;
			else if (x.children == null) {
				x.ok = false;
			}
			else {
				foreach (TreeViewItem child in x.children) {
					if (child is AssetViewItem assetViewItem && assetViewItem.ok) {
						x.ok = true;
						break;
					}
				}
			}
		}

		private static void purge_empty(AssetViewItem x) {
			if (x.children == null)
				return;
			x.children.RemoveAll((Predicate<TreeViewItem>)(m => !((AssetViewItem)m).ok));
		}

		private void build_trees() {
			if (!this._valid.Contains(InvalidateType.Data) || this._selectedAssetGuid.Count == 0)
				return;
			this._valid.Clear();
			AssetViewItem tree = this.create_tree((IEnumerable<string>)this._selectedAssetGuid);
			if (this._view == null) {
				if (this._state == null)
					this._state = new TreeViewState();
				Rect position = this.position;
				this._view = new AssetTreeView(this._state, new MultiColumnHeader(AssetTreeView.CreateMultiColumnHeaderState(position.width)) {
					height = EditorGUIUtility.singleLineHeight,
					canSort = false
				});
			}

			if (!string.IsNullOrEmpty(this._filter)) {
				tree.Dsf(new Action<AssetViewItem>(this.purge_filter));
				tree.Dsf(new Action<AssetViewItem>(AssetWindow.purge_empty));
			}

			this._view.assetRoot = tree;
			this._view.CollapseAll();
			this._view.Reload();
		}

		private void draw_tree() {
			if (this._view == null)
				return;
			this._view.filter = this._filter;
			AssetTreeView view = this._view;
			double fixedHeight = (double)this._style.toolbarGUIStyle.fixedHeight;
			Rect position = this.position;
			double width = position.width;
			double num = position.height - this._style.toolbarGUIStyle.fixedHeight;
			Rect rect = new Rect(0.0f, (float)fixedHeight, (float)width, (float)num);
			view.OnGUI(rect);
		}

		private void on_selection_change() {
			if (!this._autoLocate)
				return;
			this.update_selection();
			this.Repaint();
		}

		protected void OnEnable() {
			if (!this._collection.Read("cache/AssetReferenceCollection.bin") || this._collection.error) {
				EditorUtility.DisplayDialog("Refresh", "由于 " + this._collection.errmsg, "OK");
				this._collection.Clear();
				this._collection.allAssets = AssetDatabase.GetAllAssetPaths();
				this._collection.cancellable = false;
				if (this._collection.Refresh())
					this._collection.Write("cache/AssetReferenceCollection.bin");
			}

			this._displayMode = (AssetDisplayMode)PlayerPrefs.GetInt("__ASSET_REF_MODE_KEY__", 0);
			this._displayFilter = (AssetWindow.DisplayFilter)PlayerPrefs.GetInt("__ASSET_REF_FILTER_KEY__", 0);
			this._autoLocate = PlayerPrefs.GetInt("__ASSET_REF_AUTO_LOCATE_KEY__", 1) == 1;
			Selection.selectionChanged -= new Action(this.on_selection_change);
			Selection.selectionChanged += new Action(this.on_selection_change);
			this.update_selection();
		}

		protected void OnDisable() {
			Selection.selectionChanged -= new Action(this.on_selection_change);
			this._collection.Clear();
		}

		protected void OnGUI() {
			if (this._style == null)
				this._style = new AssetWindow.Style();
			EditStyles.Init();
			this.draw_head();
			this.build_trees();
			this.draw_tree();
		}

		private void draw_head() {
			EditorGUILayout.BeginHorizontal(this._style.toolbarGUIStyle, Array.Empty<GUILayoutOption>());
			GUILayout.Space(5f);
			if (GUILayout.Button("刷新", EditStyles.DropDown, new GUILayoutOption[1] {
				    GUILayout.Width(50f)
			    }))
				this.draw_menu();
			AssetDisplayMode assetDisplayMode = (AssetDisplayMode)EditorGUILayout.Popup((int)this._displayMode, SettingEnum.GetContents(typeof(AssetDisplayMode)),
				this._style.popup, new GUILayoutOption[1] {
					GUILayout.Width(60f)
				});
			if (assetDisplayMode != this._displayMode) {
				this._displayMode = assetDisplayMode;
				this.on_mode_change();
			}

			AssetWindow.DisplayFilter displayFilter = (AssetWindow.DisplayFilter)EditorGUILayout.Popup((int)this._displayFilter,
				SettingEnum.GetContents(typeof(AssetWindow.DisplayFilter)), this._style.popup, new GUILayoutOption[1] {
					GUILayout.Width(100f)
				});
			if (displayFilter != this._displayFilter) {
				this._displayFilter = displayFilter;
				this.on_filter_change();
			}

			string str = EditorGUILayout.TextField(this._filter, Array.Empty<GUILayoutOption>());
			// if (GUILayout.Button("", GUIStyle.op_Implicit("ToolbarSeachCancelButton"), Array.Empty<GUILayoutOption>()) && !string.IsNullOrEmpty(str)) {
			// 	str = string.Empty;
			// 	this.Repaint();
			// }
			if (GUILayout.Button("", EditorStyles.toolbarButton, Array.Empty<GUILayoutOption>()) && !string.IsNullOrEmpty(str)) {
				str = string.Empty;
				this.Repaint();
			}
			
			if (str != this._filter) {
				this._valid.Add(InvalidateType.Data);
				this._filter = str;
				this.Repaint();
			}

			GUILayout.Space(5f);
			bool flag = GUILayout.Toggle((this._autoLocate ? 1 : 0) != 0, "自动跟踪", this._style.toolbarButton, new GUILayoutOption[1] {
				GUILayout.Width(100f)
			});
			if (flag != this._autoLocate) {
				this._autoLocate = flag;
				ProjectPrefs.SetInt("__ASSET_REF_AUTO_LOCATE_KEY__", this._autoLocate ? 1 : 0);
			}

			EditorGUILayout.EndHorizontal();
		}

		private void draw_menu() {
			Rect lastRect = GUILayoutUtility.GetLastRect();
			ref Rect local = ref lastRect;
			local.y = local.y + lastRect.height;
			GenericMenu genericMenu = new GenericMenu();
			genericMenu.AddItem(new GUIContent("Refresh"), false, do_refesh);
			genericMenu.AddItem(new GUIContent("Read"), false, do_read);
			genericMenu.DropDown(lastRect);
		}

		private void do_read() {
			if (!EditorUtility.DisplayDialog("读取", "是否重新读取缓存", "Yes", "No"))
				return;
			this._collection.Clear();
			this._collection.Read("cache/AssetReferenceCollection.bin");
			if (!this._collection.error)
				return;
			Debug.LogError((object)this._collection.errmsg);
		}

		private void do_refesh() {
			if (!EditorUtility.DisplayDialog("刷新", "是否刷新缓存,这可能需要几分钟的时间", "Yes", "No"))
				return;
			this._collection.Clear();
			this._collection.cancellable = true;
			this._collection.allAssets = AssetDatabase.GetAllAssetPaths();
			if (!this._collection.Refresh())
				return;
			this._collection.Write("cache/AssetReferenceCollection.bin");
		}

		private void on_mode_change() {
			this._valid.Add(InvalidateType.Data);
			PlayerPrefs.SetInt("__ASSET_REF_MODE_KEY__", (int)this._displayMode);
		}

		private void on_filter_change() {
			this._valid.Add(InvalidateType.Data);
			PlayerPrefs.SetInt("__ASSET_REF_FILTER_KEY__", (int)this._displayFilter);
		}

		private static AssetViewItem make_nothing() {
			AssetViewItem assetViewItem = new AssetViewItem();
			assetViewItem.id = 1;
			assetViewItem.depth = 0;
			assetViewItem.displayName = "__EMPTY_NODE__";
			assetViewItem.data = (AssetInfo)null;
			return assetViewItem;
		}

		private AssetViewItem create_tree(IEnumerable<string> selectedIds) {
			int elementCount = 0;
			AssetViewItem assetViewItem1 = new AssetViewItem();
			assetViewItem1.id = elementCount;
			assetViewItem1.depth = -1;
			assetViewItem1.displayName = "Root";
			assetViewItem1.data = (AssetInfo)null;
			assetViewItem1.mode = this._displayMode;
			AssetViewItem tree = assetViewItem1;
			Stack<AssetInfo> recruisive = new Stack<AssetInfo>();
			foreach (string selectedId in selectedIds) {
				recruisive.Clear();
				AssetViewItem assetViewItem2 = this.make_child(selectedId, ref elementCount, 0, recruisive);
				if (assetViewItem2 != null)
					tree.AddChild((TreeViewItem)assetViewItem2);
			}

			if (!tree.hasChildren)
				tree.AddChild((TreeViewItem)AssetWindow.make_nothing());
			return tree;
		}

		private static string chain(Stack<AssetInfo> stack) {
			StringBuilder stringBuilder = new StringBuilder();
			foreach (AssetInfo assetInfo in stack)
				stringBuilder.AppendLine(assetInfo.name);
			return stringBuilder.ToString();
		}

		private AssetViewItem make_child(
			string guid,
			ref int elementCount,
			int depth,
			Stack<AssetInfo> recruisive) {
			AssetInfo assetByGuid = this._collection.GetAssetByGuid(HashCode.Parse(guid));
			if (assetByGuid == null)
				return (AssetViewItem)null;
			if (recruisive.Contains(assetByGuid)) {
				Debug.LogError((object)(assetByGuid.name + " recrusive\r\n chain" + AssetWindow.chain(recruisive)));
				return (AssetViewItem)null;
			}

			List<string> stringList = this._displayMode == AssetDisplayMode.Dependency ? assetByGuid.dependencies : assetByGuid.references;
			if (depth == 0) {
				switch (this._displayFilter) {
					case AssetWindow.DisplayFilter.All:
						break;
					case AssetWindow.DisplayFilter.ContentOnly:
						if (stringList.Count == 0)
							return (AssetViewItem)null;
						break;
					case AssetWindow.DisplayFilter.NoneOnly:
						if (stringList.Count > 0)
							return (AssetViewItem)null;
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}

			recruisive.Push(assetByGuid);
			++elementCount;
			AssetViewItem assetViewItem1 = new AssetViewItem();
			assetViewItem1.id = elementCount;
			assetViewItem1.displayName = assetByGuid.name;
			assetViewItem1.data = assetByGuid;
			assetViewItem1.depth = depth;
			assetViewItem1.mode = this._displayMode;
			AssetViewItem assetViewItem2 = assetViewItem1;
			if (depth < 10) {
				foreach (string guid1 in stringList) {
					AssetViewItem assetViewItem3 = this.make_child(guid1, ref elementCount, depth + 1, recruisive);
					if (assetViewItem3 != null)
						assetViewItem2.AddChild((TreeViewItem)assetViewItem3);
				}
			}

			recruisive.Pop();
			return assetViewItem2;
		}

		private enum DisplayFilter {
			[Description("显示全部")]
			All,

			[Description("仅显示有内容")]
			ContentOnly,

			[Description("仅显示无内容")]
			NoneOnly,
		}

		private class Style {
			internal GUIStyle toolbarButton = new GUIStyle(EditorStyles.toolbarButton);
			internal GUIStyle toolbarGUIStyle = new GUIStyle(EditorStyles.toolbar);
			internal GUIStyle popup = new GUIStyle(EditorStyles.toolbarPopup);
		}
	}
}