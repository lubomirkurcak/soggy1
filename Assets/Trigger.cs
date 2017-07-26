using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trigger : MonoBehaviour {

	public enum Type{
		Toggle, Duration
	}

	public bool Disable;
	public Type type;
	public float Duration;
	[Space(10f)]
	public MovingPlatform[] platforms;
	public Zone[] zones;

	bool Enabled;
	bool Present;
	float DurationEnd;

	void Update(){
		if (Enabled) {
			if (TL.t >= DurationEnd) {
				Enabled = false;
				Action ();
			}
		}

		if (Present && Input.GetKeyDown (KeyCode.E)) {
			Enabled = true;
			Action ();

			if (Duration > 0f) {
				DurationEnd = TL.t + Duration;
			}
		}
	}

	void Action(){
		foreach (MovingPlatform p in platforms) {
			p.MoveNext ();
		}

		foreach (Zone z in zones) {
			switch (type) {
			case Type.Toggle:
				z.ToggleActive ();
				break;
			case Type.Duration:
				if (Disable) {
					z.SetActive (!Enabled);
				} else {
					z.SetActive (Enabled);
				}
				break;
			}
		}

		/*foreach(TriggerAction a in actions){
			switch (a.target) {
			case TriggerAction.Target.Zone:
				a.zone.ToggleActive ();
				break;
			case TriggerAction.Target.Platform:
				a.platform.MoveNext ();
				break;
			}
		}*/
	}

	void OnTriggerEnter(Collider col){
		if (col.transform == Global.main.player_transform) {
			Present = true;
		}
	}

	void OnTriggerExit(Collider col){
		if (col.transform == Global.main.player_transform) {
			Present = false;
		}
	}

	/*[System.Serializable]
	public struct TriggerAction{
		public enum Type{
			Toggle, Duration
		}

		public enum Target
		{
			Zone, Platform
		}

		public float Duration;
		public Zone zone;
		public Target target;
		public MovingPlatform platform;
	}*/
}
