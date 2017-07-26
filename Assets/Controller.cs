using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour {

	// camera
	public Transform camt;
	float yaw;
	float pitch;
	float sinyaw;
	float cosyaw;
	float Sensitivity = 0.006f;
	float acceleration = 10f;
	Vector2 input;
	Vector3 aim;
	Vector3 forw;

	// things
	public Vector3 pos;
	public Quaternion rot;
	public Collider col;

	int overlapCount;
	Collider[] cols;
	bool[] ignoreCols;
	Vector3 half_extents;

	void Start(){
		half_extents = new Vector3 (5f, 5f, 5f);

		cols = new Collider[16];
		ignoreCols = new bool[16];
	}

	void InputProcessing(){
		{
			yaw += CInput.GetAxis ("Mouse X") * Sensitivity;

			if (yaw > Helper.tau) {
				yaw -= Helper.tau;
			} else if (yaw < -Helper.tau) {
				yaw += Helper.tau;
			}

			/*if (InvertMouse) pitch += CInput.GetAxis ("Mouse Y") * Sensitivity;
			else */
			pitch -= CInput.GetAxis ("Mouse Y") * Sensitivity;
			pitch = Mathf.Clamp (pitch, -Helper.halfpi, Helper.halfpi);
			sinyaw = Mathf.Sin (-yaw);
			cosyaw = Mathf.Cos (yaw);
			float sinpitch = Mathf.Sin (pitch);
			float cospitch = Mathf.Cos (pitch);

			aim.x = forw.x = -sinyaw;
			forw.y = 0f;
			aim.z = forw.z = cosyaw;

			aim.x *= cospitch;
			aim.y = -sinpitch;
			aim.z *= cospitch;

			rot = Helper.Euler (pitch, yaw);
		}

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

	void Update () {
		InputProcessing ();

		Vector3 accel;
		{
			Vector3 side;
			side.x = cosyaw;
			side.y = 0f;
			side.z = sinyaw;

//			 2D
//			accel =  input.y * forw;

			// 3D
			accel =  input.y * aim;

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

		Debug.Log (iterations);
	}
}
