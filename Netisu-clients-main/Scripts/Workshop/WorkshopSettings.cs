using Godot;
using System;
using System.Collections.Generic;
using Netisu.Settings;

public partial class WorkshopSettings : Node
{
	[Export]
	PackedScene RenderOptionScene;

	[Export]
	VBoxContainer OptionsContainer;

	public override void _Ready()  => SwitchTo("Graphics");

	public void ClearContent()
	{
		foreach (Node option in OptionsContainer.GetChildren())
		{
			OptionsContainer.RemoveChild(option);
			option.QueueFree();
		}
	}

	public void SwitchTo(string tabName)
	{
		// everytime this is called we should have the contents cleared
		ClearContent();

		// would rather have a switch then ifs
		int _p = 0;
		switch (tabName)
		{
			case "Graphics":
				foreach(KeyValuePair<string, string> couple in WorkshopSettingsList._RENDER_SETTINGS_CONTENT)
				{
					_p = HandleSettings(couple, _p,  WorkshopSettingsList._RENDER_SETTINGS_DEFAULT[_p]);
				}
				break;

			case "Playtest":
				foreach(KeyValuePair<string, string> couple in WorkshopSettingsList._PLAYTEST_SETTINGS_CONTENT)
				{
					_p = HandleSettings(couple, _p,  WorkshopSettingsList._PLAYTEST_SETTINGS_DEFAULT[_p]);
				}
				break;

			case "Beta":
				foreach(KeyValuePair<string, string> couple in WorkshopSettingsList._BETA_SETTINGS_CONTENT)
				{
					_p = HandleSettings(couple, _p,  WorkshopSettingsList._BETA_SETTINGS_DEFAULT[_p]);
				}
				break;

			case "Internal":
				foreach(KeyValuePair<string, string> couple in WorkshopSettingsList._INTERNAL_SETTINGS_CONTENT)
				{
					_p = HandleSettings(couple, _p,  WorkshopSettingsList._INTERNAL_SETTINGS_DEFAULT[_p]);
				}
				break;

			default:
				GD.Print($"Attempted to switch to a unknown tab: {tabName}");
				break;
		}
	}

	public int HandleSettings(KeyValuePair<string, string> couple, int _p = 0, bool _default = false)
	{
		Node Option = RenderOptionScene.Instantiate();
		OptionsContainer.AddChild(Option);
		Option.GetNode<Label>("Option/Title").Text = couple.Key;
		Option.GetNode<RichTextLabel>("Option/Description").Text = couple.Value;
		Option.GetNode<CheckBox>("Option/Title/CheckBox").ButtonPressed = _default;
		_p++;
		return _p;
	}

	public static void HandleCheckBoxSettings() 
	{

	}

}