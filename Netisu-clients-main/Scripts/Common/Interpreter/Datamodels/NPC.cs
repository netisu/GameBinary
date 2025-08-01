using Godot;
using System;

namespace Netisu.Datamodels
{
	public partial class NPC : CharacterBody3D
	{
		private string DisplayOnTopVar = "NPC";
		private int HealthVar = 100;
		private bool DisplayOnTopEnabledVar = true;

		public float speed = 8.0f;

		public int Health
        {
            get => HealthVar;
            set
            {
                if (value == 0)
                {
                    // kill
                }
                HealthVar = value;
            }
        }
        public string DisplayOnTop
		{
			get
			{
				GetNode<Label3D>("Avatar/Head/Head/Label3D").Text = DisplayOnTopVar;
				return DisplayOnTopVar;
			}
			set
			{
				DisplayOnTopVar = value;
				GetNode<Label3D>("Avatar/Head/Head/Label3D").Text = DisplayOnTopVar;
			}
		}
		public bool DisplayOnTopEnabled
        {
            get => DisplayOnTopEnabledVar;
            set
            {
                if (value == false)
                {
                    GetNode<Label3D>("Avatar/Head/Head/Label3D").Visible = false;
                }
                DisplayOnTopEnabledVar = value;
            }
        }

        public void MoveTo(PreservedGlobalClasses.Vec3 newPos)
		{
			Vector3 GodotVec3Support = new Vector3(newPos.x, newPos.y, newPos.z);
			GD.Print(GodotVec3Support.X.ToString());
			Vector3 direction = GlobalPosition.DirectionTo(GodotVec3Support);
			Velocity = direction * speed;
			CallDeferred("DeferredMoveAndSlide");
		}

		private void DeferredMoveAndSlide()
		{
			MoveAndSlide();
		}

		public string IsA()
		{
			return "NPC";
		}

		public void Kill()
		{
			Health = 0;
		}
	}

}
