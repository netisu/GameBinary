using Godot;
using System;
using MoonSharp.Interpreter;
using Netisu.Datamodels;

public partial class PreservedGlobalClasses : Node
{

	[MoonSharpUserData]
	public class Application(Node aptf)
	{
		private Node adapter = aptf;
		public float Version = 0.1f;
	}

	// Data types

	[MoonSharpUserData]
	public class Vec3
	{
		public float x = 0.0f;
		public float y = 0.0f;
		public float z = 0.0f;

		public Vec3 @new(float x1, float y1, float z1)
		{
			return new Vec3(x1, y1, z1);
		}

		public Vec3(float x1 = 0.0f, float y1 = 0.0f, float z1 = 0.0f)
		{
			x = x1;
			y = y1;
			z = z1;
		}
	}

	[MoonSharpUserData]
	public class Vec2
	{
		public float x = 0.0f;
		public float y = 0.0f;

		public Vec2 @new(float x1, float y1)
		{
			return new Vec2(x1, y1);
		}

		public Vec2(float x1 = 0.0f, float y1 = 0.0f)
		{
			x = x1;
			y = y1;
		}
	}

	[MoonSharpUserData]
	public class Col3
	{
		public float r = 0.0f;
		public float g = 0.0f;
		public float b = 0.0f;
		public float a = 0.0f;

		public Col3 @new(float r1, float g1, float b1, float a1 = 1.0f)
		{
			return new Col3(r1, g1, b1, a1);
		}

		public Col3(float r1 = 0.0f, float g1 = 0.0f, float b1 = 0.0f, float a1 = 0.0f)
		{
			r = r1;
			g = g1;
			b = b1;
			a = a1;
		}

		public Col3 FromHex(string hex)
		{
			hex = hex.TrimStart('#');

			if (hex.Length == 6)
			{
				return new Col3(
					Convert.ToInt32(hex.Substring(0, 2), 16) / 255.0f,
					Convert.ToInt32(hex.Substring(2, 2), 16) / 255.0f,
					Convert.ToInt32(hex.Substring(4, 2), 16) / 255.0f
				);
			}
			else if (hex.Length == 8)
			{
				return new Col3(
					Convert.ToInt32(hex.Substring(0, 2), 16) / 255.0f,
					Convert.ToInt32(hex.Substring(2, 2), 16) / 255.0f,
					Convert.ToInt32(hex.Substring(4, 2), 16) / 255.0f,
					Convert.ToInt32(hex.Substring(6, 2), 16) / 255.0f
				);
			}
			return new Col3(0.0f, 0.0f, 0.0f, 1.0f);
		}

		public Col3 Random()
		{
			Random rand = new Random();
			return new Col3((float)rand.NextDouble(), (float)rand.NextDouble(), (float)rand.NextDouble(), (float)rand.NextDouble());
		}
	}


	public class LuaEvent
	{
		DynValue CallbackFunction = null!;

		public void Connect(DynValue Callback)
		{
			if (Callback.Type != DataType.Function)
			{
				throw new ScriptRuntimeException("Connect expects a function to be passed to it.");
			}

			CallbackFunction = Callback;
		}

		public void Call(params Instance[] args)
		{
			if (args.Length == 0)
			{
				CallbackFunction.Function.Call();
			}
			else
			{
				CallbackFunction.Function.Call(args);
			}
		}
	}
}
