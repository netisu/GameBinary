using Godot;
using System;

namespace Netisu.Datamodels
{
	public partial class DataStoreService : Instance
	{
		[Export] private Node JSON_READER;
		private struct ConnectionValue
		{
			public string FunctionName;
			public string ScriptName;
		}


		public object GetDataStore(int playerId)
		{
			return null;
		}

		public void CreateDataStore(int playerId)
		{

		}

		private void WriteNewDatabase(string gameName)
		{
			var file = FileAccess.Open("user://cached_databases/" + gameName + ".cdb", FileAccess.ModeFlags.Write);
			file.StoreString("{}");
		}

		private bool AnyExistingDatabase(string gameName)
		{
			return FileAccess.FileExists("user://cached_databases/" + gameName + ".cdb");
		}
	}

}
