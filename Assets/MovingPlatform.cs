using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour {

	public Rigidbody body;
//	public Transform playerCollider;
	public Vector3[] points;
	public float velocity;

	Vector3 p;
	float motionStart;
	float motionEnd;
	int at;
	int next;
	bool transitting;

	public void MoveNext(){
		int next = at + 1;
		if (next >= points.Length) {
			next = 0;
		}

		Move (next);
	}

	void Move(int next){
		this.next = next;

		if (transitting == false) {
			transitting = true;
			Vector3 delta = points [next] - points [at];
			float distance = delta.magnitude;
			motionStart = TL.t;
			motionEnd = motionStart + distance / velocity;
		}
	}

	void FixedUpdate(){
		if (transitting) {
			float t = Helper.Map (TL.ft, motionStart, motionEnd);

			if (t >= 1f) {
				at = next;
				p = points [next];
				transitting = false;
			} else {
				p = Vector3.LerpUnclamped (points [at], points [next], t);
			}

			body.MovePosition (p);
		}
	}
}
