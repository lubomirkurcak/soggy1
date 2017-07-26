using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Loader : MonoBehaviour {

	public static Loader main;

	void Awake(){
		main = this;
	}

	void Start(){
		DontDestroyStartScene ();
	}

	void DontDestroyStartScene(){
		Scene s = SceneManager.GetActiveScene ();
		GameObject[] goes = s.GetRootGameObjects ();

		for (int i = 0; i < goes.Length; i++) {
			DontDestroyOnLoad (goes[i]);
		}
	}

	public void Load(int Level){
		if (Level > 0 && Level < SceneManager.sceneCountInBuildSettings) {
			StartCoroutine (_load (Level));
		}
	}

	IEnumerator _load(int level){
		AsyncOperation ao;
		ao = SceneManager.LoadSceneAsync (level, LoadSceneMode.Single);

		while (ao.isDone == false) {
			yield return null;
		}

		Pool.ResetPools ();
		World.Snapshot ();
		World.Reset ();

		Global.main.player.Kill ();
	}
}
