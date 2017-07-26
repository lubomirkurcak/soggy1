using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class World {

	public static float Time;
	public static bool Counting;

	// Note(lubomir): rigidbodies
	static GameObject[] bodies;
	static Quaternion[] rotations;
	static Vector3[] positions;

	// Note(lubomir): berries
	static Berry[] berries;
	public static int BerriesTotal;
	public static int BerriesCollected;

	public static void BerryPickedUp(){
		BerriesCollected++;
		UIPlayer.main.UpdateThings ();

		if (BerriesCollected >= BerriesTotal) {
			ExitOpened ();
		}
	}

	static void ExitOpened(){
		// TODO(lubomir): Play a global sound?

		for (int i = 0; i < exits.Length; i++) {
			exits [i].Activate ();
		}
	}

	// Note(lubomir): others
	static LevelExit[] exits;

	public static void Snapshot(){
		SnapshotRigidbodies ();
		SnapshotBerries ();
		SnapshotExits ();
	}

	public static void Reset(){
		ResetRigidbodies ();
		ResetBerries ();
		ResetExits ();

		TL.Reset ();

		Replay.main.Reset ();
		Replay.main.Record ();

		World.Counting = true;
	}

	static void SnapshotExits(){
		exits = MonoBehaviour.FindObjectsOfType<LevelExit> ();
	}

	static void ResetExits(){
		for (int i = 0; i < exits.Length; i++) {
			exits [i].Deactivate ();
		}
	}

	static void SnapshotBerries(){
		berries = MonoBehaviour.FindObjectsOfType<Berry> ();
		BerriesCollected = 0;
		BerriesTotal = berries.Length;
		UIPlayer.main.UpdateThings ();
	}

	static void ResetBerries(){
		foreach(Berry b in berries){
			b.gameObject.SetActive (true);
		}

		BerriesCollected = 0;
		UIPlayer.main.UpdateThings ();
	}

	static void SnapshotRigidbodies () {
		int layer = LayerMask.NameToLayer ("Rigidbody Objects");

		GameObject[] goes = MonoBehaviour.FindObjectsOfType<GameObject> ();
		List<GameObject> rbs = new List<GameObject>();
		for (int i = 0; i < goes.Length; i++) {
			if (goes [i].layer == layer) {
				rbs.Add (goes [i]);
			}
		}

		bodies = rbs.ToArray ();

		int length = bodies.Length;

		rotations = new Quaternion[length];
		positions = new Vector3[length];

		for (int i = 0; i < length; i++) {
			rotations[i] = bodies [i].transform.localRotation;
			positions[i] = bodies [i].transform.localPosition;
		}
	}

	static void ResetRigidbodies(){
		for (int i = 0; i < bodies.Length; i++) {
			Rigidbody rb = bodies [i].GetComponent<Rigidbody> ();
			rb.velocity = Helper.v3zero;

			bodies [i].transform.localRotation = rotations [i];
			bodies [i].transform.localPosition = positions [i];

//			rb.Sleep ();
		}
	}
}
