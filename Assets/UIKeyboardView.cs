using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIKeyboardView : MonoBehaviour {
	public Image[] images;

	void Start(){
		Console.AddCommand ("ui_keyboard_display", (string arg) => {
			this.gameObject.SetActive(Helper.ParseBool(arg));
		});
	}

	void Update(){
		for (int Key = 0; Key < Keybinds.keybinds.Count; Key++) {
			images[Key].enabled = Replay.main.keyboardReplay.Get (Key, Replay.main.at);
		}
	}
}
