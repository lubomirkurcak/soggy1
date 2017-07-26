using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelExitTriggerEnterCallback : MonoBehaviour {
	public LevelExit exit;

	void OnTriggerEnter(){
		exit.Entered ();
	}
}
