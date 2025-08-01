using Netisu.Game;
using Netisu.Properties;
using Godot;
using Netisu.Datamodels;
using Netisu.Datamodels.Utilities;

namespace Netisu.Workshop
{
	public partial class EngineCamera : Camera3D
	{
		[Export]
		public SubViewport subViewport;

		[Export]
		public ColorPickerButton colorPickerButton;

		public static EngineCamera Instance { get; private set; } = null!;

		public bool inColorMode = false;

		public Godot.Collections.Array<Rid> ExcludedItems = [];

		float SHIFT_MULTIPLIER = 2.5f;
		float ALT_MULTIPLIER = 0.0f;
		float sensitivity = 0.27f;
		float TotalPitch = 0.0f;
		float VelocityMultipl = 4.0f;

		bool _disabled = false;

		Vector2 MousePosition = new(0, 0);
		Vector3 Direction = new(0.0f, 0.0f, 0.0f);
		Vector3 Velocity = new(0.0f, 0.0f, 0.0f);

		int Acceleration = 60;
		int Decceleration = -10;

		bool _W, _A, _S, _D, _Q, _E, _SHIFT, _ALT = false;

		public bool Disabled
		{
			get => _disabled;
			set
			{
				_disabled = value;
			}
		}

		public static void DuplicateSelection()
		{
			Netisu.Game.Map.Importer _Importer = new();
			if (Gizmo3DPlugin.Gizmo3D.Instance.Selection != null)
			{
				Godot.Collections.Dictionary serializedData = CluaObjectList.GetObjectInformation(Gizmo3DPlugin.Gizmo3D.Instance.Selection.Value.Instance);
				if (serializedData != null)
				{
					Node newlyInstanced = _Importer.SwitchCluaPackedObject(serializedData, Gizmo3DPlugin.Gizmo3D.Instance.Selection.Value.Instance.GetParent());
					ActiveInstance activeInstance = newlyInstanced as ActiveInstance;
					Gizmo3DPlugin.Gizmo3D.Instance.Select(activeInstance);
					GameExplorer.Instance.AddObjectToGameExplorer(activeInstance);
					GameExplorer.Instance.SelectedFromEngineCamera(activeInstance);
				}
				return;
			}
		}

		public override void _Ready()
		{
			ALT_MULTIPLIER = 1.0f / SHIFT_MULTIPLIER;
			Instance = this;
		}

		public void EnableColorMode()
		{
			inColorMode = !inColorMode;
			Gizmo3DPlugin.Gizmo3D.Instance.Mode = Gizmo3DPlugin.Gizmo3D.ToolMode.Select; // todo: add for selecton (entirely)!
		}

		public override void _UnhandledInput(InputEvent _event)
		{
			if (Engine3D.Instance.PlayTest) { return; }

			if (Input.IsActionJustPressed("Delete"))
			{
				if (Gizmo3DPlugin.Gizmo3D.Instance.Selection != null)
				{
					if (GameExplorer.Instance.RemoveObjectFromGameExplorer(Gizmo3DPlugin.Gizmo3D.Instance.Selection.Value.Instance, true))
					{
						Gizmo3DPlugin.Gizmo3D.Instance.ClearSelection();
						EngineUI.Instance.ClearOutExistingPropertyTabs();
					}
				}
			}

			if (Input.IsActionJustPressed("Gizmo_local"))
			{
				//Gizmo3DPlugin.Gizmo3D.Instance.GlobalAxis = !Gizmo3DPlugin.Gizmo3D.Instance.GlobalAxis;
			}

			if (Input.IsActionJustPressed("OnF"))
			{
				if (Gizmo3DPlugin.Gizmo3D.Instance.Selection.Value.Instance != null)
				{
					Tween MovementTween = GetTree().CreateTween();
					MovementTween.TweenProperty(this, "global_position", Gizmo3DPlugin.Gizmo3D.Instance.Selection.Value.Instance.Container.GlobalPosition + new Vector3(3f, 3f, 3f), 0.1f).SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Linear);
				}
			}

			if (Input.IsActionJustPressed("Duplicate"))
			{
				DuplicateSelection();
			}

			ProcessForGizmo();
			UpdateMouselook();

			if (_event is InputEventMouseMotion IEMM)
			{
				MousePosition = IEMM.Relative;
			}

			if (_event is InputEventMouseButton IeMB)
			{
				switch ((int)IeMB.ButtonIndex)
				{
					case 2:
						Input.MouseMode = IeMB.Pressed ? Input.MouseModeEnum.Captured : Input.MouseModeEnum.Visible;
						break;
					case 4:
						VelocityMultipl = (float)Mathf.Clamp(VelocityMultipl * 1.1, 0.2, 60);
						break;
					case 5:
						VelocityMultipl = (float)Mathf.Clamp(VelocityMultipl / 1.1, 0.2, 60);
						break;
					default:
						break;
				}
			}

			if (_event is InputEventKey IeK)
			{
				switch ((int)IeK.Keycode)
				{
					case 87:
						_W = IeK.Pressed;
						break;
					case 65:
						_A = IeK.Pressed;
						break;
					case 83:
						_S = IeK.Pressed;
						break;
					case 68:
						_D = IeK.Pressed;
						break;
					case 81:
						_Q = IeK.Pressed;
						break;
					case 69:
						_E = IeK.Pressed;
						break;
					case 4194325:
						_SHIFT = IeK.Pressed;
						break;
					case 4194328:
						_ALT = IeK.Pressed;
						break;
					default:
						break;
				}
			}
		}

		public static Instance AncestoryGot(Instance instance, string ancestoryName)
		{
			while (instance?.GetParent() != null)
			{
				Node parent = instance.GetParent();
				if (parent.GetType().Name == ancestoryName)
				{
					return parent as Instance;
				}

				instance = parent as Instance;
			}
			return null;
		}


		public override void _PhysicsProcess(double delta)
		{
			if (_disabled)
			{
				return;
			}
			if (Engine3D.Instance.PlayTest) { return; }

			UpdateMovement((float)delta);
		}

		private void ProcessForGizmo()
        {
            if (!Input.IsActionJustPressed("click") || Gizmo3DPlugin.Gizmo3D.Instance.Hovering || Gizmo3DPlugin.Gizmo3D.Instance.Editing)
                return;

            var cam = GetViewport().GetCamera3D();
            if (cam == null) return;

            Vector2 mousePosition = GetViewport().GetMousePosition();
            Vector3 origin = cam.ProjectRayOrigin(mousePosition);
            Vector3 rayDirection = cam.ProjectRayNormal(mousePosition) * 1000;

            var spaceState = GetWorld3D().DirectSpaceState;
            var query = PhysicsRayQueryParameters3D.Create(origin, origin + rayDirection);
            query.Exclude = ExcludedItems;
            var result = spaceState.IntersectRay(query);

            if (result.Count == 0)
            {
                // Clicked on empty space, so deselect everything.
                if (Gizmo3DPlugin.Gizmo3D.Instance.Selection != null)
                {
                    GameExplorer.Instance.DeselectedFromEngineCamera(Gizmo3DPlugin.Gizmo3D.Instance.Selection.Value.Instance);
                    Gizmo3DPlugin.Gizmo3D.Instance.ClearSelection();
                    Gizmo3DPlugin.Gizmo3D.Instance.Visible = false;
                    OnUpdateCurrentSelected();
                }
                return;
            }

            var collider = result["collider"].As<GodotObject>();
            ActiveInstance instanceToSelect = null;

            if (collider is PartUtility partUtility)
            {
                // If so, the object we actually want to select is the Part it belongs to.
                instanceToSelect = partUtility.part;
            }
            else if (collider is Node colliderNode)
            {
                // Fallback for other potential object types.
                instanceToSelect = colliderNode.GetParent() as ActiveInstance;
            }

            if (instanceToSelect == null)
            {
                return;
            }

            if (inColorMode)
            {
                if (instanceToSelect is Part part)
                {
                    part.Color = new(colorPickerButton.Color.R, colorPickerButton.Color.G, colorPickerButton.Color.B);
                }
                return;
            }

            Gizmo3DPlugin.Gizmo3D.Instance.ClearSelection();
            Gizmo3DPlugin.Gizmo3D.Instance.Visible = true;

            Instance folderInstance = AncestoryGot(instanceToSelect, "Folder");
            if (folderInstance != null)
            {
                Gizmo3DPlugin.Gizmo3D.Instance.Select(folderInstance as ActiveInstance);
            }
            else
            {
                Gizmo3DPlugin.Gizmo3D.Instance.Select(instanceToSelect);
            }

            OnUpdateCurrentSelected();
        }

		private static Node GodotInstance(string sce) => GD.Load<PackedScene>(sce).Instantiate();

		private void UpdateMouselook()
		{
			if ((int)Input.MouseMode == 2)
			{
				MousePosition *= sensitivity;
				var yaw = MousePosition.X;
				var pitch = MousePosition.Y;
				MousePosition = new Vector2(0, 0);

				pitch = Mathf.Clamp(pitch, -90 - TotalPitch, 90 - TotalPitch);
				TotalPitch += pitch;

				RotateY(Mathf.DegToRad(-yaw));
				RotateObjectLocal(new Vector3(1, 0, 0), Mathf.DegToRad(-pitch));
			}
		}

		private void UpdateMovement(float delta)
		{
			Direction = new Vector3(
				(_D ? 1.0f : 0.0f) - (_A ? 1.0f : 0.0f),
				(_E ? 1.0f : 0.0f) - (_Q ? 1.0f : 0.0f),
				(_S ? 1.0f : 0.0f) - (_W ? 1.0f : 0.0f)
			);

			var offset = Direction.Normalized() * Acceleration * VelocityMultipl * delta + Velocity.Normalized() * Decceleration * VelocityMultipl * delta;

			var speedMulti = 1.0f;
			if (_SHIFT) speedMulti *= SHIFT_MULTIPLIER;
			if (_ALT) speedMulti *= ALT_MULTIPLIER;

			if (Direction == Vector3.Zero && offset.LengthSquared() > Velocity.LengthSquared())
			{
				Velocity = Vector3.Zero;
			}
			else
			{
				Velocity.X = Mathf.Clamp(Velocity.X + offset.X, -VelocityMultipl, VelocityMultipl);
				Velocity.Y = Mathf.Clamp(Velocity.Y + offset.Y, -VelocityMultipl, VelocityMultipl);
				Velocity.Z = Mathf.Clamp(Velocity.Z + offset.Z, -VelocityMultipl, VelocityMultipl);
			}

			Translate(Velocity * delta * speedMulti);
		}

		public static void OnUpdateCurrentSelected()
		{
			if (Gizmo3DPlugin.Gizmo3D.Instance.Selection == null)
			{
				PropertiesManager.Empty();
				return;
			}
			PropertiesManager.Load(Gizmo3DPlugin.Gizmo3D.Instance.Selection.Value.Instance);
			GameExplorer.Instance.SelectedFromEngineCamera(Gizmo3DPlugin.Gizmo3D.Instance.Selection.Value.Instance);
		}

		public static void OnGizmoUpdate(int gizmo_updated_id) => Gizmo3DPlugin.Gizmo3D.Instance.Mode = (Gizmo3DPlugin.Gizmo3D.ToolMode)gizmo_updated_id;
	}

}
