using ExitGames.Client.Photon;
using GorillaExtensions;
using HoldablePad.Behaviours.Holdables;
using HoldablePad.Behaviours.Networking;
using HoldablePad.Behaviours.Pages;
using HoldablePad.Utils;
using Photon.Pun;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.XR;

namespace HoldablePad.Behaviours
{
    public class Main : MonoBehaviourPunCallbacks
    {
        public static Main Instance { get; private set; }
        public bool Initalized { get; private set; }

        public AssetBundle MainResourceBundle { get; private set; }

        /// <summary>
        /// If the pad can be opened
        /// </summary>
        private bool Pad_CanOpen = true;
        private bool Pad_ThumbPress;

        /// <summary>
        /// The index for the current HoldableList we're working with
        /// </summary>
        private int CurrentHoldableListItem;
        private readonly List<List> HoldableList = new List<List>();

        /// <summary>
        /// A holdable item we have equipped
        /// </summary>
        public Holdable CurrentHandheldL, CurrentHandheldR;

        /// <summary>
        /// The pad object
        /// </summary>
        public GameObject HoldablePadHandheld;

        /// <summary>
        /// The main menu transform
        /// </summary>
        private Transform MMObject;

        public Vector3 HPHandheldTargetPosition;
        public Quaternion HPHandheldTargetRotation;

        /// <summary>
        /// Used to determine pages for the pad
        /// </summary>
        public enum ScreenModes
        {
            InitalInfo,
            HoldableView,
            ConfigView,
            InfoView,
            FavouriteView
        }

        /// <summary>
        /// The current page used for the pad
        /// </summary>
        public ScreenModes CurrentScreenMode { get; private set; }

        /// <summary>
        /// The current transform we're attempting to focus on
        /// </summary>
        public Transform ScrollFocus;

        /// <summary>
        /// The array of transforms we're currently attempting to dock
        /// </summary>
        public Transform[] ScrollDocked = new Transform[3];

        /// <summary>
        /// The page used for configuration/settings
        /// </summary>
        public ConfigPage ConfigPage;

        /// <summary>
        /// The page displaying how to use the favourites system
        /// </summary>
        public Page NoHoldablesPage;

        /// <summary>
        /// The list of loaded holdables
        /// </summary>
        public List<Holdable> InitalizedHoldables = new List<Holdable>();

        /// <summary>
        /// A dictionary with the base path of the holdable with the holdable. Used for networking
        /// </summary>
        public Dictionary<string, Holdable> InitalizedHoldablesDict = new Dictionary<string, Holdable>();

        /// <summary>
        /// The AudioSource used to play audio on the pad
        /// </summary>
        public AudioSource PadSource { get; private set; }

        /// <summary>
        /// A sound used on the pad
        /// </summary>
        public AudioClip Open, Swap, Equip;

        /// <summary>
        /// An input device used to determine inputs in VR
        /// </summary>
        public static InputDevice HeadDevice, LeftHandDevice, RightHandDevice;

        /// <summary>
        /// The icon used on the scoreboard for HP users
        /// </summary>
        public Sprite ScoreboardIcon;

        public async void Start()
        {
            Instance = this;
            Initalized = false;
            gameObject.GetOrAddComponent<HoldableNetwork>().main = this;

            var mainList = new List("UI/BasePage");
            var favouriteList = new List("UI/FavPage", true);
            HoldableList.Add(mainList);
            HoldableList.Add(favouriteList);

            HeadDevice = InputDevices.GetDeviceAtXRNode(XRNode.Head);
            LeftHandDevice = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
            RightHandDevice = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);

            string holdablePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Holdables");
            if (!Directory.Exists(holdablePath))
                await WriteHoldables(true);

            var directoryInfo = new DirectoryInfo(holdablePath);
            FileInfo[] holdableFiles = directoryInfo.GetFiles(Constants.File_External);

            if (holdableFiles.Length == 0)
                await WriteHoldables(false);

            // Load the holdable objects
            float currentTime = Time.unscaledTime;
            foreach (FileInfo holdableFile in holdableFiles)
            {
                var holdableBundle = await AssetUtils.LoadFromFile(Path.Combine(holdablePath, holdableFile.Name));
                if (holdableBundle == null) continue;

                var holdableAsset = await AssetUtils.LoadAsset<GameObject>(holdableBundle, "holdABLE");
                holdableBundle.Unload(false);

                holdableAsset.name = holdableFile.Name;
                var holdableString = holdableAsset.GetComponent<Text>().text.Split('$');

                Holdable localHoldable = new Holdable
                {
                    HoldableObject = holdableAsset,
                    Properties = holdableString,
                    BasePath = holdableFile.Name
                };
                InitalizedHoldables.Add(localHoldable);
                InitalizedHoldablesDict.Add(localHoldable.BasePath, localHoldable);
                Destroy(holdableAsset.GetComponent<Text>());

                // Remove collision since there have been plently of cases where holdables have had them
                var holdableColliders = holdableAsset.GetComponentsInChildren<Collider>();
                if (holdableColliders.Length > 0) holdableColliders.ToList().ForEach(c => c.enabled = false);

                // Remove directional lights
                if (holdableAsset.GetComponentsInChildren<Light>().Where(a => a.type == LightType.Directional).ToList() is var lightList && lightList.Count > 0)
                    lightList.ForEach(a => a.enabled = false);

                HP_Log.Log(string.Concat("Loaded holdable ", localHoldable.GetHoldableProp(0), " by ", localHoldable.GetHoldableProp(1)));
                await Task.Yield();
            }
            int currentHoldableLength = InitalizedHoldables.Count;
            List<Holdable> HoldablesToRemove = new List<Holdable>();

            // Go through the holdables we just initalized 
            foreach (Holdable holdable in InitalizedHoldables)
            {
                try
                {
                    bool isLeftHand = bool.Parse(holdable.GetHoldableProp(Holdable.HoldablePropType.IsLeftHand).ToString());
                    string holdableName = holdable.BasePath;

                    var localPlayer = GorillaTagger.Instance.offlineVRRig;

                    var clonedHoldable = Instantiate(holdable.HoldableObject);
                    foreach (var component in clonedHoldable.GetComponentsInChildren<AudioSource>().Where(a => a.spatialBlend != 1).ToArray())
                    {
                        float maxVolume = 0.06f;
                        if (component.name == "UsedBulletSoundEffect") maxVolume = 0.1f;

                        component.spatialBlend = 1f;
                        component.volume = Mathf.Min(component.volume, maxVolume);
                    }

                    var holdableParent = PlayerUtils.GetPalm(isLeftHand);
                    clonedHoldable.transform.SetParent(holdableParent, false);
                    clonedHoldable.SetActive(false);

                    // Gun behaviors, 1:42 AM 6/2/2023
                    if (clonedHoldable.transform.Find("UsedBulletGameObject") != null)
                    {
                        HoldableGun gunHoldable = clonedHoldable.AddComponent<HoldableGun>();
                        gunHoldable.ReferenceHoldable = holdable;
                        gunHoldable.Initalize();
                    }
                    holdable.InstantiatedObject = clonedHoldable;
                }
                catch (Exception e)
                {
                    HP_Log.LogError("Exception (" + e.GetType().Name + ") while trying to create handheld object for " + holdable.GetHoldableProp(Holdable.HoldablePropType.Name));
                    HoldablesToRemove.Add(holdable);
                }
            }

            if (HoldablesToRemove.Count > 0)
            {
                HoldablesToRemove.ForEach(a =>
                {
                    InitalizedHoldables.Remove(a);
                    InitalizedHoldablesDict.Remove(a.BasePath);
                });
                HoldablesToRemove.Clear();
            }

            MainResourceBundle = await AssetUtils.LoadFromStream(Constants.Asset_Resource);
            Open = await AssetUtils.LoadAsset<AudioClip>(MainResourceBundle, "HP_Open");
            Swap = await AssetUtils.LoadAsset<AudioClip>(MainResourceBundle, "HP_Swap");
            Equip = await AssetUtils.LoadAsset<AudioClip>(MainResourceBundle, "HP_Equip");

            HoldablePadHandheld = Instantiate(await AssetUtils.LoadAsset<GameObject>(MainResourceBundle, "HoldablePad_Parent"));
            ScoreboardIcon = await AssetUtils.LoadAsset<Sprite>(MainResourceBundle, "HPLowIcon");
            PadSource = HoldablePadHandheld.GetComponent<AudioSource>();
            SetPadTheme(HP_Config.CurrentTheme.Value);

            var holdableTransform = HoldablePadHandheld.transform;
            List<Material> CC_Materials = new List<Material>();

            foreach (Holdable holdable in InitalizedHoldables)
            {
                var newPageObject = Instantiate(holdableTransform.Find(HoldableList[0].BaseObjectPath).gameObject, holdableTransform.Find(HoldableList[0].BaseObjectPath).parent);
                newPageObject.name = holdable.GetHoldableProp(Holdable.HoldablePropType.Name).ToString();

                var newPage = new Page
                {
                    Object = newPageObject.transform,
                    Header = newPageObject.transform.Find("Header").GetComponent<Text>(),
                    Author = newPageObject.transform.Find("Author").GetComponent<Text>(),
                    Description = newPageObject.transform.Find("Content").GetComponent<Text>(),
                    Slots = newPageObject.transform.Find("Items").GetComponent<RectTransform>(),
                    PreviewBase = newPageObject.transform.Find("HandheldLocation").GetComponent<RectTransform>()
                };

                try
                {
                    HoldableList[0].MenuPages.Add(newPage);

                    BoxCollider favouriteButton = newPageObject.transform.GetComponentInChildren<BoxCollider>(true);
                    favouriteButton.gameObject.AddComponent<Button>().CurrentPage = Button.ButtonPage.Favourite;

                    var holdablePreviewObject = Instantiate(holdable.HoldableObject);
                    holdable.PreviewObject = holdablePreviewObject;

                    bool isLeftHand = bool.Parse(holdable.GetHoldableProp(Holdable.HoldablePropType.IsLeftHand).ToString());
                    Transform handTransform = isLeftHand ? newPage.PreviewBase.Find("LeftExporter") : newPage.PreviewBase.Find("RightExporter");
                    holdablePreviewObject.transform.SetParent(handTransform, false);

                    var componentTypes = new List<Type>() { typeof(ParticleSystem), typeof(TrailRenderer), typeof(AudioSource), typeof(Light), typeof(VideoPlayer), typeof(LODGroup), typeof(AudioListener) };
                    var componentsList = holdablePreviewObject.GetComponentsInChildren(typeof(Component)).Where(a => componentTypes.Contains(a.GetType()));
                    foreach (var component in componentsList) Destroy(component);

                    newPage.Header.text = holdable.GetHoldableProp(Holdable.HoldablePropType.Name).ToString();
                    newPage.Author.text = string.Concat("Holdable by ", holdable.GetHoldableProp(Holdable.HoldablePropType.Author).ToString());
                    newPage.Description.text = holdable.GetHoldableProp(Holdable.HoldablePropType.Description).ToString();

                    bool isLeft = bool.Parse(holdable.GetHoldableProp(Holdable.HoldablePropType.IsLeftHand).ToString());
                    holdableTransform.Find("UI/ButtonTextMiddle").GetComponent<Text>().text = HoldableUtils.IsEquipped(holdable) ? "Unequip" : $"Equip ({(isLeft ? "Left" : "Right")})";

                    object colourProp = holdable.GetHoldableProp(Holdable.HoldablePropType.UtilizeCustomColour);
                    bool customColour = colourProp != null && bool.Parse(colourProp.ToString());

                    bool isGun = holdable.InstantiatedObject.GetComponentInChildren<HoldableGun>() != null;
                    bool gunHaptic = isGun && holdable.InstantiatedObject.GetComponentInChildren<HoldableGun>().ReferenceGun.VibrationModule;

                    var baseObject = holdable.HoldableObject;

                    var audioSources = baseObject.GetComponentsInChildren<AudioSource>();
                    audioSources = audioSources.Length > 0 ? audioSources.Where(a => a.name != "UsedBulletSoundEffect").ToArray() : audioSources;

                    var lights = baseObject.GetComponentsInChildren<Light>();
                    lights = lights.Length > 0 ? lights.Where(a => a.type != LightType.Directional).ToArray() : lights;

                    newPage.SetSlots(
                        audioSources.Length > 0,
                        customColour,
                        lights.Length > 0,
                        baseObject.GetComponentInChildren<ParticleSystem>(),
                        isGun,
                        gunHaptic,
                        baseObject.GetComponentInChildren<TrailRenderer>()
                    );

                    if (customColour)
                    {
                        var holdableRenderers = holdable.InstantiatedObject.GetComponentsInChildren<MeshRenderer>().ToList();
                        if (holdableRenderers.Count > 0 && holdableRenderers.Where(c => c.materials.Length > 0).ToList().Count > 0)
                        {
                            var holdableMaterials = new List<Material>();
                            holdableRenderers.ForEach(a => a.materials.ToList().ForEach(b => holdableMaterials.Add(b)));
                            holdableMaterials = holdableMaterials.Where(a => a.HasProperty("_Color") || a.HasProperty("_Glow")).ToList();
                            holdableMaterials.ForEach(a => CC_Materials.Add(a));
                        }
                    }

                    if (newPage.SlotsActive > 0)
                    {
                        newPage.PreviewBase.localPosition = new Vector3(0f, -18.4f, -0.41f);
                        newPage.PreviewBase.localScale = Vector3.one * 24.97094f;
                    }
                    else
                    {
                        newPage.PreviewBase.localPosition = new Vector3(0f, -25.8f, -0.41f);
                        newPage.PreviewBase.localScale = Vector3.one * 33.27129f;
                    }

                    var currentHand = isLeft ? newPage.PreviewBase.Find("PreviewObject/metarig/hand_L") : newPage.PreviewBase.Find("PreviewObject/metarig/hand_R");
                    var hiddenHand = !isLeft ? newPage.PreviewBase.Find("PreviewObject/metarig/hand_L") : newPage.PreviewBase.Find("PreviewObject/metarig/hand_R");
                    currentHand.transform.localScale = Vector3.one; hiddenHand.transform.localScale = Vector3.zero;
                }
                catch (Exception e)
                {
                    HP_Log.LogError("Exception (" + e.GetType().Name + ") while trying to create preview object for " + holdable.GetHoldableProp(Holdable.HoldablePropType.Name));
                    Destroy(newPageObject);

                    HoldableList[0].MenuPages.Remove(newPage);
                    HoldablesToRemove.Add(holdable);
                }
            }
            // Do this after we've setup our favourites buttons
            SetPadHand(HP_Config.CurrentHand.Value == HP_Config.HandPosition.LeftHand);
            SetFavoruiteButtonSide(HP_Config.CurrentHand.Value == HP_Config.HandPosition.LeftHand);

            if (HoldablesToRemove.Count > 0)
            {
                HoldablesToRemove.ForEach(a =>
                {
                    InitalizedHoldables.Remove(a);
                    InitalizedHoldablesDict.Remove(a.BasePath);
                });
                HoldablesToRemove.Clear();
            }

            // Favourites, 10:04 PM 7/3/2023
            foreach (Holdable holdable in InitalizedHoldables)
            {
                var newPageObject = Instantiate(holdableTransform.Find(HoldableList[1].BaseObjectPath).gameObject, holdableTransform.Find(HoldableList[1].BaseObjectPath).parent);
                newPageObject.name = holdable.GetHoldableProp(Holdable.HoldablePropType.Name).ToString();

                var newPage = new Page
                {
                    Object = newPageObject.transform,
                    Header = newPageObject.transform.Find("Header").GetComponent<Text>(),
                    Author = newPageObject.transform.Find("Author").GetComponent<Text>(),
                    Description = newPageObject.transform.Find("Content").GetComponent<Text>(),
                    Slots = newPageObject.transform.Find("Items").GetComponent<RectTransform>(),
                    PreviewBase = newPageObject.transform.Find("HandheldLocation").GetComponent<RectTransform>()
                };

                try
                {
                    HoldableList[1].MenuPages.Add(newPage);

                    var holdablePreviewObject = Instantiate(holdable.HoldableObject);
                    holdable.PreviewObject = holdablePreviewObject;

                    bool isLeftHand = bool.Parse(holdable.GetHoldableProp(Holdable.HoldablePropType.IsLeftHand).ToString());
                    Transform handTransform = isLeftHand ? newPage.PreviewBase.Find("LeftExporter") : newPage.PreviewBase.Find("RightExporter");
                    holdablePreviewObject.transform.SetParent(handTransform, false);

                    var componentTypes = new List<Type>() { typeof(ParticleSystem), typeof(TrailRenderer), typeof(AudioSource), typeof(Light), typeof(VideoPlayer), typeof(LODGroup), typeof(AudioListener) };
                    var componentsList = holdablePreviewObject.GetComponentsInChildren(typeof(Component)).Where(a => componentTypes.Contains(a.GetType()));
                    foreach (var component in componentsList) Destroy(component);

                    newPage.Header.text = holdable.GetHoldableProp(Holdable.HoldablePropType.Name).ToString();
                    newPage.Author.text = string.Concat("Holdable by ", holdable.GetHoldableProp(Holdable.HoldablePropType.Author).ToString());
                    newPage.Description.text = holdable.GetHoldableProp(Holdable.HoldablePropType.Description).ToString();

                    bool isLeft = bool.Parse(holdable.GetHoldableProp(Holdable.HoldablePropType.IsLeftHand).ToString());
                    holdableTransform.Find("UI/ButtonTextMiddle").GetComponent<Text>().text = HoldableUtils.IsEquipped(holdable) ? "Unequip" : $"Equip ({(isLeft ? "Left" : "Right")})";

                    object colourProp = holdable.GetHoldableProp(Holdable.HoldablePropType.UtilizeCustomColour);
                    bool customColour = colourProp != null && bool.Parse(colourProp.ToString());

                    bool isGun = holdable.InstantiatedObject.GetComponentInChildren<HoldableGun>() != null;
                    bool gunHaptic = isGun && holdable.InstantiatedObject.GetComponentInChildren<HoldableGun>().ReferenceGun.VibrationModule;

                    var baseObject = holdable.HoldableObject;

                    var audioSources = baseObject.GetComponentsInChildren<AudioSource>();
                    audioSources = audioSources.Length > 0 ? audioSources.Where(a => a.name != "UsedBulletSoundEffect").ToArray() : audioSources;

                    var lights = baseObject.GetComponentsInChildren<Light>();
                    lights = lights.Length > 0 ? lights.Where(a => a.type != LightType.Directional).ToArray() : lights;

                    newPage.SetSlots(
                        audioSources.Length > 0,
                        customColour,
                        lights.Length > 0,
                        baseObject.GetComponentInChildren<ParticleSystem>(),
                        isGun,
                        gunHaptic,
                        baseObject.GetComponentInChildren<TrailRenderer>()
                    );

                    if (newPage.SlotsActive > 0)
                    {
                        newPage.PreviewBase.localPosition = new Vector3(0f, -18.4f, -0.41f);
                        newPage.PreviewBase.localScale = Vector3.one * 24.97094f;
                    }
                    else
                    {
                        newPage.PreviewBase.localPosition = new Vector3(0f, -25.8f, -0.41f);
                        newPage.PreviewBase.localScale = Vector3.one * 33.27129f;
                    }

                    var currentHand = isLeft ? newPage.PreviewBase.Find("PreviewObject/metarig/hand_L") : newPage.PreviewBase.Find("PreviewObject/metarig/hand_R");
                    var hiddenHand = !isLeft ? newPage.PreviewBase.Find("PreviewObject/metarig/hand_L") : newPage.PreviewBase.Find("PreviewObject/metarig/hand_R");
                    currentHand.transform.localScale = Vector3.one; hiddenHand.transform.localScale = Vector3.zero;
                }
                catch (Exception e)
                {
                    HP_Log.LogError("Exception (" + e.GetType().Name + ") while trying to create favourite object for " + holdable.GetHoldableProp(Holdable.HoldablePropType.Name));
                    Destroy(newPageObject);

                    HoldableList[1].MenuPages.Remove(newPage);
                    HoldablesToRemove.Add(holdable);
                }
            }
            HoldableList[1].IsFiltered = true;

            if (HoldablesToRemove.Count > 0)
            {
                HoldablesToRemove.ForEach(a =>
                {
                    InitalizedHoldables.Remove(a);
                    InitalizedHoldablesDict.Remove(a.BasePath);
                });
                HoldablesToRemove.Clear();
            }

            gameObject.AddComponent<CustomColour>().ColourCheckMaterial = CC_Materials;
            HoldableList.ForEach(a => Destroy(holdableTransform.Find(a.BaseObjectPath).gameObject));

            ConfigPage = new ConfigPage();
            MMObject = holdableTransform.Find("UI/MainMenuPage");
            ConfigPage.Object = holdableTransform.Find("UI/ConfigPage");

            NoHoldablesPage = new Page { Object = holdableTransform.Find("UI/NoFavPage") };
            NoHoldablesPage.Object.gameObject.SetActive(true);
            NoHoldablesPage.Object.localScale = Vector3.zero;
            SetDock(NoHoldablesPage.Object, 0);

            var configSectionCount = ConfigPage.Object.childCount;
            for (int i = 0; i < configSectionCount; i++)
            {
                var configSection = ConfigPage.Object.GetChild(i);
                if (!configSection.name.StartsWith("HP_Config")) continue;

                Button.ButtonPage currentPage = configSection.name == "ConfigHeldHand" ? Button.ButtonPage.ConfigHand : configSection.name == "ConfigHandSwap" ? Button.ButtonPage.ConfigSwap : Button.ButtonPage.ConfigTheme;
                ConfigPage.configSections.Add(configSection);

                var buttonObjects = configSection.GetComponentsInChildren<BoxCollider>(true);
                foreach (var button in buttonObjects)
                {
                    var buttonComp = button.gameObject.GetOrAddComponent<Button>();
                    buttonComp.CurrentPage = currentPage;
                    ConfigPage.buttons.Add(buttonComp);
                }
            }

            ConfigPage.LoadConfig();
            foreach (var button in HoldablePadHandheld.GetComponentsInChildren<BoxCollider>(false).ToList())
            {
                if (button.gameObject.GetComponent<Button>() != null) continue;
                button.gameObject.AddComponent<Button>();
            }
            HoldablePadHandheld.SetActive(false);

            ConfigPage.Object.localScale = Vector3.zero;
            SetDock(ConfigPage.Object, 1);

            foreach (var listItem in HoldableList)
            {
                for (int i = 0; i < listItem.MenuPages.Count; i++)
                {
                    var menuPage = listItem.MenuPages[i];
                    bool isMyPage = i == listItem.CurrentIndex;

                    if (isMyPage)
                    {
                        menuPage.Object.localPosition = new Vector3(0f, -25.3f, 0f);
                        menuPage.Object.localScale = Vector3.zero;
                    }
                    else
                    {
                        bool isNextPage = i == (listItem.CurrentIndex + 1) % listItem.MenuPages.Count;
                        bool isPrevPage = i == (listItem.CurrentIndex <= 0 ? listItem.MenuPages.Count - 1 : listItem.CurrentIndex - 1);
                        Vector3 localPosition = isPrevPage ? new Vector3(-43.5f, 17.1f, 0f) : isNextPage ? new Vector3(43.5f, 17.1f, 0f) : new Vector3(0f, 17.1f, 0f);
                        menuPage.Object.localPosition = localPosition;
                        menuPage.Object.localScale = Vector3.zero;
                    }
                    menuPage.Object.gameObject.SetActive(false);
                }
            }

            var previousLeftHoldable = HP_Config.CurrentHoldableLeft.Value;
            if (InitalizedHoldablesDict.TryGetValue(previousLeftHoldable, out Holdable tempLeftHoldable))
            {
                bool isLeftPrevious = bool.Parse(tempLeftHoldable.GetHoldableProp(Holdable.HoldablePropType.IsLeftHand).ToString());
                if (isLeftPrevious)
                {
                    HoldItem(tempLeftHoldable);
                    HP_Log.Log("Equipped current holdable for left hand:" + previousLeftHoldable);
                }
            }

            var previousRightHoldable = HP_Config.CurrentHoldableRight.Value;
            if (InitalizedHoldablesDict.TryGetValue(previousRightHoldable, out Holdable tempRightHoldable))
            {
                bool isRightPrevious = !bool.Parse(tempRightHoldable.GetHoldableProp(Holdable.HoldablePropType.IsLeftHand).ToString());
                if (isRightPrevious)
                {
                    HoldItem(tempRightHoldable);
                    HP_Log.Log("Equipped current holdable for right hand:" + previousRightHoldable);
                }
            }

            var favouriteJoined = HP_Config.FavouriteHoldables.Value;
            if (favouriteJoined != "None" && favouriteJoined.Contains(";"))
            {
                var split = favouriteJoined.Split(';');
                foreach (var item in split)
                {
                    if (!InitalizedHoldablesDict.TryGetValue(item, out Holdable tempHoldable)) continue;
                    int pageIndex = InitalizedHoldables.IndexOf(tempHoldable);
                    HoldableList[0].MenuPages[pageIndex].Object.GetComponentsInChildren<Button>(true).First(a => a.CurrentPage == Button.ButtonPage.Favourite).ButtonPressMethod(true);
                    HoldableList[1].FilteredPages.Add(HoldableList[1].MenuPages[pageIndex]);
                }
            }

            Initalized = true;
            MainResourceBundle.Unload(false);

            HoldablePadHandheld.transform.Find("UI/BackPage/Flag").gameObject.SetActive(currentHoldableLength != InitalizedHoldables.Count);
            HoldablePadHandheld.transform.Find("UI/BackPage/FlagText").GetComponent<Text>().text = currentHoldableLength == InitalizedHoldables.Count ? "" : Mathf.Abs(currentHoldableLength - InitalizedHoldables.Count).ToString();

            HP_Log.Write("");
            HP_Log.Log("> " + Constants.Name + " (" + Constants.Version + ") was " + (currentHoldableLength == InitalizedHoldables.Count ? "fully loaded" : "loaded with issues"));
            HP_Log.Log(" > Mod loaded in " + Mathf.RoundToInt((Time.unscaledTime - currentTime) / 0.01f) * 0.01f + " seconds");
            HP_Log.Log(" > " + InitalizedHoldables.Count + "/" + currentHoldableLength + " holdables were loaded");
            HP_Log.Write("");
        }

        /// <summary>
        /// Unity method for update, used to handle inputs for the pad
        /// </summary>
        public void Update()
        {
            if (!Initalized) return;

            bool thumbPressed = HP_Config.CurrentHand.Value == HP_Config.HandPosition.LeftHand ? ControllerInputPoller.instance.leftControllerPrimaryButton : ControllerInputPoller.instance.rightControllerPrimaryButton;
            bool swapThumbPressed = HP_Config.CurrentHand.Value == HP_Config.HandPosition.RightHand ? ControllerInputPoller.instance.leftControllerPrimaryButton : ControllerInputPoller.instance.rightControllerPrimaryButton;

            if (swapThumbPressed && Pad_CanOpen && HP_Config.SwapHands.Value)
            {
                SetFavoruiteButtonSide(HP_Config.CurrentHand.Value != HP_Config.HandPosition.LeftHand);
                SetPadHand(HP_Config.CurrentHand.Value != HP_Config.HandPosition.LeftHand);
                HP_Config.OverwriteHand(HP_Config.CurrentHand.Value == HP_Config.HandPosition.LeftHand ? HP_Config.HandPosition.RightHand : HP_Config.HandPosition.LeftHand);

                Pad_ThumbPress = true;
                if (!HoldablePadHandheld.activeSelf)
                {
                    HoldablePadHandheld.transform.localPosition = HPHandheldTargetPosition;
                    HoldablePadHandheld.transform.localRotation = HPHandheldTargetRotation;
                }

                ForceActivate();
                return;
            }

            if (thumbPressed && Pad_ThumbPress != thumbPressed && Pad_CanOpen) ActivateMenu();
            Pad_ThumbPress = thumbPressed;
        }

        /// <summary>
        /// Unity method for fixedupdate, used to handle all the visuals
        /// </summary>
        public void FixedUpdate()
        {
            if (!Initalized) return;

            // Was going to be lerped but nevermind (7/1/2023 7:23 PM)
            HoldablePadHandheld.transform.localPosition = HPHandheldTargetPosition;
            HoldablePadHandheld.transform.localRotation = HPHandheldTargetRotation;

            foreach (var dockedItem in ScrollDocked)
            {
                if (dockedItem == null) continue;
                Vector3 localPosition = new Vector3(0f, 57.3f, 0f);
                dockedItem.localPosition = Vector3.Lerp(dockedItem.localPosition, localPosition, Constants.PageLerp * Time.unscaledDeltaTime);
                dockedItem.localScale = Vector3.Lerp(dockedItem.localScale, Vector3.zero, Constants.PageLerp * Time.unscaledDeltaTime);
            }

            if (ScrollFocus != null)
            {
                ScrollFocus.localPosition = Vector3.Lerp(ScrollFocus.localPosition, Vector3.up * 17.1f, Constants.PageLerp * Time.unscaledDeltaTime);
                ScrollFocus.localScale = Vector3.Lerp(ScrollFocus.localScale, Vector3.one, Constants.PageLerp * Time.unscaledDeltaTime);
            }

            foreach (var listItem in HoldableList)
            {
                if (listItem.IsFiltered)
                {
                    for (int i = 0; i < listItem.FilteredPages.Count; i++)
                    {
                        var menuP = listItem.FilteredPages[i];
                        var basePage = menuP as Page; // For HP_Config pages
                        if (CurrentScreenMode != ScreenModes.InitalInfo)
                        {
                            bool isMyPage = i == listItem.CurrentIndex;
                            if (isMyPage)
                            {
                                basePage.Object.localPosition = Vector3.Lerp(basePage.Object.localPosition, new Vector3(0f, listItem.HoldableHeight, 0f), Constants.PageLerp * Time.unscaledDeltaTime);
                                basePage.Object.localScale = Vector3.Lerp(basePage.Object.localScale, listItem.HoldableSupported ? Vector3.one : Vector3.zero, Constants.PageLerp * Time.unscaledDeltaTime);
                            }
                            else
                            {
                                bool isNextPage = i == (listItem.CurrentIndex + 1) % listItem.FilteredPages.Count && listItem.FilteredPages.Count > 2;
                                bool isPrevPage = i == (listItem.CurrentIndex == 0 ? listItem.FilteredPages.Count - 1 : listItem.CurrentIndex - 1) && listItem.FilteredPages.Count > 2;
                                Vector3 localPosition = isPrevPage ? new Vector3(-43.5f, listItem.HoldableHeight, 0f) : isNextPage ? new Vector3(43.5f, listItem.HoldableHeight, 0f) : new Vector3(0f, listItem.HoldableHeight, 0f);
                                basePage.Object.localPosition = Vector3.Lerp(basePage.Object.localPosition, localPosition, Constants.PageLerp * Time.unscaledDeltaTime);
                                basePage.Object.localScale = Vector3.Lerp(basePage.Object.localScale, isNextPage || isPrevPage ? Vector3.zero : Vector3.zero, Constants.PageLerp * Time.unscaledDeltaTime);
                            }
                            basePage.Object.gameObject.SetActive(basePage.Object.localScale.y > 0.03f);
                            continue;
                        }
                        basePage.Object.gameObject.SetActive(false);
                    }
                    continue;
                }
                for (int i = 0; i < listItem.MenuPages.Count; i++)
                {
                    var menuP = listItem.MenuPages[i];
                    var basePage = menuP as Page; // For HP_Config pages
                    if (CurrentScreenMode != ScreenModes.InitalInfo)
                    {
                        bool isMyPage = i == listItem.CurrentIndex;
                        if (isMyPage)
                        {
                            basePage.Object.localPosition = Vector3.Lerp(basePage.Object.localPosition, new Vector3(0f, listItem.HoldableHeight, 0f), Constants.PageLerp * Time.unscaledDeltaTime);
                            basePage.Object.localScale = Vector3.Lerp(basePage.Object.localScale, listItem.HoldableSupported ? Vector3.one : Vector3.zero, Constants.PageLerp * Time.unscaledDeltaTime);
                        }
                        else
                        {
                            bool isNextPage = i == (listItem.CurrentIndex + 1) % listItem.MenuPages.Count && listItem.MenuPages.Count > 2;
                            bool isPrevPage = i == (listItem.CurrentIndex == 0 ? listItem.MenuPages.Count - 1 : listItem.CurrentIndex - 1) && listItem.MenuPages.Count > 2;
                            Vector3 localPosition = isPrevPage ? new Vector3(-43.5f, listItem.HoldableHeight, 0f) : isNextPage ? new Vector3(43.5f, listItem.HoldableHeight, 0f) : new Vector3(0f, listItem.HoldableHeight, 0f);
                            basePage.Object.localPosition = Vector3.Lerp(basePage.Object.localPosition, localPosition, Constants.PageLerp * Time.unscaledDeltaTime);
                            basePage.Object.localScale = Vector3.Lerp(basePage.Object.localScale, isNextPage || isPrevPage ? Vector3.zero : Vector3.zero, Constants.PageLerp * Time.unscaledDeltaTime); // Just in case I decide to do something else with it I guess 7/1/2023 5:57 PM
                        }
                        basePage.Object.gameObject.SetActive(basePage.Object.localScale.y > 0.03f);
                        continue;
                    }
                    basePage.Object.gameObject.SetActive(false);
                }
            }
        }

        public Task WriteHoldables(bool createDirectory)
        {
            string holdablePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Holdables");
            if (createDirectory)
                Directory.CreateDirectory(holdablePath);

            var resourceNames = Assembly.GetExecutingAssembly().GetManifestResourceNames();
            resourceNames = resourceNames.Where(a => a.EndsWith(Constants.File_Internal)).ToArray();

            resourceNames.ToList().ForEach(async a =>
            {
                var resourceLocation = a.Replace(Constants.Asset_Holdable, "");
                var resourceMemStr = new MemoryStream();
                using (Stream resourceStr = Assembly.GetExecutingAssembly().GetManifestResourceStream(a))
                {
                    await resourceStr.CopyToAsync(resourceMemStr);
                    var resourceBytes = resourceMemStr.ToArray();
                    File.WriteAllBytes(Path.Combine(holdablePath, resourceLocation), resourceBytes);
                }
                resourceMemStr.Close();
            });
            return Task.CompletedTask;
        }

        // "0 references" took me off guard here, lol. Both HoldItem and DropItem are invoked using a string. 8:29 AM 7/6/2023
        public void HoldItem(Holdable holdable)
        {
            if (holdable != null)
            {
                var isLeftHand = holdable.GetHoldableProp(Holdable.HoldablePropType.IsLeftHand);
                var leftHandBool = bool.Parse(isLeftHand.ToString());

                if (leftHandBool && CurrentHandheldL != null)
                    DropItem(CurrentHandheldL);
                else if (!leftHandBool && CurrentHandheldR != null)
                    DropItem(CurrentHandheldR);

                var removedHoldables = InitalizedHoldables.Where(a => a != holdable && a.GetHoldableProp(Holdable.HoldablePropType.IsLeftHand) == isLeftHand).ToList();
                removedHoldables.ForEach(a => a.InstantiatedObject.SetActive(false));
                holdable.InstantiatedObject.SetActive(true);

                if (leftHandBool)
                {
                    CurrentHandheldL = holdable;
                    goto ApplyProperties;
                }
                CurrentHandheldR = holdable;

            ApplyProperties:
                if (PhotonNetwork.InRoom)
                    ApplyProps();
            }
        }

        public void DropItem(Holdable holdable)
        {
            if (HoldableUtils.IsEquipped(holdable))
            {
                var isLeftHand = holdable.GetHoldableProp(Holdable.HoldablePropType.IsLeftHand);
                holdable.InstantiatedObject.SetActive(false);

                if (bool.Parse(isLeftHand.ToString()))
                {
                    CurrentHandheldL = null;
                    goto ApplyProperties;
                }
                CurrentHandheldR = null;

            ApplyProperties:
                if (PhotonNetwork.InRoom)
                    ApplyProps();
            }
        }

        public void ApplyProps()
        {
            Hashtable customProps = new Hashtable()
                {
                    { "HP_Left", CurrentHandheldL == null ? "None" : CurrentHandheldL.BasePath },
                    { "HP_Right", CurrentHandheldR == null ? "None" : CurrentHandheldR.BasePath },
                };
            PhotonNetwork.LocalPlayer.SetCustomProperties(customProps);
        }

        #region Menu Pages

        /// <summary>
        /// Sets the pad in the user's hand
        /// </summary>
        /// <param name="isLeftHand">Determines if the pad will be placed in the left hand</param>
        public void SetPadHand(bool isLeftHand)
        {
            var holdableTransform = HoldablePadHandheld.transform;
            holdableTransform.SetParent(PlayerUtils.GetPalm(isLeftHand));

            if (isLeftHand)
            {
                HPHandheldTargetPosition = new Vector3(0.012f, 0.079f, 0.028f);
                HPHandheldTargetRotation = Quaternion.Euler(new Vector3(80.107f, 87.647f, 350.932f));
                holdableTransform.localScale = Vector3.one * 0.134f;
                return;
            }

            HPHandheldTargetPosition = new Vector3(-0.02846912f, 0.07900026f, -0.01084045f);
            HPHandheldTargetRotation = Quaternion.Euler(new Vector3(80.107f, 184.706f, 369.068f));
            holdableTransform.localPosition = HPHandheldTargetPosition;
            holdableTransform.localRotation = HPHandheldTargetRotation;
            holdableTransform.localScale = new Vector3(-0.134f, 0.134f, -0.134f);
            SetFavoruiteButtonSide(isLeftHand);
        }

        public void SetFavoruiteButtonSide(bool isLeftHand)
        {
            var favouriteButton = HoldablePadHandheld.GetComponentsInChildren<Button>(true).Where(a => a.CurrentPage == Button.ButtonPage.Favourite);
            favouriteButton.ToList().ForEach(a => a.transform.localPosition = isLeftHand ? new Vector3(39.8f, 41.84f, 3.5f) : new Vector3(-39.8f, 41.84f, 3.5f));
        }

        /// <summary>
        /// Sets the theme of the pad
        /// </summary>
        /// <param name="padTheme">The theme the pad will be set to</param>
        public void SetPadTheme(HP_Config.PadTheme padTheme)
        {
            var holdableTransform = HoldablePadHandheld.transform;
            MeshRenderer padRenderer = holdableTransform.GetChild(0).GetComponent<MeshRenderer>();

            var padMaterials = padRenderer.materials;
            var padColours = new List<Material>();

            for (int i = 0; i < padMaterials.Length; i++)
            {
                var currentMaterial = padMaterials[i];
                Vector3 texOffset = HoldableUtils.MainTexOffset(padTheme);
                if (i == 3)
                {
                    texOffset = HoldableUtils.MenuTexOffset(padTheme);
                    currentMaterial.mainTextureOffset = texOffset;
                    continue;
                }
                padColours.Add(currentMaterial);
                currentMaterial.mainTextureOffset = texOffset;
            }

            if (padColours.Count > 0)
            {
                if (padTheme == HP_Config.PadTheme.Fur || padTheme == HP_Config.PadTheme.Crystals)
                {
                    padRenderer.gameObject.GetOrAddComponent<CustomColour>().ColourCheckMaterial = padColours;
                    return;
                }
                Destroy(padRenderer.gameObject.GetOrAddComponent<CustomColour>());
                // I mean I guess it gets the job done (7/1/2023 6:33 PM)
            }
        }

        /// <summary>
        /// Resets a docked transform to the bottom of the menu
        /// </summary>
        /// <param name="index">The index of the dock that will be set</param>
        public void ResetDock(int index)
        {
            if (index > 2 || ScrollDocked[index] == null) return;
            ScrollDocked[index].localPosition = new Vector3(0f, -23.5f, 0f);
        }

        /// <summary>
        /// Resets all docked transforms to the bottom of the menu
        /// </summary>
        public void ResetDocks()
        {
            if (ScrollDocked.Length == 0) return;
            for (int i = 0; i < ScrollDocked.Length; i++) ResetDock(i);
        }

        /// <summary>
        /// Adds a docked transform
        /// </summary>
        /// <param name="dock">The dock transform that will be set with the index</param>
        /// <param name="index">The index of the dock that will be set</param>
        public void SetDock(Transform dock, int index)
        {
            if (index > 2) return;
            ScrollDocked[index] = dock;
        }

        /// <summary>
        /// Removes a docked transform
        /// </summary>
        /// <param name="index">The index of the dock that will be set</param>
        public void RemoveDock(int index)
        {
            if (index > 2 || ScrollDocked[index] == null) return;
            ScrollDocked[index] = null;
        }

        /// <summary>
        /// Sets the page of the pad. This determines docking, text, and other factors
        /// </summary>
        /// <param name="setScreenMode">The page the pad will be set to</param>
        public void SetPage(ScreenModes setScreenMode) // Polished, 5:14 AM, 7/4/2023
        {
            if (setScreenMode == CurrentScreenMode || setScreenMode == ScreenModes.HoldableView || setScreenMode == ScreenModes.InfoView && CurrentScreenMode == ScreenModes.InitalInfo)
            {
                Static.GlobalBtnCooldownTime = Time.unscaledTime;
                HoldableList.ForEach(a => a.ForceDocked = false);

                CurrentHoldableListItem = 0;
                HoldableList[0].HoldablesDocked = false;
                HoldableList[1].HoldablesHidden = true;

                var holdableTransform = HoldablePadHandheld.transform;
                SetDock(NoHoldablesPage.Object, 0);
                SetDock(ConfigPage.Object, 1);
                SetDock(holdableTransform.Find("UI/MainMenuPage"), 2);
                ScrollFocus = null;

                var currentList = HoldableList[CurrentHoldableListItem];
                var currentItem = currentList.MenuPages[currentList.CurrentIndex];
                currentItem = currentList.IsFiltered ? currentList.FilteredPages.Count == 0 ? currentItem : currentList.FilteredPages[currentList.CurrentIndex] : currentItem;
                var holdable = InitalizedHoldables[currentList.MenuPages.IndexOf(currentItem)];
                bool isLeft = bool.Parse(holdable.GetHoldableProp(Holdable.HoldablePropType.IsLeftHand).ToString());
                HoldablePadHandheld.transform.Find("UI/ButtonTextMiddle").GetComponent<Text>().text = HoldableUtils.IsEquipped(holdable) ? "Unequip" : $"Equip ({(isLeft ? "Left" : "Right")})";

                CurrentScreenMode = ScreenModes.HoldableView;
                return;
            }

            switch (setScreenMode)
            {
                case ScreenModes.ConfigView:
                    HoldableList.ForEach(a => a.ForceDocked = true);

                    if (CurrentScreenMode == ScreenModes.InfoView || CurrentScreenMode == ScreenModes.FavouriteView || CurrentScreenMode == ScreenModes.InitalInfo)
                        ResetDocks();

                    SetDock(NoHoldablesPage.Object, 0);
                    RemoveDock(1);
                    SetDock(MMObject, 2);
                    ScrollFocus = ConfigPage.Object;

                    ConfigPage.Object.gameObject.SetActive(true);
                    HoldablePadHandheld.transform.Find("UI/ButtonTextMiddle").GetComponent<Text>().text = "Apply";
                    break;
                case ScreenModes.InfoView: // same as InitalInfo
                    HoldableList.ForEach(a => a.ForceDocked = true);

                    if (CurrentScreenMode == ScreenModes.ConfigView || CurrentScreenMode == ScreenModes.FavouriteView)
                        ResetDocks();

                    SetDock(NoHoldablesPage.Object, 0);
                    SetDock(ConfigPage.Object, 1);
                    RemoveDock(2);
                    ScrollFocus = MMObject;

                    ConfigPage.Object.gameObject.SetActive(true);
                    HoldablePadHandheld.transform.Find("UI/ButtonTextMiddle").GetComponent<Text>().text = "Proceed";
                    break;
                case ScreenModes.InitalInfo: // same as InfoView
                    HoldableList.ForEach(a => a.ForceDocked = true);

                    if (CurrentScreenMode == ScreenModes.ConfigView || CurrentScreenMode == ScreenModes.FavouriteView)
                        ResetDocks();

                    SetDock(NoHoldablesPage.Object, 0);
                    SetDock(ConfigPage.Object, 1);
                    RemoveDock(2);
                    ScrollFocus = MMObject;

                    ConfigPage.Object.gameObject.SetActive(true);
                    HoldablePadHandheld.transform.Find("UI/ButtonTextMiddle").GetComponent<Text>().text = "Proceed";
                    break;
                case ScreenModes.FavouriteView:
                    if (HoldableList[1].FilteredPages.Count > 0)
                        HoldableList[1].CurrentIndex = HoldableList[1].CurrentIndex % HoldableList[1].FilteredPages.Count;
                    if (CurrentScreenMode == ScreenModes.InfoView || CurrentScreenMode == ScreenModes.ConfigView || CurrentScreenMode == ScreenModes.InitalInfo)
                        ResetDocks();

                    RemoveDock(0);
                    SetDock(ConfigPage.Object, 1);
                    SetDock(MMObject, 2);
                    ScrollFocus = HoldableList[1].FilteredPages.Count == 0 ? NoHoldablesPage.Object : null;

                    var holdableTransform = HoldablePadHandheld.transform;
                    Transform targetDocked = CurrentScreenMode == ScreenModes.ConfigView ? ConfigPage.Object : holdableTransform.Find("UI/MainMenuPage");

                    CurrentHoldableListItem = 1;
                    HoldableList[0].HoldablesDocked = true;
                    HoldableList[1].HoldablesHidden = false;
                    HoldableList.ForEach(a => a.ForceDocked = HoldableList[1].FilteredPages.Count == 0);

                    var currentList = HoldableList[CurrentHoldableListItem];
                    var currentItem = currentList.MenuPages[currentList.CurrentIndex];
                    currentItem = currentList.IsFiltered ? currentList.FilteredPages.Count == 0 ? currentItem : currentList.FilteredPages[currentList.CurrentIndex] : currentItem;

                    var holdable = InitalizedHoldables[currentList.MenuPages.IndexOf(currentItem)];
                    bool isLeft = bool.Parse(holdable.GetHoldableProp(Holdable.HoldablePropType.IsLeftHand).ToString());

                    NoHoldablesPage.Object.gameObject.SetActive(true);
                    HoldablePadHandheld.transform.Find("UI/ButtonTextMiddle").GetComponent<Text>().text = HoldableList[1].FilteredPages.Count == 0 ? "Proceed" : HoldableUtils.IsEquipped(holdable) ? "Unequip" : $"Equip ({(isLeft ? "Left" : "Right")})";
                    break;
            }

            CurrentScreenMode = setScreenMode;
        }
        #endregion

        #region Menu Buttons

        /// <summary>
        /// Simulates a main button press
        /// </summary>
        /// <param name="objectName">Determines what action will occur</param>
        public void PageButtonPress(string objectName) // Polished, 5:08 AM, 7/4/2023
        {
            switch (objectName)
            {
                case "Cube (3)":
                    SetPage(ScreenModes.ConfigView);
                    PadSource.PlayOneShot(Open, Constants.ButtonVolume);
                    Static.GlobalBtnCooldownTime = Time.unscaledTime;
                    return;
                case "Cube (4)":
                    SetPage(ScreenModes.InfoView);
                    PadSource.PlayOneShot(Open, Constants.ButtonVolume);
                    Static.GlobalBtnCooldownTime = Time.unscaledTime;
                    return;
                case "Cube (5)":
                    SetPage(ScreenModes.FavouriteView);
                    PadSource.PlayOneShot(Open, Constants.ButtonVolume);
                    Static.GlobalBtnCooldownTime = Time.unscaledTime;
                    return;
            }

            if (CurrentScreenMode == ScreenModes.InitalInfo || CurrentScreenMode == ScreenModes.InfoView || CurrentScreenMode == ScreenModes.FavouriteView && HoldableList[1].FilteredPages.Count == 0)
            {
                SetPage(ScreenModes.HoldableView);
                PadSource.PlayOneShot(Equip, Constants.ButtonVolume);
                Static.GlobalBtnCooldownTime = Time.unscaledTime;
            }
            else if (CurrentScreenMode == ScreenModes.ConfigView)
            {
                if (objectName == "Cube (1)" || objectName == "Cube (2)")
                {
                    PadSource.PlayOneShot(Swap, Constants.ButtonVolume);
                    return;
                }

                ConfigPage.SaveConfig();
                PadSource.PlayOneShot(Equip, Constants.ButtonVolume);
                Static.GlobalBtnCooldownTime = Time.unscaledTime;
            }
            else if (CurrentScreenMode == ScreenModes.HoldableView || CurrentScreenMode == ScreenModes.FavouriteView)
            {
                switch (objectName)
                {
                    case "Cube (1)":
                        PadSource.PlayOneShot(Swap, Constants.ButtonVolume);
                        var usedList = HoldableList[CurrentHoldableListItem].IsFiltered ? HoldableList[CurrentHoldableListItem].FilteredPages : HoldableList[CurrentHoldableListItem].MenuPages;
                        HoldableList[CurrentHoldableListItem].CurrentIndex = HoldableList[CurrentHoldableListItem].CurrentIndex == 0 ? usedList.Count - 1 : HoldableList[CurrentHoldableListItem].CurrentIndex - 1;

                        var currentList3 = HoldableList[CurrentHoldableListItem];
                        var currentItem3 = currentList3.MenuPages[currentList3.CurrentIndex];
                        currentItem3 = currentList3.IsFiltered ? currentList3.FilteredPages.Count == 0 ? currentItem3 : currentList3.FilteredPages[currentList3.CurrentIndex] : currentItem3;
                        var leftHoldable = InitalizedHoldables[currentList3.MenuPages.IndexOf(currentItem3)];

                        bool isLeftLocal = bool.Parse(leftHoldable.GetHoldableProp(Holdable.HoldablePropType.IsLeftHand).ToString());
                        HoldablePadHandheld.transform.Find("UI/ButtonTextMiddle").GetComponent<Text>().text = HoldableUtils.IsEquipped(leftHoldable) ? "Unequip" : $"Equip ({(isLeftLocal ? "Left" : "Right")})";
                        break;
                    case "Cube (2)":
                        PadSource.PlayOneShot(Swap, Constants.ButtonVolume);
                        var usedList2 = HoldableList[CurrentHoldableListItem].IsFiltered ? HoldableList[CurrentHoldableListItem].FilteredPages : HoldableList[CurrentHoldableListItem].MenuPages;
                        HoldableList[CurrentHoldableListItem].CurrentIndex = (HoldableList[CurrentHoldableListItem].CurrentIndex + 1) % usedList2.Count;

                        var currentList2 = HoldableList[CurrentHoldableListItem];
                        var currentItem2 = currentList2.MenuPages[currentList2.CurrentIndex];
                        currentItem2 = currentList2.IsFiltered ? currentList2.FilteredPages.Count == 0 ? currentItem2 : currentList2.FilteredPages[currentList2.CurrentIndex] : currentItem2;
                        var rightHoldable = InitalizedHoldables[currentList2.MenuPages.IndexOf(currentItem2)];

                        bool isLeftLocal2 = bool.Parse(rightHoldable.GetHoldableProp(Holdable.HoldablePropType.IsLeftHand).ToString());
                        HoldablePadHandheld.transform.Find("UI/ButtonTextMiddle").GetComponent<Text>().text = HoldableUtils.IsEquipped(rightHoldable) ? "Unequip" : $"Equip ({(isLeftLocal2 ? "Left" : "Right")})";
                        break;
                    default:
                        var currentList = HoldableList[CurrentHoldableListItem];
                        var currentItem = currentList.MenuPages[currentList.CurrentIndex];
                        currentItem = currentList.IsFiltered ? currentList.FilteredPages.Count == 0 ? currentItem : currentList.FilteredPages[currentList.CurrentIndex] : currentItem;
                        var holdable = InitalizedHoldables[currentList.MenuPages.IndexOf(currentItem)];
                        PadSource.PlayOneShot(Equip, Constants.ButtonVolume);

                        string equipName = HoldableUtils.IsEquipped(holdable) ? "DropItem" : "HoldItem";
                        GetType().GetMethod(equipName, BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.InvokeMethod).Invoke(this, new object[] { holdable });

                        HP_Config.CurrentHoldableLeft.Value = CurrentHandheldL == null ? "None" : CurrentHandheldL.BasePath;
                        HP_Config.CurrentHoldableRight.Value = CurrentHandheldR == null ? "None" : CurrentHandheldR.BasePath;
                        HP_Config.ConfigFile.Save();

                        bool isLeft = bool.Parse(holdable.GetHoldableProp(Holdable.HoldablePropType.IsLeftHand).ToString());
                        HoldablePadHandheld.transform.Find("UI/ButtonTextMiddle").GetComponent<Text>().text = HoldableUtils.IsEquipped(holdable) ? "Unequip" : $"Equip ({(isLeft ? "Left" : "Right")})";
                        break;
                }
            }
        }

        /// <summary>
        /// Simulates a config button press
        /// </summary>
        public void ConfigButtonPress()
            => PadSource.PlayOneShot(Swap, Constants.ButtonVolume);

        /// <summary>
        /// Simulates a favourite button press
        /// </summary>
        public void FavButtonPress()
        {
            PadSource.PlayOneShot(Equip, Constants.ButtonVolume);

            var holdablePages = HoldableList[1].MenuPages;
            Page currentPage = holdablePages[HoldableList[0].CurrentIndex];

            if (!HoldableList[1].FilteredPages.Contains(currentPage))
                HoldableList[1].FilteredPages.Add(currentPage);
            else
                HoldableList[1].FilteredPages.Remove(currentPage);

            if (HoldableList[1].FilteredPages.Count == 0) HP_Config.FavouriteHoldables.Value = "None";
            else
            {
                // If you're reading this and you know of a better method for doing this sorta thing, can you show me please, 9:22 AM 7/6/2023
                try
                {
                    List<int> usedHoldables = new List<int>();
                    HoldableList[1].FilteredPages.ForEach(a => usedHoldables.Add(HoldableList[1].MenuPages.IndexOf(a)));

                    List<string> favouritedHoldables = new List<string>();
                    usedHoldables.ForEach(a => favouritedHoldables.Add(InitalizedHoldables[a].BasePath));

                    HP_Config.FavouriteHoldables.Value = string.Join(";", favouritedHoldables);
                }
                catch (Exception ex)
                {
                    HP_Log.LogError("Error while attempting to save favourited holdables: " + ex.ToString());
                }
            }
            HP_Config.ConfigFile.Save();
        }

        #endregion

        #region Menu Activation

        /// <summary>
        /// Sets the active state of the pad based on if it was or wasn't active before
        /// </summary>
        public async void ActivateMenu()
        {
            Pad_CanOpen = false;
            HoldablePadHandheld.SetActive(!HoldablePadHandheld.activeSelf);

            if (HoldablePadHandheld.activeSelf)
            {
                PadSource.PlayOneShot(Open, 0.2f);
                GorillaTagger.Instance.StartVibration(HP_Config.CurrentHand.Value == HP_Config.HandPosition.LeftHand, 0.1f, 0.07f);
            }

            if (HoldableList[CurrentHoldableListItem].CurrentIndex < InitalizedHoldables.Count && CurrentScreenMode == ScreenModes.HoldableView)
            {
                var currentList = HoldableList[CurrentHoldableListItem];
                var currentItem = currentList.MenuPages[currentList.CurrentIndex];
                currentItem = currentList.IsFiltered ? currentList.FilteredPages.Count == 0 ? currentItem : currentList.FilteredPages[currentList.CurrentIndex] : currentItem;

                var holdable = InitalizedHoldables[currentList.MenuPages.IndexOf(currentItem)];
                bool isLeft = bool.Parse(holdable.GetHoldableProp(Holdable.HoldablePropType.IsLeftHand).ToString());
                HoldablePadHandheld.transform.Find("UI/ButtonTextMiddle").GetComponent<Text>().text = HoldableUtils.IsEquipped(holdable) ? "Unequip" : $"Equip ({(isLeft ? "Left" : "Right")})";
            }
            if (CurrentScreenMode == ScreenModes.InitalInfo || CurrentScreenMode == ScreenModes.InfoView)
                HoldablePadHandheld.transform.Find("UI/ButtonTextMiddle").GetComponent<Text>().text = "Proceed";

            await Task.Delay(Constants.PageDisplay);
            Pad_CanOpen = true;
        }

        /// <summary>
        /// Sets the active state of the pad to be visible no matter what
        /// </summary>
        public async void ForceActivate()
        {
            Pad_CanOpen = false;
            HoldablePadHandheld.SetActive(true);

            PadSource.PlayOneShot(Open, 0.2f);
            GorillaTagger.Instance.StartVibration(HP_Config.CurrentHand.Value == HP_Config.HandPosition.LeftHand, 0.1f, 0.07f);

            if (HoldableList[CurrentHoldableListItem].CurrentIndex < InitalizedHoldables.Count && CurrentScreenMode == ScreenModes.HoldableView)
            {
                var holdable = InitalizedHoldables[HoldableList[CurrentHoldableListItem].CurrentIndex];
                bool isLeft = bool.Parse(holdable.GetHoldableProp(Holdable.HoldablePropType.IsLeftHand).ToString());
                HoldablePadHandheld.transform.Find("UI/ButtonTextMiddle").GetComponent<Text>().text = HoldableUtils.IsEquipped(holdable) ? "Unequip" : $"Equip ({(isLeft ? "Left" : "Right")})";
            }
            if (CurrentScreenMode == ScreenModes.InitalInfo || CurrentScreenMode == ScreenModes.InfoView)
                HoldablePadHandheld.transform.Find("UI/ButtonTextMiddle").GetComponent<Text>().text = "Proceed";

            await Task.Delay(Constants.PageDisplay);
            Pad_CanOpen = true;
        }
        #endregion
    }
}