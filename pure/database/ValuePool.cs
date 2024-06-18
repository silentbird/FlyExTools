// Decompiled with JetBrains decompiler
// Type: pure.database.ValuePool`1
// Assembly: pure, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 7542460E-F99E-4B4A-91E2-6B1672E9BAD8
// Assembly location: D:\W\UnityProj\PluginsProj\Assets\Plugins\Win\csharp\pure.dll

using pure.utils.memory;
using System;
using System.Collections.Generic;

namespace pure.database
{
  public class ValuePool<T> : IDisposable
  {
    private Dictionary<T, int> _slots;
    private List<T> _list;
    private readonly bool _pool;
    private bool _disposed;

    public int Count => this._list.Count;

    public T this[int idx] => this._list[idx];

    public ValuePool(bool pool)
    {
      this._pool = pool;
      if (this._pool)
      {
        this._slots = DictionaryPool<T, int>.Get();
        this._list = ListPool<T>.Get();
      }
      else
      {
        this._slots = new Dictionary<T, int>();
        this._list = new List<T>();
      }
    }

    public ValuePool()
      : this(false)
    {
    }

    public int Add(T item)
    {
      int num;
      if (this._slots.TryGetValue(item, out num))
        return num;
      int count = this._list.Count;
      this._list.Add(item);
      this._slots.Add(item, count);
      return count;
    }

    private void do_dispose()
    {
      if (this._disposed)
        return;
      this._disposed = true;
      if (!this._pool)
        return;
      if (this._slots != null)
      {
        DictionaryPool<T, int>.Release(this._slots);
        this._slots = (Dictionary<T, int>) null;
      }
      if (this._list == null)
        return;
      ListPool<T>.Release(this._list);
      this._list = (List<T>) null;
    }

    ~ValuePool() => this.do_dispose();

    public void Dispose()
    {
      this.do_dispose();
      GC.SuppressFinalize((object) this);
    }
  }
}
