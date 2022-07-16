using UnityEngine;
using System.Collections;

namespace HoldablePad
{
    public class CustomColour : MonoBehaviour
    {
        Color colour;

        void Start()
        {
            StartCoroutine(setColour());
        }

        void Update()
        {
            if (Page.canChangeColour)
            {
                Page.canChangeColour = false;
                StartCoroutine(setColour());
            }
        }

        void Awake()
        {
            StartCoroutine(setColour());
        }

        IEnumerator setColour()
        {
            Renderer[] renderers = gameObject.transform.GetComponentsInChildren<Renderer>();
            if (renderers != null)
            {
                foreach (var renderer in renderers)
                {
                    for (int i = 0; i < renderer.materials.Length; i++)
                    {
                        renderer.materials[i].color = Page.playerColour;
                    }
                }
            }
            yield break;
        }

    }
}
