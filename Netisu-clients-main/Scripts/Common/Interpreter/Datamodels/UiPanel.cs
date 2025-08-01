using Godot;

namespace Netisu.Datamodels
{
    public partial class UiPanel : UiInstance
    {
        [Export]
        Panel baseControl;

        [MoonSharp.Interpreter.MoonSharpHidden]
        public override void _Ready()
        {
            base.BaseControl = baseControl;
            ChildEnteredTree += OnChildEntered;
            CallDeferred("UpdateChildStatus");
        }

        private void UpdateChildStatus()
        {
            foreach (Node node in GetChildren())
            {
                if (node is UiInstance)
                {
                    OnChildEntered(node);
                }
            }
        }

        private void OnChildEntered(Node child)
        {
            if (child is UiInstance uiInstance)
            {
                uiInstance.RemoveChild(uiInstance.BaseControl);
                baseControl.CallDeferred("add_child", uiInstance.BaseControl);
            }
        }

        public PreservedGlobalClasses.Col3 BackgroundColor
        {
            get => new((baseControl.GetThemeStylebox("panel") as StyleBoxFlat).BgColor.R, (baseControl.GetThemeStylebox("panel") as StyleBoxFlat).BgColor.G, (baseControl.GetThemeStylebox("panel") as StyleBoxFlat).BgColor.B, (baseControl.GetThemeStylebox("panel") as StyleBoxFlat).BgColor.A);
            set => (baseControl.GetThemeStylebox("panel") as StyleBoxFlat).BgColor = new(value.r, value.g, value.b, value.a);
        }

        public int BorderWidth
        {
            get => 1;
        }

        public int BorderCornerRadius
        {
            get => (baseControl.GetThemeStylebox("panel") as StyleBoxFlat).CornerRadiusBottomLeft;
            set => (baseControl.GetThemeStylebox("panel") as StyleBoxFlat).SetCornerRadiusAll(value);
        }
    }
}
