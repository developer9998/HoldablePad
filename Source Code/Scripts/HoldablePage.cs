using UnityEngine;
using UnityEngine.UI;
using System.IO;
using ComputerInterface;
using System.Reflection;

namespace HoldablePad
{
    public class HoldablePage : MonoBehaviour
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
            playerColour = BaseGameInterface.GetColor();
            canChangeColour = true;
        }
        public static void Load(int index)
        {
            for (int i = 0; i < pageObject.transform.childCount; i++)
            {
                bool hide = i == index;
                pageObject.transform.GetChild(i).gameObject.SetActive(hide);
                AAAA(i);
                if (hide)
                {
                    return;
                }
            }
        }
        public static void LoadFor1(int index)
        {
            if (index == 1)
            {
                if (Hahaha > 0)
                {
                    Hahaha--;
                }
                AAAA(1);

            }
            else
            if (index == 2)
            {
                if (Hahaha < files.Length - 1)
                {
                    Hahaha++;
                }
                AAAA(1);
            }
            else if (index == 0)
            {
                if (EQUIP)
                {
                    HoldableManager.ShowName(HoldableCore.hahahah.transform.GetChild(Hahaha).gameObject.name, LEFTHAND);
                    current = HoldableCore.hahahah.transform.GetChild(Hahaha).gameObject.name;
                    AAAA(1);
                }
                else
                {
                    HoldableManager.UnequipAll(LEFTHAND);
                    EQUIP = true;
                    current = "";
                    for (int i = 0; i < files.Length; i++)
                    {
                        HoldableManager.IsEquipped(HoldableCore.hahahah.transform.GetChild(i).gameObject.name);
                    }
                    AAAA(1);
                }
            }
            else if (index == 3)
            {
                playerColour = BaseGameInterface.GetColor();
                canChangeColour = true;
                AAAA(1);
            }
            else if (index == 100)
            {
                AAAA(1);
            }
        }

        static void AAAA(int indie)
        {
            if (indie == 1)
            {
                GameObject shit = pageObject.transform.GetChild(indie).Find("Previews/left/HOLDABLE").gameObject;

                for (int i = 0; i < shit.transform.childCount; i++)
                {
                    Destroy(shit.transform.GetChild(i).gameObject);
                }

                GameObject shit2 = pageObject.transform.GetChild(indie).Find("Previews/right/HOLDABLE").gameObject;

                for (int i = 0; i < shit2.transform.childCount; i++)
                {
                    Destroy(shit2.transform.GetChild(i).gameObject);
                }

                if (current != "")
                {
                    HoldableManager.IsEquipped(HoldableCore.hahahah.transform.GetChild(Hahaha).gameObject.name);
                }

                pageObject.transform.GetChild(indie).Find("Canvas/View (1)/GameObject").gameObject.layer = 18;
                pageObject.transform.GetChild(indie).Find("Canvas/Left (1)/GameObject").gameObject.layer = 18;
                pageObject.transform.GetChild(indie).Find("Canvas/Right (1)/GameObject").gameObject.layer = 18;
                pageObject.transform.GetChild(indie).Find("Canvas/COLOURSET/GameObject").gameObject.layer = 18;
                //pageObject.transform.GetChild(indie).Find("Canvas/COLOURSET").gameObject.SetActive(false);

                string holdablePath = Path.Combine(Directory.GetCurrentDirectory().ToString(), "BepInEx", "Plugins", "HoldablePad", "CustomHoldables");

                files = Directory.GetFiles(holdablePath, "*.holdable");
                
                if (files.Length != 0)
                {
                    for (int i = 0; i < files.Length; i++)
                    {
                        GameObject objec = Instantiate(HoldableCore.hahahah.transform.GetChild(i).gameObject);
                        objec.name = HoldableCore.hahahah.transform.GetChild(i).gameObject.name;
                       // objec.transform.SetParent(pageObject.transform.GetChild(indie).Find("PUT"), false);
                        bool isThe = Hahaha == i;
                        objec.SetActive(isThe);

                        AudioSource[] audioSources = objec.transform.GetComponentsInChildren<AudioSource>();
                        ParticleSystem[] systems = objec.transform.GetComponentsInChildren<ParticleSystem>();
                        Light[] lights = objec.transform.GetComponentsInChildren<Light>();

                        if (isThe)
                        {
                            pageObject.transform.GetChild(indie).Find("Canvas/SOURCE").GetComponent<Text>().enabled = false;
                            for (int E = 0; E < objec.transform.childCount; E++)
                            {
                                if (objec.transform.GetChild(E).name == "withSounds")
                                {
                                    //Debug.Log("Has sounds");
                                    pageObject.transform.GetChild(indie).Find("Canvas/SOURCE").GetComponent<Text>().enabled = true;
                                }
                            }
                        }

                        if (audioSources.Length != 0)
                        {
                            foreach (var source in audioSources)
                            {
                                source.playOnAwake = false;
                                source.Stop();
                            }
                        }

                        if (systems.Length != 0)
                        {
                            foreach (ParticleSystem system in systems)
                            {
                                ParticleSystem.EmissionModule em = system.emission;
                                //em.enabled = false;
                            }
                        }

                        if (lights.Length != 0)
                        {
                            foreach (Light ligh in lights)
                            {
                                ligh.enabled = false;
                            }
                        }

                        string info_stream2 = objec.transform.GetComponent<Text>().text;
                        string[] hand_info2 = info_stream2.Split('$');

                        if (hand_info2[3] == "True")
                        {
                            objec.transform.SetParent(pageObject.transform.GetChild(indie).Find("Previews/left/HOLDABLE"), false);
                        }
                        else
                        {
                            objec.transform.SetParent(pageObject.transform.GetChild(indie).Find("Previews/right/HOLDABLE"), false);
                        }


                        if (isThe)
                        {
                            string info_stream = objec.transform.GetComponent<Text>().text;
                            string[] hand_info = info_stream.Split('$');

                            pageObject.transform.GetChild(indie).Find("Canvas/GameObject (1)").GetComponent<Text>().text = "Name: " + hand_info[0];
                            pageObject.transform.GetChild(indie).Find("Canvas/GameObject (2)").GetComponent<Text>().text = "Author: " + hand_info[1];
                            pageObject.transform.GetChild(indie).Find("Canvas/GameObject (3)").GetComponent<Text>().text = "Description: " + hand_info[2];
                            if (EQUIP)
                            {
                                pageObject.transform.GetChild(indie).Find("Canvas/View (1)/Text").GetComponent<Text>().text = $"{(hand_info[3] == "True" ? "Equip   (Left Hand)" : "Equip  (Right Hand)")}";
                            }
                            else
                            {
                                pageObject.transform.GetChild(indie).Find("Canvas/View (1)/Text").GetComponent<Text>().text = "Unequip";
                            }

                            if (hand_info[3] == "True")
                            {
                                LEFTHAND = true;
                                pageObject.transform.GetChild(indie).Find("Previews/left").gameObject.SetActive(true);
                                pageObject.transform.GetChild(indie).Find("Previews/right").gameObject.SetActive(false);
                            }
                            else
                            {
                                LEFTHAND = false;
                                pageObject.transform.GetChild(indie).Find("Previews/left").gameObject.SetActive(false);
                                pageObject.transform.GetChild(indie).Find("Previews/right").gameObject.SetActive(true);
                            }
                            pageObject.transform.GetChild(indie).Find("Canvas/COLOURSET").GetComponent<Image>().enabled = false;
                            pageObject.transform.GetChild(indie).Find("Canvas/COLOURSET").Find("GameObject").GetComponent<BoxCollider>().enabled = false;
                            pageObject.transform.GetChild(indie).Find("Canvas/COLOURSET").Find("Text").GetComponent<Text>().enabled = false;

                            for (int garilla = 0; garilla < hand_info.Length; garilla++) // for holdables without custom colour support
                            {
                                if (garilla == 4)
                                {
                                    if (hand_info[4] != null)
                                    {
                                        if (hand_info[4] == "True")
                                        {
                                            pageObject.transform.GetChild(indie).Find("Canvas/COLOURSET").GetComponent<Image>().enabled = true;
                                            pageObject.transform.GetChild(indie).Find("Canvas/COLOURSET").Find("GameObject").GetComponent<BoxCollider>().enabled = true;
                                            pageObject.transform.GetChild(indie).Find("Canvas/COLOURSET").Find("Text").GetComponent<Text>().enabled = true;
                                        }
                                    }

                                }
                            }

                        }
                    }

                    if (!pageObject.transform.GetChild(indie).Find("Canvas/Left (1)/GameObject").GetComponent<LoadMainMenuButton>())
                    {
                        pageObject.transform.GetChild(indie).Find("Canvas/Left (1)/GameObject").gameObject.AddComponent<LoadMainMenuButton>().meaning = 1;
                    }

                    if (!pageObject.transform.GetChild(indie).Find("Canvas/Right (1)/GameObject").GetComponent<LoadMainMenuButton>())
                    {
                        pageObject.transform.GetChild(indie).Find("Canvas/Right (1)/GameObject").gameObject.AddComponent<LoadMainMenuButton>().meaning = 2;
                    }

                    if (!pageObject.transform.GetChild(indie).Find("Canvas/View (1)/GameObject").GetComponent<LoadMainMenuButton>())
                    {
                        pageObject.transform.GetChild(indie).Find("Canvas/View (1)/GameObject").gameObject.AddComponent<LoadMainMenuButton>().meaning = 0;
                    }

                    if (!pageObject.transform.GetChild(indie).Find("Canvas/COLOURSET/GameObject").GetComponent<LoadMainMenuButton>())
                    {
                        pageObject.transform.GetChild(indie).Find("Canvas/COLOURSET/GameObject").gameObject.AddComponent<LoadMainMenuButton>().meaning = 3;
                    }

                }
                else
                {
                    pageObject.transform.GetChild(indie).Find("Canvas/GameObject (1)").GetComponent<Text>().text = "Error: No files found!";
                    pageObject.transform.GetChild(indie).Find("Canvas/GameObject (2)").GetComponent<Text>().text = "";
                    pageObject.transform.GetChild(indie).Find("Canvas/GameObject (3)").GetComponent<Text>().text = "";
                    pageObject.transform.GetChild(indie).Find("Canvas/View (1)/Text").GetComponent<Text>().text = "";

                    if (pageObject.transform.GetChild(indie).Find("Canvas/Left (1)/GameObject").GetComponent<LoadMainMenuButton>())
                    {
                        Destroy(pageObject.transform.GetChild(indie).Find("Canvas/Left (1)/GameObject").GetComponent<LoadMainMenuButton>());
                    }

                    if (pageObject.transform.GetChild(indie).Find("Canvas/Right (1)/GameObject").GetComponent<LoadMainMenuButton>())
                    {
                        Destroy(pageObject.transform.GetChild(indie).Find("Canvas/Right (1)/GameObject").GetComponent<LoadMainMenuButton>());
                    }

                    if (pageObject.transform.GetChild(indie).Find("Canvas/View (1)/GameObject").GetComponent<LoadMainMenuButton>())
                    {
                        Destroy(pageObject.transform.GetChild(indie).Find("Canvas/View (1)/GameObject").GetComponent<LoadMainMenuButton>());
                    }

                    pageObject.transform.GetChild(indie).Find("Previews/left").gameObject.SetActive(false);
                    pageObject.transform.GetChild(indie).Find("Previews/right").gameObject.SetActive(false);
                }

                return;
            }
        }
    }
}
