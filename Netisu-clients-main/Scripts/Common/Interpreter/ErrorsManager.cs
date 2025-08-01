using Godot;
using System;

public partial class ErrorsManager : Node
{
	bool debugging = false;
	RichTextLabel OutputLabel;
	
	public override void _Ready() {
		OutputLabel = GetTree().Root.GetNode<RichTextLabel>("Root/EngineGUI/Leftbar/Output/Panel/Panel/RichTextLabel");
	}
	
	public void Push(string message, string errorType, string scriptName) {
		if (debugging) {
				GD.Print(scriptName + ".clua | MESG: " + message + " | ErrType: " + errorType);
			}
		switch (errorType) {
			case "Error":
				OutputLabel.Text += "\n" + scriptName + ".clua | Error: " + message;
				break;
			case "Warning":
				OutputLabel.Text += "\n" + scriptName + ".clua | Warning: " + message;
				break;
			case "Engine":
				OutputLabel.Text += "\n" + scriptName + ".clua | Engine: " + message;
				break;
			case "Info":
				OutputLabel.Text += "\n" + scriptName + ".clua | Info: " + message;
				break;
			case "ScriptEngineFailure":
				OutputLabel.Text += "\n" + scriptName + ".clua | Clua's core scripting engine faced a failure: report immediately.";
				break;
			default:
				break;
		}
	}
	
	public void Clear(bool byplaytest = false) {
		OutputLabel.Text = "";
		if (byplaytest) {
			OutputLabel.Text = "[center][color=#a3a3a3]-- Debugging Stopped --[/color][/center]";
		}
	}
}
