using Godot;

namespace Netisu.Datamodels
{
    public partial class UiLabel : UiInstance
    {
        [Export]
        Label baseControl;

        [MoonSharp.Interpreter.MoonSharpHidden]
        public override void _Ready()
        {
            base.BaseControl = baseControl;
        }

        public string Text
        {
            get => baseControl.Text;
            set => baseControl.Text = value;
        }
    }
}
