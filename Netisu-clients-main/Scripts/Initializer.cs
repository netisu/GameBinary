using Godot;
using System.Collections.Generic;

namespace Netisu
{
	public partial class Initializer : Node
	{
		public static Dictionary<string, string> ProgramArguments { get; private set; } = [];

		public readonly string[] AppropriateScenes = ["res://Scenes/GameServer.tscn", "res://Scenes/Client.tscn", "res://Scenes/root.tscn"];

		public override void _Ready()
		{
			ProgramArguments = ArgsExtracter.Extract();
			TransferControl();
		}

		public void TransferControl()
		{
			if (OS.GetName() == "iOS" || OS.GetName() == "Android")
			{
				GetTree().CallDeferred("change_scene_to_file", AppropriateScenes[1]);
				return;
			}
			
			if (ProgramArguments.ContainsKey("game-server"))
			{
				GetTree().CallDeferred("change_scene_to_file", AppropriateScenes[0]);
				return;
			}

			if (ProgramArguments.ContainsKey("client"))
			{
				GetTree().CallDeferred("change_scene_to_file", AppropriateScenes[1]);
				return;
			}

			if (ProgramArguments.ContainsKey("workshop"))
			{
				GetTree().CallDeferred("change_scene_to_file", AppropriateScenes[2]);
				return;
			}
			
			// Default To workshop
			GetTree().CallDeferred("change_scene_to_file", AppropriateScenes[2]);
			return;
		}
	}
}
