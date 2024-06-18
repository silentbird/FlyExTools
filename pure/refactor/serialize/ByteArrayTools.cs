using JetBrains.Annotations;
// using pure.cpp;
using pure.database;
using pure.utils.debug;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace pure.refactor.serialize {
	public static class ByteArrayTools {
		private static readonly Dictionary<Type, ByteArrayTools.Writer> h_writer = new Dictionary<Type, ByteArrayTools.Writer>() {
			{
				typeof(string),
				(ByteArrayTools.Writer)((o, b, l) => ByteArrayTools.WriteString(b, o as string, l))
			}, {
				typeof(Vector2),
				(ByteArrayTools.Writer)((o, b, _) => ByteArrayTools.WriteVec2(b, (Vector2)o))
			}, {
				typeof(Vector3),
				(ByteArrayTools.Writer)((o, b, _) => ByteArrayTools.WriteVec3(b, (Vector3)o))
			}, {
				typeof(Vector4),
				(ByteArrayTools.Writer)((o, b, _) => ByteArrayTools.WriteVec4(b, (Vector4)o))
			}, {
				typeof(Rect),
				(ByteArrayTools.Writer)((o, b, _) => ByteArrayTools.WriteRect(b, (Rect)o))
			}, {
				typeof(Color),
				(ByteArrayTools.Writer)((o, b, _) => ByteArrayTools.WriteColor(b, (Color)o))
			}, {
				typeof(int),
				(ByteArrayTools.Writer)((o, b, _) => b.WriteInt((int)o))
			}, {
				typeof(float),
				(ByteArrayTools.Writer)((o, b, _) => b.WriteFloat((float)o))
			}, {
				typeof(bool),
				(ByteArrayTools.Writer)((o, b, _) => b.WriteBool((bool)o))
			}, {
				typeof(short),
				(ByteArrayTools.Writer)((o, b, _) => b.WriteShort((short)o))
			}, {
				typeof(byte),
				(ByteArrayTools.Writer)((o, b, _) => b.WriteByte((byte)o))
			}, {
				typeof(uint),
				(ByteArrayTools.Writer)((o, b, _) => b.WriteUint((uint)o))
			}
		};

		private static readonly Dictionary<Type, IBinFieldReader> h_reader = new Dictionary<Type, IBinFieldReader>();

		static ByteArrayTools() {
			ByteArrayTools.Register<string>(new BinFieldReader<string>.ReadCallBack(ByteArrayTools.ReadString));
			ByteArrayTools.Register<Vector2>((BinFieldReader<Vector2>.ReadCallBack)((b, _) => ByteArrayTools.ReadVec2(b)));
			ByteArrayTools.Register<Vector3>((BinFieldReader<Vector3>.ReadCallBack)((b, _) => ByteArrayTools.ReadVec3(b)));
			ByteArrayTools.Register<Vector4>((BinFieldReader<Vector4>.ReadCallBack)((b, _) => ByteArrayTools.ReadVec4(b)));
			ByteArrayTools.Register<Rect>((BinFieldReader<Rect>.ReadCallBack)((b, _) => ByteArrayTools.ReadRect(b)));
			ByteArrayTools.Register<Color>((BinFieldReader<Color>.ReadCallBack)((b, _) => ByteArrayTools.ReadColor(b)));
			ByteArrayTools.Register<int>((BinFieldReader<int>.ReadCallBack)((b, _) => b.ReadInt()));
			ByteArrayTools.Register<float>((BinFieldReader<float>.ReadCallBack)((b, _) => b.ReadFloat()));
			ByteArrayTools.Register<bool>((BinFieldReader<bool>.ReadCallBack)((b, _) => b.ReadBool()));
			ByteArrayTools.Register<short>((BinFieldReader<short>.ReadCallBack)((b, _) => b.ReadShort()));
			ByteArrayTools.Register<byte>((BinFieldReader<byte>.ReadCallBack)((b, _) => b.ReadByte()));
			ByteArrayTools.Register<uint>((BinFieldReader<uint>.ReadCallBack)((b, _) => b.ReadUint()));
			ByteArrayTools.Register<double>((BinFieldReader<double>.ReadCallBack)((b, _) => b.ReadDouble()));
			ByteArrayTools.Register<long>((BinFieldReader<long>.ReadCallBack)((b, _) => b.ReadLong()));
		}


		public static void Register<T>(BinFieldReader<T>.ReadCallBack reader) {
			Type key = typeof(T);
			if (ByteArrayTools.h_reader.ContainsKey(key)) {
				GlobalLogger.Error(string.Format("{0} already registed", (object)typeof(T)));
			}
			else {
				ByteArrayTools.h_reader.Add(key, (IBinFieldReader)new BinFieldReader<T>(reader));
				ByteArrayTools.h_reader.Add(typeof(T[]), (IBinFieldReader)new BinFieldReader<T[]>(new BinFieldReader<T[]>.ReadCallBack(ByteArrayTools.ReadArray<T>)));
			}
		}

		public static void WriteVec2(ByteWriter ba, Vector2 v) {
			ba.WriteFloat(v.x);
			ba.WriteFloat(v.y);
		}

		public static Vector2 ReadVec2(IByteReader ba) => new Vector2(ba.ReadFloat(), ba.ReadFloat());

		public static void WriteVec3(ByteWriter ba, Vector3 v) {
			ba.WriteFloat(v.x);
			ba.WriteFloat(v.y);
			ba.WriteFloat(v.z);
		}

		public static Vector3 ReadVec3(IByteReader ba) {
			return new Vector3(ba.ReadFloat(), ba.ReadFloat(), ba.ReadFloat());
		}

		public static void WriteVec4(ByteWriter ba, Vector4 v) {
			ba.WriteFloat(v.x);
			ba.WriteFloat(v.y);
			ba.WriteFloat(v.z);
			ba.WriteFloat(v.w);
		}

		public static Vector4 ReadVec4(IByteReader ba) {
			return new Vector4(ba.ReadFloat(), ba.ReadFloat(), ba.ReadFloat(), ba.ReadFloat());
		}

		public static void WriteRect(ByteWriter ba, Rect v) {
			ba.WriteFloat(v.x);
			ba.WriteFloat(v.y);
			ba.WriteFloat(v.width);
			ba.WriteFloat(v.height);
		}

		public static Rect ReadRect(IByteReader ba) {
			return new Rect(ba.ReadFloat(), ba.ReadFloat(), ba.ReadFloat(), ba.ReadFloat());
		}

		public static void WriteString(ByteWriter ba, string v, ValuePool<string> stringPool) {
			int num = stringPool.Add(v);
			ba.WriteInt(num);
		}

		public static string ReadString(IByteReader ba, IList<string> stringPool) {
			return stringPool[ba.ReadInt()];
		}

		public static string ReadString(IByteReader ba, string[] stringPool) {
			return stringPool[ba.ReadInt()];
		}

		public static void WriteColor(ByteWriter ba, Color v) {
			int num = (int)((double)v.a * (double)byte.MaxValue) << 24 | (int)((double)v.r * (double)byte.MaxValue) << 16 | (int)((double)v.g * (double)byte.MaxValue) << 8 |
			          (int)((double)v.b * (double)byte.MaxValue);
			ba.WriteInt(num);
		}

		public static Color ReadColor(IByteReader ba) {
			uint num = ba.ReadUint();
			return new Color() {
				a = (float)(num >> 24 & (uint)byte.MaxValue) * 0.003921569f,
				r = (float)(num >> 16 & (uint)byte.MaxValue) * 0.003921569f,
				g = (float)(num >> 8 & (uint)byte.MaxValue) * 0.003921569f,
				b = (float)(num & (uint)byte.MaxValue) * 0.003921569f
			};
		}

		public static void WriteArray<T>(ByteWriter ba, T[] arr, ValuePool<string> stringPool) {
			ByteArrayTools.Writer writer;
			if (!ByteArrayTools.h_writer.TryGetValue(typeof(T), out writer))
				throw new Exception(string.Format("{0} has no ByteWriter", (object)typeof(T)));
			if (arr.Length != 0) {
				ba.WriteInt(arr.Length);
				int index = 0;
				for (int length = arr.Length; index < length; ++index)
					writer((object)arr[index], ba, stringPool);
			}
			else
				ba.WriteInt(0);
		}

		public static void WriteArray(
			ByteWriter ba,
			IList arr,
			ValuePool<string> stringPool,
			Type element) {
			ByteArrayTools.Writer writer;
			if (!ByteArrayTools.h_writer.TryGetValue(element, out writer))
				throw new Exception(string.Format("{0} has no ByteWriter", (object)element));
			if (arr.Count > 0) {
				int count = arr.Count;
				ba.WriteInt(count);
				for (int index = 0; index < count; ++index)
					writer(arr[index], ba, stringPool);
			}
			else
				ba.WriteInt(0);
		}

		public static T[] ReadArray<T>(IByteReader ba, IList<string> stringPool) {
			IBinFieldReader binFieldReader1;
			if (!ByteArrayTools.h_reader.TryGetValue(typeof(T), out binFieldReader1))
				throw new Exception(string.Format("{0} has no Reader", (object)typeof(T)));
			BinFieldReader<T> binFieldReader2 = (BinFieldReader<T>)binFieldReader1;
			int length = ba.ReadInt();
			if (length == 0)
				return ZeroBuffer<T>.Buffer;
			T[] objArray = new T[length];
			for (int index = 0; index < length; ++index) {
				T obj = binFieldReader2.Read(ba, stringPool);
				objArray[index] = obj;
			}

			return objArray;
		}
		//
		// public static byte[] Zip(this byte[] src) => src.Zip(-1);
		//
		// public static byte[] Zip(this byte[] src, int compressLevel) {
		// 	return CppZipper.Zip(src, compressLevel);
		// }
		//
		// public static byte[] UnZip(this byte[] src) => CppZipper.Unzip(src);

		[PublicAPI] public delegate void Writer(object o, ByteWriter ba, ValuePool<string> stringPool);
	}
}