using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Berry : MonoBehaviour {

	public GameObject template;
	static int templateID;

	void Awake(){
		templateID = Pool.Register (template);

		Pool.pools [templateID].AutoRelease = true;
		Pool.pools [templateID].AutoReleaseTime = 0.6f;
		Pool.pools [templateID].AutoDeactivate = true;
	}

	void OnTriggerEnter(){
		if (gameObject.activeSelf) {
			gameObject.SetActive (false);

			Pool.Request (templateID).transform.localPosition = transform.localPosition;

			World.BerryPickedUp ();
		}
	}
}
