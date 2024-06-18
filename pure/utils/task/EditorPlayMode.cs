using System;
using UnityEditor;

namespace pure.utils.task {
	public static class EditorPlayMode {
		public static Action OnPlayModeChanged;

		[InitializeOnLoadMethod]
		internal static void Init() {
			EditorApplication.playModeStateChanged += new Action<PlayModeStateChange>(EditorPlayMode.OnPlayingOrWillChangePlayMode);
		}

		private static void OnPlayingOrWillChangePlayMode(PlayModeStateChange st) {
			bool flag;
			switch ((int)st) {
				case 0:
					flag = false;
					break;
				case 1:
					flag = true;
					break;
				case 2:
					flag = true;
					break;
				case 3:
					flag = false;
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(st), (object)st, (string)null);
			}

			if (EditorPlayMode.isPlaying == flag)
				return;
			EditorPlayMode.isPlaying = flag;
			Action onPlayModeChanged = EditorPlayMode.OnPlayModeChanged;
			if (onPlayModeChanged == null)
				return;
			onPlayModeChanged();
		}

		public static bool isPlaying { get; private set; }
	}
}