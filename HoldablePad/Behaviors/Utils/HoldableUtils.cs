using HoldablePad.Behaviors.Holdables;
using System.Collections.Generic;
using UnityEngine;

namespace HoldablePad.Behaviors.Utils
{
    public static class HoldableUtils
    {
        public static Dictionary<Config.PadTheme, Vector2> mainThemeDict = new Dictionary<Config.PadTheme, Vector2>()
        {
            {Config.PadTheme.Default, new Vector2(0f, 0.66666f) },
            {Config.PadTheme.Legacy, new Vector2(0.33333f, 0.66666f) },
            {Config.PadTheme.Fur, new Vector2(0.66666f, 0.66666f) },
            {Config.PadTheme.Ice, new Vector2(0f,0.33333f) },
            {Config.PadTheme.Metal, new Vector2(0.66666f, 0.33333f) },
            {Config.PadTheme.Crystals, new Vector2(0.33333f, 0.33333f) },
        };

        public static Dictionary<Config.PadTheme, Vector2> menuThemeDict = new Dictionary<Config.PadTheme, Vector2>()
        {
            {Config.PadTheme.Default, new Vector2(0f, 0.66666f) },
            {Config.PadTheme.Legacy, new Vector2(0.33333f, 0.66666f) },
            {Config.PadTheme.Fur, new Vector2(0.66666f, 0.66666f) },
            {Config.PadTheme.Ice, new Vector2(0f,0.33333f) },
            {Config.PadTheme.Metal, new Vector2(0.66666f, 0.33333f) },
            {Config.PadTheme.Crystals, new Vector2(0.33333f, 0.33333f) },
        };

        public static Dictionary<Config.PadTheme, Color> buttonColourDict = new Dictionary<Config.PadTheme, Color>()
        {
            {Config.PadTheme.Default, new Color32(135, 135, 135, 255) },
            {Config.PadTheme.Legacy, new Color32(192, 189, 170, 255) },
            {Config.PadTheme.Fur, new Color32(135, 135, 135, 255) },
            {Config.PadTheme.Ice, new Color32(159, 189, 197, 255) },
            {Config.PadTheme.Metal, new Color32(179, 179, 179, 255) },
            {Config.PadTheme.Crystals, new Color32(135, 135, 135, 255) },
        };

        public static bool IsEquipped(Holdable holdable)
            => Main.Instance.CurrentHandheldL == holdable || Main.Instance.CurrentHandheldR == holdable;

        public static Vector2 MainTexOffset(Config.PadTheme currentTheme)
            => mainThemeDict[currentTheme];

        public static Vector2 MenuTexOffset(Config.PadTheme currentTheme)
            => menuThemeDict[currentTheme];

        public static Color ButtonColour(Config.PadTheme currentTheme)
            => buttonColourDict[currentTheme];
    }
}
