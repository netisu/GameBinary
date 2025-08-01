using Godot;
using System;

namespace Netisu.Workshop.MenuBarControllers
{
	public partial class TabMenu : Button
	{
		[Export]
		Label TabTitle;

		[Export]
		PopupMenu popupMenu;

		[Export]
		PackedScene CodeEditorPopUpTool;

		CodeEdit NativeCodeEditor;
		public override void _Ready()
		{
			NativeCodeEditor = GetNode<CodeEdit>("/root/Root/EngineGUI/CodeEditor/CodeEdit");

			if (TabTitle.Text == "Game")
			{
				popupMenu.QueueFree();
				popupMenu = null!;
			}
		}

		public override void _GuiInput(InputEvent _event)
		{
			if (_event is InputEventMouseButton IeMB)
			{
				if ((int)IeMB.ButtonIndex == 2)
				{
					if (popupMenu == null)
					{
						return;
					}
					Vector2I MousePosAsVec2I = DisplayServer.MouseGetPosition();
					popupMenu.Position = MousePosAsVec2I;
					popupMenu.Visible = true;
				}
			}
		}

		public void IndexPressed(long id)
		{
			int intId = (int)id;

			switch (intId)
			{
				case 0:
					// Close tab
					Netisu.Workshop.EngineUI.Instance.CloseTab(TabTitle.Text);
					break;
				case 1:
					// Code editor pop out
					Window code_pop_up_scene = (Window)CodeEditorPopUpTool.Instantiate();
					code_pop_up_scene.Title = $"{TabTitle.Text} - Netisu's Scripting Editor";
					code_pop_up_scene.Visible = true;
					code_pop_up_scene.GetNode<CodeEdit>("CodeEdit").Text = NativeCodeEditor.Text;

					GetNode("/root/Root/Windows/ActiveFloatCodeEditors").AddChild(code_pop_up_scene, true);
					Netisu.Workshop.EngineUI.Instance.CloseTab(TabTitle.Text);
					code_pop_up_scene.CloseRequested += () => OnExternalCodePopUpCloseRequested(code_pop_up_scene);
					break;
				default:
					break;
			}
		}

		private void OnExternalCodePopUpCloseRequested(Window the_code_pop_up)
		{
			string newerScriptContent = the_code_pop_up.GetNode<CodeEdit>("CodeEdit").Text;
			string scriptName = the_code_pop_up.Title.Replace("- Netisu's Scripting Editor", "").Replace(" ", "");

			Datamodels.BaseScript _script = the_code_pop_up.GetNodeOrNull<Datamodels.BaseScript>($"/root/Root/Game/LocalScripts/{scriptName}");

			if (_script != null)
			{
				EngineUI.WriteIntoScript(_script, newerScriptContent);
				the_code_pop_up.GetParent().RemoveChild(the_code_pop_up);
				the_code_pop_up.QueueFree();
			}
			else
			{
				GD.PrintErr($"Script is not in the scene-tree. Attempted path (/root/Root/Game/LocalScripts/{scriptName}).");
			}
		}
	}

}
