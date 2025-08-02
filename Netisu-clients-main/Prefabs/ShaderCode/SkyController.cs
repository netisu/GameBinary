using Godot;

[Tool]
public partial class SkyController : Node3D
{
	[Export]
	private Netisu.Datamodels.Environment _environmentData;

	public override void _Process(double delta)
	{
        // This script only runs in the editor to provide a live preview.
		if (Engine.IsEditorHint() && _environmentData != null)
		{
            // When you move a slider in the workshop that changes the DayTime on the
            // Environment node, this _Process function will read that new value
            // and call the visual update function to show the changes in real-time.
			_environmentData.Call("_Process", delta);
		}
	}
}
