// This script contains code from the asset 'DevConsole' version 2.2.3 by Antonio Cobo.
// Please refer to the following resources for more informations about his asset:
// https://www.assetstore.unity3d.com/en/#!/content/16833
// https://forum.unity3d.com/threads/c...onitor-and-communicate-with-your-game.437909/

using System;

namespace DoomtrinityFPSPrototype.Utils {
	
	public class Command:CommandBase {

		// ========================================================================================================================
		// Variables

		public delegate void ConsoleMethod();

		// ========================================================================================================================
		// Constructors
		
		public Command (string name, ConsoleMethod method):base(name, method){}
		public Command (string name, ConsoleMethod method, string helpText):base(name, method, helpText){}
		public Command (ConsoleMethod method):base(method){}

		// ========================================================================================================================
		// ParseArguments

		protected override object[] ParseArguments (string args){
			if(args != null && args.Trim().ToUpper().Equals("-H")){
				throw new ArgumentException("HELP");
			}
			return new object[]{};
		}
		// ========================================================================================================================
	}

	public class CommandParam:CommandBase {

		// ========================================================================================================================
		// Variables

		public delegate void ConsoleMethod(String arg0);

		// ========================================================================================================================
		// Constructors

		public CommandParam (string name, ConsoleMethod method):base(name, method){}
		public CommandParam (string name, ConsoleMethod method, string helpText):base(name, method, helpText){}
		public CommandParam (string name, ConsoleMethod method, string helpText, string[] paramList):base(name, method, helpText, paramList){}
		public CommandParam (ConsoleMethod method):base(method){}

		// ========================================================================================================================
		// ParseArguments

		protected override object[] ParseArguments (string args) {

			try{
				string val = GetValue(args);
				if(val.ToUpper().Equals("-H")){
					throw new ArgumentException("HELP");
				}
				return new object[]{val};
			}
			catch(ArgumentException e){
				throw e;
			}

		}
		// ========================================================================================================================

	}

}