using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using edit.etui.utils;
using edit.gui;
using pure.utils.mathTools;
using pure.utils.task;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using HashCode = pure.utils.mathTools.HashCode;

namespace edit.assets {
	public class AssetWindow : EditorWindow {
		private const string MODE_PREF_KEY = "__ASSET_REF_MODE_KEY__";
		private const string FILTER_PREF_KEY = "__ASSET_REF_FILTER_KEY__";
		private const string AUTO_LOCATE_KEY = "__ASSET_REF_AUTO_LOCATE_KEY__";
		private AssetDisplayMode _displayMode;
		private DisplayFilter _displayFilter;
		private bool _autoLocate;
		private InvalidateCache _valid;
		private string _filter = string.Empty;
		private Style _style;
		private List<string> _selectedAssetGuid = new List<string>();
		private AssetTreeView _view;

		[SerializeField]
		private TreeViewState _state;

		private readonly AssetInfoCollection _collection = new AssetInfoCollection();
		private static string REF_FILE => Path.Combine(Application.persistentDataPath, "AssetReferenceCollection.bin");


		[MenuItem("Assets/杂项功能/查找引用关系")]
		protected static void FindRef() {
			OpenWindow();
			GetWindow<AssetWindow>().update_selection();
		}

		[MenuItem("Tools/资源关系", false, 2500)]
		protected static void OpenWindow() {
			AssetWindow window = GetWindow<AssetWindow>();
			window.wantsMouseMove = false;
			window.titleContent = new GUIContent("资源关系");
			window.Show();
			window.Focus();
		}

		private void update_selection() {
			_selectedAssetGuid.Clear();
			foreach (var _object in Selection.objects) {
				string assetPath = AssetDatabase.GetAssetPath(_object is GameObject gameObject ? EditPrefabUtility.GetPrefab(gameObject) : _object);
				if (Directory.Exists(assetPath)) {
					string[] strArray = new string[1] { assetPath };
					foreach (string asset in AssetDatabase.FindAssets(null, strArray)) {
						if (!_selectedAssetGuid.Contains(asset) && !Directory.Exists(AssetDatabase.GUIDToAssetPath(asset)))
							_selectedAssetGuid.Add(asset);
					}
				}
				else {
					string guid = AssetDatabase.AssetPathToGUID(assetPath);
					if (!string.IsNullOrEmpty(guid))
						_selectedAssetGuid.Add(guid);
				}
			}

			_valid.Add(InvalidateType.Data);
		}

		private void purge_filter(AssetViewItem x) {
			if (x.data == null)
				return;
			if (x.data.path.Contains(_filter))
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
			x.children.RemoveAll(m => !((AssetViewItem)m).ok);
		}

		private void build_trees() {
			if (!_valid.Contains(InvalidateType.Data) || _selectedAssetGuid.Count == 0)
				return;
			_valid.Clear();
			AssetViewItem tree = create_tree(_selectedAssetGuid);
			if (_view == null) {
				if (_state == null)
					_state = new TreeViewState();
				Rect _position = position;
				_view = new AssetTreeView(_state, new MultiColumnHeader(AssetTreeView.CreateMultiColumnHeaderState(_position.width)) {
					height = EditorGUIUtility.singleLineHeight,
					canSort = false
				});
			}

			if (!string.IsNullOrEmpty(_filter)) {
				tree.Dsf(purge_filter);
				tree.Dsf(purge_empty);
			}

			_view.assetRoot = tree;
			_view.CollapseAll();
			_view.Reload();
		}

		private void draw_tree() {
			if (_view == null)
				return;
			_view.filter = _filter;
			AssetTreeView view = _view;
			double fixedHeight = _style.toolbarGUIStyle.fixedHeight;
			Rect position = this.position;
			double width = position.width;
			double num = position.height - _style.toolbarGUIStyle.fixedHeight;
			Rect rect = new Rect(0.0f, (float)fixedHeight, (float)width, (float)num);
			view.OnGUI(rect);
		}

		private void on_selection_change() {
			if (!_autoLocate)
				return;
			update_selection();
			Repaint();
		}

		protected void OnEnable() {
			if (!_collection.Read(REF_FILE) || _collection.error) {
				EditorUtility.DisplayDialog("Refresh", "由于 " + _collection.errmsg, "OK");
				_collection.Clear();
				_collection.allAssets = AssetDatabase.GetAllAssetPaths();
				_collection.cancellable = false;
				if (_collection.Refresh())
					_collection.Write(REF_FILE);
			}

			_displayMode = (AssetDisplayMode)PlayerPrefs.GetInt("__ASSET_REF_MODE_KEY__", 0);
			_displayFilter = (DisplayFilter)PlayerPrefs.GetInt("__ASSET_REF_FILTER_KEY__", 0);
			_autoLocate = PlayerPrefs.GetInt("__ASSET_REF_AUTO_LOCATE_KEY__", 1) == 1;
			Selection.selectionChanged -= on_selection_change;
			Selection.selectionChanged += on_selection_change;
			update_selection();
		}

		protected void OnDisable() {
			Selection.selectionChanged -= on_selection_change;
			_collection.Clear();
		}

		protected void OnGUI() {
			if (_style == null)
				_style = new Style();
			EditStyles.Init();
			draw_head();
			build_trees();
			draw_tree();
		}

		private void draw_head() {
			EditorGUILayout.BeginHorizontal(_style.toolbarGUIStyle, Array.Empty<GUILayoutOption>());
			GUILayout.Space(5f);
			if (GUILayout.Button("刷新", EditStyles.DropDown, GUILayout.Width(50f)))
				draw_menu();
			AssetDisplayMode assetDisplayMode = (AssetDisplayMode)EditorGUILayout.Popup((int)_displayMode, SettingEnum.GetContents(typeof(AssetDisplayMode)),
				_style.popup, GUILayout.Width(60f));
			if (assetDisplayMode != _displayMode) {
				_displayMode = assetDisplayMode;
				on_mode_change();
			}

			DisplayFilter displayFilter = (DisplayFilter)EditorGUILayout.Popup((int)_displayFilter,
				SettingEnum.GetContents(typeof(DisplayFilter)), _style.popup, GUILayout.Width(100f));
			if (displayFilter != _displayFilter) {
				_displayFilter = displayFilter;
				on_filter_change();
			}

			string str = EditorGUILayout.TextField(_filter, Array.Empty<GUILayoutOption>());
			if (GUILayout.Button("", "ToolbarSeachCancelButton", Array.Empty<GUILayoutOption>()) && !string.IsNullOrEmpty(str)) {
				str = string.Empty;
				Repaint();
			}

			if (str != _filter) {
				_valid.Add(InvalidateType.Data);
				_filter = str;
				Repaint();
			}

			GUILayout.Space(5f);
			bool flag = GUILayout.Toggle((_autoLocate ? 1 : 0) != 0, "自动跟踪", _style.toolbarButton, GUILayout.Width(100f));
			if (flag != _autoLocate) {
				_autoLocate = flag;
				ProjectPrefs.SetInt("__ASSET_REF_AUTO_LOCATE_KEY__", _autoLocate ? 1 : 0);
			}

			EditorGUILayout.EndHorizontal();
		}

		private void draw_menu() {
			Rect lastRect = GUILayoutUtility.GetLastRect();
			ref Rect local = ref lastRect;
			local.y += lastRect.height;
			GenericMenu genericMenu = new GenericMenu();
			genericMenu.AddItem(new GUIContent("Refresh"), false, do_refesh);
			genericMenu.AddItem(new GUIContent("Read"), false, do_read);
			genericMenu.DropDown(lastRect);
		}

		private void do_read() {
			if (!EditorUtility.DisplayDialog("读取", "是否重新读取缓存", "Yes", "No"))
				return;
			_collection.Clear();
			_collection.Read(REF_FILE);
			if (!_collection.error)
				return;
			Debug.LogError(_collection.errmsg);
		}

		private void do_refesh() {
			if (!EditorUtility.DisplayDialog("刷新", "是否刷新缓存,这可能需要几分钟的时间", "Yes", "No"))
				return;
			_collection.Clear();
			_collection.cancellable = true;
			_collection.allAssets = AssetDatabase.GetAllAssetPaths();
			if (!_collection.Refresh())
				return;
			_collection.Write(REF_FILE);
		}

		private void on_mode_change() {
			_valid.Add(InvalidateType.Data);
			PlayerPrefs.SetInt("__ASSET_REF_MODE_KEY__", (int)_displayMode);
		}

		private void on_filter_change() {
			_valid.Add(InvalidateType.Data);
			PlayerPrefs.SetInt("__ASSET_REF_FILTER_KEY__", (int)_displayFilter);
		}

		private static AssetViewItem make_nothing() {
			AssetViewItem assetViewItem = new AssetViewItem {
				id = 1,
				depth = 0,
				displayName = "__EMPTY_NODE__",
				data = null
			};
			return assetViewItem;
		}

		private AssetViewItem create_tree(IEnumerable<string> selectedIds) {
			int elementCount = 0;
			AssetViewItem assetViewItem1 = new AssetViewItem {
				id = elementCount,
				depth = -1,
				displayName = "Root",
				data = null,
				mode = _displayMode
			};
			AssetViewItem tree = assetViewItem1;
			Stack<AssetInfo> recruisive = new Stack<AssetInfo>();
			foreach (string selectedId in selectedIds) {
				recruisive.Clear();
				AssetViewItem assetViewItem2 = make_child(selectedId, ref elementCount, 0, recruisive);
				if (assetViewItem2 != null)
					tree.AddChild(assetViewItem2);
			}

			if (!tree.hasChildren)
				tree.AddChild(make_nothing());
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
			AssetInfo assetByGuid = _collection.GetAssetByGuid(HashCode.Parse(guid));
			if (assetByGuid == null)
				return null;
			if (recruisive.Contains(assetByGuid)) {
				Debug.LogError(assetByGuid.name + " recrusive\r\n chain" + chain(recruisive));
				return null;
			}

			List<string> stringList = _displayMode == AssetDisplayMode.Dependency ? assetByGuid.dependencies : assetByGuid.references;
			if (depth == 0) {
				switch (_displayFilter) {
					case DisplayFilter.All:
						break;
					case DisplayFilter.ContentOnly:
						if (stringList.Count == 0)
							return null;
						break;
					case DisplayFilter.NoneOnly:
						if (stringList.Count > 0)
							return null;
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
			assetViewItem1.mode = _displayMode;
			AssetViewItem assetViewItem2 = assetViewItem1;
			if (depth < 10) {
				foreach (string guid1 in stringList) {
					AssetViewItem assetViewItem3 = make_child(guid1, ref elementCount, depth + 1, recruisive);
					if (assetViewItem3 != null)
						assetViewItem2.AddChild(assetViewItem3);
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
			internal GUIStyle toolbarButton = new GUIStyle((GUIStyle)"ToolbarButton");
			internal GUIStyle toolbarGUIStyle = new GUIStyle((GUIStyle)"Toolbar");
			internal GUIStyle popup = new GUIStyle((GUIStyle)"ToolbarPopup");
		}
	}
}