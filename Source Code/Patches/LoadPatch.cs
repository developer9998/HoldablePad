using HarmonyLib;
using GorillaLocomotion;
using System.Collections;

namespace HoldablePad
{
    /// <summary>
    /// This is an example patch, made to demonstrate how to use Harmony. You should remove it if it is not used.
    /// </summary>
    [HarmonyPatch(typeof(Player))]
    [HarmonyPatch("Awake", MethodType.Normal)]
    internal class LoadPatch
    {
        private static void Postfix(Player __instance)
        {
            __instance.StartCoroutine(OnInitialized());
        }

        private static IEnumerator OnInitialized()
        {
            yield return 0;
            Player.Instance.StartCoroutine(Main.LaunchHoldablePadMain());
        }
    }
}