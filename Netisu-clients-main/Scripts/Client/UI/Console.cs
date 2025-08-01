using Godot;

namespace Netisu.Client.UI
{
    public partial class Console : RichTextLabel
    {
        public static Console Instance { get; private set; } = null!;

        public override void _Ready()
        {
            Instance = this;
        }

        public void Add(string content)
        {
            Text += $"{content}\n";
        }

        public void ToggleVisibility()
        {
            GetParent().GetParent<Control>().Visible = !GetParent().GetParent<Control>().Visible;
        }
    }
}