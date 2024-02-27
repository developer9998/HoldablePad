using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HoldablePad.Behaviours.Pages
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
                    bool handButtonActive = button.name == "Config_LeftHand" && HP_Config.CurrentHand.Value == HP_Config.HandPosition.LeftHand || button.name == "Config_RightHand" && HP_Config.CurrentHand.Value == HP_Config.HandPosition.RightHand;
                    button.ButtonPressMethod(handButtonActive);
                    continue;
                }

                if (buttonPage == Button.ButtonPage.ConfigSwap)
                {
                    button.ButtonPressMethod(HP_Config.SwapHands.Value);
                    continue;
                }

                int currentThemeIndex = (int)HP_Config.CurrentTheme.Value;
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
            HP_Config.HandPosition currentHandPosition = HP_Config.HandPosition.LeftHand;
            HP_Config.PadTheme currentTheme = HP_Config.PadTheme.Default;

            var activeButtons = buttons.Where(a => a.Pressed);
            foreach (var button in activeButtons)
            {
                if (button.CurrentPage == Button.ButtonPage.ConfigHand)
                {
                    currentHandPosition = button.name == "Config_LeftHand" ? HP_Config.HandPosition.LeftHand : HP_Config.HandPosition.RightHand;
                    continue;
                }
                if (button.CurrentPage == Button.ButtonPage.ConfigTheme)
                {
                    List<Button> themeButton = buttons.Where(a => a.CurrentPage == Button.ButtonPage.ConfigTheme).ToList();
                    int themeIndex = themeButton.IndexOf(button);
                    currentTheme = (HP_Config.PadTheme)themeIndex;
                }
            }

            var swapButton = buttons.First(a => a.CurrentPage == Button.ButtonPage.ConfigSwap);
            bool currentSwapState = swapButton.Pressed;

            HP_Config.CurrentHand.Value = currentHandPosition;
            HP_Config.SwapHands.Value = currentSwapState;
            HP_Config.CurrentTheme.Value = currentTheme;
            HP_Config.ConfigFile.Save();

            Main.Instance.SetFavoruiteButtonSide(currentHandPosition == HP_Config.HandPosition.LeftHand);
            Main.Instance.SetPadHand(currentHandPosition == HP_Config.HandPosition.LeftHand);
            Main.Instance.SetPadTheme(currentTheme);
        }
    }
}
