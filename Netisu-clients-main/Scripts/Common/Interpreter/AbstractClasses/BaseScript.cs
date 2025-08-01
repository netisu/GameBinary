using Godot;
using System;

namespace Netisu.Datamodels
{
	public partial class BaseScript : Instance
	{
		private string _source = string.Empty;
		private string _side = string.Empty;

		public virtual string Source
		{
			get => _source;
			set => _source = value;
		}

		public virtual string Side
		{
			get => _side;
			set => _side = value;
		}

		public override Instance this[string key]
		{
			get => null;
		}

		public override Instance FindFirstChild(string key) => null;

		public override Instance FindChild(string key) => null;

		public override Instance Parent
		{
			get => null;
			set => _ = 2;
		}

	}

}
