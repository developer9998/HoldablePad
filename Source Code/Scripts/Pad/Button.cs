using Photon.Pun;
using UnityEngine;

namespace HoldablePad
{
    public class Button : MonoBehaviour
    {
        public float debounceTime = 0.25f;

        public float touchTime;

        private void OnTriggerEnter(Collider collider)
        {
            if (!enabled || !(touchTime + debounceTime < Time.time))
            {
                return;
            }

            if (collider.name == "LeftHandTriggerCollider")
            {
                return;
            }

            if (!(collider.GetComponentInParent<GorillaTriggerColliderHandIndicator>() != null))
            {
                return;
            }

            touchTime = Time.time;

            GorillaTriggerColliderHandIndicator component = collider.GetComponent<GorillaTriggerColliderHandIndicator>();
            ButtonActivation();
            ButtonActivationWithHand(component.isLeftHand);
            if (component != null)
            {
                GorillaTagger.Instance.offlineVRRig.PlayHandTapLocal(67, component.isLeftHand, 0.05f);
                GorillaTagger.Instance.StartVibration(component.isLeftHand, GorillaTagger.Instance.tapHapticStrength / 2f, GorillaTagger.Instance.tapHapticDuration);
                if (PhotonNetwork.InRoom && GorillaTagger.Instance.myVRRig != null)
                {
                    //PhotonView.Get(GorillaTagger.Instance.myVRRig).RPC("PlayHandTap", RpcTarget.Others, 67, component.isLeftHand, 0.05f);
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
