using System;

namespace DoomtrinityFPSPrototype.Utils {

	public class Cvar<T>:CvarsBase {

		// ========================================================================================================================
		// Variables

		public T curVal {
			get; private set;
		}
		public T defaultVal{
			get; private set;
		}

		public delegate void ConsoleMethod(T arg0);

		public Cvar (string name, ConsoleMethod method, T curVal, T defaultVal, string helpText):base(name, method, helpText){
			this.curVal = curVal;
			this.defaultVal = defaultVal;
		}
		public Cvar (ConsoleMethod method, T curVal, T defaultVal, string helpText):base(method, helpText){
			this.curVal = curVal;
			this.defaultVal = defaultVal;
		}

		// ========================================================================================================================
		// ParseArguments

		protected override object[] ParseArguments (string args) {
			if(args != null && args.Trim().ToUpper().Equals("-H")){
				throw new ArgumentException("HELP");
			}
			try{
				curVal = GetValueType<T>(args);
				return new object[]{curVal};
			}
			catch(ArgumentException e){
				throw e;
			}

		}

		// ========================================================================================================================
		// Execute

		public override void Execute(string args){
			try{
				object[] o = ParseArguments(args);
				method.Method.Invoke(method.Target,o);

			}catch(ArgumentException e){
				if (e.Message.Equals ("NO_PARAM") || e.Message.Equals ("HELP")) {
					Type paramType = typeof(T);
					if (paramType == typeof(bool)) {
						DevConsole.LogInfo (string.Format ("'{0}' is: {1}   default: {2}\n{3}", this.name, Convert.ToInt32 (this.curVal), Convert.ToInt32 (this.defaultVal), this.helpText));
					} else {
						DevConsole.LogInfo (string.Format ("'{0}' is: {1}   default: {2}\n{3}", this.name, this.curVal, this.defaultVal, this.helpText));
					}
				} else {
					DevConsole.LogError(e.Message+(DevConsole.Verbose?"\n"+e.StackTrace:string.Empty));
				}
			}
		}
		// ========================================================================================================================

	}

}