using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIPlayer : MonoBehaviour {
	public static UIPlayer main;
	void Awake(){
		main = this;
	}

	public Text text;

	public void UpdateThings(){
		text.text = "Berries: " + World.BerriesCollected + " / " + World.BerriesTotal;
	}
}
