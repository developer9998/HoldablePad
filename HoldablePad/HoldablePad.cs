using BepInEx;
using HarmonyLib;
using HoldablePad.Scripts;
using System.Reflection;

namespace HoldablePad
{
    [BepInPlugin(Constants.GUID, Constants.Name, Constants.Version)]
    internal class HoldablePad : BaseUnityPlugin
    {
        internal void Awake()
        {
            Scripts.Config.Initalize();
            Scripts.Logger.manualLogSource = BepInEx.Logging.Logger.CreateLogSource(" " + Constants.Name);

            new Harmony(Constants.GUID).PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}
