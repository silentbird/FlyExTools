using pure.utils.mathTools;
using System;
using System.IO;
using HashCode = pure.utils.mathTools.HashCode;

namespace pure.refactor.serialize {
	public class ByteWriter : IDisposable {
		private MemoryStream _stream;

		public long position {
			get => _stream.Position;
			set => _stream.Position = value;
		}

		public long length => _stream.Length;

		public int capacity => _stream.Capacity;

		public ByteWriter() => _stream = new MemoryStream();

		public ByteWriter(byte[] ba)
			: this() {
			_stream.Write(ba, (int)_stream.Position, ba.Length);
			_stream.Seek(0L, SeekOrigin.Begin);
		}

		public void Reserve(int x) => _stream.Capacity = x;

		public long WriteBytes(byte[] bytes, int offset = 0, int count = -1) {
			if (count == -1)
				count = bytes.Length;
			_stream.Write(bytes, offset, count);
			return position;
		}

		public long WriteBool(bool value) => WriteBytes(BitConverter.GetBytes(value));

		public long WriteByte(char value) {
			_stream.WriteByte((byte)value);
			return position;
		}

		public long WriteByte(byte value) {
			_stream.WriteByte(value);
			return position;
		}

		public long WriteShort(short value) => WriteBytes(BitConverter.GetBytes(value));

		public long WriteInt(int value) => WriteBytes(BitConverter.GetBytes(value));

		public long WriteUint(uint value) => WriteBytes(BitConverter.GetBytes(value));

		public long WriteLong(long value) => WriteBytes(BitConverter.GetBytes(value));

		public long WriteDouble(double value) => WriteBytes(BitConverter.GetBytes(value));

		public long WriteFloat(float value) => WriteBytes(BitConverter.GetBytes(value));

		public long WriteChars(string str) {
			if (!string.IsNullOrEmpty(str))
				WriteBytes(StringConvert.GetByte(str));
			return _stream.Position;
		}

		public long WriteString(string value) {
			if (string.IsNullOrEmpty(value))
				return WriteInt(0);
			byte[] bytes = StringConvert.GetByte(value);
			WriteInt(bytes.Length);
			WriteBytes(bytes);
			return _stream.Position;
		}

		public void WriteHash(HashCode code) {
			WriteUint(code[0]);
			WriteUint(code[1]);
			WriteUint(code[2]);
			WriteUint(code[3]);
		}

		public void Reset() => _stream.Seek(0L, SeekOrigin.Begin);

		public long WriteChild(ByteWriter value) {
			byte[] buffer = value._stream.GetBuffer();
			int length = (int)value._stream.Length;
			WriteInt(length);
			WriteBytes(buffer, count: length);
			return position;
		}

		public byte[] ToBuffer(int len) {
			byte[] buffer = _stream.GetBuffer();
			byte[] dst = new byte[len];
			int position = (int)_stream.Position;
			Buffer.BlockCopy(buffer, position, dst, 0, len);
			_stream.Position += len;
			return dst;
		}

		public byte[] RawBuffer() {
			byte[] buffer = _stream.GetBuffer();
			int count = (int)(_stream.Length - _stream.Position);
			byte[] dst = new byte[count];
			int position = (int)_stream.Position;
			Buffer.BlockCopy(buffer, position, dst, 0, count);
			_stream.Position += count;
			return dst;
		}

		public byte[] ToBuffer() {
			_stream.Seek(0L, SeekOrigin.Begin);
			return ToBuffer((int)_stream.Length);
		}

		// public ByteWriter Compress() => Compress(-1);
		//
		// public ByteWriter Compress(int level) {
		// 	byte[] buffer = ToBuffer().Zip(level);
		// 	Clear();
		// 	_stream.Write(buffer, 0, buffer.Length);
		// 	_stream.Seek(0L, SeekOrigin.Begin);
		// 	return this;
		// }
 
		// public ByteWriter Decompress() {
		// 	byte[] buffer = ToBuffer().UnZip();
		// 	Clear();
		// 	_stream.Write(buffer, 0, buffer.Length);
		// 	_stream.Seek(0L, SeekOrigin.Begin);
		// 	return this;
		// }

		public void Clear() => _stream.SetLength(0L);

		~ByteWriter() => do_dispose();

		private void do_dispose() {
			if (_stream != null)
				_stream.Close();
			_stream = (MemoryStream)null;
		}

		public void Dispose() {
			do_dispose();
			GC.SuppressFinalize((object)this);
		}
	}
}