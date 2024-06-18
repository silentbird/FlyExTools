using UnityEditor;
using UnityEngine;

namespace edit.gui {
	public static class ProjectPrefs {
		public static void SetString(string key, string val) => PlayerPrefs.SetString(key, val);

		public static string GetString(string key, string defaultValue = "") {
			return PlayerPrefs.GetString(key, defaultValue);
		}

		public static void SetInt(string key, int val) => PlayerPrefs.SetInt(key, val);

		public static int GetInt(string key, int defaultValue = 0) {
			return PlayerPrefs.GetInt(key, defaultValue);
		}

		public static void SetFloat(string key, float defaultValue) {
			PlayerPrefs.SetFloat(key, defaultValue);
		}

		public static float GetFloat(string key, float defaultValue) {
			return PlayerPrefs.GetFloat(key, defaultValue);
		}

		public static void SetBool(string key, bool value) => PlayerPrefs.SetInt(key, value ? 1 : 0);

		public static bool GetBool(string key, bool defaultValue = false) {
			return PlayerPrefs.GetInt(key, defaultValue ? 1 : 0) == 1;
		}

		public static void SetWorkString(string key, string val) => EditorPrefs.SetString(key, val);

		public static string GetWorkString(string key, string defaultValue = "") {
			return EditorPrefs.GetString(key, defaultValue);
		}

		public static void SetWorkInt(string key, int val) => EditorPrefs.SetInt(key, val);

		public static int GetWorkInt(string key, int defaultValue = 0) {
			return EditorPrefs.GetInt(key, defaultValue);
		}

		public static void SetWorkFloat(string key, float defaultValue) {
			EditorPrefs.SetFloat(key, defaultValue);
		}

		public static float GetWorkFloat(string key, float defaultValue) {
			return EditorPrefs.GetFloat(key, defaultValue);
		}

		public static void SetWorkBool(string key, bool value) {
			EditorPrefs.SetInt(key, value ? 0 : 1);
		}

		public static bool GetWorkBool(string key, bool defaultValue = false) {
			return EditorPrefs.GetInt(key, defaultValue ? 1 : 0) == 1;
		}
	}
}