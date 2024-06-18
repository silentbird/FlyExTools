// Decompiled with JetBrains decompiler
// Type: edit.assets.AssetViewItem
// Assembly: edit, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3ADECF35-F68B-48ED-B268-19248EA3422D
// Assembly location: D:\W\UnityProj\PluginsProj\Assets\Plugins\Win\csharp\edit.dll

using System;
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;


namespace edit.assets
{
  public class AssetViewItem : TreeViewItem
  {
    public AssetInfo data;
    internal AssetDisplayMode mode;
    public bool ok;

    public void Dsf(Action<AssetViewItem> call)
    {
      if (this.children != null)
      {
        foreach (TreeViewItem child in this.children)
        {
          if (child is AssetViewItem assetViewItem)
            assetViewItem.Dsf(call);
        }
      }
      call(this);
    }

    public bool Filter(string str)
    {
      HashSet<AssetViewItem> assetViewItemSet = new HashSet<AssetViewItem>();
      Queue<AssetViewItem> assetViewItemQueue = new Queue<AssetViewItem>();
      assetViewItemQueue.Enqueue(this);
      assetViewItemSet.Add(this);
      while (assetViewItemQueue.Count > 0)
      {
        AssetViewItem assetViewItem1 = assetViewItemQueue.Dequeue();
        if (assetViewItem1.data != null && assetViewItem1.data.path.Contains(str))
          return true;
        if (assetViewItem1.children != null)
        {
          foreach (TreeViewItem child in assetViewItem1.children)
          {
            if (child is AssetViewItem assetViewItem2 && !assetViewItemSet.Contains(assetViewItem2))
            {
              assetViewItemQueue.Enqueue(assetViewItem2);
              assetViewItemSet.Add(assetViewItem2);
            }
          }
        }
      }
      return false;
    }
  }
}
