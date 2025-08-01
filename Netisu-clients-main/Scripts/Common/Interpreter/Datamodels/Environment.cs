using Godot;
using System;
using Netisu.Workshop;

namespace Netisu.Datamodels
{
    public partial class Environment : Instance
    {
        [Export] private WorldEnvironment env = null!;

        [Export] private SkyBox _skyController = null!;

        public override void _Ready()
        {
            // The _skyController is now assigned from the editor, so we don't need GetNode here.
        }

        public float DayTime
        {
            get => _skyController != null ? _skyController.DayTime : 12.0f;
            set
            {
                if (_skyController != null)
                {
                    _skyController.SetDayTimeFromUI(value);
                }
            }
        }

        public bool ManualTimeControl
        {
            get => _skyController != null && _skyController.ManualTimeControl;
            set
            {
                if (_skyController != null)
                {
                    _skyController.ManualTimeControl = value;
                }
            }
        }

        public float Brightness
        {
            get => env!.Environment.BackgroundEnergyMultiplier;
            set
            {
                if (Netisu.Project.Flags.IsServer)
                {
                    Godot.Collections.Dictionary dat = new() { { "v", value } };
                    GetNodeOrNull<Server>("/root/Root")?.CallDeferred("DoSetAttribute", "brightness_env", dat);
                }
                env!.Environment.BackgroundEnergyMultiplier = value;
            }
        }

        public PreservedGlobalClasses.Col3 AmbientLightColor
        {
            get => new(env!.Environment.AmbientLightColor.R, env.Environment.AmbientLightColor.G, env.Environment.AmbientLightColor.B);
            set => env!.Environment.AmbientLightColor = new Godot.Color(value.r, value.g, value.b);
        }


        public bool VolumetricFogEnabled
        {
            get => env!.Environment.VolumetricFogEnabled;
            set
            {
                if (Netisu.Project.Flags.IsServer)
                {
                    Godot.Collections.Dictionary dat = new() { { "v", value } };
                    GetNodeOrNull<Server>("/root/Root")?.CallDeferred("DoSetAttribute", "vol_fog_enable", dat);
                }
                env!.Environment.VolumetricFogEnabled = value;
            }
        }

        public bool SSREnabled
        {
            get => env!.Environment.SsrEnabled;
            set
            {
                if (Netisu.Project.Flags.IsServer)
                {
                    Godot.Collections.Dictionary dat = new() { { "v", value } };
                    GetNodeOrNull<Server>("/root/Root")?.CallDeferred("DoSetAttribute", "ssr_enabled", dat);
                }
                env!.Environment.SsrEnabled = value;
            }
        }

        public bool SSAOEnabled
        {
            get => env!.Environment.SsaoEnabled;
            set
            {
                if (Netisu.Project.Flags.IsServer)
                {
                    Godot.Collections.Dictionary dat = new() { { "v", value } };
                    GetNodeOrNull<Server>("/root/Root")?.CallDeferred("DoSetAttribute", "ssao_enabled", dat);
                }
                env!.Environment.SsaoEnabled = value;
            }
        }

        // here are overriden functions or properties to not allow.
        public override Instance this[string key]
        {
            get => RuntimeErrors.ScriptRuntimeException("cannot index children inside environment");
        }

        public override Instance FindFirstChild(string key) => RuntimeErrors.ScriptRuntimeException("cannot index children inside environment");

        public override Instance FindChild(string key) => RuntimeErrors.ScriptRuntimeException("cannot index children inside environment");
    }
}
