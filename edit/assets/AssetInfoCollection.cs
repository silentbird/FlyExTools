// Decompiled with JetBrains decompiler
// Type: edit.assets.AssetInfoCollection
// Assembly: edit, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3ADECF35-F68B-48ED-B268-19248EA3422D
// Assembly location: D:\W\UnityProj\PluginsProj\Assets\Plugins\Win\csharp\edit.dll

using pure.database;
using pure.refactor.serialize;
using pure.utils.fileTools;
using pure.utils.mathTools;
using pure.utils.memory;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;


namespace edit.assets {
	public class AssetInfoCollection {
		public const string REF_FILE = "cache/AssetReferenceCollection.bin";
		private readonly Dictionary<HashCode, AssetInfo> _guids = new Dictionary<HashCode, AssetInfo>();
		private readonly Dictionary<string, AssetInfo> _paths = new Dictionary<string, AssetInfo>();
		private readonly AssetNameIndexer _names = new AssetNameIndexer();
		public string[] allAssets;
		public bool cancellable;
		private bool _done;

		private static bool is_cachable(string path) {
			return !string.IsNullOrEmpty(path) && path.StartsWith("Assets/") && !path.StartsWith("Assets/StreamingAssets") && !Path.GetFileName(path).StartsWith("~");
		}

		public bool error => !string.IsNullOrEmpty(this.errmsg);

		public string errmsg { get; private set; } = string.Empty;

		public void Clear() {
			this._guids.Clear();
			this._paths.Clear();
			this._names.Clear();
			this._done = false;
			this.errmsg = string.Empty;
		}

		public IEnumerator<KeyValuePair<HashCode, AssetInfo>> GetGuids() {
			return (IEnumerator<KeyValuePair<HashCode, AssetInfo>>)this._guids.GetEnumerator();
		}

		public void CollectBundle(string bn, HashSet<AssetInfo> to) {
			foreach (KeyValuePair<HashCode, AssetInfo> guid in this._guids) {
				if (guid.Value.bundleName == bn)
					to.Add(guid.Value);
			}
		}

		public void CollectBundleAsset(string bn, Dictionary<HashCode, AssetInfo> to) {
			foreach (KeyValuePair<HashCode, AssetInfo> guid in this._guids) {
				if (guid.Value.bundleName == bn)
					to.Add(guid.Key, guid.Value);
			}
		}

		public AssetInfo GetAssetByGuid(HashCode guid) {
			AssetInfo assetInfo;
			return !this._guids.TryGetValue(guid, out assetInfo) ? (AssetInfo)null : assetInfo;
		}

		public bool TryGetPathByName(string name, out string[] path) {
			return this._names.TryGetPathByName(name, out path);
		}

		public bool TryGet(HashCode h, out AssetInfo ad) => this._guids.TryGetValue(h, out ad);

		public bool Refresh() {
			try {
				this.Clear();
				int length = this.allAssets.Length;
				for (int index = 0; index < this.allAssets.Length; ++index) {
					if (index % 100 == 0 && this.display_progress(string.Format("Refresh {0}/{1}", (object)index, (object)length),
						    string.Format("collecting {0} assets", (object)index), (float)index / (float)length)) {
						EditorUtility.ClearProgressBar();
						return false;
					}

					string allAsset = this.allAssets[index];
					if (File.Exists(allAsset) && AssetInfoCollection.is_cachable(allAsset)) {
						this.import_asset(this.allAssets[index]);
						if (index % 2000 == 0)
							GC.Collect();
					}
				}

				EditorUtility.DisplayCancelableProgressBar(nameof(Refresh), "Generating asset reference info", 1f);
				this.update_nestinfo();
				EditorUtility.DisplayCancelableProgressBar(nameof(Refresh), "Write to cache", 1f);
				EditorUtility.ClearProgressBar();
				Debug.Log((object)string.Format("total cache: {0}, {1}", (object)this._guids.Count, (object)this._names.Count));
			}
			catch (Exception ex) {
				this.errmsg = ex.ToString();
				Debug.LogError((object)ex);
				return false;
			}
			finally {
				EditorUtility.ClearProgressBar();
			}

			return true;
		}

		private bool display_progress(string title, string message, float percent) {
			if (!this.cancellable) {
				EditorUtility.DisplayProgressBar(title, message, percent);
				return false;
			}

			if (!EditorUtility.DisplayCancelableProgressBar(title, message, percent))
				return false;
			EditorUtility.ClearProgressBar();
			return true;
		}

		private void update_nestinfo() {
			foreach (KeyValuePair<HashCode, AssetInfo> guid in this._guids)
				guid.Value.references.Clear();
			foreach (KeyValuePair<HashCode, AssetInfo> guid in this._guids) {
				HashCode key = guid.Key;
				foreach (string dependency in guid.Value.dependencies) {
					AssetInfo assetInfo;
					if (this._guids.TryGetValue(HashCode.Parse(dependency), out assetInfo))
						assetInfo.references.Add(key.ToString());
				}
			}
		}

		private void import_asset(string assetPath) {
			string guid = AssetDatabase.AssetPathToGUID(assetPath);
			List<string> list = ((IEnumerable<string>)AssetDatabase.GetDependencies(assetPath, false))
				.Select<string, string>(new Func<string, string>(AssetDatabase.AssetPathToGUID)).ToList<string>();
			if (assetPath.ContainsExtra()) {
				using (RecycleSetVO<string> recycleSetVo = new RecycleSetVO<string>()) {
					assetPath.CollectDepencies((ICollection<string>)recycleSetVo.hashSet);
					list.AddRange((IEnumerable<string>)recycleSetVo.hashSet);
				}
			}

			string assetBundleName = AssetImporter.GetAtPath(assetPath).assetBundleName;
			AssetInfo asset = new AssetInfo() {
				guid = guid,
				name = Path.GetFileNameWithoutExtension(assetPath),
				path = assetPath,
				assetDependencyHash = AssetDatabase.GetAssetDependencyHash(assetPath).ToString(),
				dependencies = list,
				bundleName = assetBundleName
			};
			HashCode key = HashCode.Parse(guid);
			if (this._guids.ContainsKey(key))
				this._guids[key] = asset;
			else
				this._guids.Add(key, asset);
			if (this._paths.ContainsKey(assetPath))
				this._paths[assetPath] = asset;
			else
				this._paths.Add(assetPath, asset);
			this._names.Add(asset);
		}

		public bool Read(string file) {
			if (this._done)
				return true;
			this.Clear();
			byte[] buffer;
			if (!FileTools.ReadFile(file, out buffer)) {
				this.errmsg = file + " not exist";
				return false;
			}

			using (ByteReader byteReader1 = new ByteReader(buffer)) {
				if (byteReader1.ReadUint() != 2874143930U) {
					this.errmsg = "bad file struct";
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
							asset.Read(ba, (IList<string>)recycleListVo.list, _);
							HashCode key = HashCode.Parse(asset.guid);
							AssetInfo assetInfo;
							if (!this._guids.TryGetValue(key, out assetInfo))
								this._guids.Add(key, asset);
							else
								Debug.LogError((object)("guid " + asset.guid + " confict " + asset.path + " = " + assetInfo.path));
							if (!this._paths.TryGetValue(asset.path, out assetInfo)) {
								this._paths.Add(asset.path, asset);
								this._names.Add(asset);
							}
							else {
								string guid = AssetDatabase.AssetPathToGUID(asset.path);
								if (!(assetInfo.guid == guid)) {
									if (asset.guid == guid) {
										this._guids.Remove(HashCode.Parse(assetInfo.guid));
										this._guids.Add(key, asset);
										this._paths[asset.path] = asset;
									}

									Debug.LogError((object)("path " + asset.path + " confilict \r\nself: " + asset.guid + "\r\nprev: " + assetInfo.guid + "\r\nright:" + guid));
								}
							}
						}
					}
				}
			}

			this._done = true;
			return true;
		}

		public void Write(string file) {
			using (ValuePool<string> stringPool = new ValuePool<string>()) {
				ByteWriter ba = new ByteWriter();
				foreach (KeyValuePair<HashCode, AssetInfo> guid in this._guids)
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