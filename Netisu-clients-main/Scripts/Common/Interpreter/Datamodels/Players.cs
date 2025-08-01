using Godot;
using MoonSharp.Interpreter;
using System;

namespace Netisu.Datamodels
{
	public partial class Players : Game
	{
		[MoonSharpHidden]
		public static Players Instance { get; private set; } = null!;

		[MoonSharpHidden]
		public override void _Ready()
		{
			Instance = this;
		}

		[MoonSharpHidden]
		public PreservedGlobalClasses.LuaEvent playerAdded { get; private set; } = new();

		public int playerCount
		{
			get => GetChildCount();
		}

		public float respawnTime = 5.0f;

		public Player getPlayer(string username)
			=> GetNodeOrNull<Player>(username);
	}

}