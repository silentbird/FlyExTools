// Decompiled with JetBrains decompiler
// Type: edit.assets.AssetInfoExtension
// Assembly: edit, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3ADECF35-F68B-48ED-B268-19248EA3422D
// Assembly location: D:\W\UnityProj\PluginsProj\Assets\Plugins\Win\csharp\edit.dll

using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;


namespace edit.assets
{
  internal static class AssetInfoExtension
  {
    private static readonly Regex s_extra_reg = new Regex("Assets/([^|\\n\"]+)");

    internal static bool ContainsExtra(this string assetPath)
    {
      return assetPath.EndsWith(".tree") || assetPath.EndsWith(".machine") || assetPath.EndsWith(".xjson") || assetPath.EndsWith(".prefab") || assetPath.EndsWith(".unity") || assetPath.EndsWith(".controller");
    }

    internal static void CollectDepencies(this string assetPath, ICollection<string> to)
    {
      string input = File.ReadAllText(assetPath);
      MatchCollection matchCollection = AssetInfoExtension.s_extra_reg.Matches(input);
      for (int i = 0; i < matchCollection.Count; ++i)
      {
        string guid = AssetDatabase.AssetPathToGUID("Assets/" + matchCollection[i].Groups[1].Value);
        if (!string.IsNullOrEmpty(guid))
          to.Add(guid);
      }
    }
  }
}
