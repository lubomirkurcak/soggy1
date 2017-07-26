using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour {
	public Text text;

	void Start(){
		Console.AddCommand("timer", new Console.Cmd((string arg) => {
			this.gameObject.SetActive(Helper.ParseBool(arg));
		}));
	}

	string FormatTime(float Seconds){
		int seconds = (int)Seconds;
		int minutes = 0;
		int hours = 0;

		while (seconds >= 60) {
			seconds -= 60;
			if (++minutes >= 60) {
				minutes -= 60;
				hours++;
			}
		}

		string result = "";
		if (hours > 0) {
			result = hours.ToString () + ":";
			result += minutes.ToString ("00") + ":" + seconds.ToString ("00");
		} else {
			result += minutes.ToString ("0") + ":" + seconds.ToString ("00");	
		}

		return result;
	}

	string FormatTimeMilliseconds(float Seconds){
		int seconds = (int)Seconds;
		int minutes = 0;
		int hours = 0;

		while (seconds >= 60) {
			seconds -= 60;
			if (++minutes >= 60) {
				minutes -= 60;
				hours++;
			}
		}

		string result = "";
		if (hours > 0) {
			result = hours.ToString () + ":";
			result += minutes.ToString ("00") + ":" + seconds.ToString ("00");
		} else {
			result += minutes.ToString ("0") + ":" + seconds.ToString ("00");	
		}

		int milliseconds = ((int)(Seconds * 1000f)) % 1000;
		result += "." + milliseconds.ToString ("000");

		return result;
	}

	void Update(){
		if (World.Counting) {
			text.text = FormatTime (TL.mt);
		} else {
			text.text = FormatTimeMilliseconds (World.Time);
		}
	}
}
