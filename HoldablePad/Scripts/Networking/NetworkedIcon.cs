using UnityEngine;

namespace HoldablePad.Scripts.Networking
{
    public class NetworkedIcon : MonoBehaviour
    {
        public Client myClient;
        public GameObject myIcon;

        public void Update()
            => myIcon.SetActive(myClient != null && (myClient.IsHoldablePadUser || myClient.HP_CurrentLeft != null || myClient.HP_Current != null));
    }
}
