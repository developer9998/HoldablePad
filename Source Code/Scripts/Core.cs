using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
using System.IO;
using System.Reflection;

namespace HoldablePad
{
    public class Core : MonoBehaviour
    {
        public static GameObject tabletObject;
        public static string[] files;
        public static GameObject hahahah;
        public static void LaunchHoldablePad()
        {
            Stream manifestResourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("HoldablePad.Resources.holdable");
            AssetBundle assetBundle = AssetBundle.LoadFromStream(manifestResourceStream);

            GameObject otherObject = new GameObject();
            otherObject.transform.position = new Vector3(0.022f, 0.079f, 0.028f);
            otherObject.transform.rotation = Quaternion.Euler(-80.107f, -92.353f, 9.068001f);
            otherObject.transform.localScale = new Vector3(0.158782f, 0.158782f, 0.158782f);
            otherObject.transform.SetParent(GetPalm(true), false);

            GameObject obj = assetBundle.LoadAsset<GameObject>("menu");

            tabletObject = Instantiate(obj);
            tabletObject.transform.position = Vector3.zero;
            tabletObject.transform.rotation = Quaternion.identity;
            tabletObject.transform.localScale = Vector3.one;
            tabletObject.transform.SetParent(otherObject.transform, false);

            hahahah = new GameObject();
            hahahah.name = "Holdable";
            tabletObject.AddComponent<PageSystem>();
            tabletObject.transform.Find("Pages/MainScreen/Canvas/LoadHoldables/GameObject").gameObject.layer = 18;
            tabletObject.transform.Find("Pages/MainScreen/Canvas/LoadHoldables/GameObject").gameObject.AddComponent<LoadButton>();

            string holdablePath = Path.Combine(Directory.GetCurrentDirectory().ToString(), "BepInEx", "Plugins", "HoldablePad", "CustomHoldables");

            if (!Directory.Exists(holdablePath))
            {
                Directory.CreateDirectory(holdablePath);
            }

            files = Directory.GetFiles(holdablePath, "*.holdable");
            string[] fileNames = new string[files.Length];

            for (int i = 0; i < fileNames.Length; i++)
            {
                fileNames[i] = Path.GetFileName(files[i]);
            }

            GameObject[] objects = new GameObject[files.Length];
            for (int i = 0; i < files.Length; i++)
            {
                AssetBundle holdable_bundle = AssetBundle.LoadFromFile(Path.Combine(holdablePath, fileNames[i]));
                GameObject assetbundle = holdable_bundle.LoadAsset<GameObject>("holdABLE");
                objects[i] = Instantiate(assetbundle);

                objects[i].transform.SetParent(hahahah.transform, false);
                objects[i].SetActive(false);

                holdable_bundle.Unload(false);

                string handheldData = objects[i].GetComponent<Text>().text;
                string[] handheldInfo = handheldData.Split('$');

                objects[i].GetComponent<Text>().enabled = false; // hide pls

                objects[i].name = handheldInfo[0] + handheldInfo[1] + handheldInfo[2] + handheldInfo[3] + fileNames[i];

                var audioSources = objects[i].transform.GetComponentsInChildren<AudioSource>();

                if (audioSources != null)
                {

                    foreach (var source in audioSources)
                    {
                        if (source.volume > 0.1f)
                        {
                            source.volume = 0.1f; // so its not loud ingame
                        }

                        if (objects[i].transform.Find("withSounds") == null) 
                        {
                            GameObject tinotin = new GameObject();
                            tinotin.name = "withSounds";
                            tinotin.transform.SetParent(objects[i].transform, false);
                        }
                    }
                }

                var colliders = objects[i].transform.GetComponentsInChildren<Collider>();

                if (colliders != null)
                {
                    foreach (var collider in colliders)
                    {
                        collider.enabled = false;
                        collider.gameObject.layer = 8; // ground
                    }
                }
            }

            HoldableManager.Load();
            otherObject.AddComponent<ToggleShit>();

        }

        static Transform GetPalm(bool isLeftHand)
        {
            if (isLeftHand)
                return GameObject.Find("OfflineVRRig/Actual Gorilla/rig/body/shoulder.L/upper_arm.L/forearm.L/hand.L/palm.01.L/").transform;
            else
                return GameObject.Find("OfflineVRRig/Actual Gorilla/rig/body/shoulder.R/upper_arm.R/forearm.R/hand.R/palm.01.R/").transform;
        }
    }

    public class ToggleShit : MonoBehaviour
    {
        bool buttonDown;
        bool canToggle = true;
        bool isActivate = false;
        GameObject objecto;

        void Start()
        {
            objecto = gameObject.transform.GetChild(0).gameObject;
            canToggle = true;
            isActivate = false;
        }

        void Update()
        {
            InputDevices.GetDeviceAtXRNode(XRNode.LeftHand).TryGetFeatureValue(CommonUsages.primaryButton, out buttonDown);
            
            if (buttonDown && canToggle)
            {
                canToggle = false;
                isActivate = !isActivate;
            }
            else if (!buttonDown && !canToggle)
            {
                canToggle = true;
            }
            objecto.SetActive(isActivate);
        }
    }
}
