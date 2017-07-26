using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIDebug : MonoBehaviour {
	void Awake(){
		Console.AddCommand ("ui_debug", (string arg) => {
			this.gameObject.SetActive(Helper.ParseBool(arg));
		});
	}

	public Text text;
	public Player p;
//	public CharController c;
	public Replay r;
	public Host h;

	int frames;
	int last_frames;

	/*const float[] intervals = {
		1f,
		0.5f,
		0.25f,
		0.125f,
		0.0625f
	};*/

	const float interval = 0.5f;
	const int fps_bitshift = 1;
	float next;

	Vector3 lastPos;

	string DrawFloat(string name, float f, string format = "0.0"){
		return name + ": " + ColorFloat (f, format) + "\n";
	}
	string ColorFloat(float f, string format = "0.0"){
		if (f > 0f) {
			return "<color=#00ff00>" + f.ToString (format) + "</color>";
		} else if (f < 0f) {
			return "<color=#ff4444>" + f.ToString (format) + "</color>";
		} else {
			return f.ToString (format);
		}
	}

	void Update () {
		if (TL.t >= next) {
			last_frames = frames << fps_bitshift;
			frames = 0;
			next += interval;
		}

		Vector3 dpos = (p.pos - lastPos) * TL.OneOverDT;
		lastPos = p.pos;
//		Vector3 dpos = (c.pos - lastPos) / TL.dt;
//		lastPos = c.pos;

		Vector3 vel = p.vel;
//		Vector3 vel = p.getVel;

		text.text =
		//			"pos: " + p.pos + "\n" +
			"fps: " + last_frames + "\n" +
//			"time: " + TL.t.ToString ("0.0") + "\n" +

			/*"vel xz: " + Mathf.Sqrt(c.vel.x*c.vel.x + c.vel.z*c.vel.z).ToString("0.0") + "\n" +
			"vel y: " + c.vel.y.ToString("0.0") + "\n" +
			"dpos xz: " + Mathf.Sqrt(dpos.x*dpos.x + dpos.z*dpos.z).ToString("0.0") + "\n" +
			"dpos y: " + dpos.y.ToString("0.0") + "\n" +
			"touching_ground: " + c.touching_ground.ToString() + "\n" +
			"MOVES: " + c.MOVES.ToString() + "\n" +*/

			DrawFloat ("vel xz", Mathf.Sqrt (vel.x * vel.x + vel.z * vel.z)) + 
			DrawFloat ("vel y", vel.y) + 
			#if false
			DrawFloat ("dpos xz", Mathf.Sqrt(dpos.x*dpos.x + dpos.z*dpos.z)) +
			DrawFloat ("dpos y", dpos.y) +
			#endif
			DrawFloat ("slope", Mathf.Acos(p.slope) * Mathf.Rad2Deg, "0") +
			"grounded: " + p.grounded + "\n" +
//			"norm y: " + p.slope.ToString("0.0") + "\n" +
//			"exact vel: " + Mathf.Sqrt(p.vel.x*p.vel.x + p.vel.z*p.vel.z) + "\n" +
//			"pitch: " + p.pitch.ToString("0.0") + "\n" +
//			"yaw: " + p.yaw.ToString("0.0") + "\n" +
			"state: " + p.state + "\n" +

			"frame: " + r.at  + "\n" +

			/*((TL.t - p.LastCollision < 0.2f) ? 
			"COLLISION" : "") + */

//			Physics.r


			"";



		frames++;
	}
}
