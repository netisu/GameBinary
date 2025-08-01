using Netisu;
using Godot;
using System.Linq;

namespace Netisu.Workshop.UI
{
	public partial class GizmoUiHandler : Control
	{
		public override void _Ready()
		{
			for (int i = 0; i < GetChildCount(); i++)
			{
				int buttonIndex = i; 
				Button button = GetChild<Button>(i);
				
				button.Pressed += () => EngineUI.GizmoModeUpdate(buttonIndex);
			}
		}
	}
}
