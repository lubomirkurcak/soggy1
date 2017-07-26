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

	[System.NonSerialized]
	public float yaw;
	[System.NonSerialized]
	public float pitch;
	[System.NonSerialized]
	public float sinyaw;
	[System.NonSerialized]
	public float cosyaw;
	float Sensitivity = 0.006f;
	bool InvertMouse;
	[System.NonSerialized]
	public Vector3 aim;
	[System.NonSerialized]
	public Vector3 forw;

	public enum Behaviour{
		Free,
		Player,
		Controller,
		Transform
	}

	public Behaviour b;
	public Vector3 pos;
	public Quaternion rot;
	Vector2 input;
	Vector3 vel;

	void Start(){
		Console.AddCommand ("sensitivity", (string arg) => {
			Helper.ParseFloat(arg, ref Sensitivity);
		});
		Console.AddCommand ("mouse_invert", (string arg) => {
			InvertMouse = Helper.ParseBool(arg);
		});
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

	public void apply(){
		camt.localRotation = rot;
		camt.localPosition= pos;
	}

	public void update(){
		// NOTE(lubomir): Arrows camera controls
		const float arrowSens = 4f;
		if (CInput.GetKey (KeyCode.RightArrow)) {
			yaw += TL.dt * arrowSens;
		}
		if (CInput.GetKey (KeyCode.LeftArrow)) {
			yaw -= TL.dt * arrowSens;
		}
		if (CInput.GetKey (KeyCode.UpArrow)) {
			pitch -= TL.dt * arrowSens;
		}
		if (CInput.GetKey (KeyCode.DownArrow)) {
			pitch += TL.dt * arrowSens;
		}

		yaw += CInput.GetAxis ("Mouse X") * Sensitivity;

		if (yaw > Helper.tau) {
			yaw -= Helper.tau;
		} else if (yaw < -Helper.tau) {
			yaw += Helper.tau;
		}

		if (InvertMouse)
			pitch += CInput.GetAxis ("Mouse Y") * Sensitivity;
		else
			pitch -= CInput.GetAxis ("Mouse Y") * Sensitivity;
		pitch = Mathf.Clamp (pitch, -Helper.halfpi, Helper.halfpi);

		// Calculate rot
		sinyaw = Mathf.Sin (-yaw);
		cosyaw = Mathf.Cos (yaw);
		float sinpitch = Mathf.Sin (pitch);
		float cospitch = Mathf.Cos (pitch);

		aim.x = forw.x = -sinyaw;
		forw.y = 0f;
		aim.z = forw.z = cosyaw;

		aim.x *= cospitch;
		aim.y = -sinpitch;
		aim.z *= cospitch;

		rot = Helper.Euler (pitch, yaw);
	}
}
