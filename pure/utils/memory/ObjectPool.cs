using pure.utils.debug;
using System;
using System.Collections.Generic;

namespace pure.utils.memory {
	public class ObjectPool<T> where T : new() {
		private readonly Stack<T> _stack = new Stack<T>();
		private readonly Action<T> _actionOnGet;
		private readonly Action<T> _actionOnRelease;
		private readonly Action<T> _actionOnCreate;

		public int countAll { get; private set; }

		public int countActive => this.countAll - this.countInactive;

		public int countInactive {
			get {
				lock (this._stack)
					return this._stack.Count;
			}
		}

		public ObjectPool()
			: this((Action<T>)null, (Action<T>)null, (Action<T>)null) {
		}

		public ObjectPool(Action<T> onGet, Action<T> onRelease)
			: this(onGet, onRelease, (Action<T>)null) {
		}

		public ObjectPool(Action<T> onCreate)
			: this((Action<T>)null, (Action<T>)null, onCreate) {
		}

		public ObjectPool(Action<T> onGet, Action<T> onRelease, Action<T> onCreate) {
			this._actionOnGet = onGet;
			this._actionOnRelease = onRelease;
			this._actionOnCreate = onCreate;
		}

		public bool TryGet(out T val) {
			lock (this._stack) {
				if (this._stack.Count > 0) {
					val = this.Get();
					return true;
				}
			}

			val = default(T);
			return false;
		}

		public T Get() {
			T obj;
			lock (this._stack) {
				if (this._stack.Count == 0) {
					obj = new T();
					Action<T> actionOnCreate = this._actionOnCreate;
					if (actionOnCreate != null)
						actionOnCreate(obj);
					++this.countAll;
				}
				else
					obj = this._stack.Pop();

				Action<T> actionOnGet = this._actionOnGet;
				if (actionOnGet != null)
					actionOnGet(obj);
			}

			return obj;
		}

		public void Release(T item) {
			lock (this._stack) {
				if (this._stack.Count > 0 && (object)this._stack.Peek() == (object)item) {
					GlobalLogger.Error("Internal error. Trying to destroy object that is already released to pool." + item?.ToString());
				}
				else {
					Action<T> actionOnRelease = this._actionOnRelease;
					if (actionOnRelease != null)
						actionOnRelease(item);
					this._stack.Push(item);
				}
			}
		}

		public void Clear() {
			lock (this._stack) {
				this._stack.Clear();
				this.countAll = 0;
			}
		}
	}
}