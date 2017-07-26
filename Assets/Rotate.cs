using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotate : MonoBehaviour {
	public Transform t;
//	public float degrees;

	void Update(){
		if(t && Global.main.cam_transform)
		t.Rotate (
				Vector3.up,
				3f*Mathf.Rad2Deg
				* TL.dt
				/ (Global.main.cam_transform.position
					- t.position)
				.sqrMagnitude
				, Space.World);
	}
}
