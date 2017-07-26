using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pool {
	public GameObject template;
	public Transform parent;

	public bool HideInHierarchy = true;
	public bool AutoDeactivate;
	public bool AutoRelease;
	public float AutoReleaseTime;

	GameObject[] instances;
	PoolObject[] poolObjects;
	bool[] usage;
	int Size;
	int UsageCount;
	int LastUsed;

	public DynamicExpansion expansion = DynamicExpansion.Double;
	public enum DynamicExpansion{
		None,
		CycleUsedValues,
		Double
	}

	public void Resize(int NewSize){
		System.Array.Resize (ref instances, NewSize);
		System.Array.Resize (ref poolObjects, NewSize);
		System.Array.Resize (ref usage, NewSize);

		for(int i=Size; i < NewSize; ++i){
			instances [i] = GameObject.Instantiate<GameObject> (template);
			instances [i].transform.SetParent (parent, false);

			// NOTE(lubomir): Hides in hierarchy so it doesn't get cluttered
			if (HideInHierarchy) {
				instances [i].hideFlags = HideFlags.HideInHierarchy;
			}

			PoolObject po = instances [i].AddComponent<PoolObject> ();
			poolObjects [i] = po;
			po.enabled = false;
			po.pool = this;
			po.go = instances [i];
			po.transform = instances [i].transform;
			po.ID = i;
		}

		Size = NewSize;
	}

	public PoolObject Request(){
		RequestID ();

		if (AutoDeactivate) {
			instances [LastUsed].SetActive (true);
		}

		return poolObjects [LastUsed];
	}

	public GameObject RequestGameObject(){
		RequestID ();

		if (AutoDeactivate) {
			instances [LastUsed].SetActive (true);
		}

		return instances [LastUsed];
	}

	public int RequestID(){
		if (UsageCount >= Size) {
			switch (expansion) {
			case DynamicExpansion.Double:
				Resize (Mathf.Max (Size * 2, 1));
				break;
			case DynamicExpansion.CycleUsedValues:
				if (++LastUsed >= Size) {
					LastUsed = 0;
				};
				usage [LastUsed] = true;
				return LastUsed;
			case DynamicExpansion.None:
				return -1;
			}
		}

		UsageCount++;
		for (int i = 1; i < Size; ++i) {
			if (++LastUsed >= Size) {
				LastUsed = 0;
			};

			if (usage [LastUsed] == false) {
				usage [LastUsed] = true;
				return LastUsed;
			}
		}

		if (AutoRelease) {
			poolObjects [LastUsed].ReleaseAfter (AutoReleaseTime);
		}

		usage [LastUsed] = true;
		return LastUsed;
	}

	public static void Release(GameObject go){
		go.GetComponent<PoolObject> ().Release ();
	}

	public static void ReleaseAfter(GameObject go, float Seconds){
		go.GetComponent<PoolObject> ().ReleaseAfter (Seconds);
	}

	public void Release(int ID){
		UsageCount--;
		usage [ID] = false;

		if (AutoDeactivate) {
			instances [ID].SetActive (false);
		}
	}

	public void ReleaseAll(){
		UsageCount = 0;

		for (int i = 0; i < Size; i++) {
			usage [i] = false;
		}

		if (AutoDeactivate) {
			for (int i = 0; i < Size; i++) {
				instances [i].SetActive (false);
			}
		}
	}

	void Reset(){
		UsageCount = 0;
		LastUsed = 0;
		Size = 0;
		Resize (0);
	}

	public static List<Pool> pools;

	static Pool(){
		pools = new List<Pool> ();
	}

	public static int Register(GameObject go){
		int i;
		for (i = 0; i < pools.Count; ++i) {
			Pool p = pools [i];

			if (p.template == go) {
				return i;
			}
		}

		Pool pool = new Pool();
		pool.template = go;
		pools.Add (pool);
		return i;
	}

	public static int RequestID(int ID){
		return pools [ID].RequestID ();
	}

	public static GameObject Request(int ID){
		return pools [ID].RequestGameObject ();
	}

	public static void ResetPools(){
		foreach (Pool p in pools) {
			p.Reset ();
		}
	}
}
