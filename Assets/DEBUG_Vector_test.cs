using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DEBUG_Vector_test : MonoBehaviour {
	
	public LineRenderer normal;
	public LineRenderer downl;
	public LineRenderer test;
	public Vector3 vector;
	public Vector3 n;

	Vector3 down;

	void Update () {
		n = vector.normalized;
		normal.SetPosition (1, n);

		down = Vector3.RotateTowards(n, Vector3.down, 90f * Mathf.Deg2Rad, 0f);
		downl.SetPosition (1, down);

		down = RotateDown90 (n);
		down.Normalize ();
		test.SetPosition (1, down);
	}

	Vector3 RotateDown90(Vector3 v){
//		return Cross (v, Cross (v, Vector3.up));

		Vector3 r;
		r.x = v.y * v.x;
		r.y = - v.z * v.z - v.x * v.x;
		r.z = v.y * v.z;

		return r;
	}

	Vector3 Cross(Vector3 a, Vector3 b){
		Vector3 r;

		r.x = a.y * b.z - a.z * b.y;
		r.y = a.z * b.x - a.x * b.z;
		r.z = a.x * b.y - a.y * b.x;

		return r;
	}
}
