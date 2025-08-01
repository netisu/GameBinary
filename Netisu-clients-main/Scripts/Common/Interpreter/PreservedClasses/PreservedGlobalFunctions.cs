using Godot;
using Netisu.Client.UI;
using System.Threading.Tasks;

namespace Netisu
{
	public partial class PreservedGlobalFunctions : Node
	{

		public static void Printl(string content)
		{
			GD.Print($"DUMP -> {content}");
			Console.Instance?.Add(content);
		}

		public static void Wait(int time)
		{
			Task.Delay(time * 1000).Wait();
		}
	}

}