using Godot;
using System;

namespace Netisu.Datamodels
{
	public partial class Sun : ActiveInstance
	{
		[MoonSharp.Interpreter.MoonSharpHidden, Export]
		public DirectionalLight3D directionalLight3D;

		[MoonSharp.Interpreter.MoonSharpHidden]
		public override void _Ready()
		{
			if (Client.Client.Instance != null)
			{
				GetNode("Sprite3D").QueueFree();
			}
		}

		public float Intensity
        { get => directionalLight3D.LightEnergy; set => directionalLight3D.LightEnergy = value;
        }

        public float MaximumDistance
        { get => directionalLight3D.DirectionalShadowMaxDistance; set => directionalLight3D.DirectionalShadowMaxDistance = value;
        }

        public bool Shadows
        { get => directionalLight3D.ShadowEnabled; set => directionalLight3D.ShadowEnabled = value;
        }

        public bool Disabled
		{
			get => directionalLight3D.Visible;
			set => directionalLight3D.Visible = !value;
		}

		public PreservedGlobalClasses.Col3 LightColor
        {
            get
            {
                Color lightCol = directionalLight3D.LightColor;
                return new PreservedGlobalClasses.Col3(lightCol.R, lightCol.G, lightCol.G);
            }

            set => _ = new Godot.Color();
        }

        public override PreservedGlobalClasses.Vec3 Scale
		{
			get => null;
			set => GD.Print("blbo");
		}

	}

}
