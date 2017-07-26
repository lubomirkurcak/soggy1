/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharController : MonoBehaviour {
	public float Height;
	public float Radius;
	public float Speed;
	const float reserve = 0.001f;

	int CollisionLayer = -1;
	public bool touching_ground;
	public bool sliding;
	RaycastHit hit;

	float yaw;
	float pitch;
	bool InvertMouse = false;
	float Sensitivity = 0.006f;
	public Quaternion rot;

	public Vector3 pos;
	public Vector3 vel;
	public Vector3 eye;

	Vector2 input;
	bool Jump;
	int InputID;

	void Awake(){
		InputID = CInput.Register ();
		CInput.Activate (InputID);
	}

	Vector3 Debug_Respawn;
	void __DEBUG__(){
		if(touching_ground && CInput.GetKeyDown(KeyCode.Return)){
			Debug_Respawn = pos;
		}

		if (CInput.GetKeyDown (KeyCode.R)) {
			pos = Debug_Respawn;
			vel = Vector3.zero;
		}

		if (CInput.GetKeyDown (KeyCode.LeftShift)) {
			vel = rot * Vector3.forward * 20f;
		}
	}

	void Update(){
		__DEBUG__ ();

		// Request input
		CInput.Requester = InputID;

		// Mouse
		yaw += CInput.GetAxis ("Mouse X") * Sensitivity;

		if (yaw > Helper.tau) {
			yaw -= Helper.tau;
		} else if (yaw < -Helper.tau){
			yaw += Helper.tau;
		}

		if (InvertMouse) pitch += CInput.GetAxis ("Mouse Y") * Sensitivity;
		else pitch -= CInput.GetAxis ("Mouse Y") * Sensitivity;
		pitch = Mathf.Clamp (pitch, -Mathf.PI*0.5f, Mathf.PI*0.5f);

		// Calculate rotation
		float sinyaw = Mathf.Sin (-yaw);
		float cosyaw = Mathf.Cos (yaw);
		rot = Quaternion.Euler (pitch*Mathf.Rad2Deg, yaw*Mathf.Rad2Deg, 0f);

		// Input
		input.x = input.y = 0f;
		if (CInput.GetKey (Keybindings.Active.moveForward)) {
			input.y = 1f;
		}
		if (CInput.GetKey (Keybindings.Active.moveRight)) {
			input.x = 1f;
		}
		if (CInput.GetKey (Keybindings.Active.moveBack)) {
			input.y -= 1f;
		}
		if (CInput.GetKey (Keybindings.Active.moveLeft)) {
			input.x -= 1f;
		}
		if (input.x != 0f && input.y != 0f) {
			input.x *= Helper.one_over_sqrt2;
			input.y *= Helper.one_over_sqrt2;
		}
		Jump = CInput.GetKeyDown (Keybindings.Active.jump) || CInput.GetAxis ("Mouse ScrollWheel") != 0f;


		float ax = (input.x * cosyaw - input.y * sinyaw);
		float az = (input.x * sinyaw + input.y * cosyaw);

		vel.x = ax * Speed;
		vel.z = az * Speed;

		if (touching_ground) {
			if (Jump) {
				const float JumpVel = 4f;
				vel.y = JumpVel;
			}
		} else {
			const float Gravity = 9.81f;
			vel.y -= TL.dt * Gravity;
		}

		Vector3 step = vel * TL.dt;
		if (touching_ground && Jump == false) {
//			step.y = -0.1f;
		}

		if (step.y > 0f) {
			touching_ground = false;
		}

		MOVES = 0;
		Move (step);
//		CheckTouchingGround ();

		eye = pos;
		eye.y += 0.5f;
		transform.localPosition = pos;
	}

	public int MOVES;
	public void Move(Vector3 move){
		MOVES++;
		float distance = move.magnitude;
		const float min_distance = 0.001f;
		if (distance <= min_distance) {
			return;
		}

		Vector3 direction = move / distance;
		float HalfHeight = Height * 0.5f - Radius;
		Vector3 p1 = pos;
		p1.y += HalfHeight;
		Vector3 p2 = pos;
		p2.y -= HalfHeight;

		if (Physics.CapsuleCast (p1, p2, Radius, direction, out hit, distance, CollisionLayer, QueryTriggerInteraction.Ignore)) {
			Vector3 normal = hit.normal;

			if (normal.y > 0f) {
				touching_ground = true;
			}

			// Subtract successful motion
			Vector3 success = hit.distance * direction;
			move -= success;
			pos += success;

			bool walkable = normal.y >= Helper.one_over_sqrt2;

			if (walkable) {
				float one_over_ny = 1f / normal.y;

				pos.y += reserve * one_over_ny;
				move.y = Mathf.Sqrt (move.x * move.x + move.z * move.z) * one_over_ny;
//				move.x -= project * normal.x;
//				move.z -= project * normal.z;
			} else {
				pos += normal * reserve;
				
				float project;
				project = Helper.Inner (move, normal);
				move -= project * normal;
				
				project = Helper.Inner (vel, normal);
				vel -= project * normal;
			}

			Move (move);
		} else {
			pos += distance * direction;
		}
	}

	/*public void CheckTouchingGround(){
		Vector3 p = pos;
		p.y -= Height * 0.5f - Radius;
		const float testDistance = 0.1f;
//		touching_ground = Physics.SphereCast (p, Radius, Helper.v3down, out hit, reserve, CollisionLayer, QueryTriggerInteraction.Ignore);
//		touching_ground = Physics.SphereCast (p, Radius, Helper.v3down, out hit, testDistance, CollisionLayer, QueryTriggerInteraction.Ignore);

		if (Physics.SphereCast (p, Radius, Helper.v3down, out hit, testDistance, CollisionLayer, QueryTriggerInteraction.Ignore)) {
			pos.y -= hit.distance;
			pos.y += reserve;

			touching_ground = true;
		} else {
			touching_ground = false;
		}
	}*//*
}
*/