using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour {
	
	public Rotate rotate;
	public MeshRenderer renderer;
	public Material mat;
	public Vector3 spawn;
	public Quaternion spawn_rotation;

	void Start(){
		spawn = transform.localPosition + Vector3.up*2.5f;
	}

	void OnTriggerEnter(){
		rotate.enabled = true;
		renderer.sharedMaterial = mat;
	}

}
