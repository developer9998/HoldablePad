using UnityEngine;

namespace HoldablePad
{
    public class LoadMainMenu : MonoBehaviour
    {
        //public override void ButtonActivation()
        //{
        //    base.ButtonActivation();
        //    PageSystem.Load(1);
        //}

        void Start() // fuck it
        {
            HoldablePage.Instance.Load(1);
        }
    }
}
