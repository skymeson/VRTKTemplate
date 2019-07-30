using UnityEngine;
using System.Collections;
using StringBuilder = System.Text.StringBuilder;

namespace DoomtrinityFPSPrototype.Utils {
	// util class
	public static class Util {

		// ========================================================================================================================
		// nested struct PosRot_s

		// Transform structure which can be used in other structures to save the transform 
		[System.Serializable]
		public struct PosRot_s
		{
			public float X;
			public float Y;
			public float Z;
			public float RotX;
			public float RotY;
			public float RotZ;
		}

		// ========================================================================================================================
		// GetTransform

		public static PosRot_s GetTransform(PersistantMono obj) {
			PosRot_s transform_s = new PosRot_s ();
			if (obj != null) {
				transform_s.X = obj.transform.position.x;
				transform_s.Y = obj.transform.position.y;
				transform_s.Z = obj.transform.position.z;
				transform_s.RotX = obj.transform.localRotation.eulerAngles.x;
				transform_s.RotY = obj.transform.localRotation.eulerAngles.y;
				transform_s.RotZ = obj.transform.localRotation.eulerAngles.z;
			} else {
					Debug.LogError ("Util.GetTransform(): null object reference");
			}
			return transform_s;
		}

		// ========================================================================================================================
		// FindCommonPrefix

		public static string FindCommonPrefix(string[] ss ) {

			if (ss.Length > 0) {

				string str_x = ss[0];
				string str_y = string.Empty;
				StringBuilder sb = null;

				if (ss.Length > 1) {
					sb = new StringBuilder ();
					for (int i = 1; i < ss.Length; i++) {
						str_y = ss [i];
						if (string.IsNullOrEmpty (str_x) || string.IsNullOrEmpty (str_y)) {
							// Debug.LogWarning ("FindCommonPrefix: empty or null string in array!");// log warning
							return string.Empty;
						}
						int j = 0;
						int y_length = str_y.Length;
						foreach (char c in str_x) {
							if ((j==0) && (c!=str_y [j])) {
								break;
							}
							if (j < y_length) {
								if (c == str_y [j]) {
									sb.Append (c);
								} else {
									break;
								}
							} else {
								break;
							}
							j++;
						}
						str_x = sb.ToString ();
						sb.Remove (0,sb.Length);
					}
					return str_x;
				} else {
					return ss[0];
				}
			} else {
				// Debug.LogWarning ("FindCommonPrefix: empty array!");
				return string.Empty;
			}			
		}

		// ========================================================================================================================
		// isNullOrWhiteSpace

		public static bool isNullOrWhiteSpace(string s){
			if (string.IsNullOrEmpty (s)) {
				return true;
			}
			for (int i = 0; i < s.Length; i++) {
				if (!char.IsWhiteSpace (s [i])) {
					return false;
				}
			}
			return true;
		}

		// ========================================================================================================================
	}
}