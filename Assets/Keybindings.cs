using System.Collections;
using System.Collections.Generic;
using UnityEngine;

static class Keybinds{
	public static Keybind MoveForward;
	public static Keybind MoveBack;
	public static Keybind MoveLeft;
	public static Keybind MoveRight;
	public static Keybind Jump;
	public static Keybind Crouch;
	public static Keybind Interrupt;
	public static Keybind Ability1;
	public static Keybind Ability2;

	static void ConsoleCommands (){
		Console.AddCommand ("bind_list", (string arg) => {
			for (int i = 0; i < Keybinds.keybinds.Count; i++) {
				string s;
				s = (i+1).ToString() + ". " + Keybinds.keybinds[i].name + ": ";

				for (int j = 0; j < Keybinds.keybinds[i].keys.Count; j++) {
					if(j > 0){
						s += ", ";
					}

					s += CInput.KeycodeToText(Keybinds.keybinds[i].keys[j]);
				}

				Console.Line(s);
			}
		});

		Console.AddCommand ("bind", (string arg) => {
			const char space = ' ';
			int s = arg.IndexOf(space);
			if(s < 0){
				return;
			}

			CInput.Keycode key;
			{
				string key_text = arg.Substring(0, s);
				key = CInput.TextToKeycode(key_text);

				if(key == CInput.Keycode.None){
					return;
				}
			}

			Keybind k;
			{
				string keybind_name = arg.Substring(s+1);
				k = Find(keybind_name);

				if(k == null){
					return;
				}
			}

			k.AddKey(key);
		});
	}

	static Keybinds(){
		ConsoleCommands ();

		MoveForward = Add ("Move Forward");
		MoveForward.AddKey (CInput.Keycode.W);

		MoveBack = Add ("Move Back");
		MoveBack.AddKey (CInput.Keycode.S);

		MoveLeft = Add ("Move Left");
		MoveLeft.AddKey (CInput.Keycode.A);

		MoveRight = Add ("Move Right");
		MoveRight.AddKey (CInput.Keycode.D);

		Jump = Add ("Jump");
		Jump.AddKey (CInput.Keycode.Space);
		Jump.AddKey (CInput.Keycode.MouseWheelUp);
		Jump.AddKey (CInput.Keycode.MouseWheelDown);

		Crouch = Add ("Crouch");
		Crouch.AddKey (CInput.Keycode.LeftControl);

		Interrupt = Add ("Interrupt");
		Interrupt.AddKey (CInput.Keycode.Mouse3);

//		Ability1 = Add ("Ability 1");
//		Ability2 = Add ("Ability 2");
	}

	public static List<Keybind> keybinds = new List<Keybind>();

	public static Keybind Add(string name){
		Keybind k = Find (name);

		if (k == null) {
			k = new Keybind ();
			keybinds.Add (k);
			k.name = name;
			k.keys = new List<CInput.Keycode> ();
		}

		return k;
	}

	public static Keybind Find(string name){
		for (int i = 0; i < keybinds.Count; i++) {
			if (keybinds [i].name == name) {
				return keybinds [i];
			}
		}
		return null;
	}

	public static bool Get(int id){
		return keybinds [id].Value;
	}

	public static bool GetDown(int id){
		return keybinds [id].GetDown;
	}

	public static bool GetUp(int id){
		return keybinds [id].GetUp;
	}

	public static void Update(){
		for (int i = 0; i < keybinds.Count; i++) {
			keybinds [i].Update ();
		}
	}
}

class Keybind{
	bool LastValue;
	public bool Value;
	public string name;
	public List<CInput.Keycode> keys;

	/*public bool Get(){
		return Value;
	}*/

	public void AddKey(CInput.Keycode key){
		for (int i = 0; i < keys.Count; i++) {
			if (keys [i] == key) {
				return;
			}
		}

		keys.Add (key);
	}

	public void RemoveKey(CInput.Keycode key){
		keys.Remove (key);
		/*for (int i = 0; i < keys.Count; i++) {
			if (keys [i] == key) {
				keys.Remove
			}
		}*/
	}

	public bool GetDown{
		get{
			return Value && LastValue == false;
		}
	}

	public bool GetUp{
		get{
			return Value == false && LastValue == true;
		}
	}

	public void Update(){
		LastValue = Value;
		Value = false;
		for (int i = 0; i < keys.Count; i++) {
			if (CInput.GetKey (keys [i])) {
				Value = true;
				break;
			}
		}
	}
}