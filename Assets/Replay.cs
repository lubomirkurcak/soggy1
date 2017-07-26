using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Replay : MonoBehaviour {

	public Transform spectate;
	public Transform cam;
	public CameraScript camScript;

	// default max length is 1 hour
	const int DefaultMaxLength = 50 * 60 * 60 * 1;

	enum State{
		Off, Recording, Replaying
	}

	Vector3[] pos;
	Quaternion[] rot;
	public KeyboardReplay keyboardReplay;

	State state;
	[System.NonSerialized]
	public int at;
	int len;
	int last;

	public static Replay main;

	void Awake(){
		main = this;
	}

	void Start(){
		pos = new Vector3[DefaultMaxLength];
		rot = new Quaternion[DefaultMaxLength];

		keyboardReplay = new KeyboardReplay ();

		Record ();
//		TestRecording ();
	}

	public void Record(){
		camScript.b = CameraScript.Behaviour.Player;
		state = State.Recording;
		keyboardReplay.Reset ();
	}

	void Play(){
		camScript.b = CameraScript.Behaviour.Free;
		state = State.Replaying;
//		Reset ();
	}

	int FramesFromSeconds(float Seconds){
		return (int)(Seconds / TL.fdt);
	}

	void FastForward(float Seconds){
		FastForward (FramesFromSeconds (Seconds));
	}

	void FastForward(int Frames){
		at += Frames;
		if (at > len) {
			at = len;
		}
	}

	void Rewind(float Seconds){
		Rewind (FramesFromSeconds (Seconds));
	}

	void Rewind(int Frames){
		at -= Frames;
		if (at < 0) {
			at = 0;
		}
	}

	void Pause(){
		state = State.Off;
	}

	void SnapToEnd(){
		last = at = len;
	}

	public void Reset(){
		last = at = 0;
	}

	float nextAutoRewind;
	float autoRewindDelay = 0.4f;
	float autoRewindPeriod = 0.08f;
	float RewindTime = 5f;

	void Update(){
		if (Input.GetKeyDown (KeyCode.LeftBracket)) {
			Rewind (RewindTime);
			Play ();

			nextAutoRewind = TL.t + autoRewindDelay;
		}
		if (Input.GetKey (KeyCode.LeftBracket) && TL.t > nextAutoRewind) {
			Rewind (RewindTime);
			Play ();

			nextAutoRewind = TL.t + autoRewindPeriod;
		}

		if (Input.GetKeyDown (KeyCode.RightBracket)) {
			FastForward (RewindTime);
			Play ();

			nextAutoRewind = TL.t + autoRewindDelay;
		}
		if (Input.GetKey (KeyCode.RightBracket) && TL.t > nextAutoRewind) {
			FastForward (RewindTime);
			Play ();

			nextAutoRewind = TL.t + autoRewindPeriod;
		}

		switch (state) {
		case State.Replaying:
			float t = TL.framet / TL.fdt;
			cam.localPosition = Vector3.LerpUnclamped (pos [last], pos [at], t);
			#if false
			cam.localRotation = Quaternion.LerpUnclamped (rot [last], rot [at], t);
			#else
			cam.localRotation = Quaternion.SlerpUnclamped (rot [last], rot [at], t);
			#endif
			break;
		}

	}

	void FixedUpdate(){
		switch (state) {
		case State.Recording:
			if (len >= DefaultMaxLength) {
				state = State.Off;
				break;
			}

			pos [at] = spectate.localPosition;
			rot [at] = spectate.localRotation;
			keyboardReplay.Snapshot ();

			len = ++at;
			break;
		case State.Replaying:
			if (at >= len) {
				Reset ();
			}

			last = at++;
			break;
		}

	}

	public class KeyboardReplay{
		public KeyboardReplay(){
			keys = new RunList<bool>[Keybinds.keybinds.Count];
			for (int i = 0; i < Keybinds.keybinds.Count; i++) {
				keys [i] = new RunList<bool>();
			}
		}

		RunList<bool>[] keys;

		public void Snapshot(){
			for (int i = 0; i < Keybinds.keybinds.Count; i++) {
				keys [i].Add (Keybinds.keybinds [i].Value);
			}
		}

		public void Reset(){
			for (int i = 0; i < Keybinds.keybinds.Count; i++) {
				keys [i].Clear ();
			}
		}

		public bool Get(int Key, int Frame){
			return keys [Key].Get (Frame);
		}
	}

	class RunList<T>{
		/*public RunList(){
			Clear();
		}*/

		int delta_Length;
		int delta_Count;
		public int Count;

		public T[] Values;
		public int[] Durations;

		public void Clear(){
			Count = 0;
			delta_Count = 0;
			delta_Length = 1;
			Values = new T[delta_Length];
			Durations = new int[delta_Length];
		}

		public void Add(T item){
			if (delta_Count > 0 && item.Equals(Values [delta_Count - 1])) {
				Durations [delta_Count - 1]++;
			} else {
				delta_Count++;

				if (delta_Count > delta_Length) {
					delta_Length *= 2;
					System.Array.Resize (ref Values, delta_Length);
					System.Array.Resize (ref Durations, delta_Length);
				}

				Values [delta_Count - 1] = item;
				Durations [delta_Count - 1] = 1;
			}

			Count++;
		}

		public T Get(int index){
			int at = 0;
			while (index >= Durations [at]) {
				index -= Durations [at];
				if (++at >= delta_Count) {
					at--;
					break;
				}
			}

			return Values [at];
		}
	}
}
