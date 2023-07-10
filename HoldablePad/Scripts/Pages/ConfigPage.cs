using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HoldablePad.Scripts.Pages
{
    public class ConfigPage : Page
    {
        public List<Transform> configSections = new List<Transform>();
        public List<Button> buttons = new List<Button>();

        public void Debug_EnableAll(Button.ButtonPage page)
            => buttons.ForEach(a => a.ButtonPressMethod(a.CurrentPage == page));

        public void LoadConfig()
        {
            foreach (var button in buttons)
            {
                var buttonPage = button.CurrentPage;
                if (buttonPage == Button.ButtonPage.ConfigHand)
                {
                    bool handButtonActive = (button.name == "Config_LeftHand" && Config.CurrentHand.Value == Config.HandPosition.LeftHand) || (button.name == "Config_RightHand" && Config.CurrentHand.Value == Config.HandPosition.RightHand);
                    button.ButtonPressMethod(handButtonActive);
                    continue;
                }

                if (buttonPage == Button.ButtonPage.ConfigSwap)
                {
                    button.ButtonPressMethod(Config.SwapHands.Value);
                    continue;
                }

                int currentThemeIndex = (int)Config.CurrentTheme.Value;
                List<Button> themeButton = buttons.Where(a => a.CurrentPage == Button.ButtonPage.ConfigTheme).ToList();
                bool themeButtonActive = themeButton.IndexOf(button) == currentThemeIndex;
                button.ButtonPressMethod(themeButtonActive);
            }
        }

        public void ExcludeOthers(Button ignoreButton)
        {
            List<Button> excludedButtons = buttons.Where(a => a != ignoreButton && a.CurrentPage == ignoreButton.CurrentPage).ToList();
            excludedButtons.ForEach(a => a.ButtonPressMethod(false));
        }

        public void SaveConfig()
        {
            Config.HandPosition currentHandPosition = Config.HandPosition.LeftHand;
            Config.PadTheme currentTheme = Config.PadTheme.Default;

            var activeButtons = buttons.Where(a => a.Pressed);
            foreach (var button in activeButtons)
            {
                if (button.CurrentPage == Button.ButtonPage.ConfigHand)
                {
                    currentHandPosition = button.name == "Config_LeftHand" ? Config.HandPosition.LeftHand : Config.HandPosition.RightHand;
                    continue;
                }
                if (button.CurrentPage == Button.ButtonPage.ConfigTheme)
                {
                    List<Button> themeButton = buttons.Where(a => a.CurrentPage == Button.ButtonPage.ConfigTheme).ToList();
                    int themeIndex = themeButton.IndexOf(button);
                    currentTheme = (Config.PadTheme)themeIndex;
                }
            }

            var swapButton = buttons.First(a => a.CurrentPage == Button.ButtonPage.ConfigSwap);
            bool currentSwapState = swapButton.Pressed;

            Config.CurrentHand.Value = currentHandPosition;
            Config.SwapHands.Value = currentSwapState;
            Config.CurrentTheme.Value = currentTheme;
            Config.ConfigFile.Save();

            Main.Instance.SetFavoruiteButtonSide(currentHandPosition == Config.HandPosition.LeftHand);
            Main.Instance.SetPadHand(currentHandPosition == Config.HandPosition.LeftHand);
            Main.Instance.SetPadTheme(currentTheme);
        }
    }
}
