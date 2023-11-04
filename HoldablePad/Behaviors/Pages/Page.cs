using UnityEngine;
using UnityEngine.UI;

namespace HoldablePad.Behaviors.Pages
{
    public class Page
    {
        public Transform Object;

        public Text Header;
        public Text Author;
        public Text Description;
        public Transform PreviewBase;

        public int SlotsActive;
        public RectTransform Slots;

        public void SetSlots(bool? AudioSlot, bool? CustomColourSlot, bool? LightSlot, bool? ParticlesSlot, bool? GunSlot, bool? VibrationsSlot, bool? Trail)
        {
            var array = new bool?[] { AudioSlot, CustomColourSlot, LightSlot, ParticlesSlot, GunSlot, VibrationsSlot, Trail };
            var itemLength = Slots.childCount;
            SlotsActive = 0;
            for (int i = 0; i < array.Length; i++)
            {
                var item = array[i];
                if (itemLength > i)
                {
                    Slots.GetChild(i).gameObject.SetActive(item != null && item.HasValue && item.Value == true);
                    if (Slots.GetChild(i).gameObject.activeSelf) SlotsActive++;
                }
            }
        }
    }
}
