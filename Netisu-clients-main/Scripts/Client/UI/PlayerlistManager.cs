using Godot;

namespace Netisu.Client.UI
{
    public partial class PlayerlistManager : Control
    {
        public const string PlayerlistScene = "res://Assets/Scenes/PlayerPlate.tscn";

        [Export]
        public VBoxContainer playerVerticalBoxContainer = null!;

        public static PlayerlistManager Instance { get; private set; } = null!;

        public override void _Ready()
        {
            Instance = this;
        }

        public Control AddPlayer(string username)
        {
            Control PlayerPlate = GD.Load<PackedScene>(PlayerlistScene).Instantiate<Control>();
            PlayerPlate.GetNode<Label>("Panel/Label").Text = username;
            playerVerticalBoxContainer.AddChild(PlayerPlate);

            return PlayerPlate;
        }
    }
}