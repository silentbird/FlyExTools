using pure.ui;
using pure.utils.color;
using System;
using UnityEngine;

namespace pure.database {
	[Serializable]
	public class ColorPlatter {
		public Color color = Color.white;
		public string key;
		public string name;
		internal string hexColor;

		public void Init(IValueField<Color> comp) {
			Color color;
			if (!Application.isPlaying || !ColorUtils.TryGetColor(this.key, out color))
				return;
			comp.SetValue(color);
		}
	}
}