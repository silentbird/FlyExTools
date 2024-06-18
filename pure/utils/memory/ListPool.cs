// Decompiled with JetBrains decompiler
// Type: pure.utils.memory.ListPool`1
// Assembly: pure, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 7542460E-F99E-4B4A-91E2-6B1672E9BAD8
// Assembly location: D:\W\UnityProj\PluginsProj\Assets\Plugins\Win\csharp\pure.dll

using System;
using System.Collections.Generic;

namespace pure.utils.memory {
	public static class ListPool<T> {
		private static ObjectPool<ListPool<T>.item> m_pool;

		private static ObjectPool<ListPool<T>.item> pool {
			get { return ListPool<T>.m_pool ?? (ListPool<T>.m_pool = new ObjectPool<ListPool<T>.item>((Action<ListPool<T>.item>)null)); }
		}

		public static List<T> Get() {
			ListPool<T>.item obj = ListPool<T>.pool.Get();
			obj.pooling = true;
			return (List<T>)obj;
		}

		public static List<T> Get(int capacity) {
			ListPool<T>.item obj = ListPool<T>.pool.Get();
			obj.pooling = true;
			if (obj.Capacity < capacity)
				obj.Capacity = capacity;
			return (List<T>)obj;
		}

		public static void Release(List<T> list) {
			if (!(list is ListPool<T>.item obj) || !obj.pooling)
				return;
			obj.pooling = false;
			obj.Clear();
			ListPool<T>.pool.Release(obj);
		}

		public static int countAll => ListPool<T>.pool.countAll;

		public static int countActive => ListPool<T>.pool.countActive;

		public static int countInactive => ListPool<T>.pool.countInactive;

		private class item : List<T> {
			internal bool pooling;
		}
	}
}