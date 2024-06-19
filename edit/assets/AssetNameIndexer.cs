using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace edit.assets
{
  internal class AssetNameIndexer
  {
    private readonly Dictionary<string, Entry> _hash = new Dictionary<string, Entry>();

    internal int Count { get; private set; }

    internal void Add(AssetInfo asset)
    {
      string fileName = Path.GetFileName(asset.path);
      if (string.IsNullOrEmpty(fileName))
        return;
      Entry entry;
      if (!_hash.TryGetValue(fileName, out entry))
      {
        entry = new Entry();
        _hash.Add(fileName, entry);
      }
      Dictionary<string, string> guids = entry.guids;
      if (guids.ContainsKey(asset.path))
        return;
      guids.Add(asset.path, asset.guid);
      ++Count;
    }

    internal void Remove(string path)
    {
      string fileName = Path.GetFileName(path);
      Entry entry;
      if (string.IsNullOrEmpty(fileName) || !_hash.TryGetValue(fileName, out entry) || !entry.guids.Remove(path))
        return;
      --Count;
      if (entry.guids.Count != 0)
        return;
      _hash.Remove(fileName);
    }

    internal bool TryGetGuidByName(string name, out string[] guid)
    {
      name = Path.GetFileName(name);
      Entry entry;
      if (!string.IsNullOrEmpty(name) && _hash.TryGetValue(name, out entry))
      {
        guid = entry.guids.Values.ToArray();
        return true;
      }
      guid = null;
      return false;
    }

    internal bool TryGetPathByName(string name, out string[] paths)
    {
      name = Path.GetFileName(name);
      Entry entry;
      if (!string.IsNullOrEmpty(name) && _hash.TryGetValue(name, out entry))
      {
        paths = entry.guids.Keys.ToArray();
        return true;
      }
      paths = null;
      return false;
    }

    internal void Clear()
    {
      _hash.Clear();
      Count = 0;
    }

    private class Entry
    {
      internal Dictionary<string, string> guids = new Dictionary<string, string>();
    }
  }
}
