using System;
using System.Globalization;

namespace pure.utils.mathTools {
	[Serializable]
	public struct HashCode {
		public static readonly HashCode Zero;
		public const int SIZE = 4;
		public uint m0;
		public uint m1;
		public uint m2;
		public uint m3;

		public HashCode(uint x = 0, uint y = 0, uint z = 0, uint w = 0) {
			this.m0 = x;
			this.m1 = y;
			this.m2 = z;
			this.m3 = w;
		}

		public bool Empty() => this.m0 == 0U && this.m1 == 0U && this.m2 == 0U && this.m3 == 0U;

		public override string ToString() {
			return string.Format("{0:x8}{1:x8}{2:x8}{3:x8}", (object)this.m0, (object)this.m1, (object)this.m2, (object)this.m3);
		}

		public override bool Equals(object obj) => obj is HashCode hashCode && this == hashCode;

		public override int GetHashCode() {
			return this.m0.GetHashCode() ^ this.m1.GetHashCode() ^ this.m2.GetHashCode() ^ this.m3.GetHashCode();
		}

		public static unsafe bool operator ==(HashCode hash1, HashCode hash2) {
			HashCode* hashCodePtr1 = &hash1;
			HashCode* hashCodePtr2 = &hash2;
			int* numPtr1 = (int*)hashCodePtr1;
			int* numPtr2 = (int*)hashCodePtr2;
			for (int index = 0; index < 4; ++index) {
				if (numPtr1[index] != numPtr2[index])
					return false;
			}

			return true;
		}

		public static bool operator !=(HashCode hash1, HashCode hash2) => !(hash1 == hash2);

		public unsafe uint this[int index] {
			get {
				if (index < 0 || index >= 4)
					throw new ArgumentOutOfRangeException(nameof(index), "out of range");
				fixed (HashCode* hashCodePtr = &this)
					return *(uint*)((IntPtr)hashCodePtr + index * 4);
			}
			set {
				if (index < 0 || index >= 4)
					throw new ArgumentOutOfRangeException(nameof(index), "out of range");
				fixed (HashCode* hashCodePtr = &this)
					*(int*)((IntPtr)hashCodePtr + index * 4) = (int)value;
			}
		}

		public static unsafe implicit operator HashCode(byte[] buf) {
			if (buf.Length > 16)
				throw new Exception(string.Format("{0} is too long for hash code", (object)buf));
			fixed (int* numPtr1 = new int[4]) {
				byte* numPtr2 = (byte*)numPtr1;
				int index1 = 16 - buf.Length;
				for (int index2 = 0; index2 < buf.Length; ++index2)
					(numPtr2 + index2)[index1] = buf[index2];
				for (int index3 = 0; index3 < 4; ++index3) {
					uint num1 = (uint)numPtr1[index3];
					int num2 = (int)byte.MaxValue & (int)(num1 >> 24) | 65280 & (int)(num1 >> 8) | 16711680 & (int)num1 << 8 | -16777216 & (int)num1 << 24;
					numPtr1[index3] = num2;
				}

				return *(HashCode*)numPtr1;
			}
		}

		public static unsafe HashCode Parse(string str) {
			int length1 = str.Length;
			if (length1 > 32)
				throw new Exception(string.Format("{0} is too long for hash code", (object)str));
			fixed (int* numPtr = new int[4]) {
				int index = 4;
				int length2;
				for (; length1 > 0; length1 -= length2) {
					length2 = Math.Min(length1, 8);
					int num = int.Parse(str.Substring(length1 - length2, length2), NumberStyles.HexNumber, (IFormatProvider)CultureInfo.CurrentCulture);
					--index;
					numPtr[index] = num;
				}

				return *(HashCode*)numPtr;
			}
		}

		public static HashCode Parse(long v) => new HashCode(z: (uint)(v >> 32), w: (uint)v);

		public long ToLong() => (long)this.m2 << 32 | (long)this.m3;
	}
}