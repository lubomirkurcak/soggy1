using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour {

	// things
	public CameraScript cams;
	public Vector3 pos;
	public Quaternion rot;
	public Collider col;

	int overlapCount;
	Collider[] cols;
	bool[] ignoreCols;
	Vector3 half_extents;
	Vector2 input;

	float acceleration = 10f;

	void Start(){
		half_extents = new Vector3 (5f, 5f, 5f);

		cols = new Collider[16];
		ignoreCols = new bool[16];
	}

	void InputProcessing(){
		{
			input.x = 0f;
			input.y = 0f;

			if (Input.GetKey (KeyCode.W)) {
				input.y = 1f;
			}
			if (Input.GetKey (KeyCode.S)) {
				input.y -= 1f;
			}
			if (Input.GetKey (KeyCode.D)) {
				input.x = 1f;
			}
			if (Input.GetKey (KeyCode.A)) {
				input.x -= 1f;
			}
			if (input.x != 0f && input.y != 0f) {
				input.x *= Helper.one_over_sqrt2;
				input.y *= Helper.one_over_sqrt2;
			}
		}
	}

	public void update () {
		InputProcessing ();

		Vector3 accel;
		{
			Vector3 forw = cams.forw;
			Vector3 side;
			side.x = forw.z;
			side.y = 0f;
			side.z = -forw.x;

			// 2D
			accel =  input.y * forw;
			// 3D
//			accel =  input.y * cams.aim;

			accel += input.x * side;
		}

		pos += accel * (TL.dt * acceleration);

		overlapCount = Physics.OverlapBoxNonAlloc (pos, half_extents, cols, Quaternion.identity, Helper.PlayerCollisionMask, QueryTriggerInteraction.Ignore);
		for (int i = 0; i < overlapCount; i++) {
			ignoreCols [i] = false;
		}

		int computes;
		int iterations = 0;
		const int max_iterations = 10;

		iteration:
		iterations++;
		computes = 0;
		for (int i = 0; i < overlapCount; i++) {
			Collider c = cols[i];

			if (c == col) {
				continue;
			}

			Transform t = c.transform;

			Vector3 dir;
			float dist;
			if (Physics.ComputePenetration (col, pos, rot, c, t.position, t.rotation, out dir, out dist)) {

				if (dist < 0.005f) {
					ignoreCols [i] = true;
				} else {
					computes++;
				}

				Vector3 penetration = dir * dist;
				pos += penetration;
			}
		}

		if (computes > 0 && iterations < max_iterations) {
			goto iteration;
		}

		transform.localPosition = pos;
	}
}
