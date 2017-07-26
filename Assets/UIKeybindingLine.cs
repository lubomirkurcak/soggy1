using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIKeybindingLine : UIListThing {

	public Text keybindName;
	public GameObject keybindButtonPrefab;

	/*int pool;
	List<int> refs;

	void Start(){
		pool = Pool.Register (keybindButtonPrefab);
	}

	void ReleaseButtons(){
		if (refs != null) {
			for (int i = 0; i < refs.Count; i++) {
				Pool.pools [pool].Release (refs [i]);
			}
		}
	}

	void Draw(){
		ReleaseButtons ();

		Keybind k;

		for (int i = 0; i < k.keys.Count; i++) {
			PoolObject po = Pool.Request (pool);

			refs.Add (po.ID);

			po.go.GetComponent<RectTransform> ();

			k.keys[i]
		}
	}*/
}
