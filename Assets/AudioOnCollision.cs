using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioOnCollision : MonoBehaviour {
	AudioSource sauce;

	void Start(){
		sauce = GetComponent<AudioSource> ();
	}

	void OnCollisionEnter(Collision collision){
		/*audio.volume = 0.1f + collision.impulse.sqrMagnitude / 100f;
		Debug.Log (audio.volume);*/
		sauce.Play ();
	}
}
