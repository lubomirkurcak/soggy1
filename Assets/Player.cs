#define PLATFORM_SUPPORT
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

	public Char c;
	public CharacterController cc;
//	public CharController charc;
	public Transform me;

	public Vector3 getVel{
		get{
			if (grounded) {
				return new Vector3 (vel.x, 0f, vel.z);
			}
			return vel;
		}
	}

	[System.NonSerialized]public float yaw;
	[System.NonSerialized]public float pitch;
	[System.NonSerialized]public Quaternion q;
	[System.NonSerialized]public Vector3 pos;
	[System.NonSerialized]public Vector3 vel;
	[System.NonSerialized]public Vector3 eye;
	[System.NonSerialized]public bool grounded;
	[System.NonSerialized]public bool crouching;
	[System.NonSerialized]public float slope;
	Vector3 step;
	Vector2 input;
	float sinyaw;
	float cosyaw;
	//	bool touching_ground; // TODO(lubomir): Reimplement this for turrets?
	bool walkable_ground;
	float lastY;
	bool last_grounded;

	float slope_limit = Mathf.Cos (49f * Mathf.Deg2Rad);

//	Vector3 platformVel;
//	float crouched_accel_multiplier = 0.33f;

	public float paralyzed;

	Vector3 Debug_Respawn = new Vector3(0,6,0);

	public enum State{
		Normal,
		Air
	}

	[System.NonSerialized]public State state = State.Air;

	[Space(10)]
	public bool InvertMouse = false;
	public float Sensitivity = 0.006f;
	[Space(10)]
	public float MaxSpeed;
	public float Accel;
	public float MaxAirSpeed;
	public float AirAccel;
	public float MaxSpeedCrouch;
	public float CrouchAccel;
	public float CrouchAirAccel;
	public float DynamicFriction;
	public float StaticFriction;
	public float Gravity = 9.81f;
	public float JumpVel = 4.5f;

//	float liftOffHeight;

	// Sounds
	public AudioSource footsteps;
	public AudioClip jump;
	public AudioClip land;
	public AudioClip concrete;
	public AudioClip stone;
	public AudioClip wood;
	float footstep_length = 1.3f;
	float footstep_distance;

	const float capsule_radius = 0.4f;

	int InputID;
	void Awake(){
		InputID = CInput.Register ();
		CInput.Activate (InputID);

		cc.enableOverlapRecovery = false;
	}

	void Start(){
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;

		Console.AddCommand ("accel", (string arg) => {
			Helper.ParseFloat(arg, ref Accel);
		});
		Console.AddCommand ("fric", (string arg) => {
			Helper.ParseFloat(arg, ref DynamicFriction);
		});
		Console.AddCommand ("static_fric", (string arg) => {
			Helper.ParseFloat(arg, ref StaticFriction);
		});
		Console.AddCommand ("max_speed", (string arg) => {
			Helper.ParseFloat(arg, ref MaxSpeed);
		});
		Console.AddCommand ("air_accel", (string arg) => {
			Helper.ParseFloat(arg, ref AirAccel);
		});
		Console.AddCommand ("max_air_speed", (string arg) => {
			Helper.ParseFloat(arg, ref MaxAirSpeed);
		});
		Console.AddCommand ("footstep", (string arg) => {
			Helper.ParseFloat(arg, ref footstep_length);
		});
		Console.AddCommand ("sensitivity", (string arg) => {
			Helper.ParseFloat(arg, ref Sensitivity);
		});
		Console.AddCommand ("mouse_invert", (string arg) => {
			InvertMouse = Helper.ParseBool(arg);
		});
		Console.AddCommand ("gravity", (string arg) => {
			Helper.ParseFloat(arg, ref Gravity);
			Physics.gravity = new Vector3(0,-Gravity,0);
		});
		Console.AddCommand ("jump", (string arg) => {
			Helper.ParseFloat(arg, ref JumpVel);
		});
		Console.AddCommand ("slope_limit", (string arg) => {
			float f;
			if(Helper.TryParseFloat(arg, out f)){
				f = Mathf.Clamp(f, 0, 180);
				slope_limit = Mathf.Cos(f * Mathf.Deg2Rad);
			}
		});
	}

	public void Kill(){
		transform.localPosition = Debug_Respawn;
		vel = Vector3.zero;
	}

	void __DEBUG__(){
		if(grounded && CInput.GetKeyDown(KeyCode.Return)){
			Debug_Respawn = pos;
		}

		if (CInput.GetKeyDown (KeyCode.R)) {
			transform.localPosition = Debug_Respawn;
			vel = Vector3.zero;

			World.Reset ();
		}

		if (CInput.GetKey (KeyCode.LeftShift)) {
			vel = q * Vector3.forward * 20f;
		}
	}

	public void update(){
		// TODO(lubomir): Remove
		__DEBUG__ ();

		// NOTE(lubomir): Request input
		CInput.Requester = InputID;

		// NOTE(lubomir): Arrows camera controls
		const float arrowSens = 4f;
		if (CInput.GetKey (KeyCode.RightArrow)) {
			yaw += TL.dt * arrowSens;
		}
		if (CInput.GetKey (KeyCode.LeftArrow)) {
			yaw -= TL.dt * arrowSens;
		}
		if (CInput.GetKey (KeyCode.UpArrow)) {
			pitch -= TL.dt * arrowSens;
		}
		if (CInput.GetKey (KeyCode.DownArrow)) {
			pitch += TL.dt * arrowSens;
		}

		// NOTE(lubomir): Mouse camera controls
		yaw += CInput.GetAxis ("Mouse X") * Sensitivity;

		if (yaw > Helper.tau) {
			yaw -= Helper.tau;
		} else if (yaw < -Helper.tau){
			yaw += Helper.tau;
		}

		if (InvertMouse) pitch += CInput.GetAxis ("Mouse Y") * Sensitivity;
		else pitch -= CInput.GetAxis ("Mouse Y") * Sensitivity;
		pitch = Mathf.Clamp (pitch, -Helper.halfpi, Helper.halfpi);

		// NOTE(lubomir): Calculate rotation
		sinyaw = Mathf.Sin (-yaw);
		cosyaw = Mathf.Cos (yaw);
		q = Helper.Euler (pitch, yaw);

		// NOTE(lubomir): Calculate input vector
		// TODO(lubomir): Add controller version
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

		// NOTE(lubomir): Crouching
		if (crouching) {
			const float collider_skin_thickness = 0.08f;

			// NOTE(lubomir): Players tries to stop crouching
			if (Keybinds.Crouch.Value == false) {
				// NOTE(lubomir): Check is there's enough space to stand up
				RaycastHit hit;
				float spaceUp = float.PositiveInfinity;
				float spaceDown = float.PositiveInfinity;
				if (Physics.SphereCast (pos, capsule_radius, Helper.v3up, out hit, 1f, Helper.PlayerCollisionMask)) {
					spaceUp = hit.distance - 0.1f - collider_skin_thickness;
				}
				if (Physics.SphereCast (pos, capsule_radius, Helper.v3down, out hit, 1f, Helper.PlayerCollisionMask)) {
					spaceDown = hit.distance - 0.1f - collider_skin_thickness;
				}

				// NOTE(lubomir): If there is enough space, resize collider
				// and move it so that it doesn't clip with geometry
				if (spaceUp + spaceDown >= 1f) {
					if (spaceDown < 0.5f) {
						cc.Move (new Vector3 (0f, 0.5f - spaceDown, 0f));
					} else if (spaceUp < 0.5f) {
						cc.Move (new Vector3 (0f, spaceUp - 0.5f, 0f));
					}

					cc.height = 2f;
					crouching = false;
				}
			}
		} else {
			if (Keybinds.Crouch.Value) {
				cc.height = 1.1f;
				crouching = true;

				if (grounded) {
					cc.Move (new Vector3 (0f, -0.5f, 0f));
				}
			}
		}

		// NOTE(lubomir): ...
		bool CanAccelerate = true;
		if (c.abilities.Casting) {
			CanAccelerate &= c.abilities.CastedAbility.MoveWhileCasting;
		}

		// NOTE(lubomir): Perform behaviour for the environmental state
		switch (state) {
		case State.Normal:
			{
				// NOTE(lubomir): Grounded state

				if (Keybinds.Jump.GetDown) {
					grounded = false;
//					const float jumpvel = 4.5f;
					vel.y = JumpVel/* + platformVel.y*/;

					footsteps.clip = jump;
					footsteps.Play ();

					goto case State.Air;
				}

				// NOTE(lubomir): Calculate velocity magnitude
				float sqrmagn = vel.x * vel.x + vel.z * vel.z;
				float svel = Mathf.Sqrt (sqrmagn);

				// NOTE(lubomir): Static friction
				if (sqrmagn < StaticFriction * StaticFriction * TL.dt * TL.dt) {
					vel.x = 0f;
					vel.z = 0f;
				} else {
					float sfric = 1f - StaticFriction * TL.dt / svel;
					vel.x *= sfric;
					vel.z *= sfric;
				}

				// NOTE(lubomir): Dynamic friction
				if (svel > 0f) {
					float loss = svel * DynamicFriction * TL.dt;
					float dfric = svel - loss;
					if (dfric > 0f) {
						dfric /= svel;
						vel.x *= dfric;
						vel.z *= dfric;
					}
				}

				if (CanAccelerate) {
					// NOTE(lubomir): Calculate acceleration vector
					float ax = (input.x * cosyaw - input.y * sinyaw);
					float az = (input.x * sinyaw + input.y * cosyaw);

					// NOTE(lubomir): Modify acceleration vector so that we don't exceed maximum speed
					float dot = vel.x * ax + vel.z * az;
					float frame_accel = (crouching ? CrouchAccel : Accel) * TL.dt;
					float max_speed = (crouching ? MaxSpeedCrouch : MaxSpeed);

					if (dot + frame_accel > max_speed) {
						frame_accel = max_speed - dot;
					}

					// NOTE(lubomir): 
					if (frame_accel > 0f) {
						vel.x += ax * frame_accel;
						vel.z += az * frame_accel;
					}
				}

				// NOTE(lubomir): Footsteps
				footstep_distance += svel*TL.dt;
				if(footstep_distance > footstep_length){
					footstep_distance = 0f;

					// TODO(lubomir): Determine which sound to play when stepping on different materials.
					footsteps.clip = concrete;
					footsteps.pitch = UnityEngine.Random.Range (0.9f, 1.1f);
					footsteps.Play ();
				}
			}
			break;
		case State.Air:
			{
				if (CanAccelerate) {
					// NOTE(lubomir): Calculate velocity indicator
					float sqrmagn = vel.x * vel.x + vel.z * vel.z;

					// NOTE(lubomir): Calculate acceleration vector
					float ax = (input.x * cosyaw - input.y * sinyaw);
					float az = (input.x * sinyaw + input.y * cosyaw);
//					float frame_accel = AirAccel * TL.dt;
					float frame_accel = (crouching ? CrouchAirAccel : AirAccel) * TL.dt;

//					if(sqrmagn > 0f){
					float dot = vel.x * ax + vel.z * az;

					// NOTE(lubomir): Modify acceleration vector so that we don't exceed maximum speed
					if (dot + frame_accel > MaxAirSpeed) {
						frame_accel = MaxAirSpeed - dot;
					}

					// NOTE(lubomir): Don't apply acceleration it does negative work
					if (frame_accel > 0f) {
						vel.x += ax * frame_accel;
						vel.z += az * frame_accel;
					}
				}

				vel.y -= TL.dt * Gravity;
			}
			break;
		}

		// NOTE(lubomir): Calculate step, which is the actual movement vector for this frame.
		step = vel * TL.dt;

		// NOTE(lubomir): If we are grounded apply a downward force to keep us stuck to the ground
		// when going down angled terrain
		if (grounded) {
			step.y = -0.1f;
//			vel.y = -0f;
		}

		// NOTE(lubomir): Move.
		walkable_ground = false;

//		Debug.Log (grounded + " - " + step.y);

		cc.Move (step);

		// NOTE(lubomir): Save previous height
		// TODO(lubomir): Detect when ground is lost when running off the edge of a collider
		// and restore previous height to remove the sudden change of position.
//		liftOffHeight = pos.y;

		// NOTE(lubomir): Update script values
		eye = pos = me.localPosition;
		/*if (crouching) {
			eye.y += 0.45f;
		} else {
			eye.y += 0.45f;
		}*/
		eye.y += 0.45f;

//		touching_ground = cc.isGrounded;
//		touching_ground = charc.touching_ground;

		grounded = walkable_ground && vel.y <= 0f;

		// NOTE(lubomir): Grounded state change behaviour
		if (last_grounded != grounded) {
			last_grounded = grounded;

			// Landed
			if (grounded) {
				state = State.Normal;

				// TODO(lubomir): Fall damage
//				vel.y = platformVel.y;
				footsteps.clip = land;
				footsteps.Play ();

			// Lift off
			} else {
				if (vel.y < 0f) {
					vel.y = -0f;
				}
				state = State.Air;

				// TODO(lubomir): This was very buggy, disabling for now
				// NOTE(lubomir): We try move downward to stick to the ground as much as possible.
				// When the player loses ground we want to reset the position to before sticking to ground.
//				pos.y = liftOffHeight;
//				me.localPosition = pos;
			}
		}
	}

	void OnControllerColliderHit(ControllerColliderHit hit){
		Rigidbody body = hit.collider.attachedRigidbody;
//		platformVel = Helper.v3zero;
		if (body != null && body.isKinematic == false && hit.moveDirection.y > -0.3f) {
			Vector3 vel = getVel;
			float pushPower = 10f*Mathf.Sqrt (Helper.Inner (vel, vel));
			Vector3 pushDir = hit.moveDirection;

			body.AddForceAtPosition (pushDir * pushPower, hit.point, ForceMode.Force);

//			body.AddForce (pushDir * pushPower, ForceMode.Force);
//			body.WakeUp();
//			body.velocity = vel;
		} else {
			Vector3 n = hit.normal;

			// Get GEOMETRY normal
			{
				RaycastHit t;
				Vector3 dir = hit.moveDirection;
				Vector3 point = hit.point - 0.5f * dir;
				if (Physics.Raycast (point, dir, out t, 0.6f, Helper.PlayerCollisionMask, QueryTriggerInteraction.Ignore)) {
					Vector3 norm = t.normal;

					if (norm.y > n.y) {
						n = norm;
					}
				}
			}

			slope = n.y;

			if (n.y >= slope_limit) {
				walkable_ground = true;
				return;
			}

			float projection = Helper.Inner (vel, n);
			if (projection > 0f) {
				return;
			}

//			Physics.ComputePenetration

			/*
			// TODO(lubomir): This is a hack, if there would be a way for the engine not to interp
			// normals of surfaces that we hit sides of...
			// NOTE(lubomir): Only slide on slopes, not edges of flat surfaces...
			{
				RaycastHit hitDown;
				const float downRayLength = 1.5f;
				if (Physics.Raycast (pos, Helper.v3down, out hitDown, downRayLength, PlayerCollisionMask, QueryTriggerInteraction.Ignore)) {
					Vector3 normal = hitDown.normal;

					if (normal.y < slope_limit) {
						goto scope_end;
					}
				}

				float force = projection * n.y;
				if (force < 0f) {
					vel.y -= force;
				}
				return;

				scope_end:;
			}*/

			/*if (n.y > 0f) {
				// vector down along the wall
				Vector3 hor;
				hor.x = -n.z;
				hor.z = n.x;
				hor.y = 0f;

				Vector3 d = Vector3.Cross (n, hor);
				d.Normalize ();

				RaycastHit rayhit;
				if (Physics.Raycast (pos, d, out rayhit, 0.5f, PlayerCollisionMask, QueryTriggerInteraction.Ignore)) {
					cc.Move (d * rayhit.distance);
					Debug.Log ("Distance: " + rayhit.distance);
				}
			}*/

			#if false
			if (n.y > 0f) {
//				Vector3 downTheSlope = Vector3.RotateTowards(n, Helper.v3down, 90f * Mathf.Deg2Rad, 0f);
				Vector3 downTheSlope = RotateDown90(n);
				downTheSlope.Normalize();

				RaycastHit rayhit;

				if (Physics.SphereCast (hit.point + n * 0.4f, 0.4f, downTheSlope, out rayhit, 1f, PlayerCollisionMask, QueryTriggerInteraction.Ignore)) {
					cc.Move (downTheSlope * rayhit.distance);
					Debug.Log ("Distance: " + rayhit.distance);
					return;
				}

				/*pos = me.localPosition;*/

				/*Vector3 p1;
				Vector3 p2;
				p2.x = p1.x = pos.x;
				p2.z = p1.z = pos.z;
				p1.y = pos.y + 0.5f;
				p2.y = pos.y - 0.5f;
				if (Physics.CapsuleCast (p1, p2, 0.4f, downTheSlope, out rayhit, 0.4f, PlayerCollisionMask, QueryTriggerInteraction.Ignore)) {
					cc.Move (downTheSlope * rayhit.distance);
					Debug.Log ("Distance: " + rayhit.distance);
				}*/

				/*if (Physics.Raycast (hit.point + n * 0.01f, downTheSlope, out rayhit, 0.4f, PlayerCollisionMask, QueryTriggerInteraction.Ignore)) {
					cc.Move (downTheSlope * rayhit.distance);
					Debug.Log ("Distance: " + rayhit.distance);
				}*/
			}
			#endif

			if (grounded) {
				float magn = 1f / Mathf.Sqrt (n.x * n.x + n.z * n.z);
				float nx2d = n.x * magn;
				float nz2d = n.z * magn;
				float projection2D = vel.x * nx2d + vel.z * nz2d;
				if (projection2D < 0f) {
					vel.x -= projection2D * nx2d;
					vel.z -= projection2D * nz2d;
				}
			} else {
				Vector3 a = projection * n;

				/*RaycastHit hitDown;
				const float downRayLength = 1.5f;
				if (Physics.Raycast (pos, Helper.v3down, out hitDown, downRayLength, PlayerCollisionMask, QueryTriggerInteraction.Ignore)) {
					if (hitDown.normal.y < slope_limit) {
						if (a.y < 0f) {
							a.y = 0f;
						}
					}
				} else {
					if (a.y < 0f) {
						a.y = 0f;
					}
				}*/

				vel -= a;
			}
		}
	}

	// NOTE(lubomir): To be used for sliding down slopes
	Vector3 RotateDown90(Vector3 v){
		return new Vector3 (v.y * v.x, -v.z * v.z - v.x * v.x, v.y * v.z);
	}
}
