using pure.database;
using pure.utils.debug;
using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace pure.utils.color {
	public static class ColorUtils {
		private static Dictionary<string, ColorPlatter> h_platters = new Dictionary<string, ColorPlatter>();
		private const float INV_FF = 0.003921569f;

		public static void InitPlatters(ColorPlatter[] platters) {
			ColorUtils.h_platters.Clear();
			foreach (ColorPlatter platter in platters) {
				if (ColorUtils.h_platters.ContainsKey(platter.key))
					GlobalLogger.Log(string.Format("color key:{0} conflict", (object)platter.key));
				else
					ColorUtils.h_platters.Add(platter.key, platter);
			}
		}

		public static bool TryGetColor(string key, out string color) {
			ColorPlatter colorPlatter;
			if (ColorUtils.h_platters.TryGetValue(key, out colorPlatter)) {
				if (string.IsNullOrEmpty(colorPlatter.hexColor))
					colorPlatter.hexColor = ColorUtils.ColorToString(colorPlatter.color);
				color = colorPlatter.hexColor;
				return true;
			}

			color = string.Empty;
			return false;
		}

		public static bool TryGetColor(string key, out Color color) {
			ColorPlatter colorPlatter;
			if (ColorUtils.h_platters.TryGetValue(key, out colorPlatter)) {
				if (string.IsNullOrEmpty(colorPlatter.hexColor))
					colorPlatter.hexColor = ColorUtils.ColorToString(colorPlatter.color);
				color = colorPlatter.color;
				return true;
			}

			color = Color.white;
			return false;
		}

		public static int ColorToInt(Color c) {
			return (int)((double)c.a * (double)byte.MaxValue) << 24 | (int)((double)c.r * (double)byte.MaxValue) << 16 | (int)((double)c.g * (double)byte.MaxValue) << 8 |
			       (int)((double)c.b * (double)byte.MaxValue);
		}

		public static Color IntToColor(int v) {
			return new Color() {
				a = (float)(v >> 24 & (int)byte.MaxValue) * 0.003921569f,
				r = (float)(v >> 16 & (int)byte.MaxValue) * 0.003921569f,
				g = (float)(v >> 8 & (int)byte.MaxValue) * 0.003921569f,
				b = (float)(v & (int)byte.MaxValue) * 0.003921569f
			};
		}

		public static Color Mix(Color a, Color b) => a * b;

		// public static Color StringToColor(string str) {
		// 	if (string.IsNullOrEmpty(str))
		// 		return Color.white;
		// 	if (str.StartsWith("0x"))
		// 		str = str.Substring(2, str.Length - 2);
		// 	if (str.StartsWith("#"))
		// 		str = str.Substring(1, str.Length - 1);
		// 	if (ColorUtils.h_platters.Count == 0)
		// 		ConfigureData_Dll.instance.Init();
		// 	ColorPlatter colorPlatter;
		// 	if (ColorUtils.h_platters.TryGetValue(str, out colorPlatter))
		// 		return colorPlatter.color;
		// 	uint result;
		// 	if (!uint.TryParse(str, NumberStyles.HexNumber, (IFormatProvider)CultureInfo.CurrentCulture, out result))
		// 		return Color.white;
		// 	return new Color() {
		// 		r = (float)(result >> 24 & (uint)byte.MaxValue) / (float)byte.MaxValue,
		// 		g = (float)(result >> 16 & (uint)byte.MaxValue) / (float)byte.MaxValue,
		// 		b = (float)(result >> 8 & (uint)byte.MaxValue) / (float)byte.MaxValue,
		// 		a = (float)(result & (uint)byte.MaxValue) / (float)byte.MaxValue
		// 	};
		// }

		public static Color Html2Color(string hex) {
			if (string.IsNullOrEmpty(hex))
				return Color.black;
			hex = hex.ToLower();
			if (hex.IndexOf("#", StringComparison.Ordinal) == 0 && hex.Length == 7)
				return new Color((float)Convert.ToInt32(hex.Substring(1, 2), 16) / (float)byte.MaxValue, (float)Convert.ToInt32(hex.Substring(3, 2), 16) / (float)byte.MaxValue,
					(float)Convert.ToInt32(hex.Substring(5, 2), 16) / (float)byte.MaxValue);
			switch (hex) {
				case "black":
					return Color.black;
				case "blue":
					return Color.blue;
				case "cyan":
					return Color.cyan;
				case "gray":
					return Color.gray;
				case "green":
					return Color.green;
				case "grey":
					return Color.grey;
				case "magenta":
					return Color.magenta;
				case "red":
					return Color.red;
				case "white":
					return Color.white;
				case "yellow":
					return Color.yellow;
				default:
					return Color.black;
			}
		}

		public static string ColorToString(Color color) {
			return string.Format("{0:X2}{1:X2}{2:X2}{3:X2}", (object)(uint)((double)color.r * (double)byte.MaxValue), (object)(uint)((double)color.g * (double)byte.MaxValue),
				(object)(uint)((double)color.b * (double)byte.MaxValue), (object)(uint)((double)color.a * (double)byte.MaxValue));
		}
	}
}