using Godot;
using System;
using MoonSharp.Interpreter;
using Netisu.Datamodels;

namespace Netisu
{
	public partial class GlobalManager : Node
	{
		public static GlobalManager Instance { get; private set; } = null!;
		[Export] private PreservedGlobalFunctions pgf = null!;
		[Export] private PreservedGlobalClasses pgc = null!;

        public override void _Ready()
        {
			Instance = this;
        }

		private MoonSharp.Interpreter.Script RegisterGlobalFunctions(MoonSharp.Interpreter.Script script)
		{
			script.Globals["wait"] = (Action<int>)PreservedGlobalFunctions.Wait;
			script.Globals["printl"] = (Action<string>)PreservedGlobalFunctions.Printl;
			script.Globals["Map"] = GetNode<Map>("/root/Root/Game/Map");
			script.Globals["Game"] = GetNode<Datamodels.Game>("/root/Root/EngineGUI/SubViewportContainer/SubViewport/Game");
			script.Globals["Application"] = new PreservedGlobalClasses.Application(this);
			script.Globals["Vector3"] = new PreservedGlobalClasses.Vec3();
			script.Globals["Vector2"] = new PreservedGlobalClasses.Vec2();
			script.Globals["Color3"] = new PreservedGlobalClasses.Col3();
			script.Globals["Environment"] = GetNode<Datamodels.Environment>("/root/Root/Game/Environment");
			return script;
		}

		private static void RegisterUserDatas()
		{
			UserData.RegisterType<PreservedGlobalClasses.Application>();
			UserData.RegisterType<PreservedGlobalClasses.Vec3>();
			UserData.RegisterType<Map>();
			UserData.RegisterType<Part>();
			UserData.RegisterType<NPC>();
			UserData.RegisterType<Sun>();
			UserData.RegisterType<Datamodels.Environment>();
			UserData.RegisterType<ChatService>();
			UserData.RegisterType<Datamodels.Game>();
			UserData.RegisterType<Players>();
			UserData.RegisterType<Player>();
			UserData.RegisterType<RemoteEventService>();
			UserData.RegisterType<Seat>();
			UserData.RegisterType<PreservedGlobalClasses.Vec2>();
			UserData.RegisterType<PreservedGlobalClasses.Col3>();
		}

		public MoonSharp.Interpreter.Script Init(MoonSharp.Interpreter.Script script)
		{
            RegisterUserDatas();
			MoonSharp.Interpreter.Script stackscript = RegisterGlobalFunctions(script);
			return stackscript;
		}
	}

}
