using UnityEngine;

namespace HoldablePad.Scripts
{
    public static class Static
    {
        public static bool IsGlobalBtnCooldown
            => (GlobalBtnCooldownTime + 0.4f) > Time.unscaledTime;

        public static bool IsHoldableBtnCooldown
            => (HBBtnCooldownTime + (Constants.ButtonDebounce / 1.5f)) > Time.unscaledTime;

        public static float 
            GlobalBtnCooldownTime, 
            HBBtnCooldownTime;
    }
}
