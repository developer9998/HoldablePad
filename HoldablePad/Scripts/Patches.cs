using HarmonyLib;
using HoldablePad.Scripts.Networking;
using UnityEngine;

namespace HoldablePad.Scripts
{
    [HarmonyPatch]
    public static class Patches
    {
        public static Color PlayerColour;

        [HarmonyPatch(typeof(GorillaTagger), "Start"), HarmonyPostfix]
        public static void InitPatch(GorillaTagger __instance)
            => __instance.gameObject.AddComponent<Main>();

        [HarmonyPatch(typeof(GorillaTagger), "UpdateColor"), HarmonyPrefix]
        public static void ColourPatch(float red, float green, float blue)
            => PlayerColour = new Color(red, green, blue);

        [HarmonyPatch(typeof(VRRig), "SetColor"), HarmonyPrefix]
        public static void RigColourPatch(VRRig __instance, Color color)
        {
            if (!__instance.gameObject.TryGetComponent(out Client client))
                return;
            client.currentColour = color;
        }
    }
}
