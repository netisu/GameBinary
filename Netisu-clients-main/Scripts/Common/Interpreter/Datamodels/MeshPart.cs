using Godot;
using System;

namespace Netisu.Datamodels
{

	public partial class MeshPart : Part
	{
		private int _MESHID = 0;
		private string _anim_to_play = "[REST]";

		public string shape
		{
			get => "";
			private set { }
		}

		private bool priv_let_engine_make_collisons = true;

		public bool let_engine_make_collisions
		{
			get => priv_let_engine_make_collisons;
			set => HandleEngineCollUpd(value);
		}

		public int mesh_id
		{
			get => _MESHID;
			set { }
		}

		public string CurrentAnimation
		{
			get => CurrentAnimationPlaying();
			private set { }
		}

		public string play_anim_on_start
		{
			get => _anim_to_play;
			set => _anim_to_play = value;
		}

		private void HandleEngineCollUpd(bool _v)
		{
			priv_let_engine_make_collisons = _v;
			if (!priv_let_engine_make_collisons)
			{
				DeleteMeshEngineGeneratedCollisions();
				return;
			}
			CreateTrimeshShapesForGLTFSceneRootMeshes(GetNode<Node3D>("GLTFRoot"));
		}

		public void CreateTrimeshShapesForGLTFSceneRootMeshes(Node3D gltf_root)
		{
			void TraverseAndCreateTrimeshShapes(Node3D currentNode)
			{
				foreach (Node child in currentNode.GetChildren())
				{
					if (child is MeshInstance3D meshInstance3D)
					{
						meshInstance3D.CreateConvexCollision();
					}

					if (child is Node3D childNode3D)
					{
						TraverseAndCreateTrimeshShapes(childNode3D);
					}
				}
			}

			TraverseAndCreateTrimeshShapes(GetNode<Node3D>("GLTFRoot"));
		}

		private void DeleteCollisionStaticBody(MeshInstance3D _mesh)
		{
			foreach (Node child in _mesh.GetChildren())
			{
				if (child is StaticBody3D zluv)
				{
					_mesh.RemoveChild(zluv);
					zluv.QueueFree();
				}
			}
		}

		private void DeleteMeshEngineGeneratedCollisions()
		{
			void TraverseAndRidShapes(Node3D currentNode)
			{
				if (currentNode == null)
					return;
				foreach (Node child in currentNode.GetChildren())
				{
					if (child is MeshInstance3D childNode3D)
					{
						DeleteCollisionStaticBody(childNode3D);
					}
					TraverseAndRidShapes(child as Node3D);
				}
			}

			TraverseAndRidShapes(GetNode<Node3D>("GLTFRoot"));
		}

		private string CurrentAnimationPlaying()
		{
			AnimationPlayer animation_player = GetNodeOrNull<AnimationPlayer>("GLTFRoot/AnimationPlayer");
			if (animation_player == null)
			{
				return "Animations are not supported with this mesh.";
			}

			return animation_player.CurrentAnimation;
		}

		public object Play(string _animation_key)
		{
			AnimationPlayer animation_player = GetNodeOrNull<AnimationPlayer>("GLTFRoot/AnimationPlayer");
			if (animation_player == null)
			{
				return "Animations are not supported with this mesh.";
			}

			if (_animation_key == "[REST]")
			{
				animation_player.Stop();
				return null;
			}

			try
			{
				animation_player.Play(_animation_key);
			}
			catch
			{
				return "An error occured";
			}
			return null;
		}

		public string IsA() => "MeshPart";

		public object PlayBackwards(string _animation_key)
		{
			AnimationPlayer animation_player = GetNodeOrNull<AnimationPlayer>("GLTFRoot/AnimationPlayer");
			if (animation_player == null)
			{
				return "Animations are not supported with this mesh.";
			}

			try
			{
				animation_player.PlayBackwards(_animation_key);
			}
			catch
			{
				return "An error occured";
			}
			return null;
		}

		public void StopAnimation()
		{
			AnimationPlayer animation_player = GetNodeOrNull<AnimationPlayer>("GLTFRoot/AnimationPlayer");
			if (animation_player != null)
				animation_player.Stop();
		}
	}
}