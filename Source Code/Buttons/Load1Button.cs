namespace HoldablePad
{
    public class Load1Button : GorillaPressableButtonRightHand
    {
        public int meaning;
        public override void ButtonActivation()
        {
            base.ButtonActivation();
            PageSystem.LoadFor1(meaning);
        }
    }
}
