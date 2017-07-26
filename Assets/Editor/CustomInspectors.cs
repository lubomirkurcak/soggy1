using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

#if false
[CustomEditor(typeof(Zone))]
public class CustomInspectors : Editor {
	public override void OnInspectorGUI ()
	{
		Zone z = (Zone)target;

		Zone.Action last = z.action;
		z.action = (Zone.Action)EditorGUILayout.EnumPopup ("Type", z.action);

		if (z.action != last) {

			Collider col = z.GetComponent<Collider> ();

			MeshRenderer mr = z.GetComponent<MeshRenderer> ();
//			mr.material = 

			switch (z.action) {
			case Zone.Action.Kill:
				break;
			}
		}
	}
}
#endif