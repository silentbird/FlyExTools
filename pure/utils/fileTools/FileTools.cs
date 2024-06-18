using System;
using System.IO;
using UnityEngine;

namespace pure.utils.fileTools {
	public static class FileTools {
		public const string Platform = "Android";
		public static string dataPath = Application.dataPath;
		private static bool started;

		private static readonly byte[] bom = new byte[3] {
			(byte)239,
			(byte)187,
			(byte)191
		};

		public static string wwwStreamingAssetsPath { get; private set; }

		public static string streamAssetAssetsPath { get; private set; }

		public static string persistentPath { get; private set; }

		public static string persistentRuntimePath { get; private set; }

		public static string persistentCachePath { get; private set; }

		public static string wwwPersistentCachePath { get; private set; }

		public static void Start() {
			if (FileTools.started)
				return;
			FileTools.started = true;
			FileTools.preset_path("file:///", "file:///");
		}

		private static void preset_path(string streamWWW, string persistentWWW) {
			FileTools.dataPath = Application.dataPath;
			FileTools.streamAssetAssetsPath = Application.streamingAssetsPath;
			FileTools.wwwStreamingAssetsPath = streamWWW + Application.streamingAssetsPath;
			FileTools.persistentPath = Application.persistentDataPath;
			FileTools.persistentCachePath = FileTools.persistentPath + "/cache";
			FileTools.persistentRuntimePath = FileTools.persistentPath + "/runTime";
			FileTools.wwwPersistentCachePath = persistentWWW + FileTools.persistentCachePath;
		}

		public static string FileSize(this long size) {
			if (size < 1024L)
				return string.Format("{0} B", (object)size);
			return size < 10485760L ? string.Format("{0:##.000}K", (object)((double)size / 1024.0)) : string.Format("{0:##.000}M", (object)((double)size / 1048576.0));
		}

		public static string GetPath(string path) {
			path = path.Replace("\\", "/");
			path = path.Replace(FileTools.dataPath, "Assets");
			path = string.Intern(path);
			return path;
		}

		public static string GetFolder(string file) {
			if (string.IsNullOrEmpty(file))
				return file;
			file = FileTools.GetPath(file);
			string str = Path.GetDirectoryName(file);
			if (string.IsNullOrEmpty(str))
				return str;
			if (str.EndsWith("/"))
				str = str.Substring(0, str.Length - 1);
			return string.Intern(str);
		}

		public static string GetFileName(string path) {
			string withoutExtension = Path.GetFileNameWithoutExtension(path);
			return !string.IsNullOrEmpty(withoutExtension) ? string.Intern(withoutExtension) : withoutExtension;
		}

		public static string GetExtension(string path) {
			string extension = Path.GetExtension(path);
			return !string.IsNullOrEmpty(extension) ? string.Intern(extension.Substring(1, extension.Length - 1)) : extension;
		}

		public static bool ReadFile(string file, out byte[] buffer) {
			if (!File.Exists(file)) {
				buffer = (byte[])null;
				return false;
			}

			buffer = File.ReadAllBytes(file);
			return true;
		}

		public static byte[] RemoveBom(byte[] ba) {
			if ((int)ba[0] == (int)FileTools.bom[0] && (int)ba[1] == (int)FileTools.bom[1] && (int)ba[2] == (int)FileTools.bom[2]) {
				byte[] dst = new byte[ba.Length - 3];
				Buffer.BlockCopy((Array)ba, 3, (Array)dst, 0, dst.Length);
				ba = dst;
			}

			return ba;
		}

		private static void ensure_directory(string file) {
			string directoryName = Path.GetDirectoryName(file);
			if (string.IsNullOrEmpty(directoryName) || Directory.Exists(directoryName))
				return;
			Directory.CreateDirectory(directoryName);
		}

		public static void WriteFile(string file, string content) {
			FileTools.ensure_directory(file);
			File.WriteAllText(file, content);
		}

		public static void WriteFile(string file, byte[] buffer) {
			FileTools.ensure_directory(file);
			File.WriteAllBytes(file, buffer);
		}
	}
}