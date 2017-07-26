using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Console : MonoBehaviour {

	static Console main;

	public CanvasGroup group;
	public InputField input;
	public ScrollRect scroll;

	public GameObject lineTemplate;
	public RectTransform content;
	public float lineHeight;
	public float lineOffset;
	public float topSpace;
	public float bottomSpace;

	int LineCount;
	Pool pool;
	bool open;

	int InputID;

	static List<Command> commands;
	public static void AddCommand(string name, Cmd cmd){
		Command c = main.FindCommand(name);
		if(c == null){
			commands.Add(new Command(name, cmd));
		}else{
			c.cmd = cmd;
		}
	}

	public static void Error(string line){
		Line (line, Color.red);
	}

	public static void Line(string line){
		main.AddLine (line);
	}

	public static void Line(string line, Color color){
		main.AddLine (line, color);
	}

	// TODO(lubomir): Make History[0] save what user typed in, but whatever...
	// NOTE(lubomir): Command history
	bool MOVE_CARET_TO_END;
	int HistorySize = 10;
	int HistoryOccupied = 0;
	int HistoryAt;
	string[] History;
	void AddToHistory(string command){
		HistoryAt = -1;

		if (++HistoryOccupied > HistorySize) {
			HistoryOccupied = HistorySize;
		}

		for (int i = HistoryOccupied - 1; i > 0;) {
			History [i] = History [--i];
		}

		History [0] = command;
	}

	void LateUpdate(){
		if (MOVE_CARET_TO_END) {
			input.MoveTextEnd (false);
		}
	}

	void Update(){
		if (Input.GetKeyDown (KeyCode.BackQuote)) {
			open = !open;

			if (open) {
				CInput.Activate (InputID);

				Cursor.lockState = CursorLockMode.None;
				Cursor.visible = true;

				group.alpha = 1f;
				group.blocksRaycasts = true;
				input.enabled = true;
				input.ActivateInputField ();

				input.onEndEdit.RemoveAllListeners ();
				input.onEndEdit.AddListener ((string arg0) => {
					if (arg0 != string.Empty) {
						AddLine (arg0, Color.grey);
						Submit (arg0);
					}

					input.text = string.Empty;
					input.ActivateInputField ();
				});
			} else {
				CInput.Deactivate (/*InputID*/);
				Cursor.lockState = CursorLockMode.Locked;
				Cursor.visible = false;

				input.onEndEdit.RemoveAllListeners ();
				input.DeactivateInputField ();
				input.enabled = false;
				group.alpha = 0f;
				group.blocksRaycasts = false;
			}
		}

		if (open) {
			if (Input.GetKeyDown (KeyCode.UpArrow)) {
				if (++HistoryAt >= HistoryOccupied) {
					HistoryAt = 0;
				}
				input.text = History [HistoryAt];
				MOVE_CARET_TO_END = true;
				input.MoveTextEnd (true);
			}
			if (Input.GetKeyDown (KeyCode.DownArrow)) {
				if (--HistoryAt < 0) {
					HistoryAt = HistoryOccupied - 1;
				}
				input.text = History [HistoryAt];
				MOVE_CARET_TO_END = true;
				input.MoveTextEnd (true);
			}
		}
	}

	void Awake(){
		main = this;

		History = new string[HistorySize];

		pool = new Pool ();
		pool.expansion = Pool.DynamicExpansion.CycleUsedValues;
		pool.template = lineTemplate;
		pool.parent = content;
		pool.AutoDeactivate = true;

		// TODO(lubomir): Implement into console too
		const int MaxLines = 200;
		pool.Resize (MaxLines);

		InputID = CInput.Register ();

		// NOTE(lubomir): Commands
		if (commands == null) {
			commands = new List<Command> ();
		}

		commands.Add (new Command ("print", (string arg) => {
			AddLine (arg);
		}));
		commands.Add (new Command ("clear", (string arg) => {
			Clear();
		}));
		commands.Add (new Command ("quit", (string arg) => {
			Application.Quit();
		}));
		commands.Add (new Command ("url", (string arg) => {
			Application.OpenURL(arg);
		}));
		commands.Add (new Command ("load", (string arg) => {
			int level;
			if(int.TryParse(arg, out level)){
				Loader.main.Load(level);
			}
		}));
		commands.Add (new Command ("fps", (string arg) => {
			int fps;
			if(int.TryParse(arg, out fps)){
				Application.targetFrameRate = fps;
			}
		}));
		commands.Add (new Command ("vsync", (string arg) => {
			int vsync;
			if(int.TryParse(arg, out vsync)){
				QualitySettings.vSyncCount = vsync;
			}
		}));
		commands.Add (new Command ("cmdlist", (string arg) => {
			Console.main.StartCoroutine(CMDLISTPROGRESSIVE());

			/*int i = 0;
			foreach(Command cmd in commands){
				++i;
				AddLine(i + ". " + cmd.name);
			}*/
		}));
		commands.Add (new Command ("func", (string arg) => {
			char[] split = new char[1];
			split [0] = ' ';
			string[] s = arg.Split (split, 2, System.StringSplitOptions.RemoveEmptyEntries);

			if(s.Length >= 2){
				s[1] = Trim(s[1]);

				Cmd cmd = (string arg1) => {
					Submit(s[1]);
				};

				AddCommand(s[0], cmd);
			}
		}));
		commands.Add (new Command ("run_in_background", (string arg) => {
			int i;
			if(int.TryParse(arg, out i)){
				Application.runInBackground = i>0;
			}
		}));
		commands.Add (new Command ("console_command_history", (string arg) => {
			Line("Command History Size: " + HistoryOccupied, Color.yellow);
			for(int i=0;i<HistoryOccupied;++i){
				Line((i+1) + ". " + History[i]);
			}
		}));
	}

	IEnumerator CMDLISTPROGRESSIVE(){
		int i = 0;
		foreach(Command cmd in commands){
			yield return null;
			++i;
			AddLine(i + ". " + cmd.name);
		}
	}

	public void AddLine(string line){
		AddLine (line, new Color (1f, 1f, 1f, 1f));
	}

	public void AddLine(string line, Color color){
		bool stick = scroll.normalizedPosition.y == 0f;
		RectTransform t = pool.RequestGameObject ().GetComponent<RectTransform> ();

		LineCount++;

		float y = LineCount * lineHeight + topSpace;
		t.offsetMax = new Vector2 (0f, lineHeight - y);
		t.offsetMin = new Vector2 (lineOffset, -y);

		Vector2 size = content.sizeDelta;
		size.y = y + bottomSpace;
		content.sizeDelta = size;
//		content.offsetMin = new Vector2 (0f, -y - bottomSpace);

		if (stick) {
			scroll.normalizedPosition = new Vector2 (0f, 0f);
		}

		Text text = t.GetComponent<Text> ();
		text.text = line;
		text.color = color;
	}

	void Clear(){
		LineCount = 0;
		pool.ReleaseAll ();
	}

	string Trim(string s){
		int start = 0;
		int end = s.Length - 1;

		trim_start:
		// trim white space
		while (s [start] == ' ' && start <= end) {
			start++;
		}

		while (s [end] == ' ' && end >= start) {
			end--;
		}

		if (end == start) {
			return string.Empty;
		}

		// check if we can remove brackets
		if(s[start] == '"' && s[end] == '"'){
			for (int i = start + 1; i < end; ++i) {
				if (s [i] == '"') {
					goto finished;
				}
			}

			start++;
			end--;

			goto trim_start;
		}

		finished:
		return s.Substring (start, end - start + 1);
	}

	string[] SplitCommands(string cmd){
		List<string> strings = new List<string> ();

		int last = 0;
		int quotes = 0;
		int i;
		for (i = 0; i < cmd.Length; ++i) {
			switch (cmd [i]) {
			case '"':
				if (quotes > 0) {
					quotes = 0;
				} else {
					quotes = 1;
				}
				break;
			case ';':
				if (quotes == 0) {
					if (i - last > 0) {
						string s = cmd.Substring (last, i - last);
						strings.Add (s);
					}
					last = i+1;
				}
				break;
			}
		}

		if (i - last > 0) {
			string lastString = cmd.Substring (last, i - last);
			strings.Add (lastString);
		}

		return strings.ToArray ();
	}

	void Submit(string command){
		AddToHistory (command);

		#if false
		char[] split = new char[1];
		split [0] = ';';
		string[] commands = command.Split (split, System.StringSplitOptions.RemoveEmptyEntries);
		#else
		string[] commands = SplitCommands (command);
		#endif
		foreach (string s in commands) {
			Submit2 (s);
		}
	}

	void Submit2(string command){
		char[] split = new char[1];
		split [0] = ' ';
//		split [1] = '"';
		string[] s = command.Split (split, 2, System.StringSplitOptions.RemoveEmptyEntries);

		foreach(Command cmd in commands){
			if (cmd.name == s [0]) {
				if (s.Length >= 2) {
					cmd.cmd (s [1]);
				} else {
					cmd.cmd (string.Empty);
				}
				return;
			}
		}

		AddLine ("'" + s [0] + "' is not a recognized command.", Color.red);
	}

	Command FindCommand(string name){
		foreach (Command cmd in commands) {
			if (cmd.name == name) {
				return cmd;
			}
		}
		return null;
	}

	public delegate void Cmd(string arg);
	class Command{
		public Command (string name, Cmd cmd){
			this.name = name;
			this.cmd = cmd;
		}

		public string name;
		public Cmd cmd;
	}
}
