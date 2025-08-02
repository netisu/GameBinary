using Godot;

namespace Netisu.Datamodels
{
	public partial class Environment : Instance
	{
		// These will be assigned in _Ready() instead of the editor.
		private WorldEnvironment _worldEnv;
		private DirectionalLight3D _sun;

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
			if (!Multiplayer.IsServer())
			{
				UpdateVisuals();
			}
		}

		private void UpdateVisuals()
		{
			if (_worldEnv == null || _sun == null) return;

			// Simple sun rotation based on time
			float dayProgress = DayTime / 24.0f;
			_sun.RotationDegrees = new Vector3(-90 + (dayProgress * 180), -30, 0);

			if (_worldEnv.Environment?.Sky?.SkyMaterial is ShaderMaterial skyShader)
			{
				skyShader.SetShaderParameter("sun_dir_world", -_sun.GlobalTransform.Basis.Z.Normalized());
			}
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
