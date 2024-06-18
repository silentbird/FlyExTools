// Decompiled with JetBrains decompiler
// Type: edit.assets.AssetInfo
// Assembly: edit, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3ADECF35-F68B-48ED-B268-19248EA3422D
// Assembly location: D:\W\UnityProj\PluginsProj\Assets\Plugins\Win\csharp\edit.dll

using pure.database;
using pure.refactor.serialize;
using System.Collections.Generic;


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
        ByteArrayTools.WriteString(ba1, this.guid, stringPool);
        ByteArrayTools.WriteString(ba1, this.assetDependencyHash, stringPool);
        ByteArrayTools.WriteString(ba1, this.name, stringPool);
        ByteArrayTools.WriteString(ba1, this.path, stringPool);
        ByteArrayTools.WriteString(ba1, this.bundleName, stringPool);
        ByteArrayTools.WriteArray<string>(ba1, this.dependencies.ToArray(), stringPool);
        ByteArrayTools.WriteArray<string>(ba1, this.references.ToArray(), stringPool);
        ba.WriteChild(ba1);
      }
    }

    public void Read(IByteReader ba, IList<string> stringPool, int _)
    {
      using (IByteReader ba1 = ba.ReadChild())
      {
        this.guid = ByteArrayTools.ReadString(ba1, stringPool);
        this.assetDependencyHash = ByteArrayTools.ReadString(ba1, stringPool);
        this.name = ByteArrayTools.ReadString(ba1, stringPool);
        this.path = ByteArrayTools.ReadString(ba1, stringPool);
        this.bundleName = ByteArrayTools.ReadString(ba1, stringPool);
        this.dependencies.Clear();
        this.dependencies.AddRange((IEnumerable<string>) ByteArrayTools.ReadArray<string>(ba1, stringPool));
        this.references.Clear();
        this.references.AddRange((IEnumerable<string>) ByteArrayTools.ReadArray<string>(ba1, stringPool));
      }
    }
  }
}
