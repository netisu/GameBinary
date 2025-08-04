using Godot;

namespace Netisu.Datamodels
{
	public partial class Environment : Instance
	{
		// These will be assigned in _Ready() instead of the editor.
		[Export]
		private WorldEnvironment _worldEnv;
		[Export]
		private DirectionalLight3D _sun;
		[Export]
		private DirectionalLight3D _moon = null!;

		// This is the "source of truth" for the time of day.
		// A MultiplayerSynchronizer should be used to sync this from server to clients.
		[Export(PropertyHint.Range, "0.0,24.0,0.0001")]
		public float DayTime { get; set; } = 12.0f;

		public override void _Ready()
		{
			// Get references to the necessary nodes.
			_worldEnv = GetParent().GetNode<WorldEnvironment>("WorldEnvironment");
			_sun = GetParent().GetNode<DirectionalLight3D>("DirectionalLight3D");
		}

		public override void _Process(double delta)
		{
			// Add this check to ensure we only run networking-related code
			// when a multiplayer session is actually active.
			if (Multiplayer.MultiplayerPeer == null)
			{
				return;
			}
			if (!Multiplayer.IsServer())
			{
				UpdateVisuals();
			}
		}

		private void UpdateVisuals()
		{
			if (_worldEnv?.Environment?.Sky?.SkyMaterial is not ShaderMaterial skyShader) return;
			if (!IsInstanceValid(_sun) || !IsInstanceValid(_moon)) return;

			float dayProgress = DayTime / 24.0f;
			_sun.RotationDegrees = new Vector3(-90 + (dayProgress * 180), 0, 0);

			Vector3 sunDirWorld = -_sun.GlobalTransform.Basis.Z.Normalized();
			skyShader.SetShaderParameter("sun_dir_world", sunDirWorld);
		}

		public float Brightness
		{
			get => _worldEnv?.Environment?.BackgroundEnergyMultiplier ?? 1.0f;
			set
			{
				if (_worldEnv?.Environment != null)
				{
					_worldEnv.Environment.BackgroundEnergyMultiplier = value;
				}
			}
		}

	}
}
