using UnityEngine;
using System.Collections;

namespace HoldablePad
{
    public class HoldableCustomColour : MonoBehaviour
    {
        Color colour;
        Color GetColour(float max)
        {
            float r = Mathf.Clamp(PlayerPrefs.GetFloat("redValue"), 0, max);
            float g = Mathf.Clamp(PlayerPrefs.GetFloat("greenValue"), 0, max);
            float b = Mathf.Clamp(PlayerPrefs.GetFloat("blueValue"), 0, max);
            return new Color(r, g, b, 1);
        }

        void Start()
        {
            StartCoroutine(SetColour());
        }

        public IEnumerator SetColour()
        {
            Renderer[] renderers = gameObject.transform.GetComponentsInChildren<Renderer>();
            if (renderers != null)
            {
                foreach (var renderer in renderers)
                {
                    for (int i = 0; i < renderer.materials.Length; i++)
                    {
                        renderer.materials[i].color = GetColour(1);
                    }
                }
            }
            yield break;
        }

    }
}
