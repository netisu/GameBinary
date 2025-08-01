using Godot;
using System;
using System.Collections.Generic;

namespace Netisu.Datamodels
{
	public partial class Seat : Area3D
{
	public bool locked = false;
	public Player? Occupant = null;

	public string IsA()
	{
		return "Seat";
	}
}

}
