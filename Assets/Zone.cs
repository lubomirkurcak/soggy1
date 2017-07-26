using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Zone : MonoBehaviour {
	
	public Action action;

	public AudioSource audio;

	public enum Action
	{
		Kill,
		SelfDestroy,
		PlayerForce,
		Sound
	}

	MeshRenderer rend;
	Collider col;

	void Start(){
		rend = GetComponentInChildren<MeshRenderer> ();
		col = GetComponentInChildren<Collider> ();
	}

	public void ToggleActive(){
		enabled = !enabled;
		SetActive (enabled);
	}

	public void SetActive(bool enable){
		rend.enabled = enable;
		col.enabled = enable;
	}

	void OnTriggerEnter(Collider col){
		Player p = col.GetComponent<Player> ();

		switch (action) {
		case Action.Kill:
			p.Kill ();
			break;
		case Action.SelfDestroy:
			Destroy (gameObject);
			break;
		case Action.PlayerForce:
			Vector3 vel = p.pos - transform.localPosition;
			vel.Normalize ();
			p.vel = vel * 10f;
			p.paralyzed = 2f;
			audio.Play ();
			break;
		case Action.Sound:
			audio.Play ();
			break;
		}
	}
}
