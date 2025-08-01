using Godot;

namespace Netisu.Datamodels
{
    public partial class UIHorizontalLayout : UiInstance
    {
        [Export]
        private HBoxContainer baseControl;

        public int Separation
        {
            get => baseControl.GetThemeConstant("separation");
            set => baseControl.AddThemeConstantOverride("separation", value);
        }

        [MoonSharp.Interpreter.MoonSharpHidden]
        public override void _Ready()
        {
            base.BaseControl = baseControl;
            base.ChildEnteredTree += OnChildEntered;
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
            else
            {
                (child as Instance).Destroy();
            }
        }
    }

}
