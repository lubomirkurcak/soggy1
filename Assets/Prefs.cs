using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.InteropServices;

public static class Prefs {
	static Prefs(){
		files = new List<SettingsFile> ();

		Load();
	}

	public static bool FileExists{
		get{
			return File.Exists (ActiveFile);
		}
	}

	public static string ActiveFile = "soggy_settings.txt";
	static List<SettingsFile> files;

	static SettingsFile FindFile(string path){
		foreach (SettingsFile sf in files) {
			if (sf.path == path) {
				return sf;
			}
		}

		return null;
	}

	static SettingsFile FindOrCreateFile(string path){
		SettingsFile file = FindFile (path);

		if (file == null) {
			file = new SettingsFile ();
			file.path = path;
			file.settings = new List<Setting> ();
			files.Add (file);
		}

		return file;
	}

	class SettingsFile{
		public string path;
		public List<Setting> settings;
	}

	[StructLayout(LayoutKind.Explicit)]
	class Setting{
		public enum ValueType : byte{
			Bool, Int, Float, Keycode
		}

		[FieldOffsetAttribute(0)]
		public bool b;
		[FieldOffsetAttribute(0)]
		public float f;
		[FieldOffsetAttribute(0)]
		public int i;
		[FieldOffsetAttribute(0)]
		public KeyCode k;

		[FieldOffsetAttribute(32)]
		public ValueType type;

		[FieldOffsetAttribute(40)]
		public string name;
	}

	static Setting Find(string name, SettingsFile file){
		foreach (Setting s in file.settings) {
			if (s.name == name) {
				return s;
			}
		}

		return null;
	}

	static Setting FindOrCreate(string name, SettingsFile file){
		Setting s = Find (name, file);

		if (s == null) {
			s = new Setting();
			s.name = name;
			file.settings.Add (s);
		}

		return s;
	}

	static void ConvertToType(Setting s, Setting.ValueType type){
		switch (type) {
		case Setting.ValueType.Float:
			switch (s.type) {
			case Setting.ValueType.Int:
				s.f = s.i;
				break;
			case Setting.ValueType.Bool:
				s.f = s.b ? 1f : 0f;
				break;
			}
			break;

		case Setting.ValueType.Int:
			switch (s.type) {
			case Setting.ValueType.Float:
				s.i = (int)s.f;
				break;
			case Setting.ValueType.Bool:
				s.i = s.b ? 1 : 0;
				break;
			}
			break;

		case Setting.ValueType.Bool:
			switch (s.type) {
			case Setting.ValueType.Float:
				s.b = s.f != 0f;
				break;
			case Setting.ValueType.Int:
				s.b = s.i != 0;
				break;
			}
			break;
		}
	}

	public static float GetFloat(string name){
		return GetFloat (name, ActiveFile);
	}
	public static float GetFloat(string name, string file){
		Load (file);

		SettingsFile f = FindFile(file);
		Setting s = Find (name, f);
		if (s != null) {
			ConvertToType (s, Setting.ValueType.Float);
			return s.f;
		}

		Debug.LogError("Unrecognized field: '" + name + "' in file '" + file + "'");
		return 0f;
	}

	public static int GetInt(string name){
		return GetInt (name, ActiveFile);
	}
	public static int GetInt(string name, string file){
		Load (file);

		SettingsFile f = FindFile(file);
		Setting s = Find (name, f);
		if (s != null) {
			ConvertToType (s, Setting.ValueType.Int);
			return s.i;
		}

		Debug.LogError("Unrecognized field: '" + name + "' in file '" + file + "'");
		return 0;
	}

	public static bool GetBool(string name){
		return GetBool (name, ActiveFile);
	}
	public static bool GetBool(string name, string file){
		Load (file);

		SettingsFile f = FindFile(file);
		Setting s = Find (name, f);
		if (s != null) {
			ConvertToType (s, Setting.ValueType.Bool);
			return s.b;
		}

		Debug.LogError("Unrecognized field: '" + name + "' in file '" + file + "'");
		return false;
	}

	public static void GetKeycode(string name, ref KeyCode result){
		GetKeycode (name, ActiveFile, ref result);
	}
	public static void GetKeycode(string name, string file, ref KeyCode result){
		Load (file);

		SettingsFile f = FindFile(file);
		Setting s = Find (name, f);
		if (s != null) {
			ConvertToType (s, Setting.ValueType.Bool);
			result = s.k;
		}
	}

	public static void SetFloat(string name, float f){
		SetFloat (name, f, ActiveFile);
	}
	public static void SetFloat(string name, float f, string file){
		SettingsFile ff = FindOrCreateFile(file);
		Setting s = FindOrCreate (name, ff);
		s.type = Setting.ValueType.Float;
		s.f = f;

		Save (file);
	}

	public static void SetInt(string name, int i){
		SetInt (name, i, ActiveFile);
	}
	public static void SetInt(string name, int i, string file){
		SettingsFile f = FindOrCreateFile(file);
		Setting s = FindOrCreate (name,f);
		s.type = Setting.ValueType.Int;
		s.i = i;

		Save (file);
	}

	public static void SetBool(string name, bool b){
		SetBool (name, b, ActiveFile);
	}
	public static void SetBool(string name, bool b, string file){
		SettingsFile f = FindOrCreateFile(file);
		Setting s = FindOrCreate (name,f);
		s.type = Setting.ValueType.Bool;
		s.b = b;

		Save (file);
	}

	public static void SetKeycode(string name, KeyCode k){
		SetKeycode (name, k, ActiveFile);
	}
	public static void SetKeycode(string name, KeyCode k, string file){
		SettingsFile f = FindOrCreateFile(file);
		Setting s = FindOrCreate (name,f);
		s.type = Setting.ValueType.Keycode;
		s.k = k;

		Save (file);
	}

	public static void Save(){
		Save (ActiveFile);
	}
	public static void Save(string file){
		StreamWriter w = new StreamWriter (file);
		SettingsFile f = FindFile(file);

		string line;
		foreach (Setting s in f.settings) {
			line = s.name + ": ";

			switch (s.type) {
			case Setting.ValueType.Float:
				line += s.f.ToString ();
				break;
			case Setting.ValueType.Int:
				line += s.i.ToString ();
				break;
			case Setting.ValueType.Bool:
				line += s.b.ToString ();
				break;
			}

			w.WriteLine (line);
		}

		w.Close ();
	}

	public static void Load(){
		Load (ActiveFile);
	}
	public static void Load(string file){
		SettingsFile f = FindOrCreateFile(file);
		f.settings = new List<Setting> ();

		if (File.Exists (file)) {
			StreamReader r = new StreamReader (file);

			while (r.EndOfStream == false) {
				Setting s = new Setting ();
				string line = r.ReadLine ();

				int split = line.IndexOf (':');
				string name = line.Substring (0, split);
				string value = line.Substring (split + 2);

				if (bool.TryParse (value, out s.b)) {
					goto done;
				} else if (int.TryParse (value, out s.i)) {
					goto done;
				} else if (float.TryParse (value, out s.f)) {
					goto done;
				} else {
					Debug.LogError ("Failed to read value '" + value + "' of key '" + name + "'");
					continue;
				}

				done:
				f.settings.Add (s);
			}

			r.Close ();
		}
	}
}
