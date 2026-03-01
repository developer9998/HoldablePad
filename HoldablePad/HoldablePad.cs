using BepInEx;
using HarmonyLib;
using HoldablePad.Behaviors;
using System.Reflection;

namespace HoldablePad
{
    [BepInPlugin(Behaviors.Constants.GUID, Behaviors.Constants.Name, Behaviors.Constants.Version)]
    internal class HoldablePad : BaseUnityPlugin
    {
        internal void Awake()
        {
            Behaviors.Config.Initalize();
            Behaviors.Logger.manualLogSource = BepInEx.Logging.Logger.CreateLogSource(" " + Behaviors.Constants.Name);

            new Harmony(Behaviors.Constants.GUID).PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}
