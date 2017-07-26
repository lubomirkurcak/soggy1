using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Helper {
	static Helper(){
		{
			int PlayerLayer = 8;

			PlayerCollisionMask = 0;
			for (int i = 0; i < 32; ++i) {
				PlayerCollisionMask <<= 1;

				if (Physics.GetIgnoreLayerCollision (PlayerLayer, i) == false) {
					PlayerCollisionMask |= 1;
				}
			}
		}
	}

	public const float tau = 6.283185307179586476925286766559f;
	public const float pi = tau * 0.5f;
	public const float halfpi = tau * 0.25f;
	public const float quarterpi = tau * 0.125f;
	public const float one_over_sqrt2 = 0.70710678118654752440084436210485f;
	public static Vector3 v3zero = new Vector3 (0, 0, 0);
	public static Vector3 v3up = new Vector3 (0, 1, 0);
	public static Vector3 v3down = new Vector3 (0, -1, 0);
	public static Vector3 v3forw = new Vector3 (0, 0, 1);
	public static int PlayerCollisionMask = -1;

	public static float Square(float Value){
		return Value * Value;
	}

	public static float Map(float Value, float Min, float Max){
		return (Value - Min) / (Max - Min);
	}

	public static float Inner(Vector3 v1, Vector3 v2){
		return v1.x * v2.x + v1.y * v2.y + v1.z * v2.z;
	}

	public static float InnerXZ(Vector3 v1, Vector3 v2){
		return v1.x * v2.x + v1.z * v2.z;
	}

	public static bool TryParseFloat(string arg, out float f){
		return float.TryParse (arg, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out f);
	}

	public static void ParseFloat(string arg, ref float f){
		float result;
		if (float.TryParse (arg, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out result)) {
			f = result;
		}
	}

	public static void ParseInt(string arg, ref int i){
		int result;
		if (int.TryParse (arg, out result)) {
			i = result;
		}
	}

	public static bool ParseBool(string arg){
		switch(arg){
		case "0":
		case "false":
		case "off":
			return false;
		default:
			return true;
		}
	}

	public static Quaternion Euler(float x, float y, float z) {
		x *= 0.5f;
		y *= 0.5f;
		z *= 0.5f;

		float c1 = Mathf.Cos(y);
		float s1 = Mathf.Sin(y);
		float c2 = Mathf.Cos(z);
		float s2 = Mathf.Sin(z);
		float c3 = Mathf.Cos(x);
		float s3 = Mathf.Sin(x);

		float c1c2 = c1*c2;
		float s1s2 = s1*s2;

		Quaternion q;
		q.w =c1c2*c3 - s1s2*s3;
		q.x =c1c2*s3 + s1s2*c3;
		q.y =s1*c2*c3 + c1*s2*s3;
		q.z =c1*s2*c3 - s1*c2*s3;

		return q;
	}

	public static Quaternion Euler(float x, float y) {
		x *= 0.5f;
		y *= 0.5f;

		float cy = Mathf.Cos(y);
		float sy = Mathf.Sin(y);
		float cx = Mathf.Cos(x);
		float sx = Mathf.Sin(x);

		Quaternion q;
		q.w = cy*cx;
		q.x = cy*sx;
		q.y = sy*cx;
		q.z =-sy*sx;

		return q;
	}

	public static Quaternion Euler(float z) {
		z *= 0.5f;
		return new Quaternion (Mathf.Cos(z), 0f, 0f, Mathf.Sin(z));
	}

	// NOTE(lubomir): Gimmicks below
	public static string UnitPrefix(int order){
		switch (order) {
		case 0:
			return string.Empty;
		case 1:
			return "k";
		case 2:
			return "M";
		case 3:
			return "G";
		case 4:
			return "T";
		case 5:
			return "P";
		case 6:
			return "E";
		case -1:
			return "m";
		case -2:
			return "μ";
		case -3:
			return "n";
		case -4:
			return "p";
		case -5:
			return "f";
		case -6:
			return "a";
		default:
			return "?";
		}
	}

	public static string FormatFloat(float f, int min_unit_prefix = -6, int max_unit_prefix = 6){
		int k = 0;
		{
			const int max_prefix = 6;
			const int min_prefix = -6;
			min_unit_prefix = Mathf.Max (min_unit_prefix, min_prefix);
			max_unit_prefix = Mathf.Min (max_unit_prefix, max_prefix);

			float test;
			test = f;
			while (k < max_unit_prefix && (test *= 0.001f) >= 0.995f) {
				f = test;

				k++;
			}

			if (k <= 0) {
				test = f;
				while (k > min_unit_prefix && test < 0.995f) {
					test *= 1000f;
					f = test;

					k--;
				}
			}
		}

		string result;
		if (f < 9.95f) {
			result = f.ToString ("0.0");
		} else if (f < 99.5f) {
			result = f.ToString ("0");
		} else {
			result = (f * 0.1f).ToString ("0") + "0";
		}

		return result + " " + UnitPrefix (k);
	}

	/*public static Vector3 Multiply(Quaternion q, Vector3 v){
		float num = q.x * 2f;
		float num2 = q.y * 2f;
		float num3 = q.z * 2f;
		float num4 = q.x * num;
		float num5 = q.y * num2;
		float num6 = q.z * num3;
		float num7 = q.x * num2;
		float num8 = q.x * num3;
		float num9 = q.y * num3;
		float num10 = q.w * num;
		float num11 = q.w * num2;
		float num12 = q.w * num3;
		Vector3 result;
		result.x = (1f - (num5 + num6)) * v.x + (num7 - num12) * v.y + (num8 + num11) * v.z;
		result.y = (num7 + num12) * v.x + (1f - (num4 + num6)) * v.y + (num9 - num10) * v.z;
		result.z = (num8 - num11) * v.x + (num9 + num10) * v.y + (1f - (num4 + num5)) * v.z;
		return result;
	}*/
}
