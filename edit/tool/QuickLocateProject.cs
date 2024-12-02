using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace FlyExTools.edit.tool {
	[InitializeOnLoad]
	public class QuickLocateProject {
		static QuickLocateProject() {
			EditorApplication.projectWindowItemOnGUI += ProjectWindowItemOnGUI;
		}

		private static string firstGuid;
		private static string searchText = "";
		private static float lastClickTime;

		private static void ProjectWindowItemOnGUI(string guid, Rect selectionRect) {
			//only first one execute
			if (string.IsNullOrEmpty(firstGuid)) {
				firstGuid = guid;
			}
			else if (firstGuid != guid) {
				return;
			}

			if (Event.current.type == EventType.KeyDown && Event.current.keyCode >= KeyCode.A && Event.current.keyCode <= KeyCode.Z) {
				if (isRenaming()) {
					return;
				}

				if (Time.realtimeSinceStartup - lastClickTime > .5f) {
					searchText = "";
				}

				lastClickTime = Time.realtimeSinceStartup;
				searchText += Event.current.keyCode;
				string currentPath = GetCurrentDirectory();
				string[] searchPaths = string.IsNullOrEmpty(currentPath) ? new[] { "Assets" } : new[] { currentPath };
				//只搜索当前目录，不搜索子目录
				string[] guids = AssetDatabase.FindAssets(searchText, searchPaths);

				// 过滤出匹配的文件
				string[] matchingGuids = guids
					.Where(guid => {
						var path = System.IO.Path.GetDirectoryName(AssetDatabase.GUIDToAssetPath(guid))?.Replace("\\", "/");
						return path == currentPath;
					})
					.ToArray();
				if (matchingGuids.Length > 0) {
					Selection.activeObject = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(matchingGuids[0]), typeof(Object));
					//ping the object
					EditorGUIUtility.PingObject(Selection.activeObject);
				}
			}
		}


		public static string GetCurrentDirectory() {
			var projectBrowser = typeof(EditorWindow).Assembly.GetType("UnityEditor.ProjectBrowser");
			var projectBrowserInstance = EditorWindow.focusedWindow;
			var searchFilterField = projectBrowser.GetField("m_SearchFilter", BindingFlags.Instance | BindingFlags.NonPublic);
			var m_SearchFilter = searchFilterField.GetValue(projectBrowserInstance);
			//m_SearchFilter中取到public的folders
			var foldersField = m_SearchFilter.GetType().GetField("m_Folders", BindingFlags.Instance | BindingFlags.NonPublic);
			var folders = foldersField.GetValue(m_SearchFilter) as string[];

			return folders.FirstOrDefault();
		}

		public static bool isRenaming() {
			var projectBrowser = typeof(EditorWindow).Assembly.GetType("UnityEditor.ProjectBrowser");
			var projectBrowserInstance = EditorWindow.focusedWindow;
			var listAreaStateField = projectBrowser.GetField("m_ListAreaState", BindingFlags.Instance | BindingFlags.NonPublic);
			var listAreaState = listAreaStateField.GetValue(projectBrowserInstance);
			//m_SearchFilter中取到public的folders
			var renameOverlayField = listAreaState.GetType().GetField("m_RenameOverlay", BindingFlags.Instance | BindingFlags.Public);
			var renameOverlay = renameOverlayField.GetValue(listAreaState);
			//m_SearchFilter中取到public的folders
			var isRenamingFilenameField = renameOverlay.GetType().GetField("m_IsRenaming", BindingFlags.Instance | BindingFlags.NonPublic);
			var isRenamine = (bool)isRenamingFilenameField.GetValue(renameOverlay);
			return isRenamine;
		}
	}
}