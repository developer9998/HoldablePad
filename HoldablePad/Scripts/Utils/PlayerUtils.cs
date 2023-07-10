using UnityEngine;

namespace HoldablePad.Scripts.Utils
{
    public static class PlayerUtils
    {
        public static Transform GetPalm(bool isLeftHand)
            => GetPalm(GorillaTagger.Instance.offlineVRRig, isLeftHand);

        public static Transform GetPalm(VRRig currentRig, bool isLeftHand)
        {
            Transform handCenter = isLeftHand ? currentRig.leftHandTransform : currentRig.rightHandTransform;
            string palmName = isLeftHand ? "palm.01.L" : "palm.01.R";
            return handCenter.parent.Find(palmName);
        }
    }
}
