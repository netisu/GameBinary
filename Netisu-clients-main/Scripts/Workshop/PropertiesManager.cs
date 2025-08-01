using Netisu.Game;
using Godot;
using Godot.Collections;
using Netisu.Datamodels;
using Netisu.Workshop;

namespace Netisu.Properties 
{
    public partial class PropertiesManager : Node 
    {
        public static void Empty() => Netisu.Workshop.EngineUI.Instance.ClearOutExistingPropertyTabs();

        public static void Load(Instance instance) 
        {
            Dictionary instanceInfo = CluaObjectList.GetObjectInformation(instance);
            if (instanceInfo == null)
            {
                return;
            }
            EngineUI.Instance.LoadIntoProperties(instanceInfo);
        }
    }
}
