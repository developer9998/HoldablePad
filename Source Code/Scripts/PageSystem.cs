using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Reflection;

namespace HoldablePad
{
    public class PageSystem : MonoBehaviour
    {
        static GameObject pageObject;
        public static string[] files;
        static int Hahaha;
        static bool EQUIP = true;
        static string current = "";
        void Start()
        {
            pageObject = gameObject.transform.Find("Pages").gameObject;
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
                    HoldableManager.ShowName(Core.hahahah.transform.GetChild(Hahaha).gameObject.name);
                    current = Core.hahahah.transform.GetChild(Hahaha).gameObject.name;
                    AAAA(1);
                }
                else
                {
                    HoldableManager.UnequipAll();
                    current = "";
                    EQUIP = true;
                    AAAA(1);
                }
            }
        }

        static void AAAA(int indie)
        {
            if (indie == 1)
            {
                GameObject shit = pageObject.transform.GetChild(indie).Find("PUT").gameObject;

                for (int i = 0; i < shit.transform.childCount; i++)
                {
                    Destroy(shit.transform.GetChild(i).gameObject);
                }

                if (current != "")
                {
                    if (current == Core.hahahah.transform.GetChild(Hahaha).gameObject.name)
                    {
                        EQUIP = false;
                    }
                    else
                    {
                        EQUIP = true;
                    }
                }

                pageObject.transform.GetChild(indie).Find("Canvas/View (1)/GameObject").gameObject.layer = 18;
                pageObject.transform.GetChild(indie).Find("Canvas/Left (1)/GameObject").gameObject.layer = 18;
                pageObject.transform.GetChild(indie).Find("Canvas/Right (1)/GameObject").gameObject.layer = 18;

                string holdablePath = Path.Combine(Directory.GetCurrentDirectory().ToString(), "BepInEx", "Plugins", "HoldablePad", "CustomHoldables");

                files = Directory.GetFiles(holdablePath, "*.holdable");
                
                if (files.Length != 0)
                {
                    for (int i = 0; i < files.Length; i++)
                    {
                        GameObject objec = Instantiate(Core.hahahah.transform.GetChild(i).gameObject);
                        objec.transform.position = Vector3.zero;
                        objec.name = Core.hahahah.transform.GetChild(i).gameObject.name;
                        objec.transform.rotation = Quaternion.identity;
                        objec.transform.localScale = Vector3.one * 1.5f;
                        objec.transform.SetParent(pageObject.transform.GetChild(indie).Find("PUT"), false);
                        bool isThe = Hahaha == i;
                        objec.SetActive(isThe);

                        if (isThe)
                        {
                            string info_stream = objec.transform.GetComponent<Text>().text;
                            string[] hand_info = info_stream.Split('$');

                            pageObject.transform.GetChild(indie).Find("Canvas/GameObject (1)").GetComponent<Text>().text = "NAME: " + hand_info[0];
                            pageObject.transform.GetChild(indie).Find("Canvas/GameObject (2)").GetComponent<Text>().text = "AUTHOR: " + hand_info[1];
                            pageObject.transform.GetChild(indie).Find("Canvas/GameObject (3)").GetComponent<Text>().text = "DESCRIPTION: " + hand_info[2];
                            if (EQUIP)
                            {
                                pageObject.transform.GetChild(indie).Find("Canvas/View (1)/Text").GetComponent<Text>().text = $"{(hand_info[3] == "true" ? "Equip   (Left Hand)" : "Equip  (Right Hand)")}";
                            }
                            else
                            {
                                pageObject.transform.GetChild(indie).Find("Canvas/View (1)/Text").GetComponent<Text>().text = "Unequip";
                            }

                        }
                    }

                    if (!pageObject.transform.GetChild(indie).Find("Canvas/Left (1)/GameObject").GetComponent<Load1Button>())
                    {
                        pageObject.transform.GetChild(indie).Find("Canvas/Left (1)/GameObject").gameObject.AddComponent<Load1Button>().meaning = 1;
                    }

                    if (!pageObject.transform.GetChild(indie).Find("Canvas/Right (1)/GameObject").GetComponent<Load1Button>())
                    {
                        pageObject.transform.GetChild(indie).Find("Canvas/Right (1)/GameObject").gameObject.AddComponent<Load1Button>().meaning = 2;
                    }

                    if (!pageObject.transform.GetChild(indie).Find("Canvas/View (1)/GameObject").GetComponent<Load1Button>())
                    {
                        pageObject.transform.GetChild(indie).Find("Canvas/View (1)/GameObject").gameObject.AddComponent<Load1Button>().meaning = 0;
                    }

                }
                else
                {
                    pageObject.transform.GetChild(indie).Find("Canvas/GameObject (1)").GetComponent<Text>().text = "NAME: " + "ERROR";
                    pageObject.transform.GetChild(indie).Find("Canvas/GameObject (2)").GetComponent<Text>().text = "AUTHOR: " + "ERROR";
                    pageObject.transform.GetChild(indie).Find("Canvas/GameObject (3)").GetComponent<Text>().text = "DESCRIPTION: " + "ERROR";
                    pageObject.transform.GetChild(indie).Find("Canvas/View (1)/Text").GetComponent<Text>().text = "ERROR";

                    if (pageObject.transform.GetChild(indie).Find("Canvas/Left (1)/GameObject").GetComponent<Load1Button>())
                    {
                        Destroy(pageObject.transform.GetChild(indie).Find("Canvas/Left (1)/GameObject").GetComponent<Load1Button>());
                    }

                    if (pageObject.transform.GetChild(indie).Find("Canvas/Right (1)/GameObject").GetComponent<Load1Button>())
                    {
                        Destroy(pageObject.transform.GetChild(indie).Find("Canvas/Right (1)/GameObject").GetComponent<Load1Button>());
                    }

                    if (pageObject.transform.GetChild(indie).Find("Canvas/View (1)/GameObject").GetComponent<Load1Button>())
                    {
                        Destroy(pageObject.transform.GetChild(indie).Find("Canvas/View (1)/GameObject").GetComponent<Load1Button>());
                    }
                }

                return;
            }
        }
    }
}
