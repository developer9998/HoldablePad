using BepInEx;
using BepInEx.Configuration;
using System.IO;

namespace HoldablePad.Scripts
{
    public static class Config
    {
        public static ConfigFile ConfigFile { get; private set; }

        public static ConfigEntry<HandPosition> CurrentHand;
        public static ConfigEntry<PadTheme> CurrentTheme;

        public static ConfigEntry<bool> SwapHands;
        public enum HandPosition
        {
            LeftHand,
            RightHand
        }

        public enum PadTheme
        {
            Default,
            Legacy,
            Fur,
            Ice,
            Metal,
            Crystals
        }

        public static ConfigEntry<string> CurrentHoldableLeft;
        public static ConfigEntry<string> CurrentHoldableRight;
        public static ConfigEntry<string> FavouriteHoldables;

        public static void Initalize()
        {
            ConfigFile = new ConfigFile(Path.Combine(Paths.ConfigPath, "DevHoldablePad.cfg"), true);

            CurrentHand = ConfigFile.Bind("Configuration", "Pad Hand", HandPosition.LeftHand, "This will determine which hand the pad will be put in.");
            CurrentTheme = ConfigFile.Bind("Configuration", "Pad Theme", PadTheme.Default, "This will change the current theme of the pad.");
            SwapHands = ConfigFile.Bind("Configuration", "Hand Swap", false, "This will allow swapping of the hand the pad is in.");

            CurrentHoldableLeft = ConfigFile.Bind("Internal Data", "Current Holdable (Left)", "None", "The full name of the currently equipped holdable for our left hand.");
            CurrentHoldableRight = ConfigFile.Bind("Internal Data", "Current Holdable (Right)", "None", "The full name of the currently equipped holdable for our right hand.");
            FavouriteHoldables = ConfigFile.Bind("Internal Data", "Favourited Holdables", "None", "The list of currently favourited holdables.");
        }

        public static void OverwriteHand(HandPosition handPosition)
        {
            CurrentHand.Value = handPosition;
            ConfigFile.Save();
        }
    }
}
