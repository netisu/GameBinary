using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MoonSharp.Interpreter;

namespace Netisu
{
	public partial class Interpreter : Node
	{
		public static Interpreter Instance { get; private set; } = null!;
		[Export] public Interface Interface;

		public override void _Ready()
		{
			Instance = this;
		}

		public async void Execute(string script, string scriptName)
		{
			await Task.Run(() =>
			{
				GodotThread.SetThreadSafetyChecksEnabled(false);
				try
				{
					MoonSharp.Interpreter.Script s = new(CoreModules.Preset_SoftSandbox);
					s.Globals["__SCRIPT__"] = scriptName;
					s = GlobalManager.Instance?.Init(s);
					s.DoString(script, codeFriendlyName: scriptName);
				}
				catch (ScriptRuntimeException ex)
				{
					GD.Print(ex.DecoratedMessage);
					Interface?.WriteIntoConsole(ex.DecoratedMessage);
				}
				catch (SyntaxErrorException ex)
				{
					GD.Print(ex.DecoratedMessage);
					Interface?.WriteIntoConsole(ex.DecoratedMessage);
				}
				catch (InternalErrorException ex)
				{
					GD.Print(ex.DecoratedMessage);
					Interface?.WriteIntoConsole(ex.DecoratedMessage);
				}
			});
		}
	}

}
