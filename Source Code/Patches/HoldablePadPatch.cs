using HarmonyLib;
using GorillaLocomotion;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;

namespace HoldablePad
{
    /// <summary>
    /// This is an example patch, made to demonstrate how to use Harmony. You should remove it if it is not used.
    /// </summary>
    [HarmonyPatch(typeof(Player))]
    [HarmonyPatch("Awake", MethodType.Normal)]
    internal class PlayerAwakePatch
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

    [HarmonyPatch(typeof(PlayerPrefs))]
    [HarmonyPatch("SetFloat", MethodType.Normal)]
    internal class PlayerColourChanged
    {
        private static void Postfix(string key, string value)
        {
            bool colourSwap = false;
            if (key == "redValue")
                colourSwap = true;

            if (key == "greenValue")
                colourSwap = true;

            if (key == "blueValue")
                colourSwap = true;

            if (colourSwap)
            {
                Page.playerColour = new Color(PlayerPrefs.GetFloat("redValue"), PlayerPrefs.GetFloat("greenValue"), PlayerPrefs.GetFloat("blueValue"), 1);
            }
        }
    }
}