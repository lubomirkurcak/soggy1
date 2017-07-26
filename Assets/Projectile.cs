using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour {
	public Transform self;
	public Rigidbody rb;
	public SphereCollider col;
//	public TrailRenderer tail;
	public ParticleSystem explosion;
	public MeshRenderer rend;
	public float Velocity;
	public float Duration;
	new public AudioSource audio;

	float ExplodeTime = float.PositiveInfinity;

	public void Fire(Player p){
		Fire (p.eye, p.cams.aim * Velocity + p.getVel);
	}

	public void Fire(Vector3 pos, Vector3 vel){
		self.localPosition = pos;

//		tail.Clear ();
		rb.isKinematic = false;
		rb.velocity = vel;
//		rb.angularVelocity = new Vector3(10,10,10);
		ExplodeTime = TL.ft + Duration;

		rb.WakeUp ();

		col.enabled = true;
		rend.enabled = true;
		explosion.Stop ();
	}

	void FixedUpdate(){
		if (TL.ft >= ExplodeTime) {
			ExplodeTime = float.PositiveInfinity;
			Explode ();
		}
	}

	void Explode(){
		rb.isKinematic = true;
		col.enabled = false;
		rend.enabled = false;
		explosion.Play ();
		Pool.ReleaseAfter (gameObject, 1f);

		PhysicsExplosion (self.localPosition, 3f, 800f);
	}

	void PhysicsExplosion(Vector3 p, float radius, float force){
		int layer = 1 << LayerMask.NameToLayer ("Rigidbody Objects");

		Collider[] hits = Physics.OverlapSphere (p, radius, layer, QueryTriggerInteraction.Ignore);

		for (int i = 0; i < hits.Length; ++i) {
			hits [i].attachedRigidbody.AddExplosionForce (force, p, radius);
		}
	}

	void OnCollisionEnter(Collision collision){
		/*audio.volume = 0.1f + collision.impulse.sqrMagnitude / 100f;
		Debug.Log (audio.volume);*/
		audio.Play ();
	}
}
