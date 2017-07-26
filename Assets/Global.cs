using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Global : MonoBehaviour {
	public static Global main;
	void Awake(){
		main = this;

		checkpoints = FindObjectsOfType<Checkpoint> ();
		checkpoint_count = checkpoints.Length;
	}

	void Update(){
		if (Input.GetKeyDown (KeyCode.KeypadPlus)) {
			if (++last_checkpoint >= checkpoint_count) {
				last_checkpoint = 0;
			}

			player_transform.localPosition = checkpoints [last_checkpoint].spawn;
		}
		if(Input.GetKeyDown (KeyCode.KeypadMinus)){
			if (--last_checkpoint < 0) {
				last_checkpoint = checkpoint_count - 1;
			}

			player_transform.localPosition = checkpoints [last_checkpoint].spawn;
		}
	}

	public Player player;
	public Transform player_transform;
	public Camera cam;
	public Transform cam_transform;

	int checkpoint_count;
	int last_checkpoint;
	static Checkpoint[] checkpoints;
}
