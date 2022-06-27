using Photon.Pun;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace HoldablePad
{
    public class GorillaPressableButtonRightHand : MonoBehaviour
    {
        public Material pressedMaterial;

        public Material unpressedMaterial;

        public MeshRenderer buttonRenderer;

        public bool isOn;

        public float debounceTime = 0.25f;

        public float touchTime;

        public bool testPress;

        public bool testHandLeft;

        [TextArea]
        public string offText;

        [TextArea]
        public string onText;

        public Text myText;

        private void OnTriggerEnter(Collider collider)
        {
            if (!base.enabled || !(touchTime + debounceTime < Time.time))
            {
                return;
            }

            if (collider.name == "LeftHandTriggerCollider")
            {
                return;
            }

            touchTime = Time.time;
            if (!(collider.GetComponentInParent<GorillaTriggerColliderHandIndicator>() != null))
            {
                return;
            }

            GorillaTriggerColliderHandIndicator component = collider.GetComponent<GorillaTriggerColliderHandIndicator>();
            ButtonActivation();
            ButtonActivationWithHand(component.isLeftHand);
            if (component != null)
            {
                GorillaTagger.Instance.offlineVRRig.PlayHandTapLocal(67, component.isLeftHand, 0.05f);
                GorillaTagger.Instance.StartVibration(component.isLeftHand, GorillaTagger.Instance.tapHapticStrength / 2f, GorillaTagger.Instance.tapHapticDuration);
                if (PhotonNetwork.InRoom && GorillaTagger.Instance.myVRRig != null)
                {
                    PhotonView.Get(GorillaTagger.Instance.myVRRig).RPC("PlayHandTap", RpcTarget.Others, 67, component.isLeftHand, 0.05f);
                }
            }
        }
        public virtual void ButtonActivation()
        {
        }

        public virtual void ButtonActivationWithHand(bool isLeftHand)
        {
        }
    }
}
