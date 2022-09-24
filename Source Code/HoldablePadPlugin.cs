using BepInEx;
using HarmonyLib;
using System.Reflection;

/* This product is not affiliated with Gorilla Tag or Another Axiom LLC and is not endorsed or otherwise sponsored by Another Axiom LLC. 
Portions of the materials contained herein are property of Another Axiom LLC. ©2021 Another Axiom LLC. */

namespace HoldablePad
{
    /// <summary>
    /// This is your mod's main class.
    /// </summary>
    /// 
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
