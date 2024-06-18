using System;
using System.Collections.Generic;

namespace pure.utils.memory {
	public struct RecycleListVO<T> : IDisposable {
		private bool _disposed;
		private List<T> _list;

		public List<T> list => this._list ?? (this._list = ListPool<T>.Get());

		void IDisposable.Dispose() {
			if (this._disposed)
				return;
			this._disposed = true;
			if (this._list == null)
				return;
			ListPool<T>.Release(this._list);
			this._list = (List<T>)null;
		}
	}
}