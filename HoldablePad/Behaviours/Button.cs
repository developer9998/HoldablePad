using GorillaLocomotion;
using Photon.Pun;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;

namespace HoldablePad.Behaviours
{
    public class Button : MonoBehaviour
    {
        public bool
            Pressed,
            PressedCooldown;
        public ButtonPage CurrentPage;

        private MeshRenderer Renderer;
        private Gradient ButtonGradient;
        private float GradientTimestamp = 1;

        private bool IsPage
            => name.Contains("3") || name.Contains("4") || name.Contains("5");

        public enum ButtonPage
        {
            Main,
            Favourite,
            ConfigHand,
            ConfigSwap,
            ConfigTheme,
        }

        public void Start()
        {
            Renderer = GetComponent<MeshRenderer>();
            GetComponent<BoxCollider>().isTrigger = true;
            gameObject.layer = LayerMask.NameToLayer("GorillaInteractable");

            ButtonGradient = new Gradient();
            if (CurrentPage == ButtonPage.Main)
            {
                var colourKeysH = new GradientColorKey[3];
                var alphaKeysH = new GradientAlphaKey[2];

                // Deal with alpha keys first, they're easier
                alphaKeysH[0] = new GradientAlphaKey(1f, 0f);
                alphaKeysH[1] = new GradientAlphaKey(1f, Constants.ButtonDebounce);

                // Now deal with the colours
                colourKeysH[0] = new GradientColorKey(new Color32(192, 189, 170, 255), 0);
                colourKeysH[1] = new GradientColorKey(Color.red, Constants.ButtonDebounce / 2f);
                colourKeysH[2] = new GradientColorKey(new Color32(192, 189, 170, 255), Constants.ButtonDebounce);

                ButtonGradient.SetKeys(colourKeysH, alphaKeysH);
                GradientTimestamp = Constants.ButtonDebounce;
                return;
            }

            var colourKeys = new GradientColorKey[2];
            var alphaKeys = new GradientAlphaKey[2];

            // Deal with alpha keys first, they're easier
            alphaKeys[0] = new GradientAlphaKey(1f, 0f);
            alphaKeys[1] = new GradientAlphaKey(1f, Constants.ConfigBtnDebounce);

            // Now deal with the colours
            colourKeys[0] = new GradientColorKey(new Color32(192, 189, 170, 255), 0);
            colourKeys[1] = new GradientColorKey(Color.red, Constants.ConfigBtnDebounce);

            ButtonGradient.SetKeys(colourKeys, alphaKeys);
            GradientTimestamp = Constants.ConfigBtnDebounce;
        }

        internal void Update()
        {
            if (CurrentPage == ButtonPage.Main || CurrentPage != ButtonPage.Main && Pressed)
                GradientTimestamp += Time.unscaledDeltaTime;
            else if (CurrentPage != ButtonPage.Main && !Pressed)
                GradientTimestamp -= Time.unscaledDeltaTime;
            GradientTimestamp = Mathf.Clamp(GradientTimestamp, 0f, CurrentPage == ButtonPage.Main ? Constants.ButtonDebounce : Constants.ConfigBtnDebounce);
        }

        internal void FixedUpdate()
            => Renderer.material.color = ButtonGradient.Evaluate(GradientTimestamp);

        internal void OnTriggerEnter(Collider other)
        {
            if (Player.Instance.inOverlay || Player.Instance.InReportMenu || Static.IsGlobalBtnCooldown && IsPage || Static.IsHoldableBtnCooldown)
                return;

            if (other.TryGetComponent(out GorillaTriggerColliderHandIndicator handIndicator))
            {
                if (CurrentPage == ButtonPage.Main && Pressed || CurrentPage != ButtonPage.Main && PressedCooldown) return;
                OnPress(handIndicator);
            }
        }

        private async void OnPress(GorillaTriggerColliderHandIndicator handIndicator)
        {
            if (handIndicator == null || handIndicator.isLeftHand == (HP_Config.CurrentHand.Value == HP_Config.HandPosition.LeftHand)) return;
            if (CurrentPage == ButtonPage.Main)
            {
                Pressed = true;
                GradientTimestamp = 0;
                Static.HBBtnCooldownTime = Time.unscaledTime;

                Main.Instance.PageButtonPress(gameObject.name);
                InputDevices.GetDeviceAtXRNode(handIndicator.isLeftHand ? XRNode.LeftHand : XRNode.RightHand).SendHapticImpulse(0u, Constants.ButtonAmplitude, Constants.ButtonDuration);

                await Task.Delay(Mathf.FloorToInt((gameObject.name == "Cube" && (Main.Instance.CurrentScreenMode == Main.ScreenModes.HoldableView || Main.Instance.CurrentScreenMode == Main.ScreenModes.FavouriteView) && PhotonNetwork.InRoom ? Constants.NetworkDebounce : Constants.ButtonDebounce) * 1000f));
                Pressed = false;
                return;
            }

            if (Pressed && (CurrentPage == ButtonPage.ConfigSwap || CurrentPage == ButtonPage.Favourite))
                goto Press;

            if (Pressed)
                return;

            Press:
            InputDevices.GetDeviceAtXRNode(handIndicator.isLeftHand ? XRNode.LeftHand : XRNode.RightHand).SendHapticImpulse(0u, Constants.ButtonAmplitude, Constants.ButtonDuration);
            ButtonPressMethod(CurrentPage != ButtonPage.Favourite && CurrentPage != ButtonPage.ConfigSwap || !Pressed);

            if (CurrentPage == ButtonPage.Favourite)
            {
                Main.Instance.FavButtonPress();
                return;
            }

            Main.Instance.ConfigButtonPress();
            Main.Instance.ConfigPage.ExcludeOthers(this);
        }

        public async void ButtonPressMethod(bool state)
        {
            Pressed = state;
            PressedCooldown = true;

            if (transform.GetComponentInChildren<Text>() is Text parentText && CurrentPage == ButtonPage.ConfigSwap)
                parentText.text = Pressed ? "Enabled" : "Disabled";

            await Task.Delay(Mathf.FloorToInt(Constants.ButtonDebounce * 1000f));
            PressedCooldown = false;
        }
    }
}
