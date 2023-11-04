using System.Linq;
using UnityEngine;

namespace HoldablePad.Behaviors.Holdables
{
    public class Holdable
    {
        public GameObject HoldableObject;
        public string[] Properties;

        public GameObject PreviewObject;
        public GameObject FavouriteObject;
        public GameObject InstantiatedObject;

        public string BasePath;

        public enum HoldablePropType
        {
            Name,
            Author,
            Description,
            IsLeftHand,
            UtilizeCustomColour
        }

        public object GetHoldableProp(int type)
            => GetHoldableProp((HoldablePropType)type);

        public object GetHoldableProp(HoldablePropType type)
        {
            if (Properties.ElementAtOrDefault((int)type) == null) return null;
            return Properties[(int)type];
        }
    }
}
