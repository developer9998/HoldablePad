using UnityEngine;
using System.Collections;

namespace HoldablePad
{
    public class CustomColour : MonoBehaviour
    {
        Renderer[] renderers;


        void Start()
        {
            renderers = gameObject.transform.GetComponentsInChildren<Renderer>();
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
        }

        void Update()
        {
            if (renderers != null)
            {
                foreach (var renderer in renderers)
                {
                    for (int i = 0; i < renderer.materials.Length; i++)
                    {
                        if (renderer.materials[i].color != Page.playerColour)
                        {
                            renderer.materials[i].color = Page.playerColour;
                        }
                    }
                }
            }
        }
    }
}
