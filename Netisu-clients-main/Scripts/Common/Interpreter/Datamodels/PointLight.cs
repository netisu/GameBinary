using Godot;
using System;

namespace Netisu.Datamodels
{
	public partial class PointLight : ActiveInstance
	{
		[MoonSharp.Interpreter.MoonSharpHidden, Export]
		public SpotLight3D spotLight3D;

		[MoonSharp.Interpreter.MoonSharpHidden]
		public override void _Ready()
		{
			
		}

		public float Intensity
        { get => spotLight3D.LightEnergy; set => spotLight3D.LightEnergy = value;
        }

        public float @Range
        { get => spotLight3D.SpotRange; set => spotLight3D.SpotRange = value;
        }

        public bool Shadows
        { get => spotLight3D.ShadowEnabled; set => spotLight3D.ShadowEnabled = value;
        }

        public bool Disabled
		{
			get => spotLight3D.Visible;
			set => spotLight3D.Visible = !value;
		}

		public PreservedGlobalClasses.Col3 LightColor
        {
            get
            {
                Godot.Color lightCol = spotLight3D.LightColor;
                return new PreservedGlobalClasses.Col3(lightCol.R, lightCol.G, lightCol.G);
            }

            set => _ = new Godot.Color();
        }
	}

}
