using Godot;

namespace Netisu.Datamodels
{
    public partial class UiButton : UiInstance
    {
        [Export]
        Button baseControl;

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

        public PreservedGlobalClasses.Col3 TextColor
        {
            get => new(baseControl.GetThemeColor("font_color").R, baseControl.GetThemeColor("font_color").G, baseControl.GetThemeColor("font_color").B, baseControl.GetThemeColor("font_color").A);
            set => baseControl.Set("theme_override_colors/font_color", new Color(value.r, value.g, value.b, value.a));
        }

        public PreservedGlobalClasses.Col3 BackgroundColor
        {
            get => new((baseControl.GetThemeStylebox("normal") as StyleBoxFlat).BgColor.R, (baseControl.GetThemeStylebox("normal") as StyleBoxFlat).BgColor.G, (baseControl.GetThemeStylebox("normal") as StyleBoxFlat).BgColor.B, (baseControl.GetThemeStylebox("normal") as StyleBoxFlat).BgColor.A);
            set => (baseControl.GetThemeStylebox("normal") as StyleBoxFlat).BgColor = new(value.r, value.g, value.b, value.a);
        }
    }
}
