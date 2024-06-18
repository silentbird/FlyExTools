using System;
using UnityEditor;
using UnityEngine;


namespace edit.etui.utils {
	public static class EditPrefabUtility {
		public static GameObject GetPrefab(GameObject obj) {
			switch ((int)PrefabUtility.GetPrefabAssetType(obj)) {
				case 0:
				case 4:
					return (GameObject)null;
				case 1:
				case 2:
				case 3:
					switch ((int)PrefabUtility.GetPrefabInstanceStatus(obj)) {
						case 0:
							return obj;
						case 1:
							return PrefabUtility.GetCorrespondingObjectFromSource<GameObject>(obj);
						case 2:
						case 3:
							return (GameObject)null;
						default:
							throw new ArgumentOutOfRangeException();
					}
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		public static bool IsPrefab(GameObject obj) {
			switch ((int)PrefabUtility.GetPrefabAssetType(obj)) {
				case 0:
				case 4:
					return false;
				case 1:
				case 2:
				case 3:
					switch ((int)PrefabUtility.GetPrefabInstanceStatus(obj)) {
						case 0:
							return obj != null;
						case 1:
							return false;
						case 2:
						case 3:
							return false;
						default:
							throw new ArgumentOutOfRangeException();
					}
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		public static bool IsPrefabInstance(GameObject obj) {
			switch ((int)PrefabUtility.GetPrefabAssetType(obj)) {
				case 0:
				case 4:
					return false;
				case 1:
				case 2:
				case 3:
					switch ((int)PrefabUtility.GetPrefabInstanceStatus(obj)) {
						case 0:
							return false;
						case 1:
							return true;
						case 2:
						case 3:
							return false;
						default:
							throw new ArgumentOutOfRangeException();
					}
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
	}
}