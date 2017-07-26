using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PostProcessing;

public class CameraScript : MonoBehaviour {
	public Transform camt;
	public Camera cam;
	public Player p;
	public Controller controller;
	public Transform target;
	public PostProcessingProfile cameraProfile;

	public enum Behaviour{
		Free,
		Player,
		Controller,
		Transform
	}

	public Behaviour b;
	Vector3 pos;
	Quaternion rot;
	Vector2 input;
	Vector3 vel;

	void Start(){
		Console.AddCommand("res", (string arg) => {
			char[] split = new char[1];
			split [0] = ' ';
			string[] s = arg.Split (split, 2, System.StringSplitOptions.RemoveEmptyEntries);

			if(s.Length >= 2){
				int w,h;

				if(int.TryParse(s[0], out w) && int.TryParse(s[1], out h)){
					bool fullscreen = Screen.fullScreen;
					Screen.SetResolution(w,h,fullscreen);
				}
			}
		});
		Console.AddCommand ("fov", (string arg) => {
			float fov = cam.fieldOfView;
			Helper.ParseFloat(arg, ref fov);
			cam.fieldOfView = fov;
		});
		Console.AddCommand ("near_clip_plane", (string arg) => {
			float f = cam.fieldOfView;
			Helper.ParseFloat(arg, ref f);
			cam.nearClipPlane = f;
		});
		Console.AddCommand ("far_clip_plane", (string arg) => {
			float f = cam.fieldOfView;
			Helper.ParseFloat(arg, ref f);
			cam.farClipPlane = f;
		});
		Console.AddCommand ("motion_blur", (string arg) => {
			cameraProfile.motionBlur.enabled = Helper.ParseBool(arg);
		});
		Console.AddCommand ("antialiasing", (string arg) => {
			cameraProfile.antialiasing.enabled = Helper.ParseBool(arg);
		});
		Console.AddCommand ("camera_debug", (string arg) => {
			int value = 0;
			Helper.ParseInt(arg, ref value);

			BuiltinDebugViewsModel.Settings s = cameraProfile.debugViews.settings;

			s.mode = (BuiltinDebugViewsModel.Mode)value;

			cameraProfile.debugViews.settings = s;
		});
	}

	void Update(){
		if (Input.GetKeyDown (KeyCode.F1)) {
			b = Behaviour.Player;
		}

		if (Input.GetKeyDown (KeyCode.F2)) {
			b = Behaviour.Transform;
		}

		if (Input.GetKeyDown (KeyCode.F3)) {
			b = Behaviour.Controller;
		}

		switch (b) {
		case Behaviour.Transform:
//			camt.localRotation = rot;
			camt.localPosition = pos = target.localPosition + rot*Vector3.forward*-1f;
			break;
		case Behaviour.Player:
			camt.localPosition = pos = p.eye;
			camt.localRotation = rot = p.q;
			break;
		case Behaviour.Controller:
			camt.localPosition = pos = controller.pos;
			camt.localRotation = rot = controller.rot;
			break;
		case Behaviour.Free:
			break;
		}
	}
}
