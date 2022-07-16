using BepInEx;
using HarmonyLib;
using System.Reflection;

namespace HoldablePad
{
    /// <summary>
    /// This is your mod's main class.
    /// </summary>
    [BepInPlugin("com.dev9998.gorillatag.holdablepad", "HoldablePad", "1.0.0")]
    public class HoldablePadPlugin : BaseUnityPlugin
    {
        void Awake()
        {
            Harmony holdableHarmony = new Harmony("com.dev9998.gorillatag.holdablepad");
            holdableHarmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}
