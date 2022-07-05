using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace HoldablePad
{
    public class LoadMainMenuButton : HoldableButton
    {
        readonly float[] colours = { 0f, 0.1f, 0.2f, 0.3f, 0.4f, 0.5f, 0.6f, 0.7f, 0.8f, 0.9f, 1f };
        public int meaning;
        Image image;

        void Start()
        {
            debounceTime = 0.25f;
            image = gameObject.transform.parent.GetComponent<Image>();
        }

        public override void ButtonActivation()
        {
            base.ButtonActivation();
            StartCoroutine(ButtonAnimation());
            HoldablePage.LoadFor1(meaning);
        }

        IEnumerator ButtonAnimation()
        {
            image.color = Color.red;
            for (int i = 0; i < colours.Length; i++)
            {
                image.color = new Color(1, 1 * colours[i], 1 * colours[i], 1);
                yield return new WaitForSeconds(debounceTime / colours.Length);
            }
            image.color = Color.white;
            yield break;
        }
    }
}
