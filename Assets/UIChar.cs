using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIChar : MonoBehaviour {

	public Char inspect;
	[Space(10)]
	public Image castbar;
	public Image castbarBg;

	void Update(){
		castbar.enabled = inspect.abilities.Casting;
		castbarBg.enabled = inspect.abilities.Casting;
		if (inspect.abilities.Casting) {
			castbar.fillAmount = Helper.Map (TL.t, inspect.abilities.CastingStart, inspect.abilities.CastingFinish);
		}
	}
}
