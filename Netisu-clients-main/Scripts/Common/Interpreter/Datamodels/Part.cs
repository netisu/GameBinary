using Godot;

namespace Netisu.Datamodels
{
	public partial class Part : ActiveInstance
	{

		[MoonSharp.Interpreter.MoonSharpHidden, Export]
		public CollisionShape3D collisionShape3D;

		private string CurrentShape = "Cube";
		public PreservedGlobalClasses.Col3 CachedColor = new(1, 1, 1, 1);

		private float cachedTransparency = 1.0f;

		public string Shape
		{
			get => CurrentShape;
			set => SetMeshBasedOnShape(value);
		}

		[MoonSharp.Interpreter.MoonSharpHidden]
		public int BatchRenderId = -1;

		public bool Collisions
		{
			get => Container.GetNodeOrNull<CollisionShape3D>("CollisionShape3D").Disabled;

			set
			{
				CollisionShape3D cs3d = Container.GetNodeOrNull<CollisionShape3D>("CollisionShape3D");
				if (cs3d != null)
				{
					cs3d.Disabled = value;
				}
			}
		}

		public float Transparency
		{
			get => cachedTransparency;
			set
			{
				cachedTransparency = value;
				BatchRenderPart.Instance.UpdateColor(BatchRenderId, new(CachedColor.r, CachedColor.g, CachedColor.b, value), Shape);
			}
		}

		public override void Destroy()
		{
			base.Destroy();
			BatchRenderPart.Instance.RemovePart(BatchRenderId, Shape);
		}

		public PreservedGlobalClasses.Col3 Color
		{
			get => CachedColor;
			set
			{
				CachedColor = value;
				BatchRenderPart.Instance.UpdateColor(BatchRenderId, new(value.r, value.g, value.b, cachedTransparency), Shape);
			}
		}

		[MoonSharp.Interpreter.Interop.MoonSharpVisible(false)]
		public override void _Ready()
		{
			BatchRenderId = BatchRenderPart.Instance.BuildPart(this);
		}

		private void SetMeshBasedOnShape(string shape)
		{
			return;
			
		}
	}

}
