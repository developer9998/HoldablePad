using HoldablePad.Behaviours;
using System.Collections.Generic;
using UnityEngine;

namespace HoldablePad.Utils
{
    public static class HoldableUtils
    {
        public static Dictionary<HP_Config.PadTheme, Vector2> mainThemeDict = new Dictionary<HP_Config.PadTheme, Vector2>()
        {
            {HP_Config.PadTheme.Default, new Vector2(0f, 0.66666f) },
            {HP_Config.PadTheme.Legacy, new Vector2(0.33333f, 0.66666f) },
            {HP_Config.PadTheme.Fur, new Vector2(0.66666f, 0.66666f) },
            {HP_Config.PadTheme.Ice, new Vector2(0f,0.33333f) },
            {HP_Config.PadTheme.Metal, new Vector2(0.66666f, 0.33333f) },
            {HP_Config.PadTheme.Crystals, new Vector2(0.33333f, 0.33333f) },
        };

        public static Dictionary<HP_Config.PadTheme, Vector2> menuThemeDict = new Dictionary<HP_Config.PadTheme, Vector2>()
        {
            {HP_Config.PadTheme.Default, new Vector2(0f, 0.66666f) },
            {HP_Config.PadTheme.Legacy, new Vector2(0.33333f, 0.66666f) },
            {HP_Config.PadTheme.Fur, new Vector2(0.66666f, 0.66666f) },
            {HP_Config.PadTheme.Ice, new Vector2(0f,0.33333f) },
            {HP_Config.PadTheme.Metal, new Vector2(0.66666f, 0.33333f) },
            {HP_Config.PadTheme.Crystals, new Vector2(0.33333f, 0.33333f) },
        };

        public static Dictionary<HP_Config.PadTheme, Color> buttonColourDict = new Dictionary<HP_Config.PadTheme, Color>()
        {
            {HP_Config.PadTheme.Default, new Color32(135, 135, 135, 255) },
            {HP_Config.PadTheme.Legacy, new Color32(192, 189, 170, 255) },
            {HP_Config.PadTheme.Fur, new Color32(135, 135, 135, 255) },
            {HP_Config.PadTheme.Ice, new Color32(159, 189, 197, 255) },
            {HP_Config.PadTheme.Metal, new Color32(179, 179, 179, 255) },
            {HP_Config.PadTheme.Crystals, new Color32(135, 135, 135, 255) },
        };

        public static bool IsEquipped(Holdable holdable)
            => Main.Instance.CurrentHandheldL == holdable || Main.Instance.CurrentHandheldR == holdable;

        public static Vector2 MainTexOffset(HP_Config.PadTheme currentTheme)
            => mainThemeDict[currentTheme];

        public static Vector2 MenuTexOffset(HP_Config.PadTheme currentTheme)
            => menuThemeDict[currentTheme];

        public static Color ButtonColour(HP_Config.PadTheme currentTheme)
            => buttonColourDict[currentTheme];
    }
}
