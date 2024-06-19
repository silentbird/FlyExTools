using System.Collections.Generic;
using pure.database;
using pure.refactor.serialize;

namespace edit.assets
{
  public class AssetInfo
  {
    public const uint MAGIC_CODE = 2874143930;
    public const int VERSION = 1;
    public string guid;
    public string name;
    public string path;
    public string assetDependencyHash;
    public string bundleName;
    public List<string> dependencies = new List<string>();
    public readonly List<string> references = new List<string>();

    public void WriteTo(ByteWriter ba, ValuePool<string> stringPool)
    {
      using (ByteWriter ba1 = new ByteWriter())
      {
        ByteArrayTools.WriteString(ba1, guid, stringPool);
        ByteArrayTools.WriteString(ba1, assetDependencyHash, stringPool);
        ByteArrayTools.WriteString(ba1, name, stringPool);
        ByteArrayTools.WriteString(ba1, path, stringPool);
        ByteArrayTools.WriteString(ba1, bundleName, stringPool);
        ByteArrayTools.WriteArray(ba1, dependencies.ToArray(), stringPool);
        ByteArrayTools.WriteArray(ba1, references.ToArray(), stringPool);
        ba.WriteChild(ba1);
      }
    }

    public void Read(IByteReader ba, IList<string> stringPool, int _)
    {
      using (IByteReader ba1 = ba.ReadChild())
      {
        guid = ByteArrayTools.ReadString(ba1, stringPool);
        assetDependencyHash = ByteArrayTools.ReadString(ba1, stringPool);
        name = ByteArrayTools.ReadString(ba1, stringPool);
        path = ByteArrayTools.ReadString(ba1, stringPool);
        bundleName = ByteArrayTools.ReadString(ba1, stringPool);
        dependencies.Clear();
        dependencies.AddRange(ByteArrayTools.ReadArray<string>(ba1, stringPool));
        references.Clear();
        references.AddRange(ByteArrayTools.ReadArray<string>(ba1, stringPool));
      }
    }
  }
}
