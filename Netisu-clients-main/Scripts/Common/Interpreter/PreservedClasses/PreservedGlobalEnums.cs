using Godot;
using System;

public partial class PreservedGlobalEnums : Node
{
	public enum EaseType : long
	{
		EASE_IN = Tween.EaseType.In,
		EASE_OUT = Tween.EaseType.Out,
		EASE_IN_OUT = Tween.EaseType.InOut,
		EASE_OUT_IN = Tween.EaseType.OutIn,
	}
}
