using System;
using System.Collections.Generic;

namespace pure.utils.memory
{
  public static class DictionaryPool<TK, TV>
  {
    private static ObjectPool<Dictionary<TK, TV>> m_pool;

    private static ObjectPool<Dictionary<TK, TV>> pool
    {
      get
      {
        return DictionaryPool<TK, TV>.m_pool ?? (DictionaryPool<TK, TV>.m_pool = new ObjectPool<Dictionary<TK, TV>>((Action<Dictionary<TK, TV>>) null));
      }
    }

    public static Dictionary<TK, TV> Get() => DictionaryPool<TK, TV>.pool.Get();

    public static void Release(Dictionary<TK, TV> item)
    {
      item.Clear();
      DictionaryPool<TK, TV>.pool.Release(item);
    }
  }
}
