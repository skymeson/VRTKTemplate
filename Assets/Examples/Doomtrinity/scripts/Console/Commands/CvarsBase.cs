using System;
using System.Reflection;

namespace DoomtrinityFPSPrototype.Utils {

	[System.Serializable]
	public abstract class CvarsBase {

		// ========================================================================================================================
		// Variables, properties, abstracts methods

		public string name{
			get; private set;
		}
		public string helpText{
			get; private set;
		}

		protected Delegate method;

		protected abstract object[] ParseArguments (string args);

		// ========================================================================================================================
		// Constructors

		public CvarsBase (string name, Delegate method, string helpText) {
			this.name = name;
			this.method = method;
			this.helpText = helpText;
		}

		public CvarsBase(Delegate method, string helpText):
			this(method.Method.DeclaringType.Name+"."+method.Method.Name, method, helpText){}

		// ========================================================================================================================
		// Execute

		public virtual void Execute(string args){
			try{
				object[] o = ParseArguments(args);
				method.Method.Invoke(method.Target,o);

			}catch(ArgumentException e){
				if (e.Message.Equals ("NO_PARAM")) {
					DevConsole.LogInfo ("missing parameters!");
				} else {
					DevConsole.LogError(e.Message+(DevConsole.Verbose?"\n"+e.StackTrace:string.Empty));
				}
			}
		}

		// ========================================================================================================================
		// GetValueType

		protected T GetValueType<T>(string arg){
			if (!Util.isNullOrWhiteSpace(arg)) {
				try {
					T returnValue = default(T);
					if (typeof(bool) == typeof(T)) {
						bool result;
						if (StringToBool (arg, out result)) {
							returnValue = (T)(object)result;

						} else {
							throw new ArgumentException ("The entered value is not a valid value");
						}
					} else {
						returnValue = (T)System.Convert.ChangeType (arg, typeof(T));

					}
					return returnValue;
				} catch {
					throw new ArgumentException ("The entered value is not a valid value");
				}
			} else {
				
				throw new ArgumentException ("NO_PARAM");
			}

		}

		// ========================================================================================================================
		// StringToBool

		protected bool StringToBool(string value, out bool result){
			bool isConversionSuccessful = false;
			result = false;
			int iResult = 0;

			if (int.TryParse(value, out iResult)) {
				if (iResult == 1 || iResult == 0) {
					result = iResult == 1 ? true : false;
					isConversionSuccessful = true;
				} else { 
					isConversionSuccessful = false;
				}
			}

			return isConversionSuccessful;
		}

		// ========================================================================================================================
	}
}