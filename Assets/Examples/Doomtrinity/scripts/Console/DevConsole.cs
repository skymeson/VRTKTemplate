// This script contains code from the asset 'DevConsole' version 2.2.3 by Antonio Cobo.
// Please refer to the following resources for more informations about his asset:
// https://www.assetstore.unity3d.com/en/#!/content/16833
// https://forum.unity3d.com/threads/c...onitor-and-communicate-with-your-game.437909/

using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using StringBuilder = System.Text.StringBuilder;

namespace DoomtrinityFPSPrototype.Utils {

	public enum Cheat
	{
		EnemySpawn,
		God,
		Noclip
	}

	public class CheatMessage:BaseMessage{
		public readonly Cheat cheatType;
		public CheatMessage(Cheat _cheatType){
			cheatType = _cheatType;
		}
	}

	[System.Serializable]
	public class DevConsole : MonoBehaviour,ISerializationCallbackReceiver {

		// ========================================================================================================================
		// Variables

		[SerializeField] bool dontDestroyOnLoad;							// Sets whether it destroys on load or not
		[SerializeField] KeyCode consoleKey = KeyCode.Backslash;			// Key to open/close Console
		[RangeAttribute(8, 20)]
		public int fontSize;

		const int TEXTAREA_OFFSET = 7;					// Margin for the text at the bottom of the console
		const int TEXTFIELD_HEIGHT = 25;
		const int TEXTAREA_BOTTOMOFFSET = 5;

		const int WARNING_THRESHOLD = 10000;			// Number of characters in consoleText at which a warning appears
		const int DANGER_THRESHOLD = 12000;				// Number of characters in consoleText at which a danger appears	
		const int AUTOCLEAR_THRESHOLD = 14500;			// At this many characters, a clear will be done automatically

		const int PAGEUP_PAGEDOWN_SCROLL_LINES = 3;


		List<CommandBase> consoleCommands;				// Whole list of commands available		
		List<CvarsBase> consoleCvars;
		List<string> candidates = new List<string>();	// Commands that match existing text
		IEnumerator candidates_enumerator;
		int selectedCandidate = 0;						// Index of candidate selected
		List<string> history = new List<string>();		// A history of texts sent into the console
		int selectedHistory = 0;						// Current index in the history
		List<KeyValuePair<string, string>> buffer = 
			new List<KeyValuePair<string, string>>();	// Messages buffer. Stored in order to print them on the next OnGUI update

		bool opening;									// Can write already?
		bool closed = true;								// Is the Console closed?

		//bool inHistory = false;							// Are we browsing the history?
		bool showTimeStamp = false;						// Should time stamps be shown?

		float numLinesThreshold;						// Max numes allowed in the console
		float maxConsoleHeight; 						// Screen.height/3

		float currentConsoleHeight;						// Current Y position in pixels
		Vector2 consoleScroll = Vector2.zero;

		[HideInInspector]
		[SerializeField] string serializedConsoleText = string.Empty;
		StringBuilder consoleText = new StringBuilder();// Text in the whole console log
		string inputText = string.Empty;				// Text in the input TextField
		//string lastText = string.Empty;					// Used for history mostly
		int numLines;									// Number of '\n' in consoleText
		float lineHeight;
		[SerializeField] Settings extraSettings;
		public static bool Verbose{get{return Instance.extraSettings.logVerbose;}set{Instance.extraSettings.logVerbose = value;}}
		public static bool IsOpen{get{return !Instance.closed;}}

		// ========================================================================================================================
		// Singleton stuff

		private static DevConsole _instance;
		public static DevConsole Instance{						
			get{
				return _instance;
			}
		}

		// Awake

		void Awake(){
			if (_instance != null && _instance != this) {
				Destroy(this.gameObject);            
			}
			else {
				_instance = this;            
			}
			if (dontDestroyOnLoad)
				DontDestroyOnLoad(this.gameObject);
			if (extraSettings.showDebugLog)
				Application.logMessageReceived+=LogCallback;
		}

		// ========================================================================================================================
		// OnEnable

		void OnEnable(){
			if (consoleCommands == null) {
				consoleCommands = new List<CommandBase> ();
			}

			if (consoleCvars == null) {
				consoleCvars = new List<CvarsBase> ();
			}

			if (extraSettings.defaultCommands) {
				AddCommands (
					new Command ("clear", Clear, "Clear the console"),
					new Command ("listcmds", ListCmds, "Show a list of all Commands"),
					new Command ("listcvars", ListCvars, "Show a list of all Cvars"),
					new Command ("quit", Application.Quit, "Exit the game"),
					new Command ("help", HelpMessage, "Show the help message"),
					new CommandParam ("conDump", ConsoleDump, string.Format("'{0}'\n{1}","Usage: conDump <file>","Dump console text to file"))
				);

				AddCvars (
					new Cvar<bool> ("com_showTimestamp", ShowTimeStamp, false, false, "Establishes whether or not to show the time stamp for each command"),
					new Cvar<bool> ("com_showDebugLog", ShowLog, false, false, "Establishes whether or not to show Unity Debug Log")
				);
			}
			HelpMessage ();
		}

		// ========================================================================================================================
		// OnBeforeSerialize

		public void OnBeforeSerialize(){
			serializedConsoleText = consoleText.ToString();
		}

		// ========================================================================================================================
		// OnAfterDeserialize

		public void OnAfterDeserialize(){
			consoleText.Append(serializedConsoleText);
		}

		// ========================================================================================================================
		// OnGUI

		void OnGUI() {
			GUISkin oldSkin = GUI.skin;
			if (extraSettings.skin != null)
				GUI.skin = extraSettings.skin;
			if (consoleKey == KeyCode.None)
				return;

			Event current = Event.current;
			GUI.skin.textArea.richText = true;
			if (extraSettings.font != null)
				GUI.skin.font = extraSettings.font;

			GUI.skin.textArea.fontSize = GUI.skin.textField.fontSize = fontSize;

			//Open/Close Console
			if (current.type == EventType.KeyDown && current.keyCode == consoleKey){
				GUIUtility.keyboardControl = 0;
				StartCoroutine(FadeInOut(closed));
			}

			lineHeight = GUI.skin.textArea.lineHeight;
			//Local declarations
			bool inTransition = !((currentConsoleHeight == maxConsoleHeight) || (currentConsoleHeight == 0));
			float height = lineHeight*numLines;
			float scrollHeight = height>currentConsoleHeight-TEXTFIELD_HEIGHT?height:currentConsoleHeight-TEXTFIELD_HEIGHT;

			if (!closed){
				//Treat buffer
				for (int i = 0; i< buffer.Count; i++)
					BasePrintOnGUI(buffer[i].Key, buffer[i].Value);
				buffer.Clear();

				if (!inTransition)
					GUI.FocusControl("TextField");
				//KEYS
				if (current.type == EventType.KeyDown){
					//if (!string.IsNullOrEmpty (inputText)) {
					switch (current.keyCode) {
					case KeyCode.Return:
						ProcessEnterPress (inputText);
						break;
					case KeyCode.Tab:

						if (candidates.Count > 0) { // should get in here only if previous button was tab too

							if (candidates_enumerator.MoveNext ()) {
								inputText = candidates_enumerator.Current as string;
								inputText += ' ';
								SetCursorPos (inputText, inputText.Length);
							} else {
								candidates_enumerator.Reset ();
								if (candidates_enumerator.MoveNext ()) {
									inputText = candidates_enumerator.Current as string;
									inputText += ' ';
									SetCursorPos (inputText, inputText.Length);
								}
							}

						} else {
							if(!string.IsNullOrEmpty(inputText)){
								string prefix = FindInputCandidates (inputText,candidates); // populate candidates
								if (candidates.Count > 0) {
									candidates_enumerator = candidates.GetEnumerator ();
									if (candidates.Count > 1) { // print all candidates that contains input prefix
										DevConsole.Log (inputText);
										foreach (string cnd in candidates) {
											DevConsole.Log ('\t'+cnd);
										}
									}

									if (prefix.Length > 0) {
										inputText = prefix;
										if (candidates.Count == 1) { // don't add space if command is still partial
											inputText += ' ';
											candidates.Clear ();
										}
									}

									SetCursorPos (inputText, inputText.Length);
								}
							}

						}

						break;
					case KeyCode.Escape:

						break;
					case KeyCode.UpArrow:

						if (history.Count != 0){
							bool first = string.IsNullOrEmpty(inputText);
							selectedHistory = Mathf.Clamp(selectedHistory+(first?0:1), 0, history.Count-1);

							inputText = history[selectedHistory];
						}

						SetCursorPos(inputText, inputText.Length);
						GUIUtility.ExitGUI(); // "prevent default" seems to stop this method immediately, needed to allow the cursor to stay at the end, guess unity recognize up for its stuff, it automatically set cursor at start...
						break;
					case KeyCode.DownArrow:

						if ( history.Count != 0){
							bool first = string.IsNullOrEmpty(inputText);
							selectedHistory = Mathf.Clamp(selectedHistory-(first?0:1), 0, history.Count-1);
							inputText = history[selectedHistory];

						}

						SetCursorPos(inputText, inputText.Length);
						GUIUtility.ExitGUI(); // same here, seems that unity with down arrow moves automatically cursor at end though
						break;

					case KeyCode.PageUp:

						consoleScroll.y -= lineHeight*PAGEUP_PAGEDOWN_SCROLL_LINES;
						break;

					case KeyCode.PageDown:

						consoleScroll.y += lineHeight*PAGEUP_PAGEDOWN_SCROLL_LINES;
						break;

					}
					// we get in here 2 times: (put a tracepoint, debug.log break things here...)
					// current keycode is UnityEngine.KeyCode.Tab
					// current keycode is UnityEngine.KeyCode.None
					if (current.keyCode != KeyCode.None && current.keyCode != KeyCode.Tab && candidates.Count > 0) { // clear candidates if not pressing tab continuously
						candidates.Clear ();
					}

				}

				//CONSOLE PAINTING
				GUI.Box(new Rect(0,0,Screen.width, currentConsoleHeight), new GUIContent());
				GUI.SetNextControlName("TextField");
				GUI.enabled = !opening;
				inputText = GUI.TextField(new Rect(0, currentConsoleHeight-TEXTFIELD_HEIGHT, Screen.width, TEXTFIELD_HEIGHT), inputText);
				GUI.enabled = true;
				GUI.skin.textArea.normal.background = null;
				GUI.skin.textArea.hover.background = null;


				consoleScroll = GUI.BeginScrollView(new Rect(0,0,Screen.width, currentConsoleHeight-TEXTFIELD_HEIGHT),consoleScroll,
					new Rect(0, 0, Screen.width-20, scrollHeight));

				GUI.TextArea(	new Rect(	0, 
					currentConsoleHeight-TEXTFIELD_HEIGHT-TEXTAREA_BOTTOMOFFSET-(numLines==0?lineHeight:height)+((numLines>=numLinesThreshold-1?lineHeight*(numLines-numLinesThreshold)+TEXTFIELD_HEIGHT:0)), 
											Screen.width,
											TEXTAREA_OFFSET+(numLines==0?lineHeight:height)), 
								consoleText.ToString()
							);

				GUI.EndScrollView();

			}

			GUI.skin = oldSkin;	
		}

		// ========================================================================================================================
		// Open

		public static void Open(){
			if (IsOpen)
				return;
			GUIUtility.keyboardControl = 0;
			Instance.StartCoroutine(Instance.FadeInOut(true));
		}

		// ========================================================================================================================
		// Close

		public static void Close(){
			if (!IsOpen)
				return;
			GUIUtility.keyboardControl = 0;
			Instance.StartCoroutine(Instance.FadeInOut(false));
		}

		// ========================================================================================================================
		// FadeInOut

		IEnumerator FadeInOut(bool open){
			if (opening)
				yield break;
			opening = true;
			maxConsoleHeight = Screen.height/2.0f;
			numLinesThreshold = maxConsoleHeight/lineHeight;
			closed = false;
			float duration = extraSettings.curve[extraSettings.curve.length-1].time;
			float time = 0;
			do{
				currentConsoleHeight = maxConsoleHeight*extraSettings.curve.Evaluate(open?time:duration-time);
				yield return null;
				time=Mathf.Clamp(time+Time.unscaledDeltaTime,0,duration);
			}while (time<duration);
			currentConsoleHeight = maxConsoleHeight*extraSettings.curve.Evaluate(open?time:duration-time);
			closed = !open;
			if (closed)
				inputText =string.Empty;
			opening = false;
		}

		// ========================================================================================================================
		// MatchCommandPrefix

		string FindInputCandidates(string _inputText, List<String> _candidates) {

			string common_prefix = string.Empty;

			_candidates.Clear();

			// I was about to implement word to autocomplete based on caret pos...leave this for now
			// int caret_pos = GetCursorPos();
			string[] words = _inputText.Split(new [] {' '}, StringSplitOptions.RemoveEmptyEntries); // ignore multiple spaces between words
			if (words != null && words.Length > 0) { // the input text contains at least one command with one param
				string cmd = words [0];  // cvars don't actually implement a param list, so don't take them into accout for now

				IEnumerable<CommandBase> tmp_cmd = consoleCommands.Where (c => c.name.Equals (cmd)); // it should always be one...
				if (tmp_cmd != null && tmp_cmd.ToList ().Count > 0) { // check if entered input matches a command
					CommandBase commandBase = (tmp_cmd.ToList ()) [0];

					if (commandBase.ParamList != null && commandBase.ParamList.Length > 0) { // print command params if any
						string first_param = words.Length > 1 ? words [1] : null;
						bool printAllParams = (first_param == null);
						foreach (string p in commandBase.ParamList) {
							if (printAllParams || (first_param != null && p.ToUpper ().StartsWith (first_param.ToUpper ()))) {
								_candidates.Add (new StringBuilder ().Append (commandBase.name).Append (' ').Append (p).ToString ());
							}
						}
					}
				} else {
					for (int i = 0; i < consoleCommands.Count; i++) {

						if (consoleCommands [i].name.ToUpper ().StartsWith (_inputText.ToUpper ())) {
							_candidates.Add (consoleCommands [i].name);
						}					
					}
					for (int i = 0; i < consoleCvars.Count; i++) {

						if (consoleCvars [i].name.ToUpper ().StartsWith (_inputText.ToUpper ())) {
							_candidates.Add (consoleCvars [i].name);
						}					
					}
				}			
			}

			common_prefix = Util.FindCommonPrefix (_candidates.ToArray());
			return common_prefix;
		}

		// ========================================================================================================================
		// SetCursorPos

		#region Tools
		void SetCursorPos(string text, int pos){
			TextEditor te = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor),GUIUtility.keyboardControl);
			te.text = text;
			te.cursorIndex = pos;
			te.selectIndex = pos;
			// GUIUtility.ExitGUI();
		}

		int GetCursorPos(){
			TextEditor te = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor),GUIUtility.keyboardControl);
			return te.cursorIndex;

		}

		// ========================================================================================================================
		// ColorToHex

		public static string ColorToHex(Color color){
			string hex ="0123456789ABCDEF";
			int r = (int)(color.r*255);
			int g = (int)(color.g*255);
			int b = (int)(color.b*255);

			return	hex[(int)(Mathf.Floor(r/16))].ToString()+hex[(int)(Mathf.Round(r%16))].ToString()+
				hex[(int)(Mathf.Floor(g/16))].ToString()+hex[(int)(Mathf.Round(g%16))].ToString()+
				hex[(int)(Mathf.Floor(b/16))].ToString()+hex[(int)(Mathf.Round(b%16))].ToString();
		}

		#endregion

		// ========================================================================================================================
		// Log

		#region Logs
		/// <summary>
		/// Logs a white text to the console.
		/// </summary>
		/// <param name="text">Text to be sent.</param>
		public static void Log(string text){
			Instance.BasePrint(text);
		}

		// ========================================================================================================================
		// Log

		public static void Log(object obj){
			Log (obj.ToString());
		}
		/// <summary>
		/// Logs a ligh blue text to the console.
		/// </summary>
		/// <param name="text">Text to be sent.</param>
		public static void LogInfo(string text){
			Instance.BasePrint(text, Color.cyan);
		}

		// ========================================================================================================================
		// LogInfo

		public static void LogInfo(object obj){
			LogInfo (obj.ToString());
		}

		// ========================================================================================================================
		// LogWarning

		/// <summary>
		/// Logs a yellow text to the console.
		/// </summary>
		/// <param name="text">Text to be sent.</param>
		public static void LogWarning(string text){
			Instance.BasePrint(text, Color.yellow);
		}

		// ========================================================================================================================
		// LogWarning

		public static void LogWarning(object obj){
			LogWarning (obj.ToString());
		}

		// ========================================================================================================================
		// LogError

		/// <summary>
		/// Logs a red text to the console.
		/// </summary>
		/// <param name="text">Text to be sent.</param>
		public static void LogError(string text){
			Instance.BasePrint(text, Color.red);
		}

		// ========================================================================================================================
		// LogError

		public static void LogError(object obj){
			LogError (obj.ToString());
		}

		// ========================================================================================================================
		// Log

		/// <summary>
		/// Logs a text to the console with the specified color.
		/// </summary>
		/// <param name="text">Text to be sent.</param>
		/// <param name="color">Color provided in HTML format.</param>
		public static void Log(string text, string color){
			Instance.BasePrint(text, color);
		}

		// ========================================================================================================================
		// Log

		public static void Log(object obj, string color){
			Log(obj.ToString(), color);
		}

		// ========================================================================================================================
		// Log

		/// <summary>
		/// Logs a text to the console with the specified color.
		/// </summary>
		/// <param name="text">Text to be sent.</param>
		/// <param name="color">Color to be used.</param>
		public static void Log(string text, Color color){
			Instance.BasePrint(text,color);
		}

		// ========================================================================================================================
		// Log

		public static void Log(object obj, Color color){
			Log(obj.ToString(), color);
		}
		#endregion

		// ========================================================================================================================
		// BasePrint

		#region Prints
		void BasePrint(string text){
			BasePrint(text, ColorToHex(Color.green));
		}

		// ========================================================================================================================
		// BasePrint

		void BasePrint(string text, Color color){
			BasePrint(text, ColorToHex(color));	
		}

		// ========================================================================================================================
		// BasePrint

		void BasePrint(string text, string color){
			buffer.Add(new KeyValuePair<string, string>(text, color));
		}

		// ========================================================================================================================
		// BasePrintOnGUI

		//In case print is called from another thread, the action is cached in a buffer and processed in the next OnGUI update
		void BasePrintOnGUI(string text, string color){
			if (text == null) {
				Debug.LogWarning ("DevConsole.BasePrintOnGUI: text is null!");
				return;
			}
			int numLineJumps = 1;
			string time = (showTimeStamp?"["+System.DateTime.Now.ToShortTimeString()+"]  ":string.Empty);
			StringBuilder lastLine = new StringBuilder(time);
			for (int i = 0; i < text.Length; i++){
				if (text[i] == '\n'){
					numLineJumps++;
					lastLine = new StringBuilder(time);
				}
				else
					lastLine.Append(text[i]);
				if (GUI.skin.textArea.CalcSize(new GUIContent(lastLine.ToString())).x>Screen.width-20){
					text = text.Insert(i, "\n");
					i--;
				}
			}
			text += '\n'; // "\r\n"
			numLines+= numLineJumps;
			if (numLines >=numLinesThreshold-1)
				consoleScroll = new Vector2(0,consoleScroll.y+int.MaxValue);
			AddText(text, color);
			if (consoleText.Length>AUTOCLEAR_THRESHOLD){
				Clear();
				AddText("Console cleared automatically\n", ColorToHex(Color.yellow));
			}
			else if (consoleText.Length>DANGER_THRESHOLD)
				AddText("Console is about to auto-clean. 'conDump' to file if necessary\n", ColorToHex(Color.red));
			else if (consoleText.Length>WARNING_THRESHOLD)
				AddText("Console is about to auto-clean. 'conDump' to file if necessary\n", ColorToHex(Color.yellow));
		}

		// ========================================================================================================================
		// AddText

		void AddText(string text, string color){
			consoleText.Append(string.Format("{0}<color=#{1}>{2}</color>",showTimeStamp?"["+System.DateTime.Now.ToShortTimeString()+"]  ":string.Empty, color, text));
		}

		// ========================================================================================================================
		// ProcessEnterPress

		void ProcessEnterPress(string input){
			BasePrint(']'+input);
			if(string.IsNullOrEmpty(input)){
				return;
			}
			inputText = string.Empty;
			if ((history.Count == 0 || history[0] != input) && input.Trim() != string.Empty)
				history.Insert(0,input);
			selectedHistory = 0;
			ExecuteCommandInternal(input);
		}

		// ========================================================================================================================
		// LogCallback

		void LogCallback(string log, string stackTrace, LogType type){
			Color color;
			if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
				color = Color.red;
			else if (type == LogType.Warning)
				color = Color.yellow;
			else// if (type == LogType.Log)
				color = Color.cyan;
			BasePrint(log, color);
			BasePrint(stackTrace, color);

			int length =(int) GUI.skin.textArea.CalcSize(new GUIContent(log)).x;
			while (length>= Screen.width){
				length-=Screen.width;
				numLines++;
			}
			// numLines++;
		}
		#endregion

		// ========================================================================================================================
		// ExecuteCommand

		#region Manage Commands
		public static void ExecuteCommand(string input){
			Instance.ExecuteCommandInternal(input);
		}

		// ========================================================================================================================
		// ExecuteCommand

		public static void ExecuteCommand(string command, string args){
			Instance.ExecuteCommandInternal(command+" "+args);
		}

		// ========================================================================================================================
		// ExecuteCommandInternal

		void ExecuteCommandInternal(string input){
			
			string parameters = null;
			bool cmdExists = false;
			for (int i = 0; i < consoleCommands.Count; i++){
				if (input.ToUpper ().StartsWith (consoleCommands [i].name.ToUpper ())) { // get command
					// check if there's a space after the command, if so then execute it
					int cmd_length = consoleCommands [i].name.Length;
					if (input.Length > cmd_length && input [cmd_length].Equals (' ')) {
						parameters = input.Substring (cmd_length + 1);
						consoleCommands [i].Execute (parameters);
						cmdExists = true;
						break;
					} else if (input.Length == cmd_length) {
						consoleCommands [i].Execute (null);
						cmdExists = true;
						break;
					}
				}

			}
			if(!cmdExists){
				for (int i = 0; i < consoleCvars.Count; i++){
					if (input.ToUpper ().StartsWith (consoleCvars [i].name.ToUpper ())) {
						// check if there's a space after the cvar, if so then execute it
						int cvar_length = consoleCvars [i].name.Length;
						if (input.Length > cvar_length && input [cvar_length].Equals (' ')) {
							parameters = input.Substring (cvar_length + 1);
							consoleCvars [i].Execute (parameters);
							cmdExists = true;
							break;
						} else if (input.Length == cvar_length) {
							consoleCvars [i].Execute (null);
							cmdExists = true;
							break;
						}
					}
				}
			}

			if(!cmdExists) {
				DevConsole.Log ("command '" + (input.Split (' ')) [0] + "' not found");
			}
		}

		// ========================================================================================================================
		// AddCvars

		public static void AddCvars(params CvarsBase[] cs){
			foreach(CvarsBase c in cs)
				AddCvar(c);
		}

		// ========================================================================================================================
		// AddCvar

		public static void AddCvar(CvarsBase c){
			if (!CvarExists(c.name) && !CommandExists(c.name))
				Instance.consoleCvars.Add (c);
		}

		// ========================================================================================================================
		// CommandExists

		static bool CvarExists(string cvarName){
			foreach(CvarsBase c in Instance.consoleCvars){
				if (c.name.ToUpper() == cvarName.ToUpper()){
					LogError("The command " + cvarName + " already exists");
					return true;
				}
			}
			return false;
		}

		// ========================================================================================================================
		// AddCommands

		public static void AddCommands(params CommandBase[] cs){
			foreach(CommandBase c in cs)
				AddCommand(c);
		}

		// ========================================================================================================================
		// AddCommand

		public static void AddCommand(CommandBase c){
			if (!CommandExists(c.name) && !CvarExists(c.name))
				Instance.consoleCommands.Add (c);
		}

		// ========================================================================================================================
		// CommandExists

		static bool CommandExists(string commandName){
			foreach(CommandBase c in Instance.consoleCommands){
				if (c.name.ToUpper() == commandName.ToUpper()){
					LogError("The command " + commandName + " already exists");
					return true;
				}
			}
			return false;
		}

		// ========================================================================================================================
		// RemoveCommand

		public static void RemoveCommand(string commandName){
			foreach(CommandBase c in Instance.consoleCommands){
				if (c.name == commandName){
					Instance.consoleCommands.Remove(c);
					Log("Command " + commandName + " removed successfully", Color.green);
					return;
				}
			}
			LogWarning("The command " + commandName + " could not be found");
		}

		// ========================================================================================================================
		// RemoveCvar

		public static void RemoveCvar(string cvarName){
			foreach(CvarsBase c in Instance.consoleCvars){
				if (c.name == cvarName){
					Instance.consoleCvars.Remove(c);
					Log("Cvar " + cvarName + " removed successfully", Color.green);
					return;
				}
			}
			LogWarning("The cvar " + cvarName + " could not be found");
		}
		#endregion

		// ========================================================================================================================
		// ListCmds

		#region Predefined Commands
		void ListCmds(){ 
			StringBuilder sb = new StringBuilder();
			sb.Append ('\n');
			for (int i = 0; i < consoleCommands.Count; i++){
				sb.Append(consoleCommands[i].name).Append('\n');
			}
			LogInfo(sb.ToString());
		}

		// ========================================================================================================================
		// ListCvars

		void ListCvars(){ 
			StringBuilder sb = new StringBuilder();
			sb.Append ('\n');
			for (int i = 0; i < consoleCvars.Count; i++){
				sb.Append(consoleCvars[i].name).Append('\n');
			}
			LogInfo(sb.ToString());
		}

		// ========================================================================================================================
		// Clear

		void Clear(){
			Instance.consoleText = new StringBuilder();
			Instance.numLines = 0;
		}

		// ========================================================================================================================
		// Help

		void HelpMessage(){
			StringBuilder sb = new StringBuilder ();
			sb.Append ("---doomtrinity's FPS Prototype---\n\n")
				.Append ("*Type 'listcmds' and 'listcvars' to show all commands and cvars\n")
				.Append ("*Add '-h' parameter to show help (if available) for a specific command or cvar,\n e.g. 'spawn -h'\n")
				.Append ("*Use 'TAB' key to auto-complete/cycle through all commands/cvars or command parameters\n")
				.Append ("*Use up/down arrow to browse the input history\n")
				.Append ("*Use page-up/down to scroll the text area\n")
				.Append ("*Type 'help' to show this message\n ");

			LogInfo (sb.ToString ());
		}

		// ========================================================================================================================
		// ConsoleDump

		void ConsoleDump(string filename) {
			if(Util.isNullOrWhiteSpace(filename)){
				Log("Usage: conDump <filename>");
				return;
			}
			if(consoleText != null){
				string text = Regex.Replace(consoleText.ToString(),"<color=#.*?>|</color>",string.Empty);
				System.IO.File.WriteAllText(Application.dataPath+'/'+filename, text);
			}
		}

		// ========================================================================================================================
		// ShowVerbose

		void ShowVerbose(bool show){
			Verbose = show;
			Log("Verbose mode changed", Color.green);
		}

		// ========================================================================================================================
		// ShowLog

		void ShowLog(bool value){
			if (value)
				Application.logMessageReceived+=LogCallback;
			else
				Application.logMessageReceived-=LogCallback; 
			Log("Change successful", Color.green);
		}

		// ========================================================================================================================
		// ShowTimeStamp

		void ShowTimeStamp(bool value){
			showTimeStamp = value;
			Log("Change successful", Color.green);
		}

		// ========================================================================================================================
		// SetFontSize

		void SetFontSize(int size){
			fontSize = size;
			Log("Change successful", Color.green);
		}
		#endregion

	}

	[System.Serializable]
	struct Settings{
		public AnimationCurve curve;
		[TooltipAttribute("When checked, logs from the Debug class and exceptions will be printed to the Console")]
		public bool showDebugLog;
		[TooltipAttribute("Wether or not to add the built-in commands")]
		public bool defaultCommands;
		[TooltipAttribute("Log additional errors information")]
		public bool logVerbose;
		[TooltipAttribute("If none is set, the default one will be used")]
		public GUISkin skin;
		[TooltipAttribute("If none is set, the default one will be used")]
		public Font font;
	}

}