using Godot;
using Netisu.Client;
using Netisu.Client.UI;

namespace Netisu.Datamodels
{
	public partial class Player : CharacterBody3D
	{
		[MoonSharp.Interpreter.MoonSharpHidden]
		public bool IsServer = false;

		private int _rotateFinger = -1;

		public long RpcIdByServer = 0;

		private float PrivateJumpPower = 45.0f;
		private float PrivateMouseSensitivity = 0.003f;
		private float PrivateWalkSpeed = 11.0f;
		private float PrivateMiniumZoom = 5.0f;
		private float PrivateMaximumZoom = 30.0f;
		private float PrivateGravity = 196.0f;
		private float PrivateRotationSpeed = 12.0f;
		private float PrivateJumpBufferTime = 0.1f;
		private float PrivateCoyoteTime = 0.1f;

		// State variables
		private bool PrivateJumping = false;
		private bool PrivateWasOnFloor = false;
		private bool PrivateSitting = false;
		private bool PrivateAdmin = false;
		private bool PrivateDeveloper = false;
		private bool PrivateTouchDrag = false;
		private string PrivateUsername = string.Empty;
		private Seat SeatedSeat = null!;

		private float jumpBufferTimer = 0.0f;
		private float coyoteTimer = 0.0f;
		private float landingTimer = 0.0f;
		private Vector3 lastGroundVelocity = Vector3.Zero;

		[Export] private int PrivateHealth = 100;
		[Export] public int userID = 0;

		[Export] private SpringArm3D PrivateSpringArm = null!;
		[Export] private Node3D PrivateModel = null!;
		[Export] private AnimationPlayer PrivateAnimationPlayer = null!;
		[Export] private CollisionShape3D PrivateCollisionShape = null!;
		[Export] private AudioStreamPlayer PrivateAudioStreamPlayer = null!;
		[Export] private Label3D PrivateLabel3D = null!;


		[MoonSharp.Interpreter.MoonSharpHidden]
		[Export]
		public string NetAnimation
		{
			get => PrivateAnimationPlayer?.CurrentAnimation;
			set
			{
				if (PrivateAnimationPlayer?.CurrentAnimation != value)
				{
					PrivateAnimationPlayer?.Play(value);
				}
			}
		}


		// Properties
		public int Health
		{
			get => PrivateHealth;
			set => CallDeferred("HandleHealthChange", value);
		}

		public float JumpPower
		{
			get => PrivateJumpPower;
			set => PrivateJumpPower = value;
		}

		public string username
		{
			get => PrivateUsername!;
			set
			{
				if (PrivateLabel3D != null)
				{
					PrivateLabel3D.Text = value;
				}
				PrivateUsername = value;
			}
		}

		public float WalkSpeed
		{
			get => PrivateWalkSpeed;
			set => PrivateWalkSpeed = value;
		}

		public bool Sitting
		{
			get => PrivateSitting;
			set => PrivateSitting = value;
		}

		public bool Admin
		{
			get => PrivateAdmin;
			set => PrivateAdmin = value;
		}

		public bool Developer
		{
			get => PrivateDeveloper;
			set => PrivateDeveloper = value;
		}

		public void Kill()
		{
			if (RpcIdByServer == Multiplayer.GetUniqueId())
			{
				Health = 0;
			}
		}

		[MoonSharp.Interpreter.MoonSharpHidden]
		public override void _Ready()
		{
			// Check if we are running on the server. If so, we do NOT
			// try to access any visual nodes.
			if (Multiplayer.IsServer())
			{
				// The server version of the player has no visual components.
				// We can add any server-specific initialization here if needed.
				GD.Print($"Server-side player node {Name} is ready.");
				return;
			}
			
			// The following code will ONLY run on the client, which is safe.
			SetMultiplayerAuthority((int)RpcIdByServer);

			PrivateModel ??= GetNode<Node3D>("Avatar");
			PrivateSpringArm = GetNode<SpringArm3D>("SpringArm3D");
			PrivateAnimationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
			PrivateLabel3D = GetNode<Label3D>("Label3D");

			GD.Print($"[SpringArm Check] LocalID: {Multiplayer.GetUniqueId()} | RpcIdByServer: {RpcIdByServer}");

			if (!IsMultiplayerAuthority())
			{
				// This is a remote player on our client. We don't need their camera.
				PrivateSpringArm.QueueFree();
				return;
			}
		}

		[MoonSharp.Interpreter.MoonSharpHidden]
		public void HandleHealthChange(int healthh)
		{
			if (IsMultiplayerAuthority() && healthh < 101 && healthh > -1)
			{
				PrivateHealth = healthh;
			}
		}

		public void Respawn(bool evenWhenNotDead = false)
		{
			if (PrivateHealth == 0 || evenWhenNotDead == true)
			{
				Vector3 SpawnLocation = (Vector3)GetNode<Node>("/root/Root/Mixins/MapLoader").Get("spawnPointLocation");

				Tween MovementTween = GetTree().CreateTween();
				MovementTween.SetParallel(true);
				MovementTween.TweenProperty(this, "position", SpawnLocation, 0.3f)
					.SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Back);
				MovementTween.TweenProperty(this, "rotation", Vector3.Zero, 0.3f)
					.SetEase(Tween.EaseType.Out);

				PrivateHealth = 100;
				Velocity = Vector3.Zero;
			}
		}

		public void Sit(Seat seat = null!)
		{
			if (seat == null)
			{
				Sitting = true;
				NetAnimation = "sit";
				return;
			}

			// Smooth sitting animation
			Tween sitTween = GetTree().CreateTween();
			sitTween.SetParallel(true);
			sitTween.TweenProperty(this, "position", seat.Position, 0.2f)
				.SetEase(Tween.EaseType.Out);
			sitTween.TweenProperty(PrivateModel, "rotation:y", seat.Rotation.Y, 0.2f)
				.SetEase(Tween.EaseType.Out);

			SeatedSeat = seat;
			Sitting = true;
			seat.Occupant = this;
			NetAnimation = "sit";
		}

		[MoonSharp.Interpreter.MoonSharpHidden]
		public override void _PhysicsProcess(double delta)
		{
			if (Multiplayer.IsServer())
			{
				// The server can handle physics simulation here if needed for an authoritative model.
				// For now, we assume a client-authoritative model.
				return;
			}

			if (IsMultiplayerAuthority())
			{
				HandleTimers(delta);
				HandleGravityAndFloorDetection(delta);

				if (Sitting)
				{
					HandleSittingInput();
					return;
				}

				if (PrivateHealth == 0)
				{
					MoveAndSlide();
					return;
				}

				if (!ChatManager.Instance.Busy)
				{
					HandleMovementInput(delta);
					HandleJumpInput();
					HandleAnimations(delta);
				}

				MoveAndSlide();
			}
		}

		private void HandleTimers(double delta)
		{
			if (jumpBufferTimer > 0) jumpBufferTimer -= (float)delta;
			if (coyoteTimer > 0) coyoteTimer -= (float)delta;
			if (landingTimer > 0) landingTimer -= (float)delta;
		}

		private void HandleGravityAndFloorDetection(double delta)
		{
			bool wasOnFloor = PrivateWasOnFloor;
			PrivateWasOnFloor = IsOnFloor();

			if (!IsOnFloor())
			{
				Velocity = new Vector3(Velocity.X, Velocity.Y - PrivateGravity * (float)delta, Velocity.Z);
			}
			else
			{
				if (!wasOnFloor)
				{
					OnLanded();
				}
				coyoteTimer = PrivateCoyoteTime;
			}

			if (wasOnFloor && !IsOnFloor())
			{
				coyoteTimer = PrivateCoyoteTime;
			}
		}

		private void OnLanded()
		{
			PrivateJumping = false;
			landingTimer = 0.15f;
		}

		private void HandleMovementInput(double delta)
		{

			Vector2 inputDir = Input.GetVector("left", "right", "forward", "backward");
			bool hasInput = inputDir.LengthSquared() > 0.01f;

			Vector3 worldDir = Vector3.Zero;
			if (hasInput)
				worldDir = new Vector3(inputDir.X, 0, inputDir.Y)
							.Rotated(Vector3.Up, PrivateSpringArm.Rotation.Y)
							.Normalized();

			Vector3 horizontalVel = hasInput
				? worldDir * PrivateWalkSpeed
				: Vector3.Zero;

			if (hasInput)
			{
				float targetY = Mathf.Atan2(worldDir.X, worldDir.Z);
				float rotSpeed = PrivateRotationSpeed;
				float newY = Mathf.LerpAngle(
					PrivateModel.Rotation.Y,
					targetY,
					rotSpeed * (float)delta
				);

				PrivateModel.Rotation = new Vector3(
					PrivateModel.Rotation.X,
					newY,
					PrivateModel.Rotation.Z
				);
			}

			Velocity = new Vector3(
				horizontalVel.X,
				Velocity.Y,
				horizontalVel.Z
			);
		}

		private void HandleCharacterRotation(Vector3 direction, double delta)
		{
			if (direction.LengthSquared() > 0.01f)
			{
				float targetRotationY = Mathf.Atan2(direction.X, direction.Z);

				if (!IsOnFloor()) PrivateRotationSpeed *= 0.6f;

				PrivateModel.Rotation = new Vector3(
					PrivateModel.Rotation.X,
					Mathf.LerpAngle(PrivateModel.Rotation.Y, targetRotationY, PrivateRotationSpeed * (float)delta),
					PrivateModel.Rotation.Z
				);
			}
		}

		private void HandleJumpInput()
		{
			if (Input.IsActionJustPressed("jump"))
			{
				jumpBufferTimer = PrivateJumpBufferTime;
			}

			bool canJump = (IsOnFloor() || coyoteTimer > 0) && landingTimer <= 0;

			if (jumpBufferTimer > 0 && canJump)
			{
				Jump();
				jumpBufferTimer = 0;
				coyoteTimer = 0;
			}
		}

		private void Jump()
		{
			Velocity = new Vector3(Velocity.X, PrivateJumpPower, Velocity.Z);
			PrivateJumping = true;
			NetAnimation = "Jump";
			PrivateAudioStreamPlayer?.Play();
		}

		private void HandleAnimations(double delta)
		{
			Vector2 inputDirection = Input.GetVector("left", "right", "forward", "backward");
			bool isMoving = inputDirection.LengthSquared() > 0.01f;

			if (NetAnimation == "Jump" ||
				NetAnimation == "Land" ||
				NetAnimation == "HardLand")
			{
				return;
			}

			if (IsOnFloor() && !PrivateJumping)
			{
				if (isMoving)
				{
					if (NetAnimation != "Walk")
					{
						NetAnimation = "Walk";
					}
				}
				else
				{
					if (NetAnimation != "Idle")
					{
						NetAnimation = "Idle";
					}
				}
			}
			else if (!IsOnFloor() && PrivateJumping)
			{
				// Handle falling animation
			}
		}

		private void HandleSittingInput()
		{
			if (Input.IsActionJustPressed("jump"))
			{
				StandUp();
			}
		}

		private void StandUp()
		{
			Sitting = false;
			if (SeatedSeat != null)
			{
				SeatedSeat.Occupant = null;
				SeatedSeat = null;
			}

			Velocity = new Vector3(Velocity.X, PrivateJumpPower * 0.3f, Velocity.Z);
			NetAnimation = "StandUp";
		}

		[MoonSharp.Interpreter.MoonSharpHidden]
		public override void _UnhandledInput(InputEvent @event)
		{
			if (Multiplayer.IsServer()) return;

			if (IsMultiplayerAuthority())
			{
				if (ChatManager.Instance.Busy)
					return;

				HandleMouseInput(@event);
			}
		}

		private void HandleMouseInput(InputEvent @event)
		{
			if (@event is InputEventMouseButton eventMouseButton)
			{
				HandleMouseButton(eventMouseButton);
			}
			else if (@event is InputEventMouseMotion eventMouseMotion)
			{
				HandleMouseMotion(eventMouseMotion);
			}

			if (OS.GetName() != "iOS" && OS.GetName() != "Android")
				return;

			if (@event is InputEventScreenTouch eventScreenTouch)
			{
				HandleTouch(eventScreenTouch);
			}
			else if (@event is InputEventScreenDrag eventScreenDrag)
			{
				HandleScreenDrag(eventScreenDrag);
			}
		}

		private void HandleTouch(InputEventScreenTouch st)
		{
			if (st.Pressed)
			{
				if (st.Index != GetNode("/root/Root/Game/UserInterface/MobileControl/MovementJoystick").Call("GetActiveFinger").As<int>() && _rotateFinger == -1)
					_rotateFinger = st.Index;
			}
			else if (st.Index == _rotateFinger)
			{
				_rotateFinger = -1;
			}
		}

		private void HandleScreenDrag(InputEventScreenDrag sd)
		{
			if (!PrivateTouchDrag || sd.Index != 0)
				return;

			ApplyRotation(sd.Relative);
		}


		private void HandleMouseButton(InputEventMouseButton eventMouseButton)
		{
			switch (eventMouseButton.ButtonIndex)
			{
				case MouseButton.Right:
					Input.MouseMode = eventMouseButton.Pressed ?
						Input.MouseModeEnum.Captured : Input.MouseModeEnum.Visible;
					break;

				case MouseButton.WheelUp:
					ZoomCamera(-1.2f);
					break;

				case MouseButton.WheelDown:
					ZoomCamera(1.2f);
					break;
			}
		}

		private void ZoomCamera(float delta)
		{
			float newZoom = Mathf.Clamp(PrivateSpringArm.SpringLength + delta,
				PrivateMiniumZoom, PrivateMaximumZoom);

			GetTree().CreateTween()
				.TweenProperty(PrivateSpringArm, "spring_length", newZoom, 0.1f)
				.SetEase(Tween.EaseType.Out);
		}

		private void HandleMouseMotion(InputEventMouseMotion eventMouseMotion)
		{
			if (Input.IsMouseButtonPressed(MouseButton.Right))
			{
				ApplyRotation(eventMouseMotion.Relative);
			}
		}

		private void ApplyRotation(Vector2 Relative)
		{
			Vector3 currentRotation = PrivateSpringArm.Rotation;

			PrivateSpringArm.Rotation = new Vector3(
				currentRotation.X - Relative.Y * PrivateMouseSensitivity,
				currentRotation.Y - Relative.X * PrivateMouseSensitivity,
				currentRotation.Z
			);

			PrivateSpringArm.RotationDegrees = new Vector3(
				Mathf.Clamp(PrivateSpringArm.RotationDegrees.X, -89.0f, 45.0f),
				PrivateSpringArm.RotationDegrees.Y,
				PrivateSpringArm.RotationDegrees.Z
			);
		}

		[MoonSharp.Interpreter.MoonSharpHidden]
		public void Die()
		{
			if (!IsMultiplayerAuthority())
			{
				NetAnimation = "Death";
			}
		}
	}
}
