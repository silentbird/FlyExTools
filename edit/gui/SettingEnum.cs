using pure.utils.memory;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

namespace edit.gui {
	public class SettingEnum : DisplaySetting {
		private static readonly Dictionary<Type, SettingEnum.EnumCache> caches = new Dictionary<Type, SettingEnum.EnumCache>();

		private static SettingEnum.EnumCache getcache(Type enumType) {
			SettingEnum.EnumCache enumCache1;
			if (SettingEnum.caches.TryGetValue(enumType, out enumCache1))
				return enumCache1;
			SettingEnum.EnumCache enumCache2 = new SettingEnum.EnumCache();
			string[] names = Enum.GetNames(enumType);
			enumCache2.names = new string[names.Length];
			enumCache2.contents = new GUIContent[names.Length];
			enumCache2.values = new int[names.Length];
			for (int index = 0; index < names.Length; ++index)
				enumCache2.values[index] = (int)Enum.Parse(enumType, names[index]);
			for (int index = 0; index < names.Length; ++index) {
				object[] customAttributes = enumType.GetField(names[index]).GetCustomAttributes(typeof(DescriptionAttribute), false);
				DescriptionAttribute descriptionAttribute = customAttributes.Length != 0 ? customAttributes[0] as DescriptionAttribute : (DescriptionAttribute)null;
				string str = descriptionAttribute != null ? descriptionAttribute.Description : names[index];
				enumCache2.names[index] = str;
				enumCache2.contents[index] = new GUIContent(str, str);
			}

			SettingEnum.caches.Add(enumType, enumCache2);
			return enumCache2;
		}

		public static int[] GetValues(Type enumType) => SettingEnum.getcache(enumType).values;

		public static string[] GetNames(Type enumType) => SettingEnum.getcache(enumType).names;

		public static GUIContent[] GetContents(Type enumType) {
			return SettingEnum.getcache(enumType).contents;
		}

		public static string GetName<T>(T val) {
			int hashCode = val.GetHashCode();
			SettingEnum.EnumCache enumCache = SettingEnum.getcache(typeof(T));
			int index = Array.IndexOf<int>(enumCache.values, hashCode);
			return index < 0 || index >= enumCache.names.Length ? "error" : enumCache.names[index];
		}

		public static string GetName<T>(int val) {
			int hashCode = val.GetHashCode();
			SettingEnum.EnumCache enumCache = SettingEnum.getcache(typeof(T));
			int index = Array.IndexOf<int>(enumCache.values, hashCode);
			return index < 0 || index >= enumCache.names.Length ? "error" : enumCache.names[index];
		}

		public static string GetMaskNames<T>(T val) {
			string[] names = SettingEnum.GetNames(val.GetType());
			int hashCode = val.GetHashCode();
			SettingEnum.EnumCache enumCache = SettingEnum.getcache(typeof(T));
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

		public SettingEnum.SerializeMode jsonMode { get; }

		public SettingEnum(SettingEnum.SerializeMode mode) => this.jsonMode = mode;

		public SettingEnum(Type enumTarget, SettingEnum.SerializeMode json)
			: this(enumTarget, SettingEnum.GetNames(enumTarget), false, json) {
		}

		private SettingEnum(
			Type enumTarget,
			string[] labelValues,
			bool mask,
			SettingEnum.SerializeMode json) {
			this.enums = enumTarget;
			this.labels = labelValues;
			this.enumList = Enum.GetValues(enumTarget);
			this.datas = new int[this.enumList.Length];
			int index = 0;
			for (int length = this.enumList.Length; index < length; ++index)
				this.datas[index] = (int)this.enumList.GetValue(index);
			this.jsonMode = json;
			this.useMask = mask;
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