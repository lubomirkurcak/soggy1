﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CInput{
	public static int Requester;
	static int[] History;
	const int HistorySize = 16;

	static CInput(){
		History = new int[HistorySize];
	}

	static int Count;
	public static int Register(){
		return Count++;
	}

	public static void Activate(int ID){
		for (int i = HistorySize - 1; i > 0;) {
			History [i] = History [--i];
		}
		History [0] = ID;
	}

	public static void Deactivate(int ID){
		if (History [0] == ID) {
			Deactivate ();
		}
	}

	public static void Deactivate(){
		for (int i = 0; i < HistorySize - 1;) {
			History [i] = History [++i];
		}
	}

	public static bool GetKey(KeyCode k){
		if(History [0] == Requester){
			return Input.GetKey(k);
		}
		return false;
	}
	public static bool GetKeyDown(KeyCode k){
		if(History [0] == Requester){
			return Input.GetKeyDown (k);
		}
		return false;
	}
	public static bool GetKeyUp(KeyCode k){
		if(History [0] == Requester){
			return Input.GetKeyUp(k);
		}
		return false;
	}
	public static float GetAxis(string s){
		if (History [0] == Requester) {
			return Input.GetAxisRaw (s);
		}
		return 0f;
	}

	public static bool GetKey(Keycode key){
		if (History [0] == Requester) {
			switch (key) {
			/*case Keycode.MouseWheelAny:
				return Input.GetAxis ("Mouse ScrollWheel") != 0f;*/
			case Keycode.MouseWheelUp:
				return Input.GetAxis ("Mouse ScrollWheel") > 0f;
			case Keycode.MouseWheelDown:
				return Input.GetAxis ("Mouse ScrollWheel") < 0f;
			default:
				return Input.GetKey((KeyCode)key);
			}
		}
		return false;
	}

	public static bool GetKeyDown(Keycode key){
		if (History [0] == Requester) {
			switch (key) {
			/*case Keycode.MouseWheelAny:
				return Input.GetAxis ("Mouse ScrollWheel") != 0f;*/
			case Keycode.MouseWheelUp:
				return Input.GetAxis ("Mouse ScrollWheel") > 0f;
			case Keycode.MouseWheelDown:
				return Input.GetAxis ("Mouse ScrollWheel") < 0f;
			default:
				return Input.GetKeyDown((KeyCode)key);
			}
		}
		return false;
	}

	public static bool GetKeyUp(Keycode key){
		if (History [0] == Requester) {
			switch (key) {
			/*case Keycode.MouseWheelAny:
				return Input.GetAxis ("Mouse ScrollWheel") != 0f;*/
			case Keycode.MouseWheelUp:
				return Input.GetAxis ("Mouse ScrollWheel") > 0f;
			case Keycode.MouseWheelDown:
				return Input.GetAxis ("Mouse ScrollWheel") < 0f;
			default:
				return Input.GetKeyUp((KeyCode)key);
			}
		}
		return false;
	}

	public static string KeycodeToText(Keycode k){
		switch (k) {
		default:
			return k.ToString ();
		}
	}

	public static Keycode TextToKeycode(string s){
		switch (s) {
		default:
			return ((Keycode)System.Enum.Parse (typeof(Keycode), s));
		}
	}

	public enum Keycode
	{
		None,
		Backspace = 8,
		Delete = 127,
		Tab = 9,
		Clear = 12,
		Return,
		Pause = 19,
		Escape = 27,
		Space = 32,
		Keypad0 = 256,
		Keypad1,
		Keypad2,
		Keypad3,
		Keypad4,
		Keypad5,
		Keypad6,
		Keypad7,
		Keypad8,
		Keypad9,
		KeypadPeriod,
		KeypadDivide,
		KeypadMultiply,
		KeypadMinus,
		KeypadPlus,
		KeypadEnter,
		KeypadEquals,
		UpArrow,
		DownArrow,
		RightArrow,
		LeftArrow,
		Insert,
		Home,
		End,
		PageUp,
		PageDown,
		F1,
		F2,
		F3,
		F4,
		F5,
		F6,
		F7,
		F8,
		F9,
		F10,
		F11,
		F12,
		F13,
		F14,
		F15,
		Alpha0 = 48,
		Alpha1,
		Alpha2,
		Alpha3,
		Alpha4,
		Alpha5,
		Alpha6,
		Alpha7,
		Alpha8,
		Alpha9,
		Exclaim = 33,
		DoubleQuote,
		Hash,
		Dollar,
		Ampersand = 38,
		Quote,
		LeftParen,
		RightParen,
		Asterisk,
		Plus,
		Comma,
		Minus,
		Period,
		Slash,
		Colon = 58,
		Semicolon,
		Less,
		Equals,
		Greater,
		Question,
		At,
		LeftBracket = 91,
		Backslash,
		RightBracket,
		Caret,
		Underscore,
		BackQuote,
		A,
		B,
		C,
		D,
		E,
		F,
		G,
		H,
		I,
		J,
		K,
		L,
		M,
		N,
		O,
		P,
		Q,
		R,
		S,
		T,
		U,
		V,
		W,
		X,
		Y,
		Z,
		Numlock = 300,
		CapsLock,
		ScrollLock,
		RightShift,
		LeftShift,
		RightControl,
		LeftControl,
		RightAlt,
		LeftAlt,
		LeftCommand = 310,
		LeftApple = 310,
		LeftWindows,
		RightCommand = 309,
		RightApple = 309,
		RightWindows = 312,
		AltGr,
		Help = 315,
		Print,
		SysReq,
		Break,
		Menu,
		Mouse0 = 323,
		Mouse1,
		Mouse2,
		Mouse3,
		Mouse4,
		Mouse5,
		Mouse6,
		JoystickButton0,
		JoystickButton1,
		JoystickButton2,
		JoystickButton3,
		JoystickButton4,
		JoystickButton5,
		JoystickButton6,
		JoystickButton7,
		JoystickButton8,
		JoystickButton9,
		JoystickButton10,
		JoystickButton11,
		JoystickButton12,
		JoystickButton13,
		JoystickButton14,
		JoystickButton15,
		JoystickButton16,
		JoystickButton17,
		JoystickButton18,
		JoystickButton19,
		Joystick1Button0,
		Joystick1Button1,
		Joystick1Button2,
		Joystick1Button3,
		Joystick1Button4,
		Joystick1Button5,
		Joystick1Button6,
		Joystick1Button7,
		Joystick1Button8,
		Joystick1Button9,
		Joystick1Button10,
		Joystick1Button11,
		Joystick1Button12,
		Joystick1Button13,
		Joystick1Button14,
		Joystick1Button15,
		Joystick1Button16,
		Joystick1Button17,
		Joystick1Button18,
		Joystick1Button19,
		Joystick2Button0,
		Joystick2Button1,
		Joystick2Button2,
		Joystick2Button3,
		Joystick2Button4,
		Joystick2Button5,
		Joystick2Button6,
		Joystick2Button7,
		Joystick2Button8,
		Joystick2Button9,
		Joystick2Button10,
		Joystick2Button11,
		Joystick2Button12,
		Joystick2Button13,
		Joystick2Button14,
		Joystick2Button15,
		Joystick2Button16,
		Joystick2Button17,
		Joystick2Button18,
		Joystick2Button19,
		Joystick3Button0,
		Joystick3Button1,
		Joystick3Button2,
		Joystick3Button3,
		Joystick3Button4,
		Joystick3Button5,
		Joystick3Button6,
		Joystick3Button7,
		Joystick3Button8,
		Joystick3Button9,
		Joystick3Button10,
		Joystick3Button11,
		Joystick3Button12,
		Joystick3Button13,
		Joystick3Button14,
		Joystick3Button15,
		Joystick3Button16,
		Joystick3Button17,
		Joystick3Button18,
		Joystick3Button19,
		Joystick4Button0,
		Joystick4Button1,
		Joystick4Button2,
		Joystick4Button3,
		Joystick4Button4,
		Joystick4Button5,
		Joystick4Button6,
		Joystick4Button7,
		Joystick4Button8,
		Joystick4Button9,
		Joystick4Button10,
		Joystick4Button11,
		Joystick4Button12,
		Joystick4Button13,
		Joystick4Button14,
		Joystick4Button15,
		Joystick4Button16,
		Joystick4Button17,
		Joystick4Button18,
		Joystick4Button19,
		Joystick5Button0,
		Joystick5Button1,
		Joystick5Button2,
		Joystick5Button3,
		Joystick5Button4,
		Joystick5Button5,
		Joystick5Button6,
		Joystick5Button7,
		Joystick5Button8,
		Joystick5Button9,
		Joystick5Button10,
		Joystick5Button11,
		Joystick5Button12,
		Joystick5Button13,
		Joystick5Button14,
		Joystick5Button15,
		Joystick5Button16,
		Joystick5Button17,
		Joystick5Button18,
		Joystick5Button19,
		Joystick6Button0,
		Joystick6Button1,
		Joystick6Button2,
		Joystick6Button3,
		Joystick6Button4,
		Joystick6Button5,
		Joystick6Button6,
		Joystick6Button7,
		Joystick6Button8,
		Joystick6Button9,
		Joystick6Button10,
		Joystick6Button11,
		Joystick6Button12,
		Joystick6Button13,
		Joystick6Button14,
		Joystick6Button15,
		Joystick6Button16,
		Joystick6Button17,
		Joystick6Button18,
		Joystick6Button19,
		Joystick7Button0,
		Joystick7Button1,
		Joystick7Button2,
		Joystick7Button3,
		Joystick7Button4,
		Joystick7Button5,
		Joystick7Button6,
		Joystick7Button7,
		Joystick7Button8,
		Joystick7Button9,
		Joystick7Button10,
		Joystick7Button11,
		Joystick7Button12,
		Joystick7Button13,
		Joystick7Button14,
		Joystick7Button15,
		Joystick7Button16,
		Joystick7Button17,
		Joystick7Button18,
		Joystick7Button19,
		Joystick8Button0,
		Joystick8Button1,
		Joystick8Button2,
		Joystick8Button3,
		Joystick8Button4,
		Joystick8Button5,
		Joystick8Button6,
		Joystick8Button7,
		Joystick8Button8,
		Joystick8Button9,
		Joystick8Button10,
		Joystick8Button11,
		Joystick8Button12,
		Joystick8Button13,
		Joystick8Button14,
		Joystick8Button15,
		Joystick8Button16,
		Joystick8Button17,
		Joystick8Button18,
		Joystick8Button19,

//		MouseWheelAny,
		MouseWheelUp,
		MouseWheelDown
	}
}