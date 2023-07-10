using HoldablePad.Scripts.Holdables;
using Photon.Realtime;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace HoldablePad.Scripts.Networking
{
    public class Client : MonoBehaviour
    {
        public static List<Client> netClients = new List<Client>();
        public static Dictionary<Client, Player> keyValuePairs = new Dictionary<Client, Player>();

        public bool ClientLoaded;

        public GameObject HP_Current;
        public Holdable HP_HCurrent;
        public GameObject HP_CurrentLeft;
        public Holdable HP_HCurrentLeft;

        public VRRig currentRig;
        public Player currentPlayer;
        public Color currentColour;

        public async void OnEnable()
        {
            if (ClientLoaded || currentPlayer == null || currentRig == null)
            {
                Unequip(true, true);
                if (ClientLoaded)
                    return;

                // Try to get it back 
                if (TryGetComponent(out currentRig))
                {
                    var creatorField = currentRig.GetType().GetField("creator", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    currentPlayer = (Player)creatorField.GetValue(currentRig);
                }

                // If it can't just give up
                if (currentPlayer == null || currentRig == null)
                    return;
            }
            ClientLoaded = true;

            netClients.Add(this);
            keyValuePairs.Add(this, currentPlayer);

            if (Main.Instance == null || (Main.Instance != null && !Main.Instance.Initalized))
                await AwaitInitalize();

            if (currentPlayer != null && !currentPlayer.IsLocal)
                HoldableNetwork.Instance.OnPlayerPropertiesUpdate(currentPlayer, currentPlayer.CustomProperties);
        }

        public void OnDisable()
        {
            if (ClientLoaded)
            {
                // Bye bye!! 3:57 AM 7/2/2023
                Unequip(true, true);
                ClientLoaded = false;

                if (netClients.Contains(this))
                {
                    netClients.Remove(this);
                    keyValuePairs.Remove(this);
                }
            }
        }

        public async Task AwaitInitalize()
        {
            while (Main.Instance == null)
                await Task.Delay(50);

            while (!Main.Instance.Initalized)
                await Task.Delay(50);
        }

        public Transform GetPalm(bool isLeftHand)
        {
            Transform hand = isLeftHand ? currentRig.leftHandTransform.parent : currentRig.rightHandTransform.parent;
            string palmName = isLeftHand ? "palm.01.L" : "palm.01.R";
            return hand.Find(palmName);
        }

        // vouchSourceBool is for if it's vouching to be a holdable for the left or right hand, depending on what CustomProperties key it originates from
        public void Equip(Holdable holdable, bool vouchSourceBool)
        {
            bool isLeftHand = bool.Parse(holdable.GetHoldableProp(Holdable.HoldablePropType.IsLeftHand).ToString());

            // Do not clone the holdable if the player is already using it (Left)
            if (vouchSourceBool && HP_HCurrentLeft != null && HP_HCurrentLeft == holdable)
                return;

            // Do not clone the holdable if the player is already using it (Right)
            if (!vouchSourceBool && HP_HCurrent != null && HP_HCurrent == holdable)
                return;

            // Remove our current object if it exists
            if (vouchSourceBool && HP_CurrentLeft != null)
                Unequip(false, true);
            else if (!vouchSourceBool || HP_Current != null)
                Unequip(false, false);

            if (isLeftHand != vouchSourceBool)
            {
                Logger.LogWarning("This holdable should not be vouching for this CustomProperty. Vouch: " + vouchSourceBool + " IsLeftHand: " + isLeftHand + " Holdable Name: " + holdable.GetHoldableProp(Holdable.HoldablePropType.Name).ToString());
                return;
            }

            // There's no real way to see if whatever holdable we're trying to pick up is in the left or right hand, so some
            // tests are going to be required to ensure this, such as renaming the .holdable file to be the actual holdable name
            string holdableName = holdable.BasePath;
            var clonedHoldable = Instantiate(holdable.HoldableObject);
            foreach (var component in clonedHoldable.GetComponentsInChildren<AudioSource>().Where(a => !Mathf.Approximately(1f, a.spatialBlend)).ToArray())
            {
                float maxVolume = 0.06f;
                if (component.name == "UsedBulletSoundEffect") maxVolume = 0.1f;

                component.spatialBlend = 1f;
                component.volume = Mathf.Min(component.volume, maxVolume);
            }

            var holdableParent = GetPalm(isLeftHand);
            clonedHoldable.transform.SetParent(holdableParent, false);
            clonedHoldable.SetActive(true);

            // Networked gun behaviors, 1:52 AM 7/2/2023
            if (clonedHoldable.transform.Find("UsedBulletGameObject") != null)
            {
                NetworkedGun gunHoldable = clonedHoldable.AddComponent<NetworkedGun>();
                gunHoldable.ReferenceRig = currentRig;
                gunHoldable.ReferenceHoldable = holdable;
                gunHoldable.Initalize();
            }

            if (vouchSourceBool)
            {
                HP_CurrentLeft = clonedHoldable;
                HP_HCurrentLeft = holdable;
                return;
            }
            HP_Current = clonedHoldable;
            HP_HCurrent = holdable;
        }

        public void Unequip(bool both, bool left)
        {
            if (HP_CurrentLeft != null && (both || left))
            {
                Destroy(HP_CurrentLeft);
                HP_HCurrentLeft = null;
            }
            if (HP_Current != null && (both || !left))
            {
                Destroy(HP_Current);
                HP_HCurrent = null;
            }
        }
    }
}
