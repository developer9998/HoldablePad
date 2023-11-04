using HoldablePad.Behaviors;
using HoldablePad.Behaviors.Networking;
using System.Collections.Generic;
using UnityEngine;

namespace HoldablePad.Behaviors.Holdables
{
    public class CustomColour : MonoBehaviour
    {
        public List<Material> ColourCheckMaterial = new List<Material>();
        public Client ReferenceRig;
        private Color currentColour;

        public void Start()
        {
            if (ReferenceRig == null)
                currentColour = Patches.PlayerColour;
        }

        public void FixedUpdate()
        {
            if (ColourCheckMaterial.Count == 0) return;

            // I was going to use Gradients for this, but decided not to due to issues I kept on finding. I'll probably try to pull it off in the next release, 7:41 AM 7/2/2023
            Color lerpOrigin = currentColour;
            currentColour = Color.Lerp(lerpOrigin, Patches.PlayerColour, Constants.CustomColourLerp * Time.unscaledDeltaTime);

            // Custom colour for other clients (11:30 AM, 7/12/2023)
            if (ReferenceRig != null)
            {
                currentColour = Color.Lerp(lerpOrigin, ReferenceRig.currentColour, Constants.CustomColourLerp * Time.unscaledDeltaTime);
                ColourCheckMaterial.ForEach(a => a.color = currentColour);
                return;
            }

            ColourCheckMaterial.ForEach(a => a.color = currentColour);
        }

        public void OnDestroy()
        {
            if (ColourCheckMaterial.Count == 0) return;
            ColourCheckMaterial.ForEach(a => a.color = Color.white);
        }
    }
}
