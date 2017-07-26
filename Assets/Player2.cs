using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player2 : MonoBehaviour {
	public CameraScript cams;
	public Controller controller;

	Vector3 vel;

	void Update(){
		cams.update ();

		Vector2 input;
		{
			input.x = input.y = 0f;
			if (Keybinds.MoveForward.Value) {
				input.y = 1f;
			}
			if (Keybinds.MoveRight.Value) {
				input.x = 1f;
			}
			if (Keybinds.MoveBack.Value) {
				input.y -= 1f;
			}
			if (Keybinds.MoveLeft.Value) {
				input.x -= 1f;
			}
			if (input.x != 0f && input.y != 0f) {
				input.x *= Helper.one_over_sqrt2;
				input.y *= Helper.one_over_sqrt2;
			}
		}

		Vector3 accel;
		{
			Vector3 forw = cams.forw;
			Vector3 side;
			side.x = forw.z;
			side.y = 0f;
			side.z = -forw.x;

			accel = forw * input.y;
			accel += side * input.x;
		}

		Vector3 step = accel * TL.dt;
		step.y = -1f;

		controller.Move (accel * TL.dt);
		cams.pos = controller.pos;
		cams.apply ();
	}
}
