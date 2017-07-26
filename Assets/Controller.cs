using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour {
	public Collider col;
	[System.NonSerialized]public Vector3 pos;
	[System.NonSerialized]public Quaternion rot;

	[System.NonSerialized]public Vector3 up;
	[System.NonSerialized]public bool grounded;

	int overlapCount;
	Collider[] cols;
	bool[] ignoreCols;
	Vector2 input;

	void Start(){
		up = Helper.v3up;
		const int MaxCollisions = 16;
		cols = new Collider[MaxCollisions];
		ignoreCols = new bool[MaxCollisions];
	}

	public void Move(Vector3 step){
		Vector3 half_extents = step * 0.5f;
		Vector3 center = pos + half_extents;
		half_extents += col.bounds.extents;
//		half_extents += col.bounds.size;

		overlapCount = Physics.OverlapBoxNonAlloc (center, half_extents, cols, Quaternion.identity, Helper.PlayerCollisionMask, QueryTriggerInteraction.Ignore);
		for (int i = 0; i < overlapCount; i++) {
			ignoreCols [i] = false;
		}

		pos += step;

		grounded = false;

		const int max_iterations = 10;
		for (int iter = 0; iter < max_iterations; iter++) {
			int computes = 0;
			for (int i = 0; i < overlapCount; i++) {
				Collider c = cols[i];

				if (c == col) {
					continue;
				}

				Transform t = c.transform;

				Vector3 dir;
				float dist;
				if (Physics.ComputePenetration (col, pos, rot, c, t.position, t.rotation, out dir, out dist)) {

					if (Helper.Inner (dir, up) > 0f) {
						grounded = true;
					}

					if (dist < 0.005f) {
						ignoreCols [i] = true;
					} else {
						computes++;
					}

					Vector3 penetration = dir * dist;
					pos += penetration;
				}
			}

			if (computes <= 0) {
				break;
			}
		}
	}
}
