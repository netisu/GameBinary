using System;
using Godot;
using Netisu;

namespace Netisu.Client.UI
{
    public partial class MobileControlManager : Control
    {
        public static MobileControlManager Instance { get; private set; } = null!;

        public override void _Ready()
        {
            if (OS.GetName() != "iOS" && OS.GetName() != "Android")
            {
                QueueFree();
                return;
            }
            else
            {
                Visible = true;
            }

            Instance = this;
        }
    }
}