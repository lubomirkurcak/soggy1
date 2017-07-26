using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class EditorHelper : ScriptableWizard {

	public GameObject prefab;

	[MenuItem("Editor Helper/Replace with Prefab...")]
	static void SelectAllOfTagWizard()
	{
		EditorHelper eh = ScriptableWizard.DisplayWizard <EditorHelper>("");
	}

	void OnWizardCreate()
	{
		foreach (GameObject go in Selection.objects) {
			Transform t = go.transform;
			Vector3 pos = t.localPosition;
			Quaternion rot = t.localRotation;
			Vector3 scale = t.localScale;

			GameObject swap = (GameObject)PrefabUtility.InstantiatePrefab (prefab);
			Transform tswap = swap.transform;
			tswap.localPosition = pos;
			tswap.localRotation = rot;
			tswap.localScale = scale;
		}
	}
}
/*
[ExecuteInEditMode]
public class EditorHelper : ScriptableWizard {

	public GameObject prefab;

	[MenuItem("Editor Helper/Select all game")]
	static void SelectAllOfTagWizard()
	{
		EditorHelper eh = ScriptableWizard.DisplayWizard <EditorHelper>("");
	}

	void OnWizardCreate()
	{
		foreach (GameObject go in Selection.objects) {
			Transform t = go.transform;
			Vector3 pos = t.localPosition;
			Quaternion rot = t.localRotation;
			Vector3 scale = t.localScale;

			GameObject swap = (GameObject)PrefabUtility.InstantiatePrefab (prefab);
			Transform tswap = swap.transform;
			tswap.localPosition = pos;
			tswap.localRotation = rot;
			tswap.localScale = scale;
		}
	}
}
*/