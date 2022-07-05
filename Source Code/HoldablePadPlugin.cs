using BepInEx;
using System;
using UnityEngine;
using Utilla;

namespace HoldablePad
{
    /// <summary>
    /// This is your mod's main class.
    /// </summary>

    [BepInDependency("org.legoandmars.gorillatag.utilla", "1.5.0")]
    [BepInPlugin("com.dev9998.gorillatag.holdablepad", "HoldablePad", "1.0.0")]
    public class HoldablePadPlugin : BaseUnityPlugin
    {
        void Awake()
        {
            Events.GameInitialized += OnGameInitialized;
        }

        void OnGameInitialized(object sender, EventArgs e)
        {
            HoldableCore.LaunchHoldablePad();
        }
    }
}
