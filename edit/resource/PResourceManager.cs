using pure.utils.task;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace edit.resource {
	public static class PResourceManager {
		private static string m_resource_path = "";
		private static List<MemoryTexture> loadedTextures = new List<MemoryTexture>();
		private static double releaseTime;

		public static void SetDefaultResourcePath(string defaultResourcePath) {
			m_resource_path = defaultResourcePath;
		}

		static PResourceManager() {
			SetDefaultResourcePath("Assets/Script/Csharp/Editor/res/");
			EditorPlayMode.OnPlayModeChanged += (Action)(() => loadedTextures.Clear());
			// SceneManager.activeSceneChanged += new UnityAction<Scene, Scene>((object)\u003C\u003Ec.\u003C\u003E9, __methodptr(\u003C\u002Ecctor\u003Eb__2_1));
		}

		public static string PreparePath(string path) {
			path = path.Replace(Application.dataPath, "Assets");
			if (!path.StartsWith("Assets/"))
				path = m_resource_path + path;
			return path;
		}

		public static T[] LoadResources<T>(string path) where T : UnityEngine.Object {
			path = PreparePath(path);
			return AssetDatabase.LoadAllAssetsAtPath(path).OfType<T>().ToArray<T>();
		}

		public static T LoadResource<T>(string path) where T : UnityEngine.Object {
			path = PreparePath(path);
			return AssetDatabase.LoadAssetAtPath<T>(path);
		}

		public static Texture2D LoadTexture(string texPath) {
			if (string.IsNullOrEmpty(texPath))
				return (Texture2D)null;
			int index = loadedTextures.FindIndex((Predicate<MemoryTexture>)(memTex => memTex.path == texPath));
			if (index != -1) {
				if (loadedTextures[index].texture != null)
					return loadedTextures[index].texture;
				loadedTextures.RemoveAt(index);
			}

			Texture2D texture = LoadResource<Texture2D>(texPath);
			if (!texture) {
				Debug.LogError((object)string.Format("no texture at {0}", (object)texPath));
				return (Texture2D)null;
			}

			AddTextureToMemory(texPath, texture);
			return texture;
		}

		public static Texture2D GetTintedTexture(string texPath, Color col, float percent = 1f) {
			string str = "Tint:" + col.ToString() + ":" + percent.ToString();
			Texture2D texture = GetTexture(texPath, str);
			// if (Object.op_Equality((Object)texture, (Object)null)) {
			// 	texture = PGUIUtility.Tint(LoadTexture(texPath), col, percent);
			// 	AddTextureToMemory(texPath, texture, str);
			// }

			return texture;
		}

		// public static Texture2D GetTintedTexture(string texPath, string colorname) {
		// 	return GetTintedTexture(texPath, PGUIUtility.ToColor(colorname));
		// }
		//
		// public static Texture2D GetInvertTexture(string texPath) {
		// 	Texture2D texture = GetTexture(texPath, "Invert");
		// 	if (Object.op_Equality((Object)texture, (Object)null)) {
		// 		Texture2D texture2D = LoadTexture(texPath);
		// 		AddTextureToMemory(texPath, texture2D);
		// 		texture = PGUIUtility.Invert(texture2D);
		// 		AddTextureToMemory(texPath, texture, "Invert");
		// 	}
		//
		// 	return texture;
		// }

		public static void AddTextureToMemory(
			string texturePath,
			Texture2D texture,
			params string[] modifications) {
			if (texture == null)
				return;
			loadedTextures.Add(new MemoryTexture(texturePath, texture, modifications));
		}

		public static MemoryTexture FindInMemory(Texture2D tex) {
			int index = loadedTextures.FindIndex((Predicate<MemoryTexture>)(memTex => memTex.texture == tex));
			return index == -1 ? (MemoryTexture)null : loadedTextures[index];
		}

		public static bool HasInMemory(string texturePath, params string[] modifications) {
			int index = loadedTextures.FindIndex((Predicate<MemoryTexture>)(memTex => memTex.path == texturePath));
			return index != -1 && EqualModifications(loadedTextures[index].modifications, modifications);
		}

		private static void RemoveLostTextures() {
			double timeSinceStartup = EditorApplication.timeSinceStartup;
			if (timeSinceStartup - releaseTime < 30.0)
				return;
			releaseTime = timeSinceStartup;
			int count = loadedTextures.Count;
			while (--count >= 0) {
				if (!loadedTextures[count].texture)
					loadedTextures.RemoveAt(count);
			}
		}

		public static MemoryTexture GetMemoryTexture(
			string texturePath,
			params string[] modifications) {
			RemoveLostTextures();
			MemoryTexture memoryTexture = (MemoryTexture)null;
			for (int index = 0; index < loadedTextures.Count; ++index) {
				MemoryTexture loadedTexture = loadedTextures[index];
				if (loadedTexture.path == texturePath && EqualModifications(loadedTexture.modifications, modifications)) {
					memoryTexture = loadedTexture;
					break;
				}
			}

			return memoryTexture;
		}

		public static Texture2D GetTexture(string texturePath, params string[] modifications) {
			return GetMemoryTexture(texturePath, modifications)?.texture;
		}

		private static bool EqualModifications(string[] modsA, string[] modsB) {
			return modsA.Length == modsB.Length && Array.TrueForAll<string>(modsA,
				(Predicate<string>)(mod =>
					((IEnumerable<string>)modsB).Count<string>((Func<string, bool>)(oMod => mod == oMod)) ==
					((IEnumerable<string>)modsA).Count<string>((Func<string, bool>)(oMod => mod == oMod))));
		}

		public class MemoryTexture {
			public string path;
			public Texture2D texture;
			public string[] modifications;

			public MemoryTexture(string texPath, Texture2D tex, params string[] mods) {
				this.path = texPath;
				this.texture = tex;
				this.modifications = mods;
			}

			public override string ToString() {
				return this.path + " [" + string.Join(",", this.modifications) + "]";
			}
		}
	}
}