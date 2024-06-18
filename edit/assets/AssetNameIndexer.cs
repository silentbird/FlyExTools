// Decompiled with JetBrains decompiler
// Type: edit.assets.AssetNameIndexer
// Assembly: edit, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3ADECF35-F68B-48ED-B268-19248EA3422D
// Assembly location: D:\W\UnityProj\PluginsProj\Assets\Plugins\Win\csharp\edit.dll

using System.Collections.Generic;
using System.IO;
using System.Linq;


namespace edit.assets
{
  internal class AssetNameIndexer
  {
    private readonly Dictionary<string, AssetNameIndexer.Entry> _hash = new Dictionary<string, AssetNameIndexer.Entry>();

    internal int Count { get; private set; }

    internal void Add(AssetInfo asset)
    {
      string fileName = Path.GetFileName(asset.path);
      if (string.IsNullOrEmpty(fileName))
        return;
      AssetNameIndexer.Entry entry;
      if (!this._hash.TryGetValue(fileName, out entry))
      {
        entry = new AssetNameIndexer.Entry();
        this._hash.Add(fileName, entry);
      }
      Dictionary<string, string> guids = entry.guids;
      if (guids.ContainsKey(asset.path))
        return;
      guids.Add(asset.path, asset.guid);
      ++this.Count;
    }

    internal void Remove(string path)
    {
      string fileName = Path.GetFileName(path);
      AssetNameIndexer.Entry entry;
      if (string.IsNullOrEmpty(fileName) || !this._hash.TryGetValue(fileName, out entry) || !entry.guids.Remove(path))
        return;
      --this.Count;
      if (entry.guids.Count != 0)
        return;
      this._hash.Remove(fileName);
    }

    internal bool TryGetGuidByName(string name, out string[] guid)
    {
      name = Path.GetFileName(name);
      AssetNameIndexer.Entry entry;
      if (!string.IsNullOrEmpty(name) && this._hash.TryGetValue(name, out entry))
      {
        guid = entry.guids.Values.ToArray<string>();
        return true;
      }
      guid = (string[]) null;
      return false;
    }

    internal bool TryGetPathByName(string name, out string[] paths)
    {
      name = Path.GetFileName(name);
      AssetNameIndexer.Entry entry;
      if (!string.IsNullOrEmpty(name) && this._hash.TryGetValue(name, out entry))
      {
        paths = entry.guids.Keys.ToArray<string>();
        return true;
      }
      paths = (string[]) null;
      return false;
    }

    internal void Clear()
    {
      this._hash.Clear();
      this.Count = 0;
    }

    private class Entry
    {
      internal Dictionary<string, string> guids = new Dictionary<string, string>();
    }
  }
}
