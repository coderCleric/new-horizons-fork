using HarmonyLib;
using NewHorizons.Builder.Atmosphere;
using NewHorizons.Builder.Body;
using NewHorizons.Builder.General;
using NewHorizons.Builder.Props;
using NewHorizons.Builder.Props.Audio;
using NewHorizons.Builder.Props.TranslatorText;
using NewHorizons.Components.Fixers;
using NewHorizons.Components.Ship;
using NewHorizons.Components.SizeControllers;
using NewHorizons.External;
using NewHorizons.External.Configs;
using NewHorizons.Handlers;
using NewHorizons.OtherMods.AchievementsPlus;
using NewHorizons.OtherMods.MenuFramework;
using NewHorizons.OtherMods.OWRichPresence;
using NewHorizons.OtherMods.VoiceActing;
using NewHorizons.Utility;
using NewHorizons.Utility.DebugTools;
using NewHorizons.Utility.DebugTools.Menu;
using NewHorizons.Utility.Files;
using NewHorizons.Utility.OuterWilds;
using NewHorizons.Utility.OWML;
using OWML.Common;
using OWML.ModHelper;
using OWML.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace NewHorizons
{

    public class Main : ModBehaviour
    {
        public static AssetBundle NHAssetBundle { get; private set; }
        public static AssetBundle NHPrivateAssetBundle { get; private set; }
        public static Main Instance { get; private set; }

        // Settings
        public static bool Debug { get; private set; }
        public static bool VerboseLogs { get; private set; }
        private static bool _useCustomTitleScreen;
        private static bool _wasConfigured = false;
        private static string _defaultSystemOverride;

        public static Dictionary<string, NewHorizonsSystem> SystemDict = new Dictionary<string, NewHorizonsSystem>();
        public static Dictionary<string, List<NewHorizonsBody>> BodyDict = new Dictionary<string, List<NewHorizonsBody>>();
        public static List<IModBehaviour> MountedAddons = new List<IModBehaviour>();

        public static float SecondsElapsedInLoop = -1;

        public static bool IsSystemReady { get; private set; }

        public string DefaultStarSystem => SystemDict.ContainsKey(_defaultSystemOverride) ? _defaultSystemOverride : _defaultStarSystem;
        public string CurrentStarSystem
        {
            get
            {
                return _currentStarSystem;
            }
            private set
            {
                _previousStarSystem = _currentStarSystem;
                _currentStarSystem = value;
            }
        }
        private string _currentStarSystem = "SolarSystem";
        private string _previousStarSystem;

        public bool TimeLoopEnabled => SystemDict[CurrentStarSystem]?.Config?.enableTimeLoop ?? true;
        public bool IsWarpingFromShip { get; private set; } = false;
        public bool IsWarpingFromVessel { get; private set; } = false;
        public bool IsWarpingBackToEye { get; internal set; } = false;
        public bool DidWarpFromShip { get; private set; } = false;
        public bool DidWarpFromVessel { get; private set; } = false;
        public bool WearingSuit { get; private set; } = false;

        public bool IsChangingStarSystem { get; private set; } = false;

        public static bool HasWarpDrive { get; private set; } = false;

        private string _defaultStarSystem = "SolarSystem";

        private bool _firstLoad = true;

        private bool _playerAwake;
        public bool PlayerSpawned { get; set; }

        public ShipWarpController ShipWarpController { get; private set; }

        // API events
        public class StarSystemEvent : UnityEvent<string> { }
        public StarSystemEvent OnChangeStarSystem = new();
        public StarSystemEvent OnStarSystemLoaded = new();
        public StarSystemEvent OnPlanetLoaded = new();

        public static bool HasDLC { get => EntitlementsManager.IsDlcOwned() == EntitlementsManager.AsyncOwnershipStatus.Owned; }

        public static StarSystemConfig GetCurrentSystemConfig => SystemDict[Instance.CurrentStarSystem].Config;

        public override object GetApi()
        {
            return new NewHorizonsApi();
        }

        public override void Configure(IModConfig config)
        {
            NHLogger.LogVerbose("Settings changed");

            var currentScene = SceneManager.GetActiveScene().name;

            Debug = config.GetSettingsValue<bool>("Debug");
            VerboseLogs = config.GetSettingsValue<bool>("Verbose Logs");

            if (currentScene == "SolarSystem")
            {
                DebugReload.UpdateReloadButton();
                DebugMenu.UpdatePauseMenuButton();
            }

            if (VerboseLogs) NHLogger.UpdateLogLevel(NHLogger.LogType.Verbose);
            else if (Debug) NHLogger.UpdateLogLevel(NHLogger.LogType.Log);
            else NHLogger.UpdateLogLevel(NHLogger.LogType.Error);

            var oldDefaultSystemOverride = _defaultSystemOverride;
            _defaultSystemOverride = config.GetSettingsValue<string>("Default System Override");
            if (oldDefaultSystemOverride != _defaultSystemOverride)
            {
                ResetCurrentStarSystem();
                NHLogger.Log($"Changed default star system override to {_defaultSystemOverride}");
            }

            var wasUsingCustomTitleScreen = _useCustomTitleScreen;
            _useCustomTitleScreen = config.GetSettingsValue<bool>("Custom title screen");
            // Reload the title screen if this was updated on it
            // Don't reload if we haven't configured yet (called on game start)
            if (wasUsingCustomTitleScreen != _useCustomTitleScreen && SceneManager.GetActiveScene().name == "TitleScreen" && _wasConfigured)
            {
                NHLogger.LogVerbose("Reloading");
                SceneManager.LoadScene("TitleScreen", LoadSceneMode.Single);
            }

            _wasConfigured = true;
        }

        public void ResetConfigs(bool resetTranslation = true)
        {
            BodyDict.Clear();
            SystemDict.Clear();

            BodyDict["SolarSystem"] = new List<NewHorizonsBody>();
            BodyDict["EyeOfTheUniverse"] = new List<NewHorizonsBody>(); // Keep this empty tho fr
            SystemDict["SolarSystem"] = new NewHorizonsSystem("SolarSystem", new StarSystemConfig(), "", Instance)
            {
                Config =
                {
                    destroyStockPlanets = false,
                    Vessel = new StarSystemConfig.VesselModule()
                    {
                        coords = new StarSystemConfig.NomaiCoordinates
                        {
                            x = new int[5]{ 0,3,2,1,5 },
                            y = new int[5]{ 4,5,3,2,1 },
                            z = new int[5]{ 4,1,2,5,0 }
                        }
                    }
                }
            };
            SystemDict["EyeOfTheUniverse"] = new NewHorizonsSystem("EyeOfTheUniverse", new StarSystemConfig(), "", Instance)
            {
                Config =
                {
                    destroyStockPlanets = false,
                    factRequiredForWarp = "OPC_EYE_COORDINATES_X1",
                    Vessel = new StarSystemConfig.VesselModule()
                    {
                        coords = new StarSystemConfig.NomaiCoordinates
                        {
                            x = new int[3] { 1, 5, 4 },
                            y = new int[4] { 3, 0, 1, 4 },
                            z = new int[6] { 1, 2, 3, 0, 5, 4 }
                        }
                    }
                }
            };

            if (resetTranslation)
            {
                TranslationHandler.ClearTables();
                TextTranslation.Get().SetLanguage(TextTranslation.Get().GetLanguage());
            }

            LoadTranslations(Path.Combine(Instance.ModHelper.Manifest.ModFolderPath, "Assets/"), this);
        }

        public void Awake()
        {
            Instance = this;
        }

        public void Start()
        {
            // Patches
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
            // the campfire on the title screen calls this from RegisterShape before it gets patched, so we have to call it again. lol 
            ShapeManager.Initialize();

            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.sceneUnloaded += OnSceneUnloaded;

            GlobalMessenger<DeathType>.AddListener("PlayerDeath", OnDeath);
            GlobalMessenger.AddListener("WakeUp", OnWakeUp);

            NHAssetBundle = ModHelper.Assets.LoadBundle("Assets/newhorizons_public");
            if (NHAssetBundle == null)
            {
                NHLogger.LogError("Couldn't find NHAssetBundle: The mod will likely not work.");
            }

            NHPrivateAssetBundle = ModHelper.Assets.LoadBundle("Assets/newhorizons_private");
            if (NHPrivateAssetBundle == null)
            {
                NHLogger.LogError("Couldn't find NHPrivateAssetBundle: The mod will likely not work.");
            }

            VesselWarpHandler.Initialize();

            ResetConfigs(resetTranslation: false);

            NHLogger.Log("Begin load of config files...");

            try
            {
                LoadConfigs(this);
            }
            catch (Exception)
            {
                NHLogger.LogWarning("Couldn't find planets folder");
            }

            // Call this from the menu since we hadn't hooked onto the event yet
            Delay.FireOnNextUpdate(() => OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single));
            Delay.FireOnNextUpdate(() => _firstLoad = false);
            Instance.ModHelper.Menus.PauseMenu.OnInit += DebugReload.InitializePauseMenu;

            MenuHandler.Init();
            AchievementHandler.Init();
            VoiceHandler.Init();
            RichPresenceHandler.Init();
            OnStarSystemLoaded.AddListener(RichPresenceHandler.OnStarSystemLoaded);
            OnChangeStarSystem.AddListener(RichPresenceHandler.OnChangeStarSystem);

            LoadAddonManifest("Assets/addon-manifest.json", this);
        }

        public void OnDestroy()
        {
            NHLogger.Log($"Destroying NewHorizons");
            SceneManager.sceneLoaded -= OnSceneLoaded;
            GlobalMessenger<DeathType>.RemoveListener("PlayerDeath", OnDeath);
            GlobalMessenger.RemoveListener("WakeUp", OnWakeUp);

            AchievementHandler.OnDestroy();
        }

        private void OnWakeUp() => _playerAwake = true;

        private void OnSceneUnloaded(Scene scene)
        {
            // Caches of GameObjects must always be cleared
            SearchUtilities.ClearCache();
            ProxyHandler.OnSceneUnloaded();

            // Caches of other assets only have to be cleared if we changed star systems
            if (CurrentStarSystem != _previousStarSystem)
            {
                ImageUtilities.ClearCache();
                AudioUtilities.ClearCache();
                AssetBundleUtilities.ClearCache();
                EnumUtilities.ClearCache();
            }

            IsSystemReady = false;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            NHLogger.Log($"Scene Loaded: {scene.name} {mode} OWScene.{LoadManager.NameToScene(scene.name)}");

            PlayerSpawned = false;

            var isTitleScreen = scene.name == LoadManager.SceneToName(OWScene.TitleScreen);
            var isSolarSystem = scene.name == LoadManager.SceneToName(OWScene.SolarSystem);
            var isEyeOfTheUniverse = scene.name == LoadManager.SceneToName(OWScene.EyeOfTheUniverse);
            var isCreditsFast = scene.name == LoadManager.SceneToName(OWScene.Credits_Fast);
            var isCreditsFinal = scene.name == LoadManager.SceneToName(OWScene.Credits_Final);
            var isPostCredits = scene.name == LoadManager.SceneToName(OWScene.PostCreditsScene);

            if (isSolarSystem)
            {
                try
                {
                    AtmosphereBuilder.InitPrefabs();
                    BrambleDimensionBuilder.InitPrefabs();
                    BrambleNodeBuilder.InitPrefabs();
                    CloudsBuilder.InitPrefabs();
                    CometTailBuilder.InitPrefab();
                    DetectorBuilder.InitPrefabs();
                    EffectsBuilder.InitPrefabs();
                    FogBuilder.InitPrefabs();
                    FunnelBuilder.InitPrefabs();
                    GeometryBuilder.InitPrefab();
                    GeyserBuilder.InitPrefab();
                    LavaBuilder.InitPrefabs();
                    
                    // Backwards compat
#pragma warning disable 612, 618
                    NomaiTextBuilder.InitPrefabs();
#pragma warning restore 612, 618
                    TranslatorTextBuilder.InitPrefabs();
                    RemoteBuilder.InitPrefabs();
                    SandBuilder.InitPrefabs();
                    SingularityBuilder.InitPrefabs();
                    StarBuilder.InitPrefabs();
                    StarEvolutionController.Init();
                    SupernovaEffectBuilder.InitPrefabs();
                    TornadoBuilder.InitPrefabs();
                    VolcanoBuilder.InitPrefab();
                    VolumesBuilder.InitPrefabs();
                    WaterBuilder.InitPrefabs();
                    GravityCannonBuilder.InitPrefab();
                    ShuttleBuilder.InitPrefab();

                    ProjectionBuilder.InitPrefabs();
                    CloakBuilder.InitPrefab();
                    RaftBuilder.InitPrefab();

                    WarpPadBuilder.InitPrefabs();
                }
                catch (Exception e)
                {
                    NHLogger.LogError($"Couldn't init prefabs:\n{e}");
                }
            }

            if (isEyeOfTheUniverse)
            {
                CurrentStarSystem = "EyeOfTheUniverse";
            }
            else if (IsWarpingBackToEye)
            {
                IsWarpingBackToEye = false;
                OWTime.Pause(OWTime.PauseType.Loading);
                LoadManager.LoadSceneImmediate(OWScene.EyeOfTheUniverse);
                OWTime.Unpause(OWTime.PauseType.Loading);
                return;
            }

            if (!SystemDict.ContainsKey(CurrentStarSystem) || !BodyDict.ContainsKey(CurrentStarSystem))
            {
                NHLogger.LogError($"System \"{CurrentStarSystem}\" does not exist!");
                CurrentStarSystem = DefaultStarSystem;
            }

            // Set time loop stuff if its enabled and if we're warping to a new place
            if (IsChangingStarSystem && (SystemDict[CurrentStarSystem].Config.enableTimeLoop || CurrentStarSystem == "SolarSystem") && SecondsElapsedInLoop > 0f)
            {
                TimeLoopUtilities.SetSecondsElapsed(SecondsElapsedInLoop);
                // Prevent the OPC from firing
                var launchController = FindObjectOfType<OrbitalProbeLaunchController>();
                if (launchController != null)
                {
                    GlobalMessenger<int>.RemoveListener("StartOfTimeLoop", launchController.OnStartOfTimeLoop);
                    foreach (var fakeDebris in launchController._fakeDebrisBodies)
                    {
                        fakeDebris.gameObject.SetActive(false);
                    }
                    launchController.enabled = false;
                }
                var nomaiProbe = SearchUtilities.Find("NomaiProbe_Body");
                if (nomaiProbe != null) nomaiProbe.gameObject.SetActive(false);
            }

            // Reset this
            SecondsElapsedInLoop = -1;

            IsChangingStarSystem = false;

            if (isTitleScreen && _useCustomTitleScreen)
            {
                try
                {
                    TitleSceneHandler.DisplayBodyOnTitleScreen(BodyDict.Values.ToList().SelectMany(x => x).ToList());
                }
                catch (Exception e)
                {
                    NHLogger.LogError($"Failed to make title screen bodies: {e}");
                }
                TitleSceneHandler.InitSubtitles();
            }

            // EOTU fixes
            if (isEyeOfTheUniverse)
            {
                _playerAwake = true;
                EyeSceneHandler.OnSceneLoad();
            }

            if (isSolarSystem || isEyeOfTheUniverse)
            {
                // Stop dying while spawning please
                InvulnerabilityHandler.MakeInvulnerable(true);

                IsSystemReady = false;

                NewHorizonsData.Load();

                // If the vessel is forcing the player to spawn there, allow it to override
                IsWarpingFromVessel = VesselWarpHandler.ShouldSpawnAtVessel();

                // Some builders have to be reset each loop
                SignalBuilder.Init();
                BrambleDimensionBuilder.Init();
                AstroObjectLocator.Init();
                StreamingHandler.Init();
                AudioTypeHandler.Init();
                InterferenceHandler.Init();
                RemoteHandler.Init();
                SingularityBuilder.Init();
                AtmosphereBuilder.Init();
                BrambleNodeBuilder.Init(BodyDict[CurrentStarSystem].Select(x => x.Config).Where(x => x.Bramble?.dimension != null).ToArray());
                CloakHandler.Init();

                if (isSolarSystem)
                {
                    foreach (var supernovaPlanetEffectController in FindObjectsOfType<SupernovaPlanetEffectController>())
                    {
                        SupernovaEffectBuilder.ReplaceVanillaWithNH(supernovaPlanetEffectController);
                    }
                }

                PlanetCreationHandler.Init(BodyDict[CurrentStarSystem]);
                VesselWarpHandler.LoadVessel();
                SystemCreationHandler.LoadSystem(SystemDict[CurrentStarSystem]);

                StarChartHandler.Init(SystemDict.Values.ToArray());

                // Fix spawn point
                PlayerSpawnHandler.SetUpPlayerSpawn();

                if (isSolarSystem)
                {
                    // Warp drive
                    HasWarpDrive = StarChartHandler.CanWarp();
                    if (ShipWarpController == null)
                    {
                        ShipWarpController = SearchUtilities.Find("Ship_Body").AddComponent<ShipWarpController>();
                        ShipWarpController.Init();
                    }
                    if (HasWarpDrive == true) EnableWarpDrive();

                    var shouldWarpInFromShip = IsWarpingFromShip && ShipWarpController != null;
                    var shouldWarpInFromVessel = IsWarpingFromVessel && VesselWarpHandler.VesselSpawnPoint != null;

                    IsWarpingFromShip = false;
                    IsWarpingFromVessel = false;
                    DidWarpFromShip = shouldWarpInFromShip;
                    DidWarpFromVessel = shouldWarpInFromVessel;

                    // Fix the map satellite
                    SearchUtilities.Find("HearthianMapSatellite_Body", false).AddComponent<MapSatelliteOrbitFix>();

                    // Sector changes (so that projection pools actually turn off proxies and cull groups on these moons)

                    //Fix attlerock vanilla sector components (they were set to timber hearth's sector)
                    var thm = SearchUtilities.Find("Moon_Body/Sector_THM").GetComponent<Sector>();
                    foreach (var component in thm.GetComponentsInChildren<Component>(true))
                    {
                        if (component is ISectorGroup sectorGroup)
                        {
                            sectorGroup.SetSector(thm);
                        }

                        if (component is SectoredMonoBehaviour behaviour)
                        {
                            behaviour.SetSector(thm);
                        }
                    }
                    var thm_ss_obj = new GameObject("Sector_Streaming");
                    thm_ss_obj.transform.SetParent(thm.transform, false);
                    var thm_ss = thm_ss_obj.AddComponent<SectorStreaming>();
                    thm_ss._streamingGroup = SearchUtilities.Find("TimberHearth_Body/StreamingGroup_TH").GetComponent<StreamingGroup>();
                    thm_ss.SetSector(thm);


                    //Fix hollow's lantern vanilla sector components (they were set to brittle hollow's sector)
                    var vm = SearchUtilities.Find("VolcanicMoon_Body/Sector_VM").GetComponent<Sector>();
                    foreach (var component in vm.GetComponentsInChildren<Component>(true))
                    {
                        if (component is ISectorGroup sectorGroup)
                        {
                            sectorGroup.SetSector(vm);
                        }

                        if (component is SectoredMonoBehaviour behaviour)
                        {
                            behaviour.SetSector(vm);
                        }
                    }
                    var vm_ss_obj = new GameObject("Sector_Streaming");
                    vm_ss_obj.transform.SetParent(vm.transform, false);
                    var vm_ss = vm_ss_obj.AddComponent<SectorStreaming>();
                    vm_ss._streamingGroup = SearchUtilities.Find("BrittleHollow_Body/StreamingGroup_BH").GetComponent<StreamingGroup>();
                    vm_ss.SetSector(vm);

                    //Fix brittle hollow north pole projection platform
                    var northPoleSurface = SearchUtilities.Find("BrittleHollow_Body/Sector_BH/Sector_NorthHemisphere/Sector_NorthPole/Sector_NorthPoleSurface").GetComponent<Sector>();
                    var remoteViewer = SearchUtilities.Find("BrittleHollow_Body/Sector_BH/Sector_NorthHemisphere/Sector_NorthPole/Sector_NorthPoleSurface/Interactables_NorthPoleSurface/LowBuilding/Prefab_NOM_RemoteViewer").GetComponent<NomaiRemoteCameraPlatform>();
                    remoteViewer._visualSector = northPoleSurface;
                }
                else if (isEyeOfTheUniverse)
                {
                    IsWarpingFromShip = false;
                    IsWarpingFromVessel = false;
                    DidWarpFromVessel = false;
                    DidWarpFromShip = false;
                }

                //Stop starfield from disappearing when there is no lights
                var playerBody = SearchUtilities.Find("Player_Body");
                var playerLight = playerBody.AddComponent<Light>();
                playerLight.innerSpotAngle = 0;
                playerLight.spotAngle = 179;
                playerLight.range = 1;
                playerLight.intensity = 0.001f;

                //Do the same for map
                var solarSystemRoot = SearchUtilities.Find("SolarSystemRoot");
                var ssrLight = solarSystemRoot.AddComponent<Light>();
                ssrLight.innerSpotAngle = 0;
                ssrLight.spotAngle = 179;
                ssrLight.range = PlanetCreationHandler.SolarSystemRadius * (4f / 3f);
                ssrLight.intensity = 0.001f;

                var fluid = playerBody.FindChild("PlayerDetector").GetComponent<DynamicFluidDetector>();
                fluid._splashEffects = fluid._splashEffects.AddToArray(new SplashEffect
                {
                    fluidType = FluidVolume.Type.PLASMA,
                    ignoreSphereAligment = false,
                    minImpactSpeed = 15,
                    splashPrefab = SearchUtilities.Find("Probe_Body/ProbeDetector").GetComponent<FluidDetector>()._splashEffects.FirstOrDefault(sfx => sfx.fluidType == FluidVolume.Type.PLASMA).splashPrefab,
                    triggerEvent = SplashEffect.TriggerEvent.OnEntry
                });

                try
                {
                    NHLogger.Log($"Star system finished loading [{Instance.CurrentStarSystem}]");
                    Instance.OnStarSystemLoaded?.Invoke(Instance.CurrentStarSystem);
                }
                catch (Exception e)
                {
                    NHLogger.LogError($"Exception thrown when invoking star system loaded event with parameter [{Instance.CurrentStarSystem}]:\n{e}");
                }

                // Wait for player to be awake and also for frames to pass
                var justLinkedToStatue = PlayerData.KnowsLaunchCodes() && PlayerData._currentGameSave.loopCount == 1;
                Delay.RunWhenOrInNUpdates(() => OnSystemReady(DidWarpFromShip, DidWarpFromVessel), () => (_playerAwake && PlayerSpawned) || justLinkedToStatue, 30);
            }
            else
            {
                ResetCurrentStarSystem();
            }
        }

        // Had a bunch of separate unity things firing stuff when the system is ready so I moved it all to here
        private void OnSystemReady(bool shouldWarpInFromShip, bool shouldWarpInFromVessel)
        {
            if (IsSystemReady)
            {
                NHLogger.LogWarning("OnSystemReady was called twice.");
            }
            else
            {
                IsSystemReady = true;

                // ShipWarpController will handle the invulnerability otherwise
                if (!shouldWarpInFromShip)
                    Delay.FireOnNextUpdate(() => InvulnerabilityHandler.MakeInvulnerable(false));

                Locator.GetPlayerBody().gameObject.AddComponent<DebugRaycaster>();
                Locator.GetPlayerBody().gameObject.AddComponent<DebugPropPlacer>();
                Locator.GetPlayerBody().gameObject.AddComponent<DebugMenu>();

                PlayerSpawnHandler.OnSystemReady(shouldWarpInFromShip, shouldWarpInFromVessel);

                VesselCoordinatePromptHandler.RegisterPrompts(SystemDict.Where(system => system.Value.Config.Vessel?.coords != null).Select(x => x.Value).ToList());

                CloakHandler.OnSystemReady();
            }
        }

        public void EnableWarpDrive()
        {
            NHLogger.LogVerbose("Setting up warp drive");
            PlanetCreationHandler.LoadBody(LoadConfig(this, "Assets/WarpDriveConfig.json"));
            HasWarpDrive = true;
        }


        #region Load
        public void LoadStarSystemConfig(string starSystemName, StarSystemConfig starSystemConfig, string relativePath, IModBehaviour mod)
        {
            starSystemConfig.Migrate();
            starSystemConfig.FixCoordinates();

            if (starSystemConfig.startHere)
            {
                // We always want to allow mods to overwrite setting the main SolarSystem as default but not the other way around
                if (starSystemName != "SolarSystem")
                {
                    SetDefaultSystem(starSystemName);
                    CurrentStarSystem = DefaultStarSystem;
                }
            }

            if (SystemDict.ContainsKey(starSystemName))
            {
                if (string.IsNullOrEmpty(SystemDict[starSystemName].Config.travelAudio) && SystemDict[starSystemName].Config.Skybox == null)
                    SystemDict[starSystemName].Mod = mod;
                SystemDict[starSystemName].Config.Merge(starSystemConfig);
            }
            else
            {
                SystemDict[starSystemName] = new NewHorizonsSystem(starSystemName, starSystemConfig, relativePath, mod);
            }
        }

        public void LoadConfigs(IModBehaviour mod)
        {
            try
            {
                if (_firstLoad)
                {
                    MountedAddons.Add(mod);
                }
                var folder = mod.ModHelper.Manifest.ModFolderPath;

                var systemsFolder = Path.Combine(folder, "systems");
                var planetsFolder = Path.Combine(folder, "planets");

                // Load systems first so that when we load bodies later we can check for missing ones
                if (Directory.Exists(systemsFolder))
                {
                    var systemFiles = Directory.GetFiles(systemsFolder, "*.json", SearchOption.AllDirectories)
                        .Concat(Directory.GetFiles(systemsFolder, "*.jsonc", SearchOption.AllDirectories))
                        .ToArray();

                    if(systemFiles.Length == 0)
                    {
                        NHLogger.LogVerbose($"Found no JSON files in systems folder: {systemsFolder}");
                    }

                    foreach (var file in systemFiles)
                    {
                        var starSystemName = Path.GetFileNameWithoutExtension(file);

                        NHLogger.LogVerbose($"Loading system {starSystemName}");

                        var relativePath = file.Replace(folder, "");
                        var starSystemConfig = mod.ModHelper.Storage.Load<StarSystemConfig>(relativePath, false);
                        LoadStarSystemConfig(starSystemName, starSystemConfig, relativePath, mod);
                    }
                }
                if (Directory.Exists(planetsFolder))
                {
                    var planetFiles = Directory.GetFiles(planetsFolder, "*.json", SearchOption.AllDirectories)
                        .Concat(Directory.GetFiles(planetsFolder, "*.jsonc", SearchOption.AllDirectories))
                        .ToArray();

                    if(planetFiles.Length == 0)
                    {
                        NHLogger.LogVerbose($"Found no JSON files in planets folder: {planetsFolder}");
                    }

                    foreach (var file in planetFiles)
                    {
                        var relativeDirectory = file.Replace(folder, "");
                        var body = LoadConfig(mod, relativeDirectory);

                        if (body != null)
                        {
                            // Wanna track the spawn point of each system
                            if (body.Config.Spawn != null) SystemDict[body.Config.starSystem].Spawn = body.Config.Spawn;

                            // Add the new planet to the planet dictionary
                            if (!BodyDict.ContainsKey(body.Config.starSystem)) BodyDict[body.Config.starSystem] = new List<NewHorizonsBody>();
                            BodyDict[body.Config.starSystem].Add(body);
                        }
                    }
                }
                // Has to go before translations for achievements
                if (File.Exists(Path.Combine(folder, "addon-manifest.json")))
                {
                    LoadAddonManifest("addon-manifest.json", mod);
                }
                if (Directory.Exists(Path.Combine(folder, "translations")))
                {
                    LoadTranslations(folder, mod);
                }

            }
            catch (Exception ex)
            {
                NHLogger.LogError(ex.ToString());
            }
        }

        private void LoadAddonManifest(string file, IModBehaviour mod)
        {
            NHLogger.LogVerbose($"Loading addon manifest for {mod.ModHelper.Manifest.Name}");

            var addonConfig = mod.ModHelper.Storage.Load<AddonConfig>(file, false);

            if (addonConfig.achievements != null)
            {
                AchievementHandler.RegisterAddon(addonConfig, mod as ModBehaviour);
            }
            if (addonConfig.credits != null)
            {
                var translatedCredits = addonConfig.credits.Select(x => TranslationHandler.GetTranslation(x, TranslationHandler.TextType.UI)).ToArray();
                CreditsHandler.RegisterCredits(mod.ModHelper.Manifest.Name, translatedCredits);
            }
            if (!string.IsNullOrEmpty(addonConfig.popupMessage))
            {
                MenuHandler.RegisterOneTimePopup(mod, TranslationHandler.GetTranslation(addonConfig.popupMessage, TranslationHandler.TextType.UI), addonConfig.repeatPopup);
            }
        }

        private void LoadTranslations(string folder, IModBehaviour mod)
        {
            var foundFile = false;
            foreach (TextTranslation.Language language in EnumUtils.GetValues<TextTranslation.Language>())
            {
                if (language is TextTranslation.Language.UNKNOWN or TextTranslation.Language.TOTAL) continue;

                var relativeFile = Path.Combine("translations", language.ToString().ToLower() + ".json");

                if (File.Exists(Path.Combine(folder, relativeFile)))
                {
                    NHLogger.LogVerbose($"Registering {language} translation from {mod.ModHelper.Manifest.Name} from {relativeFile}");

                    var config = new TranslationConfig(Path.Combine(folder, relativeFile));

                    foundFile = true;

                    TranslationHandler.RegisterTranslation(language, config);

                    if (AchievementHandler.Enabled)
                    {
                        AchievementHandler.RegisterTranslationsFromFiles(mod as ModBehaviour, "translations");
                    }
                }
            }
            if (!foundFile) NHLogger.LogWarning($"{mod.ModHelper.Manifest.Name} has a folder for translations but none were loaded");
        }

        public NewHorizonsBody LoadConfig(IModBehaviour mod, string relativePath)
        {
            NewHorizonsBody body = null;
            try
            {
                var config = mod.ModHelper.Storage.Load<PlanetConfig>(relativePath, false);
                if (config == null)
                {
                    NHLogger.LogError($"Couldn't load {relativePath}. Is your Json formatted correctly?");
                    MenuHandler.RegisterFailedConfig(Path.GetFileName(relativePath));
                    return null;
                }

                NHLogger.LogVerbose($"Loaded {config.name}");

                body = RegisterPlanetConfig(config, mod, relativePath);
            }
            catch (Exception e)
            {
                NHLogger.LogError($"Error encounter when loading {relativePath}:\n{e}");
                MenuHandler.RegisterFailedConfig(Path.GetFileName(relativePath));
            }

            return body;
        }

        public NewHorizonsBody RegisterPlanetConfig(PlanetConfig config, IModBehaviour mod, string relativePath)
        {
            if (!SystemDict.ContainsKey(config.starSystem))
            {
                // Since we didn't load it earlier there shouldn't be a star system config
                var starSystemConfig = mod.ModHelper.Storage.Load<StarSystemConfig>(Path.Combine("systems", config.starSystem + ".json"), false);
                if (starSystemConfig == null) starSystemConfig = new StarSystemConfig();
                else NHLogger.LogWarning($"Loaded system config for {config.starSystem}. Why wasn't this loaded earlier?");

                starSystemConfig.Migrate();
                starSystemConfig.FixCoordinates();

                var system = new NewHorizonsSystem(config.starSystem, starSystemConfig, $"", mod);

                SystemDict.Add(config.starSystem, system);

                BodyDict.Add(config.starSystem, new List<NewHorizonsBody>());
            }

            // Has to happen after we make sure theres a system config
            config.Validate();
            config.Migrate();

            return new NewHorizonsBody(config, mod, relativePath);
        }

        public void SetDefaultSystem(string defaultSystem)
        {
            if (string.IsNullOrEmpty(defaultSystem))
            {
                _defaultStarSystem = "SolarSystem";
            }
            else
            {
                _defaultStarSystem = defaultSystem;
            }
        }

        #endregion Load

        #region Change star system
        public void ChangeCurrentStarSystem(string newStarSystem, bool warp = false, bool vessel = false)
        {
            // If we're just on the title screen set the system for later
            if (LoadManager.GetCurrentScene() == OWScene.TitleScreen)
            {
                CurrentStarSystem = newStarSystem;
                IsWarpingFromShip = warp;
                IsWarpingFromVessel = vessel;
                DidWarpFromVessel = false;

                var warpingToEye = newStarSystem == "EyeOfTheUniverse";

                if (warpingToEye) PlayerData.SaveWarpedToTheEye(180);
                else PlayerData.SaveEyeCompletion();

                var loadableScene = warpingToEye ? SubmitActionLoadScene.LoadableScenes.EYE : SubmitActionLoadScene.LoadableScenes.GAME;
                var newGame = SearchUtilities.Find("TitleMenu/TitleCanvas/TitleLayoutGroup/MainMenuBlock/MainMenuLayoutGroup/Button-NewGame")?.GetComponent<SubmitActionLoadScene>();
                var resumeGame = SearchUtilities.Find("TitleMenu/TitleCanvas/TitleLayoutGroup/MainMenuBlock/MainMenuLayoutGroup/Button-ResumeGame")?.GetComponent<SubmitActionLoadScene>();
                if (newGame != null) newGame._sceneToLoad = loadableScene;
                if (resumeGame != null) resumeGame._sceneToLoad = loadableScene;

                return;
            }

            if (IsChangingStarSystem) return;

            if (LoadManager.GetCurrentScene() == OWScene.SolarSystem || LoadManager.GetCurrentScene() == OWScene.EyeOfTheUniverse)
            {
                IsWarpingFromShip = warp;
                IsWarpingFromVessel = vessel;
                DidWarpFromVessel = false;
                OnChangeStarSystem?.Invoke(newStarSystem);

                NHLogger.Log($"Warping to {newStarSystem}");
                if (warp && ShipWarpController) ShipWarpController.WarpOut();
                IsChangingStarSystem = true;
                WearingSuit = PlayerState.IsWearingSuit();

                OWScene sceneToLoad;

                if (newStarSystem == "EyeOfTheUniverse")
                {
                    PlayerData.SaveWarpedToTheEye(TimeLoopUtilities.GetVanillaSecondsRemaining());
                    sceneToLoad = OWScene.EyeOfTheUniverse;
                }
                else
                {
                    PlayerData.SaveEyeCompletion(); // So that the title screen doesn't keep warping you back to eye

                    if (SystemDict[CurrentStarSystem].Config.enableTimeLoop) SecondsElapsedInLoop = TimeLoop.GetSecondsElapsed();
                    else SecondsElapsedInLoop = -1;

                    sceneToLoad = OWScene.SolarSystem;
                }

                CurrentStarSystem = newStarSystem;

                // Freeze player inputs
                OWInput.ChangeInputMode(InputMode.None);

                // Hide unloading
                FadeHandler.FadeThen(1f, () =>
                {
                    // Slide reel unloading is tied to being removed from the sector, so we do that here to prevent a softlock
                    Locator.GetPlayerSectorDetector().RemoveFromAllSectors();
                    LoadManager.LoadSceneImmediate(sceneToLoad);
                });
            }
        }

        void OnDeath(DeathType _)
        {
            // We reset the solar system on death
            if (!IsChangingStarSystem)
            {
                if (SystemDict[CurrentStarSystem].Config.respawnHere) return;

                ResetCurrentStarSystem();
            }
        }

        private void ResetCurrentStarSystem()
        {
            if (SystemDict.ContainsKey(_defaultSystemOverride))
            {
                CurrentStarSystem = _defaultSystemOverride;

                // Sometimes the override will not support spawning regularly, so always warp in
                IsWarpingFromShip = true;
            }
            else
            {
                // Ignore first load because it doesn't even know what systems we have
                if (!_firstLoad && !string.IsNullOrEmpty(_defaultSystemOverride))
                {
                    NHLogger.LogError($"The given default system override {_defaultSystemOverride} is invalid - no system exists with that name");
                }

                CurrentStarSystem = _defaultStarSystem;
                IsWarpingFromShip = false;
            }
        }
        #endregion Change star system


    }
}
