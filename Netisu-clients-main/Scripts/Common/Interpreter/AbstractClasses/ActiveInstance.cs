using Godot;
using Netisu.Client;
using System;
using MoonSharp.Interpreter;

namespace Netisu.Datamodels
{
	public partial class ActiveInstance : Instance
	{

		[MoonSharpHidden]
		public bool Locked = false;

		[Export, MoonSharpHidden]
		public RigidBody3D Container = null!;

		// these properties should be automatically replicated-across the authenticated peers.
		public virtual PreservedGlobalClasses.Vec3 Position
		{
			get => Container != null ? new(Container.Position.X, Container.Position.Y, Container.Position.Z) : new(0,0,0);
			set
			{
				if (Container != null)
				{
					Container.Position = new Vector3(value.x, value.y, value.z);
				}
			}
		}
		public virtual PreservedGlobalClasses.Vec3 Scale
		{
			get => Container != null ? new(Container.Scale.X, Container.Scale.Y, Container.Scale.Z) : new(1,1,1);
			set
			{
				if (Container != null)
				{
					Container.Scale = new Vector3(value.x, value.y, value.z);
				}
			}
		}
		public virtual PreservedGlobalClasses.Vec3 Rotation
		{
			get => Container != null ? new(Container.RotationDegrees.X, Container.RotationDegrees.Y, Container.RotationDegrees.Z) : new(0,0,0);
			set
			{
				if (Container != null)
				{
					Container.RotationDegrees = new Vector3(value.x, value.y, value.z);
				}
			}
		}

		public virtual bool Anchored
		{
			get => Container != null && Container.Freeze;
			set
			{
				if (Client.Client.Instance != null)
					return;
				
				if (Container != null)
				{
					Container.Freeze = value;
				}
			}
		}

		[MoonSharp.Interpreter.Interop.MoonSharpVisible(false)]
		public override void _EnterTree()
		{
			if (Client.Server.Instance != null)
			{
				SetMultiplayerAuthority(1);
			}
		}
	}

}
