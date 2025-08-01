using Godot;
using Netisu.Datamodels;
using System.IO;

namespace Netisu.Game
{
	public partial class Exporter
	{
		public static void ExportGameToFile(string path, Datamodels.Environment env, Node gameRoot)
		{
			try
			{
				// Step 1: Serialize the game data into a JSON string.
				string jsonContent = SerializeTheGame(env, gameRoot);

				// Step 2: Write the JSON string to the selected file.
                using var file = Godot.FileAccess.Open(path, Godot.FileAccess.ModeFlags.Write);
				if (file == null)
				{
                    GD.PrintErr($"Failed to open file for writing: {Godot.FileAccess.GetOpenError()}");
					return;
				}
				file.StoreString(jsonContent);
				GD.Print($"Game successfully exported to: {path}");
			}
			catch (System.Exception e)
			{
				GD.PrintErr($"An error occurred during export: {e.Message}");
			}
		}
		public static string SerializeTheGame(Datamodels.Environment env, Node gameRoot)
		{
			// Serialize Environment data
			Godot.Collections.Dictionary env_data = CluaObjectList.GetObjectInformation(env, true);

			// Prepare the main data dictionary
			Godot.Collections.Dictionary game_data = new()
			{
				{ "Map", new Godot.Collections.Array() },
				{ "LocalScripts", new Godot.Collections.Array() },
				{ "Scripts", new Godot.Collections.Array() },
				{ "ServerStorage", new Godot.Collections.Array() },
				{ "SharedStorage", new Godot.Collections.Array() },
				{ "Environment", env_data },
			};

			// Serialize all children of the main folders
			SerializeChildrenOf(gameRoot.GetNode("Map"), game_data["Map"].As<Godot.Collections.Array>());
			SerializeChildrenOf(gameRoot.GetNode("LocalScripts"), game_data["LocalScripts"].As<Godot.Collections.Array>());
			SerializeChildrenOf(gameRoot.GetNode("Scripts"), game_data["Scripts"].As<Godot.Collections.Array>());
			SerializeChildrenOf(gameRoot.GetNode("ServerStorage"), game_data["ServerStorage"].As<Godot.Collections.Array>());
			SerializeChildrenOf(gameRoot.GetNode("SharedStorage"), game_data["SharedStorage"].As<Godot.Collections.Array>());

			// Convert the dictionary to a nicely formatted JSON string
			return Json.Stringify(game_data, "\t");
		}

		private static void SerializeChildrenOf(Node parent, Godot.Collections.Array targetArray)
		{
			foreach (Node child in parent.GetChildren())
			{
				if (child is Instance instance)
				{
					var objectData = CluaObjectList.GetObjectInformation(instance, true);
					if (objectData != null)
					{
						targetArray.Add(objectData);
					}
				}
			}
		}
	}
}

