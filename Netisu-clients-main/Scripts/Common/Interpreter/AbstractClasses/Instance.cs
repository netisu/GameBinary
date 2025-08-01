using Godot;
using MoonSharp.Interpreter;
using System.Collections.Generic;
using System.Linq;

namespace Netisu.Datamodels
{
	public partial class Instance : Node
	{
		private readonly Dictionary<string, DynValue> Attributes = [];

		// attributes
		public DynValue GetAttribute(string AttributeName) => Attributes.TryGetValue(AttributeName, out var value) ? value : null;
		public void SetAttribute(string attribute_name, DynValue attribute_value) => Attributes[attribute_name] = attribute_value;

		// object search
		public virtual Instance this[string key]
		{
			get
			{
				if (GetParent() == null)
					return null!;
				return (Instance)GetNodeOrNull(key);
			}

			private set { }
		}

		public virtual Instance FindFirstChild(string key)
		{
			if (GetParent() == null)
				return null!;

			return GetNodeOrNull<Instance>(key);
		}

		public virtual Instance FindChild(string key) => FindFirstChild(key);

		public virtual Instance[] GetChildren()
		{
			// Add a check to ensure the object hasn't been disposed.
			if (!IsInstanceValid(this))
			{
				// If it's disposed, return an empty array to prevent a crash.
				return [];
			}
			return [.. base.GetChildren().OfType<Instance>()];
		}
        public virtual void Rename(string newName) => base.Name = newName;

		 public virtual new string Name
        {
            get => base.Name;
            set => base.Name = value; // Set the base name directly to avoid recursion.
        }


		public virtual Instance Parent
		{
			get => GetParent<Instance>();
			set
			{
				GetParent()?.CallDeferred("remove_child", this);
				value.CallDeferred("add_child", this, true);
			}
		}

		public virtual void Destroy()
		{
			if (Netisu.Project.Flags.IsServer && GetParent() != null)
			{
				Godot.Collections.Dictionary dat = new()
			{
				{"ObjectName", GetPath()},
			};
				GetNode<Server>("/root/Root").CallDeferred("DoSetAttribute", "destroy_change", dat);
			}

			GetParent()?.CallDeferred("remove_child", this);
			QueueFree();
		}



		public string IsA() => GetType().Name;
	}

}
