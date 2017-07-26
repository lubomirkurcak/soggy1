using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Char : MonoBehaviour {
	public CameraScript cams;
	public Player player;
	public Abilities abilities;

	[System.NonSerialized]public Transform target;

	void Update(){
		if (Input.GetKeyDown (KeyCode.Escape)) {
			Application.Quit ();
		}

		cams.update ();

		if (Input.GetKeyDown (KeyCode.F7)) {
			player.UseCustomController = !player.UseCustomController;

			player.cc.enabled = !player.UseCustomController;
		}

		player.update ();
		abilities.update ();


		cams.apply ();
	}
}
