using System;
using System.Collections.Generic;
using System.ComponentModel;
using pure.utils.memory;
using UnityEngine;

namespace edit.gui {
	public class SettingEnum : DisplaySetting {
		private static readonly Dictionary<Type, EnumCache> caches = new Dictionary<Type, EnumCache>();

		private static EnumCache getcache(Type enumType) {
			EnumCache enumCache1;
			if (caches.TryGetValue(enumType, out enumCache1))
				return enumCache1;
			EnumCache enumCache2 = new EnumCache();
			string[] names = Enum.GetNames(enumType);
			enumCache2.names = new string[names.Length];
			enumCache2.contents = new GUIContent[names.Length];
			enumCache2.values = new int[names.Length];
			for (int index = 0; index < names.Length; ++index)
				enumCache2.values[index] = (int)Enum.Parse(enumType, names[index]);
			for (int index = 0; index < names.Length; ++index) {
				object[] customAttributes = enumType.GetField(names[index]).GetCustomAttributes(typeof(DescriptionAttribute), false);
				DescriptionAttribute descriptionAttribute = customAttributes.Length != 0 ? customAttributes[0] as DescriptionAttribute : null;
				string str = descriptionAttribute != null ? descriptionAttribute.Description : names[index];
				enumCache2.names[index] = str;
				enumCache2.contents[index] = new GUIContent(str, str);
			}

			caches.Add(enumType, enumCache2);
			return enumCache2;
		}

		public static int[] GetValues(Type enumType) => getcache(enumType).values;

		public static string[] GetNames(Type enumType) => getcache(enumType).names;

		public static GUIContent[] GetContents(Type enumType) {
			return getcache(enumType).contents;
		}

		public static string GetName<T>(T val) {
			int hashCode = val.GetHashCode();
			EnumCache enumCache = getcache(typeof(T));
			int index = Array.IndexOf(enumCache.values, hashCode);
			return index < 0 || index >= enumCache.names.Length ? "error" : enumCache.names[index];
		}

		public static string GetName<T>(int val) {
			int hashCode = val.GetHashCode();
			EnumCache enumCache = getcache(typeof(T));
			int index = Array.IndexOf(enumCache.values, hashCode);
			return index < 0 || index >= enumCache.names.Length ? "error" : enumCache.names[index];
		}

		public static string GetMaskNames<T>(T val) {
			string[] names = GetNames(val.GetType());
			int hashCode = val.GetHashCode();
			EnumCache enumCache = getcache(typeof(T));
			using (RecycleListVO<string> recycleListVo = new RecycleListVO<string>()) {
				List<string> list = recycleListVo.list;
				for (int index = 0; index < enumCache.values.Length; ++index) {
					int num = enumCache.values[index];
					if ((hashCode & num) != 0)
						list.Add(names[index]);
				}

				return list.Count > 0 ? string.Join(",", list.ToArray()) : "Nothing";
			}
		}

		public string[] labels { get; }

		public Type enums { get; }

		public Array enumList { get; }

		public int[] datas { get; }

		public bool useMask { get; }

		public SerializeMode jsonMode { get; }

		public SettingEnum(SerializeMode mode) => jsonMode = mode;

		public SettingEnum(Type enumTarget, SerializeMode json)
			: this(enumTarget, GetNames(enumTarget), false, json) {
		}

		private SettingEnum(
			Type enumTarget,
			string[] labelValues,
			bool mask,
			SerializeMode json) {
			enums = enumTarget;
			labels = labelValues;
			enumList = Enum.GetValues(enumTarget);
			datas = new int[enumList.Length];
			int index = 0;
			for (int length = enumList.Length; index < length; ++index)
				datas[index] = (int)enumList.GetValue(index);
			jsonMode = json;
			useMask = mask;
		}

		public enum SerializeMode {
			Int,
			String,
			Description,
		}

		private class EnumCache {
			internal int[] values;
			internal string[] names;
			internal GUIContent[] contents;
		}
	}
}