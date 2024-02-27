using HarmonyLib;
using HoldablePad.Behaviours;
using HoldablePad.Behaviours.Networking;
using UnityEngine;

namespace HoldablePad
{
    [HarmonyPatch]
    public static class Patches
    {
        public static Color PlayerColour;

        [HarmonyPatch(typeof(GorillaTagger), "Start"), HarmonyPostfix]
        public static void InitPatch(GorillaTagger __instance)
        {
            var holdablePadParent = new GameObject("HoldablePadMain", typeof(Main));
            holdablePadParent.transform.parent = __instance.gameObject.transform;
        }

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

        [HarmonyPatch(typeof(TransferrableBall), "OnCollisionEnter"), HarmonyPrefix, HarmonyWrapSafe]
        public static bool BallCollisionEnterPatch(Collision collision)
        {
            if (collision.rigidbody != null && collision.rigidbody.name.StartsWith("UsedBulletGameObject(Clone)")) return false;
            return true;
        }
    }
}
