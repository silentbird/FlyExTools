using System;
using System.Collections.Generic;

namespace pure.utils.memory {
	public struct RecycleSetVO<T> : IDisposable {
		private bool _disposed;
		private HashSet<T> _set;

		public HashSet<T> hashSet => this._set ?? (this._set = HashSetPool<T>.Get());

		void IDisposable.Dispose() {
			if (this._disposed)
				return;
			this._disposed = true;
			if (this._set == null)
				return;
			HashSetPool<T>.Release(this._set);
			this._set = (HashSet<T>)null;
		}
	}
}