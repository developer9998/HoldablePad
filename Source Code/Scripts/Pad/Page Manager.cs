using System.IO;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using CodeAnalysis = System.Diagnostics.CodeAnalysis;

namespace HoldablePad
{
    public class Page : MonoBehaviour
    {
        static GameObject pageObject;
        public static string[] files;
        static int Hahaha;
        public static bool EQUIP = true;
        public static string current = "";
        public static Color playerColour;
        public static bool canChangeColour = true;
        static bool LEFTHAND;

        void Start()
        {
            pageObject = gameObject.transform.Find("Pages").gameObject;
            GetColour(out float r, out float g, out float b);
            playerColour = new Color(r, g, b);
            canChangeColour = true;
        }

        static void GetColour(out float r, out float g, out float b)
        {
            r = PlayerPrefs.GetFloat("redValue");
            g = PlayerPrefs.GetFloat("greenValue");
            b = PlayerPrefs.GetFloat("blueValue");
        }

        public static void Load(int index)
        {
            Debug.Log("HoldablePad successfully launched");
            UpdatePad();
        }

        public static void ButtonPad(int index)
        {
            if (index == 0)
            {
                if (EQUIP)
                {
                    Manager.ShowName(Main.hahahah.transform.GetChild(Hahaha).gameObject.name, LEFTHAND);
                    current = Main.hahahah.transform.GetChild(Hahaha).gameObject.name;
                }
                else
                {
                    Manager.UnequipAll(LEFTHAND);
                    EQUIP = true;
                    current = "";
                    for (int i = 0; i < files.Length; i++)
                    {
                        Manager.IsEquipped(Main.hahahah.transform.GetChild(i).gameObject.name);
                    }
                }
            }

            else if (index == 1)
            {
                if (Hahaha > 0)
                {
                    Hahaha--;
                }
                else
                {
                    Hahaha = files.Length - 1;
                }

            }

            else if (index == 2)
            {
                if (Hahaha < files.Length - 1)
                {
                    Hahaha++;
                }
                else
                {
                    Hahaha = 0;
                }
            }

            else if (index == 3)
            {
                GetColour(out float r, out float g, out float b);
                playerColour = new Color(r, g, b);
                canChangeColour = true;
            }

            UpdatePad();
        }

        [CodeAnalysis.SuppressMessage("Style", "IDE0059:Unnecessary assignment of a value", Justification = "<Pending>")]
        static void UpdatePad()
        {
            int pageObjectIndex = 1;
            GameObject shit = pageObject.transform.GetChild(pageObjectIndex).Find("Previews/left/HOLDABLE").gameObject;

            for (int i = 0; i < shit.transform.childCount; i++)
            {
                Destroy(shit.transform.GetChild(i).gameObject);
            }

            GameObject shit2 = pageObject.transform.GetChild(pageObjectIndex).Find("Previews/right/HOLDABLE").gameObject;

            for (int i = 0; i < shit2.transform.childCount; i++)
            {
                Destroy(shit2.transform.GetChild(i).gameObject);
            }

            if (current != "")
            {
                Manager.IsEquipped(Main.hahahah.transform.GetChild(Hahaha).gameObject.name);
            }

            pageObject.transform.GetChild(pageObjectIndex).Find("Canvas/View (1)/GameObject").gameObject.layer = 18;
            pageObject.transform.GetChild(pageObjectIndex).Find("Canvas/Left (1)/GameObject").gameObject.layer = 18;
            pageObject.transform.GetChild(pageObjectIndex).Find("Canvas/Right (1)/GameObject").gameObject.layer = 18;
            pageObject.transform.GetChild(pageObjectIndex).Find("Canvas/COLOURSET/GameObject").gameObject.layer = 18;

            string holdablePath = Path.Combine(Directory.GetCurrentDirectory().ToString(), "BepInEx", "Plugins", "HoldablePad", "CustomHoldables");

            files = Directory.GetFiles(holdablePath, "*.holdable");

            if (files.Length != 0)
            {
                for (int i = 0; i < files.Length; i++)
                {
                    GameObject objec = Instantiate(Main.hahahah.transform.GetChild(i).gameObject);
                    objec.name = Main.hahahah.transform.GetChild(i).gameObject.name;
                    bool isThe = Hahaha == i;
                    objec.SetActive(isThe);

                    bool containsAudio = false;
                    bool containsGun = false;
                    bool containsLight = false;
                    bool containsParticles = false;
                    bool containsCustomColours = false;
                    bool containsVibrations = false;

                    int containsIndex = -1;
                    List<string> containsStringList = new List<string>();

                    AudioSource[] audioSources = objec.transform.GetComponentsInChildren<AudioSource>();

                    if (audioSources.Length != 0)
                    {
                        foreach (var source in audioSources)
                        {
                            source.playOnAwake = false;
                            source.Stop();
                        }
                    }

                    if (isThe)
                    {
                        ParticleSystem[] systems = objec.transform.GetComponentsInChildren<ParticleSystem>();
                        Light[] lights = objec.transform.GetComponentsInChildren<Light>();
                        if (lights.Length != 0)
                        {
                            foreach (Light ligh in lights)
                            {
                                ligh.enabled = false;
                                containsLight = true;
                            }
                        }
                        if (systems.Length != 0)
                        {
                            foreach (ParticleSystem system in systems)
                            {
                                ParticleSystem.EmissionModule em = system.emission;
                                containsParticles = true;
                            }
                        }

                        for (int E = 0; E < objec.transform.childCount; E++)
                        {
                            if (objec.transform.GetChild(E).name == "withSounds")
                            {
                                containsAudio = true;
                            }
                            if (objec.transform.GetChild(E).name == "UsedBulletGameObject")
                            {
                                containsAudio = false;
                                containsGun = true;
                                GameObject bulletObject = objec.transform.GetChild(E).gameObject;

                                string info_stream = bulletObject.GetComponent<Text>().text;
                                string[] hand_info = info_stream.Split('$');

                                for (int garilla = 0; garilla < hand_info.Length; garilla++) // for holdables without custom colour support
                                {
                                    if (garilla == 3)
                                    {
                                        if (hand_info[3] != null)
                                        {
                                            if (hand_info[3] == "True")
                                            {
                                                containsVibrations = true;
                                            }
                                        }
                                    }
                                }

                                objec.transform.GetChild(E).gameObject.SetActive(false);
                            }
                        }
                    }

                    string info_stream2 = objec.transform.GetComponent<Text>().text;
                    string[] hand_info2 = info_stream2.Split('$');

                    if (hand_info2[3] == "True")
                    {
                        objec.transform.SetParent(pageObject.transform.GetChild(pageObjectIndex).Find("Previews/left/HOLDABLE"), false);
                    }
                    else
                    {
                        objec.transform.SetParent(pageObject.transform.GetChild(pageObjectIndex).Find("Previews/right/HOLDABLE"), false);
                    }

                    if (isThe)
                    {
                        string info_stream = objec.transform.GetComponent<Text>().text;
                        string[] hand_info = info_stream.Split('$');

                        pageObject.transform.GetChild(pageObjectIndex).Find("Canvas/GameObject (1)").GetComponent<Text>().text = "Name: " + hand_info[0];
                        pageObject.transform.GetChild(pageObjectIndex).Find("Canvas/GameObject (2)").GetComponent<Text>().text = "Author: " + hand_info[1];
                        pageObject.transform.GetChild(pageObjectIndex).Find("Canvas/GameObject (3)").GetComponent<Text>().text = "Description: " + hand_info[2];
                        if (EQUIP)
                        {
                            pageObject.transform.GetChild(pageObjectIndex).Find("Canvas/View (1)/Text").GetComponent<Text>().text = $"{(hand_info[3] == "True" ? "Equip\n(Left Hand)" : "Equip\n(Right Hand)")}";
                        }
                        else
                        {
                            pageObject.transform.GetChild(pageObjectIndex).Find("Canvas/View (1)/Text").GetComponent<Text>().text = "Unequip";
                        }

                        if (hand_info[3] == "True")
                        {
                            LEFTHAND = true;
                            pageObject.transform.GetChild(pageObjectIndex).Find("Previews/left").gameObject.SetActive(true);
                            pageObject.transform.GetChild(pageObjectIndex).Find("Previews/right").gameObject.SetActive(false);
                        }
                        else
                        {
                            LEFTHAND = false;
                            pageObject.transform.GetChild(pageObjectIndex).Find("Previews/left").gameObject.SetActive(false);
                            pageObject.transform.GetChild(pageObjectIndex).Find("Previews/right").gameObject.SetActive(true);
                        }

                        pageObject.transform.GetChild(pageObjectIndex).Find("Canvas/COLOURSET").GetComponent<Image>().enabled = false;
                        pageObject.transform.GetChild(pageObjectIndex).Find("Canvas/COLOURSET").Find("GameObject").GetComponent<BoxCollider>().enabled = false;
                        pageObject.transform.GetChild(pageObjectIndex).Find("Canvas/COLOURSET").Find("Text").GetComponent<Text>().enabled = false;

                        for (int garilla = 0; garilla < hand_info.Length; garilla++) // for holdables without custom colour support
                        {
                            if (garilla == 4)
                            {
                                if (hand_info[4] != null)
                                {
                                    if (hand_info[4] == "True")
                                    {
                                        pageObject.transform.GetChild(pageObjectIndex).Find("Canvas/COLOURSET").GetComponent<Image>().enabled = true;
                                        pageObject.transform.GetChild(pageObjectIndex).Find("Canvas/COLOURSET").Find("GameObject").GetComponent<BoxCollider>().enabled = true;
                                        pageObject.transform.GetChild(pageObjectIndex).Find("Canvas/COLOURSET").Find("Text").GetComponent<Text>().enabled = true;
                                        containsCustomColours = true;
                                    }
                                }

                            }
                        }
                    }

                    if (containsAudio)
                    {
                        containsIndex++;
                        containsStringList.Add("AudioSource");
                    }

                    if (containsGun)
                    {
                        containsIndex++;
                        containsStringList.Add("Gun");
                    }

                    if (containsVibrations)
                    {
                        containsIndex++;
                        containsStringList.Add("Vibrate");
                    }

                    if (containsParticles)
                    {
                        containsIndex++;
                        containsStringList.Add("Particles");
                    }

                    if (containsLight)
                    {
                        containsIndex++;
                        containsStringList.Add("Light");
                    }

                    if (isThe && containsCustomColours)
                    {
                        containsIndex++;
                        containsStringList.Add("CustomColour");
                    }

                    if (isThe)
                    {
                        GameObject previewObject = pageObject.transform.GetChild(pageObjectIndex).Find("Canvas/INFOICON").gameObject;

                        for (int EASPORTS = 0; EASPORTS < previewObject.transform.childCount; EASPORTS++)
                        {
                            previewObject.transform.GetChild(EASPORTS).gameObject.SetActive(false);
                        }

                        for (int EASPORTS = 0; EASPORTS < containsStringList.Count; EASPORTS++)
                        {
                            //Debug.Log(containsStringList[EASPORTS]);
                            previewObject.transform.GetChild(EASPORTS).gameObject.SetActive(true);

                            Transform images = previewObject.transform.GetChild(EASPORTS).Find("Images");

                            for (int AESPORTS = 0; AESPORTS < images.childCount; AESPORTS++)
                            {
                                if (images.GetChild(AESPORTS).name == containsStringList[EASPORTS])
                                {
                                    images.GetChild(AESPORTS).gameObject.SetActive(true);
                                }
                                else
                                {
                                    images.GetChild(AESPORTS).gameObject.SetActive(false);
                                }
                            }

                        }
                    }

                }

                if (!pageObject.transform.GetChild(pageObjectIndex).Find("Canvas/Left (1)/GameObject").GetComponent<PageButton>())
                {
                    pageObject.transform.GetChild(pageObjectIndex).Find("Canvas/Left (1)/GameObject").gameObject.AddComponent<PageButton>().meaning = 1;
                }

                if (!pageObject.transform.GetChild(pageObjectIndex).Find("Canvas/Right (1)/GameObject").GetComponent<PageButton>())
                {
                    pageObject.transform.GetChild(pageObjectIndex).Find("Canvas/Right (1)/GameObject").gameObject.AddComponent<PageButton>().meaning = 2;
                }

                if (!pageObject.transform.GetChild(pageObjectIndex).Find("Canvas/View (1)/GameObject").GetComponent<PageButton>())
                {
                    pageObject.transform.GetChild(pageObjectIndex).Find("Canvas/View (1)/GameObject").gameObject.AddComponent<PageButton>().meaning = 0;
                }

                if (!pageObject.transform.GetChild(pageObjectIndex).Find("Canvas/COLOURSET/GameObject").GetComponent<PageButton>())
                {
                    pageObject.transform.GetChild(pageObjectIndex).Find("Canvas/COLOURSET/GameObject").gameObject.AddComponent<PageButton>().meaning = 3;
                }

            }
            else
            {
                pageObject.transform.GetChild(pageObjectIndex).Find("Canvas/GameObject (1)").GetComponent<Text>().text = "No files were found!";
                pageObject.transform.GetChild(pageObjectIndex).Find("Canvas/GameObject (2)").GetComponent<Text>().text = "What to do:";
                pageObject.transform.GetChild(pageObjectIndex).Find("Canvas/GameObject (3)").GetComponent<Text>().text = "Ensure you are putting the files in the right path.";
                pageObject.transform.GetChild(pageObjectIndex).Find("Canvas/View (1)/Text").GetComponent<Text>().text = "";

                if (pageObject.transform.GetChild(pageObjectIndex).Find("Canvas/Left (1)/GameObject").GetComponent<PageButton>())
                {
                    Destroy(pageObject.transform.GetChild(pageObjectIndex).Find("Canvas/Left (1)/GameObject").GetComponent<PageButton>());
                }

                if (pageObject.transform.GetChild(pageObjectIndex).Find("Canvas/Right (1)/GameObject").GetComponent<PageButton>())
                {
                    Destroy(pageObject.transform.GetChild(pageObjectIndex).Find("Canvas/Right (1)/GameObject").GetComponent<PageButton>());
                }

                if (pageObject.transform.GetChild(pageObjectIndex).Find("Canvas/View (1)/GameObject").GetComponent<PageButton>())
                {
                    Destroy(pageObject.transform.GetChild(pageObjectIndex).Find("Canvas/View (1)/GameObject").GetComponent<PageButton>());
                }

                pageObject.transform.GetChild(pageObjectIndex).Find("Previews/left").gameObject.SetActive(false);
                pageObject.transform.GetChild(pageObjectIndex).Find("Previews/right").gameObject.SetActive(false);
            }

            return;
        }
    }
}
