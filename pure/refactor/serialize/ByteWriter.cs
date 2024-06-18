using pure.utils.mathTools;
using System;
using System.IO;

namespace pure.refactor.serialize {
	public class ByteWriter : IDisposable {
		private MemoryStream _stream;

		public long position {
			get => this._stream.Position;
			set => this._stream.Position = value;
		}

		public long length => this._stream.Length;

		public int capacity => this._stream.Capacity;

		public ByteWriter() => this._stream = new MemoryStream();

		public ByteWriter(byte[] ba)
			: this() {
			this._stream.Write(ba, (int)this._stream.Position, ba.Length);
			this._stream.Seek(0L, SeekOrigin.Begin);
		}

		public void Reserve(int x) => this._stream.Capacity = x;

		public long WriteBytes(byte[] bytes, int offset = 0, int count = -1) {
			if (count == -1)
				count = bytes.Length;
			this._stream.Write(bytes, offset, count);
			return this.position;
		}

		public long WriteBool(bool value) => this.WriteBytes(BitConverter.GetBytes(value));

		public long WriteByte(char value) {
			this._stream.WriteByte((byte)value);
			return this.position;
		}

		public long WriteByte(byte value) {
			this._stream.WriteByte(value);
			return this.position;
		}

		public long WriteShort(short value) => this.WriteBytes(BitConverter.GetBytes(value));

		public long WriteInt(int value) => this.WriteBytes(BitConverter.GetBytes(value));

		public long WriteUint(uint value) => this.WriteBytes(BitConverter.GetBytes(value));

		public long WriteLong(long value) => this.WriteBytes(BitConverter.GetBytes(value));

		public long WriteDouble(double value) => this.WriteBytes(BitConverter.GetBytes(value));

		public long WriteFloat(float value) => this.WriteBytes(BitConverter.GetBytes(value));

		public long WriteChars(string str) {
			if (!string.IsNullOrEmpty(str))
				this.WriteBytes(StringConvert.GetByte(str));
			return this._stream.Position;
		}

		public long WriteString(string value) {
			if (string.IsNullOrEmpty(value))
				return this.WriteInt(0);
			byte[] bytes = StringConvert.GetByte(value);
			this.WriteInt(bytes.Length);
			this.WriteBytes(bytes);
			return this._stream.Position;
		}

		public void WriteHash(HashCode code) {
			this.WriteUint(code[0]);
			this.WriteUint(code[1]);
			this.WriteUint(code[2]);
			this.WriteUint(code[3]);
		}

		public void Reset() => this._stream.Seek(0L, SeekOrigin.Begin);

		public long WriteChild(ByteWriter value) {
			byte[] buffer = value._stream.GetBuffer();
			int length = (int)value._stream.Length;
			this.WriteInt(length);
			this.WriteBytes(buffer, count: length);
			return this.position;
		}

		public byte[] ToBuffer(int len) {
			byte[] buffer = this._stream.GetBuffer();
			byte[] dst = new byte[len];
			int position = (int)this._stream.Position;
			Buffer.BlockCopy((Array)buffer, position, (Array)dst, 0, len);
			this._stream.Position += (long)len;
			return dst;
		}

		public byte[] RawBuffer() {
			byte[] buffer = this._stream.GetBuffer();
			int count = (int)(this._stream.Length - this._stream.Position);
			byte[] dst = new byte[count];
			int position = (int)this._stream.Position;
			Buffer.BlockCopy((Array)buffer, position, (Array)dst, 0, count);
			this._stream.Position += (long)count;
			return dst;
		}

		public byte[] ToBuffer() {
			this._stream.Seek(0L, SeekOrigin.Begin);
			return this.ToBuffer((int)this._stream.Length);
		}

		// public ByteWriter Compress() => this.Compress(-1);
		//
		// public ByteWriter Compress(int level) {
		// 	byte[] buffer = this.ToBuffer().Zip(level);
		// 	this.Clear();
		// 	this._stream.Write(buffer, 0, buffer.Length);
		// 	this._stream.Seek(0L, SeekOrigin.Begin);
		// 	return this;
		// }

		// public ByteWriter Decompress() {
		// 	byte[] buffer = this.ToBuffer().UnZip();
		// 	this.Clear();
		// 	this._stream.Write(buffer, 0, buffer.Length);
		// 	this._stream.Seek(0L, SeekOrigin.Begin);
		// 	return this;
		// }

		public void Clear() => this._stream.SetLength(0L);

		~ByteWriter() => this.do_dispose();

		private void do_dispose() {
			if (this._stream != null)
				this._stream.Close();
			this._stream = (MemoryStream)null;
		}

		public void Dispose() {
			this.do_dispose();
			GC.SuppressFinalize((object)this);
		}
	}
}