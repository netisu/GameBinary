using Godot;
using System.Collections.Generic;

namespace Netisu
{
	public class ArgsExtracter
	{
		public static Dictionary<string, string> Extract()
		{
			Dictionary<string, string> Args = [];
			foreach (string argument in OS.GetCmdlineArgs())
			{
				if (argument.Contains('='))
				{
					string[] KeyVal = argument.Split('=');
					Args[KeyVal[0].TrimPrefix("--")] = KeyVal[1];
				}
				else
				{
					Args[argument.TrimPrefix("--")] = string.Empty;
				}
			}

			return Args;
		}
	}
}
