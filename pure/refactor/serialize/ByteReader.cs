
using pure.database;
using pure.utils.mathTools;
using System;

namespace pure.refactor.serialize
{
  public class ByteReader : IByteReader, IDisposable
  {
    private byte[] _buffer;
    private int _position;
    private int _end;
    private int _length;
    private readonly int _begin;
    private bool _disposed;

    public int byteAvailable => this._end - this._position;

    public int position => this._position;

    public int length => this._length;

    public ByteReader(byte[] data)
      : this(data, 0, data.Length)
    {
    }

    public ByteReader(byte[] data, int offset, int size)
    {
      this._buffer = data;
      this._position = offset;
      this._begin = offset;
      this._end = offset + size;
      this._length = size;
    }

    private void assert(int size)
    {
      if (this._disposed)
        throw new Exception("stream disposed");
      if (size < 0 || this._position + size > this._end)
        throw new ArgumentOutOfRangeException(nameof (size), string.Format("p:{0}, size:{1}, end:{2}", (object) this._position, (object) size, (object) this._end));
    }

    public byte ReadByte()
    {
      this.assert(1);
      byte num = this._buffer[this._position];
      ++this._position;
      return num;
    }

    public bool ReadBool()
    {
      this.assert(1);
      bool boolean = BitConverter.ToBoolean(this._buffer, this._position);
      ++this._position;
      return boolean;
    }

    public string ReadChars(int len)
    {
      this.assert(len);
      string str = StringConvert.GetString(this._buffer, this._position, len);
      this._position += len;
      return str;
    }

    public short ReadShort()
    {
      this.assert(2);
      short int16 = BitConverter.ToInt16(this._buffer, this._position);
      this._position += 2;
      return int16;
    }

    public int ReadInt()
    {
      this.assert(4);
      int int32 = BitConverter.ToInt32(this._buffer, this._position);
      this._position += 4;
      return int32;
    }

    public uint ReadUint()
    {
      this.assert(4);
      uint uint32 = BitConverter.ToUInt32(this._buffer, this._position);
      this._position += 4;
      return uint32;
    }

    public float ReadFloat()
    {
      this.assert(4);
      float single = BitConverter.ToSingle(this._buffer, this._position);
      this._position += 4;
      return single;
    }

    public long ReadLong()
    {
      this.assert(8);
      long int64 = BitConverter.ToInt64(this._buffer, this._position);
      this._position += 8;
      return int64;
    }

    public double ReadDouble()
    {
      this.assert(8);
      double num = BitConverter.ToDouble(this._buffer, this._position);
      this._position += 8;
      return num;
    }

    public HashCode ReadHash()
    {
      return new HashCode(this.ReadUint(), this.ReadUint(), this.ReadUint(), this.ReadUint());
    }

    public IByteReader ReadChild()
    {
      int size = this.ReadInt();
      this.assert(size);
      ByteReader byteReader = new ByteReader(this._buffer, this._position, size);
      this._position += size;
      return (IByteReader) byteReader;
    }

    // public void Decompress()
    // {
    //   byte[] numArray = this._buffer.UnZip();
    //   if (numArray == null)
    //     return;
    //   this._end = numArray.Length;
    //   this._position = 0;
    //   this._length = numArray.Length;
    //   this._buffer = numArray;
    // }

    public void Reset() => this._position = this._begin;

    public byte[] ToBuffer()
    {
      byte[] dst = new byte[this._length];
      if (this._length > 0)
        Buffer.BlockCopy((Array) this._buffer, this._begin, (Array) dst, 0, this._length);
      return dst;
    }

    public byte[] ReadBytes(int len)
    {
      this.assert(len);
      if (len == 0)
        return ZeroBuffer<byte>.Buffer;
      byte[] dst = new byte[len];
      if (len > 0)
        Buffer.BlockCopy((Array) this._buffer, this._position, (Array) dst, 0, len);
      this._position += len;
      return dst;
    }

    public string ReadString()
    {
      int num = this.ReadInt();
      if (num <= 0)
        return string.Empty;
      this.assert(num);
      string str = StringConvert.GetString(this._buffer, this._position, num);
      this._position += num;
      return str;
    }

    private void dispose() => this._disposed = true;

    ~ByteReader() => this.dispose();

    public void Dispose()
    {
      this.dispose();
      GC.SuppressFinalize((object) this);
    }
  }
}
