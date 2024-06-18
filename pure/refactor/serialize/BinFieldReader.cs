using System.Collections.Generic;

namespace pure.refactor.serialize {
	public class BinFieldReader<T> : IBinFieldReader {
		private readonly BinFieldReader<T>.ReadCallBack _call;

		public BinFieldReader(BinFieldReader<T>.ReadCallBack call) => this._call = call;

		public T Read(IByteReader ba, IList<string> stringPool) => this._call(ba, stringPool);

		public object ReadObject(IByteReader ba, IList<string> stringPool) {
			return (object)this._call(ba, stringPool);
		}

		public delegate T ReadCallBack(IByteReader ba, IList<string> stringPool);
	}
}