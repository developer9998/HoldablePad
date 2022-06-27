namespace HoldablePad
{
    public class LoadButton : GorillaPressableButtonRightHand
    {
        public override void ButtonActivation()
        {
            base.ButtonActivation();
            PageSystem.Load(1);
        }
    }
}
