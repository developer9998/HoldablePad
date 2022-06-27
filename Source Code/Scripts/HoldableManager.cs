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

            GameObject handhelds = Core.hahahah;

            for (int i = 0; i < handhelds.transform.childCount; i++)
            {
                GameObject handheldObject = Object.Instantiate(handhelds.transform.GetChild(i).gameObject);
                string handheldData = handheldObject.GetComponent<Text>().text;
                string[] handheldInfo = handheldData.Split('$');

                handheldObject.GetComponent<Text>().enabled = false; // hide pls
                handheldObject.name = handhelds.transform.GetChild(i).name;

                bool isLeft = handheldInfo[3] == "true";
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

        public static void ShowName(string nam)
        {
            for (int i = 0; i < rightHandObject.transform.childCount; i++)
            {
                bool showOrNo = rightHandObject.transform.GetChild(i).gameObject.name == nam;
                rightHandObject.transform.GetChild(i).gameObject.SetActive(showOrNo);
            }

            for (int i = 0; i < leftHandObject.transform.childCount; i++)
            {
                bool showOrNo = leftHandObject.transform.GetChild(i).gameObject.name == nam;
                leftHandObject.transform.GetChild(i).gameObject.SetActive(showOrNo);
            }
        }

        public static void UnequipAll()
        {
            for (int i = 0; i < rightHandObject.transform.childCount; i++)
            {
                rightHandObject.transform.GetChild(i).gameObject.SetActive(false);
            }

            for (int i = 0; i < leftHandObject.transform.childCount; i++)
            {
                leftHandObject.transform.GetChild(i).gameObject.SetActive(false);
            }
        }

        static Transform GetPalm(bool isLeftHand)
        {
            if (isLeftHand)
                return GameObject.Find("OfflineVRRig/Actual Gorilla/rig/body/shoulder.L/upper_arm.L/forearm.L/hand.L/palm.01.L/").transform;
            else
                return GameObject.Find("OfflineVRRig/Actual Gorilla/rig/body/shoulder.R/upper_arm.R/forearm.R/hand.R/palm.01.R/").transform;
        }
    }
}
