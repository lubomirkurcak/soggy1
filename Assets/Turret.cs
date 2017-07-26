using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Turret : MonoBehaviour {

	public Transform self;
	public Transform target;
	public Transform gunModel;
	public Player targetPlayer;
	public GameObject projectile;
	public AudioSource source;

	public float Speed;
	public float Cooldown;

	float nextFire;
	public float MaxDistance = 13f;
	float MaxDistanceSqr;

	int ProjectilePoolID;

	Quaternion q = Quaternion.identity;
//	Quaternion targetQ = Quaternion.identity;

	Vector3 heading = Helper.v3up;

	void Start(){
		ProjectilePoolID = Pool.Register (projectile);

		MaxDistanceSqr = MaxDistance * MaxDistance;
	}

	void Update () {
		// NOTE(lubomir): Debug?
		if(target == null){
			target = Global.main.player_transform;
			return;
		}
		if (targetPlayer == null) {
			targetPlayer = Global.main.player;
		}

		const float aimTime = 0.5f;
		const float maxAimSpeed = Helper.halfpi;
		float aimingSpeed = maxAimSpeed * Helper.Map (TL.t, nextFire - aimTime, nextFire);
		if (aimingSpeed <= 0f) {
			return;
		}
		// TODO(lubomir): When turret activates it should have slowed down movement speed as well
		if (aimingSpeed > maxAimSpeed) {
			aimingSpeed = maxAimSpeed;
		}

		// NOTE(lubomir): Collect necessary data
		Vector3 myPos = self.position;
		Vector3 pos = target.localPosition;
		Vector3 delta = pos - myPos;

		if (delta.sqrMagnitude > MaxDistanceSqr) {
			return;
		}

		float gravity = 9.81f;
		Vector3 vel = Helper.v3zero;

		if (targetPlayer) {
			vel = targetPlayer.getVel;

			if (targetPlayer.grounded == false) {
				gravity = 0f;
			}
		}

		// NOTE(lubomir): Calculate aim direction
		Vector3 dir;
		bool validSolution = PredictiveAim (myPos, Speed, pos, vel, gravity, out dir);

		// NOTE(lubomir): Set rotation
		bool DeadOn = RotateTo(ref heading, dir, aimingSpeed*TL.dt);
		V3Quaternion (ref heading, ref q);
		gunModel.localRotation = q;

		/*V3Quaternion (ref dir, ref targetQ);
		Normalize (ref targetQ);
		bool DeadOn = RotateTo (ref q, targetQ, aimingSpeed * TL.dt);
		gunModel.localRotation = q;*/

		// NOTE(lubomir): Fire
		if (DeadOn && validSolution && TL.t > nextFire) {
			nextFire = TL.t + Cooldown;
			Projectile proj = Pool.Request (ProjectilePoolID).GetComponent<Projectile> ();
			proj.Fire (myPos, dir);

			source.pitch = UnityEngine.Random.Range (0.8f, 1.2f);
			source.Play ();
		}
	}

	bool RotateTo(ref Vector3 v0, Vector3 v1, float radians){
		float angle = Vector3.Angle (v0, v1) * Mathf.Deg2Rad;

		if (radians >= angle) {
			v0 = v1;
			return true;
		}

		v0 = Vector3.SlerpUnclamped (v0, v1, radians / angle);
		return false;
	}

	bool RotateTo(ref Quaternion q0, Quaternion q1, float radians){
		float angle = Quaternion.Angle (q0, q1) * Mathf.Deg2Rad;

		if (radians >= angle) {
			q0 = q1;
			return true;
		}

		q0 = Quaternion.LerpUnclamped (q0, q1, radians / angle);
//		q0 = Quaternion.SlerpUnclamped (q0, q1, radians / angle);
		return false;
	}

	void Normalize(ref Quaternion q){
		float magn = 1f/Magnitude (q);
		q.x *= magn;
		q.y *= magn;
		q.z *= magn;
		q.w *= magn;
	}

	float Magnitude(Quaternion q){
		return Mathf.Sqrt (q.x * q.x + q.y * q.y + q.z * q.z + q.w * q.w);
	}

	// NOTE(lubomir): I think this returns a quaternion which directs forward vector to dir vector when multiplied by.
	// Also, it seems to require the dir vector to be normalized.
	public static void V3NormQuaternion(ref Vector3 dir, ref Quaternion result){
		result.x = -dir.z;
		result.y = 0f;
		result.z = dir.x;
		result.w = -dir.y - 1f;
	}

	public static void V3Quaternion(ref Vector3 dir, ref Quaternion result){
		result.x = -dir.z;
		result.y = 0f;
		result.z = dir.x;
		result.w = -dir.y - dir.magnitude;
	}

	static public bool PredictiveAim(Vector3 muzzlePosition, float projectileSpeed, Vector3 targetPosition, Vector3 targetVelocity, float gravity, out Vector3 projectileVelocity)
	{
		Debug.Assert(projectileSpeed > 0, "What are you doing shooting at something with a projectile that doesn't move?");
		if (muzzlePosition == targetPosition)
		{
			//Why dost thou hate thyself so?
			//Do something smart here. I dunno... whatever.
			projectileVelocity = projectileSpeed * (Random.rotation * Vector3.forward);
			return true;
		}

		//Much of this is geared towards reducing floating point precision errors
		float projectileSpeedSq = projectileSpeed * projectileSpeed;
		float targetSpeedSq = targetVelocity.sqrMagnitude; //doing this instead of self-multiply for maximum accuracy
		float targetSpeed = Mathf.Sqrt(targetSpeedSq);
		Vector3 targetToMuzzle = muzzlePosition - targetPosition;
		float targetToMuzzleDistSq = targetToMuzzle.sqrMagnitude; //doing this instead of self-multiply for maximum accuracy
		float targetToMuzzleDist = Mathf.Sqrt(targetToMuzzleDistSq);
		Vector3 targetToMuzzleDir = targetToMuzzle;
		targetToMuzzleDir.Normalize();

		Vector3 targetVelocityDir = targetVelocity;
		targetVelocityDir.Normalize();

		//Law of Cosines: A*A + B*B - 2*A*B*cos(theta) = C*C
		//A is distance from muzzle to target (known value: targetToMuzzleDist)
		//B is distance traveled by target until impact (targetSpeed * t)
		//C is distance traveled by projectile until impact (projectileSpeed * t)
		float cosTheta = Vector3.Dot(targetToMuzzleDir, targetVelocityDir);

		bool validSolutionFound = true;
		float t;
		if (Mathf.Approximately(projectileSpeedSq, targetSpeedSq))
		{
			//a = projectileSpeedSq - targetSpeedSq = 0
			//We want to avoid div/0 that can result from target and projectile traveling at the same speed
			//We know that C and B are the same length because the target and projectile will travel the same distance to impact
			//Law of Cosines: A*A + B*B - 2*A*B*cos(theta) = C*C
			//Law of Cosines: A*A + B*B - 2*A*B*cos(theta) = B*B
			//Law of Cosines: A*A - 2*A*B*cos(theta) = 0
			//Law of Cosines: A*A = 2*A*B*cos(theta)
			//Law of Cosines: A = 2*B*cos(theta)
			//Law of Cosines: A/(2*cos(theta)) = B
			//Law of Cosines: 0.5f*A/cos(theta) = B
			//Law of Cosines: 0.5f * targetToMuzzleDist / cos(theta) = targetSpeed * t
			//We know that cos(theta) of zero or less means there is no solution, since that would mean B goes backwards or leads to div/0 (infinity)
			if (cosTheta > 0)
			{
				t = 0.5f * targetToMuzzleDist / (targetSpeed * cosTheta);
			}
			else
			{
				validSolutionFound = false;
				t = UnityEngine.Random.Range(1f, 5f);
			}
		}
		else
		{
			//Quadratic formula: Note that lower case 'a' is a completely different derived variable from capital 'A' used in Law of Cosines (sorry):
			//t = [ -b � Sqrt( b*b - 4*a*c ) ] / (2*a)
			float a = projectileSpeedSq - targetSpeedSq;
			float b = 2.0f * targetToMuzzleDist * targetSpeed * cosTheta;
			float c = -targetToMuzzleDistSq;
			float discriminant = b * b - 4.0f * a * c;

			if (discriminant < 0)
			{
				//Square root of a negative number is an imaginary number (NaN)
				//Special thanks to Rupert Key (Twitter: @Arakade) for exposing NaN values that occur when target speed is faster than or equal to projectile speed
				validSolutionFound = false;
				t = UnityEngine.Random.Range(1f, 5f);
			}
			else
			{
				//a will never be zero because we protect against that with "if (Mathf.Approximately(projectileSpeedSq, targetSpeedSq))" above
				float uglyNumber = Mathf.Sqrt(discriminant);
				float t0 = 0.5f * (-b + uglyNumber) / a;
				float t1 = 0.5f * (-b - uglyNumber) / a;
				//Assign the lowest positive time to t to aim at the earliest hit
				t = Mathf.Min(t0, t1);
				if (t < Mathf.Epsilon)
				{
					t = Mathf.Max(t0, t1);
				}

				if (t < Mathf.Epsilon)
				{
					//Time can't flow backwards when it comes to aiming.
					//No real solution was found, take a wild shot at the target's future location
					validSolutionFound = false;
					t = UnityEngine.Random.Range(1f, 5f);
				}
			}
		}

		//Vb = Vt - 0.5*Ab*t + [(Pti - Pbi) / t]
		projectileVelocity = targetVelocity + (-targetToMuzzle / t);
		if (!validSolutionFound)
		{
			//PredictiveAimWildGuessAtImpactTime gives you a t that will not result in impact
			// Which means that all that math that assumes projectileSpeed is enough to impact at time t breaks down
			// In this case, we simply want the direction to shoot to make sure we
			// don't break the gameplay rules of the cannon's capabilities aside from gravity compensation
			projectileVelocity = projectileSpeed * projectileVelocity.normalized;
		}

		if (!Mathf.Approximately(gravity, 0))
		{
			//By adding gravity as projectile acceleration, we are essentially breaking real world rules by saying that the projectile
			// gets any upwards/downwards gravity compensation velocity for free, since the projectileSpeed passed in is a constant that assumes zero gravity
			Vector3 projectileAcceleration = gravity * Vector3.down;
			//assuming gravity is a positive number, this next line will apply a free magical upwards lift to compensate for gravity
			Vector3 gravityCompensation = (0.5f * projectileAcceleration * t);
			//Let's cap gravityCompensation to avoid AIs that shoot infinitely high
			float gravityCompensationCap = 0.5f * projectileSpeed;  //let's assume we won't lob higher than 50% of the canon's shot range
			if (gravityCompensation.magnitude > gravityCompensationCap)
			{
				gravityCompensation = gravityCompensationCap * gravityCompensation.normalized;
			}
			projectileVelocity -= gravityCompensation;
		}

		//FOR CHECKING ONLY (valid only if gravity is 0)...
		//float calculatedprojectilespeed = projectileVelocity.magnitude;
		//bool projectilespeedmatchesexpectations = (projectileSpeed == calculatedprojectilespeed);
		//...FOR CHECKING ONLY

		return validSolutionFound;
	}
}
