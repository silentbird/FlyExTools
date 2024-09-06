using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using pure.database;
using pure.refactor.serialize;
using pure.utils.fileTools;
using pure.utils.mathTools;
using pure.utils.memory;
using UnityEditor;
using UnityEngine;
using HashCode = pure.utils.mathTools.HashCode;

namespace edit.assets {
	public class AssetInfoCollection {
		private readonly Dictionary<HashCode, AssetInfo> _guids = new Dictionary<HashCode, AssetInfo>();
		private readonly Dictionary<string, AssetInfo> _paths = new Dictionary<string, AssetInfo>();
		private readonly AssetNameIndexer _names = new AssetNameIndexer();
		public string[] allAssets;
		public bool cancellable;
		private bool _done;

		private static bool is_cachable(string path) {
			return !string.IsNullOrEmpty(path) && path.StartsWith("Assets/") && !path.StartsWith("Assets/StreamingAssets") && !Path.GetFileName(path).StartsWith("~");
		}

		public bool error => !string.IsNullOrEmpty(errmsg);

		public string errmsg { get; private set; } = string.Empty;

		public void Clear() {
			_guids.Clear();
			_paths.Clear();
			_names.Clear();
			_done = false;
			errmsg = string.Empty;
		}

		public IEnumerator<KeyValuePair<HashCode, AssetInfo>> GetGuids() {
			return _guids.GetEnumerator();
		}

		public void CollectBundle(string bn, HashSet<AssetInfo> to) {
			foreach (KeyValuePair<HashCode, AssetInfo> guid in _guids) {
				if (guid.Value.bundleName == bn)
					to.Add(guid.Value);
			}
		}

		public void CollectBundleAsset(string bn, Dictionary<HashCode, AssetInfo> to) {
			foreach (KeyValuePair<HashCode, AssetInfo> guid in _guids) {
				if (guid.Value.bundleName == bn)
					to.Add(guid.Key, guid.Value);
			}
		}

		public AssetInfo GetAssetByGuid(HashCode guid) {
			AssetInfo assetInfo;
			return !_guids.TryGetValue(guid, out assetInfo) ? null : assetInfo;
		}

		public bool TryGetPathByName(string name, out string[] path) {
			return _names.TryGetPathByName(name, out path);
		}

		public bool TryGet(HashCode h, out AssetInfo ad) => _guids.TryGetValue(h, out ad);

		public bool Refresh() {
			try {
				Clear();
				int length = allAssets.Length;
				for (int index = 0; index < allAssets.Length; ++index) {
					if (index % 100 == 0 && display_progress(string.Format("Refresh {0}/{1}", index, length),
						    string.Format("collecting {0} assets", index), index / (float)length)) {
						EditorUtility.ClearProgressBar();
						return false;
					}

					string allAsset = allAssets[index];
					if (File.Exists(allAsset) && is_cachable(allAsset)) {
						import_asset(allAssets[index]);
						if (index % 2000 == 0)
							GC.Collect();
					}
				}

				EditorUtility.DisplayCancelableProgressBar(nameof(Refresh), "Generating asset reference info", 1f);
				update_nestinfo();
				EditorUtility.DisplayCancelableProgressBar(nameof(Refresh), "Write to cache", 1f);
				EditorUtility.ClearProgressBar();
				Debug.Log($"total cache: {_guids.Count}, {_names.Count}");
			}
			catch (Exception ex) {
				errmsg = ex.ToString();
				Debug.LogError(ex);
				return false;
			}
			finally {
				EditorUtility.ClearProgressBar();
			}

			return true;
		}

		private bool display_progress(string title, string message, float percent) {
			if (!cancellable) {
				EditorUtility.DisplayProgressBar(title, message, percent);
				return false;
			}

			if (!EditorUtility.DisplayCancelableProgressBar(title, message, percent))
				return false;
			EditorUtility.ClearProgressBar();
			return true;
		}

		private void update_nestinfo() {
			foreach (KeyValuePair<HashCode, AssetInfo> guid in _guids)
				guid.Value.references.Clear();
			foreach (KeyValuePair<HashCode, AssetInfo> guid in _guids) {
				HashCode key = guid.Key;
				foreach (string dependency in guid.Value.dependencies) {
					AssetInfo assetInfo;
					if (_guids.TryGetValue(HashCode.Parse(dependency), out assetInfo))
						assetInfo.references.Add(key.ToString());
				}
			}
		}

		private void import_asset(string assetPath) {
			string guid = AssetDatabase.AssetPathToGUID(assetPath);
			List<string> list = AssetDatabase.GetDependencies(assetPath, false)
				.Select(AssetDatabase.AssetPathToGUID).ToList();
			if (assetPath.ContainsExtra()) {
				using (RecycleSetVO<string> recycleSetVo = new RecycleSetVO<string>()) {
					assetPath.CollectDepencies(recycleSetVo.hashSet);
					list.AddRange(recycleSetVo.hashSet);
				}
			}

			string assetBundleName = AssetImporter.GetAtPath(assetPath).assetBundleName;
			AssetInfo asset = new AssetInfo {
				guid = guid,
				name = Path.GetFileNameWithoutExtension(assetPath),
				path = assetPath,
				assetDependencyHash = AssetDatabase.GetAssetDependencyHash(assetPath).ToString(),
				dependencies = list,
				bundleName = assetBundleName
			};
			HashCode key = HashCode.Parse(guid);
			if (!_guids.ContainsKey(key))
				_guids[key] = asset;
			else
				_guids.Add(key, asset);
			if (!_paths.ContainsKey(assetPath))
				_paths[assetPath] = asset;
			else
				_paths.Add(assetPath, asset);
			_names.Add(asset);
		}

		public bool Read(string file) {
			if (_done)
				return true;
			Clear();
			byte[] buffer;
			if (!FileTools.ReadFile(file, out buffer)) {
				errmsg = file + " not exist";
				return false;
			}

			using (ByteReader byteReader1 = new ByteReader(buffer)) {
				if (byteReader1.ReadUint() != 2874143930U) {
					errmsg = "bad file struct";
					return false;
				}

				int _ = byteReader1.ReadInt();
				using (RecycleListVO<string> recycleListVo = new RecycleListVO<string>()) {
					using (IByteReader byteReader2 = byteReader1.ReadChild()) {
						while (byteReader2.byteAvailable > 0)
							recycleListVo.list.Add(byteReader2.ReadString());
					}

					using (IByteReader ba = byteReader1.ReadChild()) {
						while (ba.byteAvailable > 0) {
							AssetInfo asset = new AssetInfo();
							asset.Read(ba, recycleListVo.list, _);
							HashCode key = HashCode.Parse(asset.guid);
							AssetInfo assetInfo;
							if (!_guids.TryGetValue(key, out assetInfo))
								_guids.Add(key, asset);
							else
								Debug.LogError("guid " + asset.guid + " confict " + asset.path + " = " + assetInfo.path);
							if (!_paths.TryGetValue(asset.path, out assetInfo)) {
								_paths.Add(asset.path, asset);
								_names.Add(asset);
							}
							else {
								string guid = AssetDatabase.AssetPathToGUID(asset.path);
								if (!(assetInfo.guid == guid)) {
									if (asset.guid == guid) {
										_guids.Remove(HashCode.Parse(assetInfo.guid));
										_guids.Add(key, asset);
										_paths[asset.path] = asset;
									}

									Debug.LogError("path " + asset.path + " confilict \r\nself: " + asset.guid + "\r\nprev: " + assetInfo.guid + "\r\nright:" + guid);
								}
							}
						}
					}
				}
			}

			_done = true;
			return true;
		}

		public void Write(string file) {
			using (ValuePool<string> stringPool = new ValuePool<string>()) {
				ByteWriter ba = new ByteWriter();
				foreach (KeyValuePair<HashCode, AssetInfo> guid in _guids)
					guid.Value.WriteTo(ba, stringPool);
				ByteWriter byteWriter1 = new ByteWriter();
				for (int idx = 0; idx < stringPool.Count; ++idx)
					byteWriter1.WriteString(stringPool[idx]);
				ByteWriter byteWriter2 = new ByteWriter();
				byteWriter2.WriteUint(2874143930U);
				byteWriter2.WriteInt(1);
				byteWriter2.WriteChild(byteWriter1);
				byteWriter2.WriteChild(ba);
				FileTools.WriteFile(file, byteWriter2.ToBuffer());
				ba.Dispose();
				byteWriter1.Dispose();
				byteWriter2.Dispose();
			}
		}
	}
}