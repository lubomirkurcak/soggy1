using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolObject : MonoBehaviour {
	[System.NonSerialized]
	public Pool pool;
	[System.NonSerialized]
	public int ID;
	[System.NonSerialized]
	public GameObject go;
	[System.NonSerialized]
	public Transform transform;

	public void Release(){
		pool.Release (ID);
	}

	public void ReleaseAfter(float Seconds){
		release = TL.t + Seconds;
		this.enabled = true;
	}

	float release;
	void Update(){
		if (TL.t >= release) {
			this.enabled = false;
			Release ();
		}
	}
}
