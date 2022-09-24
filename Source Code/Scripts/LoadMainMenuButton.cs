using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace HoldablePad
{
    public class LoadMainMenuButton : HoldableButton
    {
        readonly float[] colours = {0, 0, .2f, .2f, .4f, .4f, .6f, .6f, .8f, .8f, 1};
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
            HoldablePage.Instance.ButtonActivationBasedOnIndex(meaning);
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
