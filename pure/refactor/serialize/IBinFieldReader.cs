using System.Collections.Generic;

namespace pure.refactor.serialize {
	public interface IBinFieldReader {
		object ReadObject(IByteReader ba, IList<string> strigPool);
	}
}