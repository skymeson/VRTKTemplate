// This script contains code from the asset 'DevConsole' version 2.2.3 by Antonio Cobo.
// Please refer to the following resources for more informations about his asset:
// https://www.assetstore.unity3d.com/en/#!/content/16833
// https://forum.unity3d.com/threads/c...onitor-and-communicate-with-your-game.437909/

using System;
using System.Reflection;

namespace DoomtrinityFPSPrototype.Utils {
	
	[System.Serializable]
	public abstract class CommandBase {

		// ========================================================================================================================
		// Variables

		public string name{
			get; private set;
		}
		public string helpText{
			get; private set;
		}

		private string[] paramList = null;
		public virtual string[] ParamList {
			get{ 
				return paramList;
			}
			protected set{ 
				paramList = value;
			}
		} 

		Delegate method;
		protected abstract object[] ParseArguments (string args);
		
		// ========================================================================================================================
		// Constructors

		public CommandBase (string name, Delegate method){
			this.name = name;
			this.method = method;
		}
		public CommandBase (string name, Delegate method, string helpText):this(name, method){
			this.helpText = helpText;
		}

		public CommandBase (string name, Delegate method, string helpText, string[] paramList):this(name, method){
			this.helpText = helpText;
			this.paramList = paramList;
		}

		public CommandBase(Delegate method):this(method.Method.DeclaringType.Name+"."+method.Method.Name, method){}

		// ========================================================================================================================
		// Execute

		public void Execute(string args){
			try{
				object[] o = ParseArguments(args);
				method.Method.Invoke(method.Target,o);

			}catch(ArgumentException e){
				if (e.Message.Equals ("NO_PARAM") || e.Message.Equals ("HELP") ) {
					DevConsole.LogInfo (this.helpText);
				} else {
					DevConsole.LogError(e.Message+(DevConsole.Verbose?"\n"+e.StackTrace:string.Empty));
				}
			}
		}


		// ========================================================================================================================
		// GetValue

		protected virtual string GetValue(string arg){
			if (!Util.isNullOrWhiteSpace(arg)) {
				return arg.Trim();
			} else {
				throw new ArgumentException ("NO_PARAM");
			}

		}

		// ========================================================================================================================
	}
}