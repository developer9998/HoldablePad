using UnityEngine;
using UnityEngine.UI;

namespace HoldablePad
{
    public class HoldableManager
    {
        static GameObject leftHandObject;
        static GameObject rightHandObject;

        public static void Load()
        {
            leftHandObject = new GameObject();
            rightHandObject = new GameObject();
            leftHandObject.transform.SetParent(GetPalm(true), false);
            rightHandObject.transform.SetParent(GetPalm(false), false);

            leftHandObject.transform.localPosition = Vector3.zero;
            leftHandObject.transform.localRotation = Quaternion.identity;
            leftHandObject.transform.localScale = Vector3.one;
            rightHandObject.transform.localPosition = Vector3.zero;
            rightHandObject.transform.localRotation = Quaternion.identity;
            rightHandObject.transform.localScale = Vector3.one;

            GameObject handhelds = HoldableCore.hahahah;

            for (int i = 0; i < handhelds.transform.childCount; i++)
            {
                GameObject handheldObject = Object.Instantiate(handhelds.transform.GetChild(i).gameObject);
                string handheldData = handheldObject.GetComponent<Text>().text;
                string[] handheldInfo = handheldData.Split('$');

                handheldObject.GetComponent<Text>().enabled = false; // hide pls
                handheldObject.name = handhelds.transform.GetChild(i).name;

                bool isLeft = handheldInfo[3] == "True";
                if (isLeft)
                {
                    handheldObject.transform.SetParent(leftHandObject.transform, false);
                }
                else
                {
                    handheldObject.transform.SetParent(rightHandObject.transform, false);
                }
                handheldObject.SetActive(false);
            }
        }

        public static void ShowName(string nam, bool isLeftHand)
        {
            if (isLeftHand)
            {
                for (int i = 0; i < leftHandObject.transform.childCount; i++)
                {
                    bool showOrNo = leftHandObject.transform.GetChild(i).gameObject.name == nam;
                    leftHandObject.transform.GetChild(i).gameObject.SetActive(showOrNo);
                }
            }
            else
            {
                for (int i = 0; i < rightHandObject.transform.childCount; i++)
                {
                    bool showOrNo = rightHandObject.transform.GetChild(i).gameObject.name == nam;
                    rightHandObject.transform.GetChild(i).gameObject.SetActive(showOrNo);
                }
            }
        }

        public static void IsEquipped(string theName)
        {
            HoldablePage.Instance.EQUIP = false;
            for (int i = 0; i < leftHandObject.transform.childCount; i++)
            {
                if (theName == leftHandObject.transform.GetChild(i).gameObject.name)
                {
                    if (leftHandObject.transform.GetChild(i).gameObject.activeInHierarchy == false)
                    {
                        HoldablePage.Instance.EQUIP = true;
                        HoldablePage.Instance.current = leftHandObject.transform.GetChild(i).gameObject.name;
                    }
                }
            }

            for (int i = 0; i < rightHandObject.transform.childCount; i++)
            {
                if (theName == rightHandObject.transform.GetChild(i).gameObject.name)
                {
                    if (rightHandObject.transform.GetChild(i).gameObject.activeInHierarchy == false)
                    {
                        HoldablePage.Instance.EQUIP = true;
                        HoldablePage.Instance.current = rightHandObject.transform.GetChild(i).gameObject.name;
                    }
                }
            }
        }

        public static void UnequipAll(bool isLeftHand)
        {
            if (isLeftHand)
            {
                for (int i = 0; i < leftHandObject.transform.childCount; i++)
                {
                    leftHandObject.transform.GetChild(i).gameObject.SetActive(false);
                }
            }
            else
            {
                for (int i = 0; i < rightHandObject.transform.childCount; i++)
                {
                    rightHandObject.transform.GetChild(i).gameObject.SetActive(false);
                }
            }
        }

        static Transform GetPalm(bool isLeftHand)
        {
            if (isLeftHand)
                return GorillaTagger.Instance.offlineVRRig.leftHandTransform.parent.Find("palm.01.L");
            else
                return GorillaTagger.Instance.offlineVRRig.rightHandTransform.parent.Find("palm.01.R");
        }
    }
}
