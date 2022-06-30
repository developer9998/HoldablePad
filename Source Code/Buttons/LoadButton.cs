using UnityEngine;

namespace HoldablePad
{
    public class LoadButton : MonoBehaviour
    {
        //public override void ButtonActivation()
        //{
        //    base.ButtonActivation();
        //    PageSystem.Load(1);
        //}

        void Start() // fuck it
        {
            PageSystem.Load(1);
        }
    }
}
