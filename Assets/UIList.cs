using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIList : MonoBehaviour {

	public RectTransform parent;
	public GameObject template;

	float HeightSoFar;
	int poolID;

	void Start(){
		poolID = Pool.Register (template);

		KeybindsMenu ();
	}

	public RectTransform Add(){
		RectTransform line = Pool.Request (poolID).GetComponent<RectTransform> ();
		float Height = line.GetComponent<UIListThing> ().Height;

		line.SetParent (parent, false);

		line.offsetMax = new Vector2 (0f, HeightSoFar);
		HeightSoFar -= Height;
		line.offsetMin = new Vector2 (0f, HeightSoFar);

		parent.sizeDelta = new Vector2 (0f, -HeightSoFar);

		return line;
	}

	void KeybindsMenu (){
		for (int i = 0; i < Keybinds.keybinds.Count; i++) {
			RectTransform line = Add ();

			line.GetChild (0).GetComponent<Text> ().text = Keybinds.keybinds [i].name;

//			Keybinds.keybinds [i].keys.Count
		}
	}
}
