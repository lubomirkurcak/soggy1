using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelExit : MonoBehaviour {
	public ParticleSystem particles;
	public AudioSource sauce;
	public Collider insideCollider;

//	bool enable;

	public void Activate(){
		insideCollider.enabled = true;
		particles.Play ();
	}

	public void Deactivate(){
		insideCollider.enabled = false;
		particles.Stop ();
	}

	public void Entered(){
		if (World.Counting) {
			sauce.Play ();
			World.Time = TL.mt;
			World.Counting = false;
		}
	}
}
