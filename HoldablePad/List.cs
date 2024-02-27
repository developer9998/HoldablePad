using System.Collections.Generic;

namespace HoldablePad
{
    public class List
    {
        public string BaseObjectPath { get; private set; }

        public List(string baseObjectPath, bool isHidden)
        {
            BaseObjectPath = baseObjectPath;
            HoldablesHidden = isHidden;
        }

        public List(string baseObjectPath)
            => BaseObjectPath = baseObjectPath;

        public int CurrentIndex;
        public List<Page> MenuPages = new List<Page>();

        public bool ForceDocked;
        public bool HoldablesDocked;
        public bool HoldablesHidden;

        public float HoldableHeight
            => HoldablesDocked || ForceDocked ? -23.5f : !HoldablesHidden ? 17.1f : 57.3f;

        public bool HoldableSupported
            => !HoldablesDocked && !ForceDocked && !HoldablesHidden;

        public bool IsFiltered;
        public List<Page> FilteredPages = new List<Page>();
    }
}
