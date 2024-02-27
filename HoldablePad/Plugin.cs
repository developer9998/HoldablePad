using BepInEx;
using HarmonyLib;
using System.Reflection;

namespace HoldablePad
{
    [BepInPlugin(Constants.GUID, Constants.Name, Constants.Version)]
    internal class Plugin : BaseUnityPlugin
    {
        internal void Awake()
        {
            HP_Config.Initalize();
            HP_Log.manualLogSource = Logger;

            new Harmony(Constants.GUID).PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}
