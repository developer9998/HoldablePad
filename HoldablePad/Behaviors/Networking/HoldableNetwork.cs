using ExitGames.Client.Photon;
using HoldablePad.Behaviors.Holdables;
using HoldablePad.Behaviors.Utils;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HoldablePad.Behaviors.Networking
{
    public class HoldableNetwork : MonoBehaviourPunCallbacks
    {
        public static HoldableNetwork Instance { get; private set; }
        public Main main;

        private Dictionary<Player, Client> Clients { get; } = new Dictionary<Player, Client>();

        public void Start()
            => Instance = this;

        public override async void OnJoinedRoom()
        {
            base.OnJoinedRoom();
            main.ApplyProps();

            await Task.Delay(400);
            int currentClientCount = Clients.Count;
            foreach (var player in PhotonNetwork.PlayerList)
            {
                if (player.IsLocal) continue;
                if (GorillaGameManager.instance != null && GorillaGameManager.instance.FindPlayerVRRig(player) is VRRig tempRig)
                {
                    Client tempClient = tempRig.gameObject.AddComponent<Client>();
                    tempClient.currentPlayer = player;
                    tempClient.currentRig = tempRig;
                    tempClient.enabled = true;
                    Clients.Add(player, tempClient);
                    CheckHoldables(tempClient, player.CustomProperties);
                }
            }

            await Task.Delay(250);
            if (currentClientCount != Clients.Count && ScoreboardUtils.GetAllScoreboards() is var scoreboards && scoreboards.Count > 0)
                scoreboards.ForEach(a => ScoreboardUtils.UpdateScoreboardHP(a));
        }

        public async override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
        {
            base.OnPlayerPropertiesUpdate(targetPlayer, changedProps);

            try
            {
                // This requires a GorillaGameManager, so if there isn't one then no networked holdables
                if (!targetPlayer.IsLocal && GorillaGameManager.instance != null && GorillaGameManager.instance.FindPlayerVRRig(targetPlayer) is VRRig tempRig)
                {
                    if (!tempRig.TryGetComponent(out Client tempClient))
                    {
                        // Creates a client, adds it to a dictionary, and checks it for holdables
                        tempClient = tempRig.gameObject.AddComponent<Client>();
                        tempClient.currentPlayer = targetPlayer;
                        tempClient.currentRig = tempRig;
                        tempClient.enabled = true;
                        Clients.Add(targetPlayer, tempClient);

                        await Task.Delay(50);
                        CheckHoldables(tempClient, changedProps);
                    }
                    else if (Clients.TryGetValue(targetPlayer, out tempClient))
                    {
                        // Checks the existing client for holdables
                        CheckHoldables(tempClient, changedProps);
                    }
                }
            }
            catch (Exception e)
            {
                Logger.LogError("Error with OnPlayerPropertiesUpdate: " + e.ToString());
            }
        }

        public void CheckHoldables(Client currentClient, Hashtable changedProps)
        {
            // Left holdable, still no real way to check if it is a left hand holdable without doing even more checks
            if (changedProps.TryGetValue("HP_Left", out object hpKeyLeft))
            {
                currentClient.IsHoldablePadUser = true;
                if (hpKeyLeft is string hpKeyLeftString)
                {
                    if (main.InitalizedHoldablesDict.TryGetValue(hpKeyLeftString, out Holdable value))
                        currentClient.Equip(value, true); // Equip the holdable we found from our local basepath records
                    else
                        currentClient.Unequip(false, true); // If the key we have can't be found in our local basepath records, unequip our current holdable (make multiplayer sync)
                }
                else
                    currentClient.Unequip(false, true); // If the key isn't a string, unequip our current holdable (attempted bypass or hack??)
            }

            // Right holdable, still no real way to check if it is a right hand holdable without doing even more checks
            if (changedProps.TryGetValue("HP_Right", out object hpKeyRight))
            {
                currentClient.IsHoldablePadUser = true;
                if (hpKeyRight is string hpKeyLeftString)
                {
                    if (main.InitalizedHoldablesDict.TryGetValue(hpKeyLeftString, out Holdable value))
                        currentClient.Equip(value, false); // Equip the holdable we found from our local basepath records
                    else
                        currentClient.Unequip(false, false); // If the key we have can't be found in our local basepath records, unequip our current holdable (make multiplayer sync)
                }
                else
                    currentClient.Unequip(false, false); // If the key isn't a string, unequip our current holdable (attempted bypass or hack??)
            }
        }

        public override void OnLeftRoom()
        {
            base.OnLeftRoom();

            if (!GorillaTagger.Instance.offlineVRRig.isQuitting)
            {
                ScoreboardUtils.cachedLines.Clear();
                Clients.Values.ToList().ForEach(x => Destroy(x)); // Remove all the clients
                Clients.Clear();
            }
        }

        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            base.OnPlayerLeftRoom(otherPlayer);

            if (Clients.ContainsKey(otherPlayer))
            {
                // https://stackoverflow.com/a/2444064/21090957
                var myKey = ScoreboardUtils.cachedLines.FirstOrDefault(x => x.Value == otherPlayer).Key;
                ScoreboardUtils.cachedLines.Remove(myKey);

                Destroy(Clients[otherPlayer]);
                Clients.Remove(otherPlayer);
            }
        }

        public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
        {
            base.OnRoomPropertiesUpdate(propertiesThatChanged);
        }
    }
}
