using UnityEngine;
using System.Collections;
using System.IO;

// Attach this script to an empty gameobject in the scene.
// This utility log any error or exception to external file.
// Useful for demo version.
public class ExceptionLogger : MonoBehaviour {

	// ========================================================================================================================
	// Instance variables

	//Internal reference to stream writer object
	private System.IO.StreamWriter SW;

	//Filename to assign log
	[SerializeField] private string LogFileName = "log.txt";

	// ========================================================================================================================
	// Singleton stuff

	private static ExceptionLogger _instance;
	public static ExceptionLogger Instance
	{
		get
		{
			return _instance;
		}
	}

	private void Awake()
	{

		if (_instance != null && _instance != this)
		{
			Destroy(this.gameObject);
		}
		else {
			_instance = this;
			DontDestroyOnLoad(this.gameObject); // Persist across scenes.                         
		}
	}

	// ========================================================================================================================
	// Start

	private void Start () 
	{
		//Create string writer object
		// SW = new System.IO.StreamWriter(Application.persistentDataPath + "/" + LogFileName);
		SW = new System.IO.StreamWriter(Application.persistentDataPath + "/" + LogFileName, true); // true --> append
		SW.WriteLine("Logged at: " + System.DateTime.Now.ToString() + " - Game started!");
	}

	// ========================================================================================================================
	// OnEnable

	//Register for exception listening, and log exceptions
	private void OnEnable() 
	{
		// Application.RegisterLogCallback(HandleLog);
		Application.logMessageReceived += HandleLog;
	}

	// ========================================================================================================================
	// OnDisable

	//Unregister for exception listening
	private void OnDisable() 
	{
		// Application.RegisterLogCallback(null);
		Application.logMessageReceived -= HandleLog;
	}

	// ========================================================================================================================
	// HandleLog

	//Log exception to a text file
	private void HandleLog(string logString, string stackTrace, LogType type)
	{
		//If an exception or error, then log to file
		if(type == LogType.Exception || type == LogType.Error)
		{
			SW.WriteLine("Logged at: " + System.DateTime.Now.ToString() + " - Log Desc: " + logString + " - Trace: " + stackTrace + " - Type: " + type.ToString());
		}
	}

	// ========================================================================================================================
	// OnDestroy

	//Called when object is destroyed
	void OnDestroy()
	{
		//Close file
		if(SW != null) { 
			// doomtrinity - this class uses singleton pattern. If start from menu, launch a new game
			// and return to menu, get a nullreferenceexception, this is due to SW
			// not initialized, because the double instance is destroyed before that.
			// So we need to check if null.
			SW.Close();
		}

	}

	// ========================================================================================================================
}
