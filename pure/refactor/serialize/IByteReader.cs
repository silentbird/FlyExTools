using pure.utils.mathTools;
using System;

namespace pure.refactor.serialize {
	public interface IByteReader : IDisposable {
		byte ReadByte();

		bool ReadBool();

		string ReadChars(int len);

		short ReadShort();

		int ReadInt();

		uint ReadUint();

		float ReadFloat();

		long ReadLong();

		double ReadDouble();

		string ReadString();

		HashCode ReadHash();

		IByteReader ReadChild();

		void Reset();

		byte[] ToBuffer();

		byte[] ReadBytes(int len);

		int byteAvailable { get; }
	}
}