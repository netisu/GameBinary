using Godot;
using System;
using System.Collections.Generic;

namespace Netisu
{
	public partial class PreservedGlobalState : Node
	{
		private Dictionary<string, MoonSharp.Interpreter.DynValue> dict = [];
		public Dictionary<string, MoonSharp.Interpreter.Script> scriptsdict = [];

		public MoonSharp.Interpreter.DynValue Seek(string what)
		{
			if (dict.ContainsKey(what))
			{
				return dict[what];
			}
			else
			{
				return null;
			}
		}

		public MoonSharp.Interpreter.DynValue SeekFunction(string scriptName, string functionName, params object[] args)
		{
			if (scriptsdict.ContainsKey(scriptName))
			{
				MoonSharp.Interpreter.Script scri = scriptsdict[scriptName];
				var func = scri.Globals.Get(functionName);
				if (func.Type != MoonSharp.Interpreter.DataType.Function)
				{
					GD.Print($"Error: {functionName} is not a function or has no __call method. Type found: {func.Type}");
					return null;
				}
				if (args.Length != 0)
				{
					return scri.Call(func, args);
				}
				else
				{
					return scri.Call(func);
				}
			}
			else
			{
				return null;
			}
		}


		public bool Push(string what, MoonSharp.Interpreter.DynValue val, bool overwrite = true)
		{
			if (dict.ContainsKey(what))
			{
				if (overwrite)
				{
					dict[what] = val;
					return true;
				}
				return false;
			}

			dict.Add(what, val);
			return true;
		}

		public bool Pop(string what)
		{
			if (dict.ContainsKey(what))
			{
				dict.Remove(what);
				return true;
			}
			return false;
		}

		public void ClearGlobalStateMemory()
		{
			dict.Clear();
		}
	}

}
