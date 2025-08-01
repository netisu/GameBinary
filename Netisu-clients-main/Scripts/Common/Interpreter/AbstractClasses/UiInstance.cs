using Godot;

namespace Netisu.Datamodels
{
    public partial class UiInstance : Instance
    {
        [MoonSharp.Interpreter.MoonSharpHidden]
        public virtual Control BaseControl { get; set; } = null!;

        public virtual PreservedGlobalClasses.Vec2 Position
        {
            get => new(BaseControl.Position.X, BaseControl.Position.Y);
            set => BaseControl.Position = new(value.x, value.y);
        }

        public virtual PreservedGlobalClasses.Vec2 Size
        {
            get => new(BaseControl.Size.X, BaseControl.Size.Y);
            set => BaseControl.Size = new(value.x, value.y);
        }

        public virtual float Rotation
        {
            get => BaseControl.RotationDegrees;
            set => BaseControl.RotationDegrees = value;
        }

        public bool Visible
        {
            get => BaseControl.Visible;
            set => BaseControl.Visible = value;
        }
    }

}
