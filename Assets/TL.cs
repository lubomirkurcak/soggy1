using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TL : MonoBehaviour {
	public static float dt;
	public static float t;
	public static float fdt;
	public static float ft;
	public static float framet;
	public static float OneOverDT;
	public static float mt;
	WaitForFixedUpdate wffu;

	static float timescale = 1f;
	static float ticks = 50f;

	public static void SetTimescale(float scale){
		TL.timescale = scale;
		ApplyTime ();
	}

	public static void SetTicks(float ticks){
		TL.ticks = ticks;
		ApplyTime ();
	}

	static void ApplyTime(){
		Time.timeScale = timescale;
		Time.fixedDeltaTime = fdt = timescale / ticks;
	}

	void Start(){
		ApplyTime ();
		wffu = new WaitForFixedUpdate ();
		fdt = Time.fixedDeltaTime;

//		StartCoroutine (Clock ());

		Console.AddCommand("timescale", (string arg) => {
			float f;
			if(float.TryParse(arg, out f)){
				TL.SetTimescale(f);
			}
		});
		Console.AddCommand ("tick", (string arg) => {
			float f;
			if(float.TryParse(arg, out f)){
				TL.SetTicks(f);
			}
		});
	}

	public static void Reset(){
		mt = 0f;
	}

	void Update(){
		dt = Time.deltaTime;
		mt += dt;
		OneOverDT = 1f / dt;

		t = Time.time;

		framet = t - ft;

		// NOTE(lubomir): Keybinds Update!
		Keybinds.Update ();
	}

	void FixedUpdate(){
		ft = Time.fixedTime;
	}
	/*
	IEnumerator Clock(){
		for (;;) {
//			ft = Time.fixedTime;
			yield return wffu;
//			ft = Time.fixedTime;
		}
	}*/
}
