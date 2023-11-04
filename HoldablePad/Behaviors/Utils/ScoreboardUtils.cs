using GorillaExtensions;
using HoldablePad.Behaviors;
using HoldablePad.Behaviors.Networking;
using Photon.Realtime;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HoldablePad.Behaviors.Utils
{
    public static class ScoreboardUtils
    {
        public static readonly Dictionary<GorillaPlayerScoreboardLine, Player> cachedLines = new Dictionary<GorillaPlayerScoreboardLine, Player>();

        public static List<GorillaScoreBoard> GetAllScoreboards()
        {
            var uiParent = GorillaUIParent.instance.gameObject;
            return uiParent.GetComponentsInChildren<GorillaScoreBoard>(true).ToList();
        }

        public static void UpdateScoreboardHP(GorillaScoreBoard scoreboard)
        {
            if (scoreboard == null || scoreboard != null && scoreboard.lines.Count == 0) return;
            for (int i = 0; i < scoreboard.lines.Count; i++)
            {
                var currentLine = scoreboard.lines[i];
                try
                {
                    if (cachedLines.Count > 0 && cachedLines.ContainsKey(currentLine)) continue;

                    var currentLinePlayer = currentLine.linePlayer;
                    var currentLineRig = currentLine.playerVRRig;
                    if (currentLinePlayer == null || currentLineRig == null || currentLine.speakerIcon == null || currentLine.playerSwatch == null) continue;

                    if (currentLinePlayer.IsLocal)
                    {
                        cachedLines.Add(currentLine, currentLinePlayer);
                        CreateIcon(currentLine, out _);
                        continue;
                    }

                    if (currentLineRig.TryGetComponent(out Client refClient))
                    {
                        cachedLines.Add(currentLine, currentLinePlayer);
                        CreateIcon(currentLine, out GameObject tempSpeaker);

                        var iconTemp = currentLine.gameObject.GetOrAddComponent<NetworkedIcon>();
                        iconTemp.myIcon = tempSpeaker;
                        iconTemp.myClient = refClient;
                    }
                }
                catch { Logger.LogError("Error while attempting to generate HP icons for " + currentLine.playerNameValue.ToUpper()); }
            }
        }

        private static void CreateIcon(GorillaPlayerScoreboardLine line, out GameObject newSpeaker)
        {
            newSpeaker = Object.Instantiate(line.speakerIcon, line.speakerIcon.transform.parent);
            try
            {
                newSpeaker.SetActive(line.linePlayer.IsLocal);
                newSpeaker.GetComponent<SpriteRenderer>().sprite = Main.Instance.ScoreboardIcon;

                newSpeaker.transform.localPosition = line.playerSwatch.transform.localPosition.WithZ(-0.1f);
                newSpeaker.transform.localScale = Vector3.one * 7f;
            }
            catch { Logger.LogError("Error while attempting to generate HP icon for " + line.playerNameValue.ToUpper()); }
        }
    }
}
