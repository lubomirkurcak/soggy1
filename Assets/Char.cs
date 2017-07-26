using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Char : MonoBehaviour {
	public CameraScript cams;
	public Player player;
	public Controller controller;
	public Abilities abilities;

	enum Mode{
		FreeCamera,
		Player,
		Controller,
		Transform,
	}

	Mode mode = Mode.Player;

	void Update(){
		if (Input.GetKeyDown (KeyCode.Escape)) {
			Application.Quit ();
		}

		if (Input.GetKeyDown (KeyCode.F1)) {
			mode = Mode.Player;
		}
		if (Input.GetKeyDown (KeyCode.F3)) {
			mode = Mode.Controller;
		}
		if (Input.GetKeyDown (KeyCode.F4)) {
			mode = Mode.FreeCamera;
		}

		cams.update ();

		switch (mode) {
		case Mode.FreeCamera:
			
		case Mode.Player:
			player.update ();
			abilities.update ();
			break;
		case Mode.Controller:
			controller.update ();
			break;
		}


		cams.apply ();
	}
}
