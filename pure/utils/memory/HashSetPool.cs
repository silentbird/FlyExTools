using System;
using System.Collections.Generic;

namespace pure.utils.memory {
	public static class HashSetPool<T> {
		private static ObjectPool<HashSet<T>> m_pool;

		private static ObjectPool<HashSet<T>> pool {
			get { return HashSetPool<T>.m_pool ?? (HashSetPool<T>.m_pool = new ObjectPool<HashSet<T>>((Action<HashSet<T>>)null)); }
		}

		public static HashSet<T> Get() => HashSetPool<T>.pool.Get();

		public static void Release(HashSet<T> item) {
			item.Clear();
			HashSetPool<T>.pool.Release(item);
		}
	}
}