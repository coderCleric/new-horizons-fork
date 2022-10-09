
using NewHorizons.Utility;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using NewHorizons.External.Modules.VariableSize;

namespace NewHorizons.External.Modules
{
    [JsonObject]
    public class PropModule
    {
        /// <summary>
        /// Place props in predefined positions on the planet
        /// </summary>
        public DetailInfo[] details;

        /// <summary>
        /// Add dialogue triggers to this planet
        /// </summary>
        public DialogueInfo[] dialogue;

        /// <summary>
        /// Add ship log entry locations on this planet
        /// </summary>
        public EntryLocationInfo[] entryLocation;

        /// <summary>
        /// Add Geysers to this planet
        /// </summary>
        public GeyserInfo[] geysers;

        /// <summary>
        /// Add translatable text to this planet
        /// </summary>
        public NomaiTextInfo[] nomaiText;

        /// <summary>
        /// Details which will be shown from 50km away. Meant to be lower resolution.
        /// </summary>
        public DetailInfo[] proxyDetails;

        /// <summary>
        /// Add rafts to this planet
        /// </summary>
        public RaftInfo[] rafts;

        /// <summary>
        /// Scatter props around this planet's surface
        /// </summary>
        public ScatterInfo[] scatter;

        /// <summary>
        /// Add slideshows (from the DLC) to the planet
        /// </summary>
        public ProjectionInfo[] slideShows;

        /// <summary>
        /// A list of quantum groups that props can be added to. An example of a group would be a list of possible locations for a QuantumSocketedObject.
        /// </summary>
        public QuantumGroupInfo[] quantumGroups;

        /// <summary>
        /// Add tornadoes to this planet
        /// </summary>
        public TornadoInfo[] tornados;

        /// <summary>
        /// Add volcanoes to this planet
        /// </summary>
        public VolcanoInfo[] volcanoes;

        /// <summary>
        /// Add black/white-holes to this planet
        /// </summary>
        public SingularityModule[] singularities;

        /// <summary>
        /// Add signalscope signals to this planet
        /// </summary>
        public SignalModule.SignalInfo[] signals;

        /// <summary>
        /// Add projection pools/platforms, whiteboards, and stones to this planet
        /// </summary>
        public RemoteInfo[] remotes;

        [Obsolete("reveal is deprecated. Use Volumes->revealVolumes instead.")] public VolumesModule.RevealVolumeInfo[] reveal;

        [Obsolete("audioVolumes is deprecated. Use Volumes->audioVolumes instead.")] public VolumesModule.AudioVolumeInfo[] audioVolumes;

        [JsonObject]
        public class ScatterInfo
        {
            /// <summary>
            /// Relative filepath to an asset-bundle
            /// </summary>
            public string assetBundle;

            /// <summary>
            /// Number of props to scatter
            /// </summary>
            public int count;

            /// <summary>
            /// Offset this prop once it is placed
            /// </summary>
            public MVector3 offset;

            /// <summary>
            /// Either the path in the scene hierarchy of the item to copy or the path to the object in the supplied asset bundle
            /// </summary>
            public string path;

            /// <summary>
            /// Rotate this prop once it is placed
            /// </summary>
            public MVector3 rotation;

            /// <summary>
            /// Scale this prop once it is placed
            /// </summary>
            [DefaultValue(1f)] public float scale = 1f;

            /// <summary>
            /// Scale each axis of the prop. Overrides `scale`.
            /// </summary>
            public MVector3 stretch;

            /// <summary>
            /// The number used as entropy for scattering the props
            /// </summary>
            public int seed;

            /// <summary>
            /// The lowest height that these object will be placed at (only relevant if there's a heightmap)
            /// </summary>
            public float? minHeight;

            /// <summary>
            /// The highest height that these objects will be placed at (only relevant if there's a heightmap)
            /// </summary>
            public float? maxHeight;

            /// <summary>
            /// Should we try to prevent overlap between the scattered details? True by default. If it's affecting load times turn it off.
            /// </summary>
            [DefaultValue(true)] public bool preventOverlap = true;
            
            /// <summary>
            /// Should this detail stay loaded even if you're outside the sector (good for very large props)
            /// </summary>
            public bool keepLoaded;
        }

        [JsonObject]
        public class DetailInfo
        {
            /// <summary>
            /// An optional rename of the detail
            /// </summary>
            public string rename;

            /// <summary>
            /// Do we override rotation and try to automatically align this object to stand upright on the body's surface?
            /// </summary>
            public bool alignToNormal;

            /// <summary>
            /// Relative filepath to an asset-bundle to load the prefab defined in `path` from
            /// </summary>
            public string assetBundle;

            /// <summary>
            /// Either the path in the scene hierarchy of the item to copy or the path to the object in the supplied asset bundle
            /// </summary>
            public string path;

            /// <summary>
            /// Position of this prop relative to the body's center
            /// </summary>
            public MVector3 position;

            /// <summary>
            /// A list of children to remove from this detail
            /// </summary>
            public string[] removeChildren;

            /// <summary>
            /// Do we reset all the components on this object? Useful for certain props that have dialogue components attached to
            /// them.
            /// </summary>
            public bool removeComponents;

            /// <summary>
            /// Rotate this prop
            /// </summary>
            public MVector3 rotation;

            /// <summary>
            /// Scale the prop
            /// </summary>
            [DefaultValue(1f)] public float scale = 1f;

            /// <summary>
            /// Scale each axis of the prop. Overrides `scale`.
            /// </summary>
            public MVector3 stretch;

            /// <summary>
            /// If this value is not null, this prop will be quantum. Assign this field to the id of the quantum group it should be a part of. The group it is assigned to determines what kind of quantum object it is
            /// </summary>
            public string quantumGroupID;

            /// <summary>
            /// The path (not including the root planet object) of the parent of this game object. Optional (will default to the root sector).
            /// </summary>
            public string parentPath;

            /// <summary>
            /// Should this detail stay loaded even if you're outside the sector (good for very large props)
            /// </summary>
            public bool keepLoaded;
        }

        [JsonObject]
        public class RaftInfo
        {
            /// <summary>
            /// Position of the raft
            /// </summary>
            public MVector3 position;

            /// <summary>
            /// Acceleration of the raft. Default acceleration is 5.
            /// </summary>
            [DefaultValue(5f)] public float acceleration = 5f;
        }

        [JsonObject]
        public class GeyserInfo
        {
            /// <summary>
            /// Position of the geyser
            /// </summary>
            public MVector3 position;

            /// <summary>
            /// Vertical offset of the geyser. From 0, the bubbles start at a height of 10, the shaft at 67, and the spout at 97.5.
            /// </summary>
            [DefaultValue(-97.5f)] public float offset = -97.5f;

            /// <summary>
            /// Force of the geyser on objects
            /// </summary>
            [DefaultValue(55f)] public float force = 55f;

            /// <summary>
            /// Time in seconds eruptions last for
            /// </summary>
            [DefaultValue(10f)] public float activeDuration = 10f;

            /// <summary>
            /// Time in seconds between eruptions
            /// </summary>
            [DefaultValue(19f)] public float inactiveDuration = 19f;

            /// <summary>
            /// Color of the geyser. Alpha sets the particle density.
            /// </summary>
            public MColor tint;

            /// <summary>
            /// Disable the individual particle systems of the geyser
            /// </summary>
            public bool disableBubbles, disableShaft, disableSpout;

            /// <summary>
            /// Loudness of the geyser
            /// </summary>
            [DefaultValue(0.7f)] public float volume = 0.7f;
        }

        [JsonObject]
        public class TornadoInfo
        {
            [JsonConverter(typeof(StringEnumConverter))]
            public enum TornadoType
            {
                [EnumMember(Value = @"upwards")] Upwards = 0,

                [EnumMember(Value = @"downwards")] Downwards = 1,

                [EnumMember(Value = @"hurricane")] Hurricane = 2
            }

            [Obsolete("Downwards is deprecated. Use Type instead.")] public bool downwards;

            /// <summary>
            /// Alternative to setting the position. Will choose a random place at this elevation.
            /// </summary>
            public float elevation;

            /// <summary>
            /// The height of this tornado.
            /// </summary>
            [DefaultValue(30f)] public float height = 30f;

            /// <summary>
            /// Position of the tornado
            /// </summary>
            public MVector3 position;

            /// <summary>
            /// The colour of the tornado.
            /// </summary>
            public MColor tint;

            /// <summary>
            /// What type of cyclone should this be? Upwards and downwards are both tornados and will push in that direction.
            /// </summary>
            [DefaultValue("upwards")] public TornadoType type = TornadoType.Upwards;

            /// <summary>
            /// Angular distance from the starting position that it will wander, in terms of the angle around the x-axis.
            /// </summary>
            [DefaultValue(45f)] public float wanderDegreesX = 45f;

            /// <summary>
            /// Angular distance from the starting position that it will wander, in terms of the angle around the z-axis.
            /// </summary>
            [DefaultValue(45f)] public float wanderDegreesZ = 45f;

            /// <summary>
            /// The rate at which the tornado will wander around the planet. Set to 0 for it to be stationary. Should be around
            /// 0.1.
            /// </summary>
            public float wanderRate;

            /// <summary>
            /// The maximum distance at which you'll hear the sounds of the cyclone. If not set it will scale relative to the size of the cyclone.
            /// </summary>
            public float audioDistance;

            /// <summary>
            /// Fluid type for sounds/effects when colliding with this tornado.
            /// </summary>
            [DefaultValue("cloud")] public FluidType fluidType = FluidType.Cloud;
        }

        [JsonObject]
        public class VolcanoInfo
        {
            /// <summary>
            /// The colour of the meteor's lava.
            /// </summary>
            public MColor lavaTint;

            /// <summary>
            /// Maximum time between meteor launches.
            /// </summary>
            [DefaultValue(20f)]
            public float maxInterval = 20f;

            /// <summary>
            /// Maximum random speed at which meteors are launched.
            /// </summary>
            [DefaultValue(150f)]
            public float maxLaunchSpeed = 150f;

            /// <summary>
            /// Minimum time between meteor launches.
            /// </summary>
            [DefaultValue(5f)]
            public float minInterval = 5f;

            /// <summary>
            /// Minimum random speed at which meteors are launched.
            /// </summary>
            [DefaultValue(50f)]
            public float minLaunchSpeed = 50f;

            /// <summary>
            /// Position of this volcano.
            /// </summary>
            public MVector3 position;

            /// <summary>
            /// Scale of the meteors.
            /// </summary>
            public float scale = 1;

            /// <summary>
            /// The colour of the meteor's stone.
            /// </summary>
            public MColor stoneTint;
        }

        [JsonObject]
        public class DialogueInfo
        {
            /// <summary>
            /// Prevents the dialogue from being created after a specific persistent condition is set. Useful for remote dialogue
            /// triggers that you want to have happen only once.
            /// </summary>
            public string blockAfterPersistentCondition;

            /// <summary>
            /// If a pathToAnimController is supplied, if you are within this distance the character will look at you. If it is set
            /// to 0, they will only look at you when spoken to.
            /// </summary>
            public float lookAtRadius;

            /// <summary>
            /// If this dialogue is meant for a character, this is the relative path from the planet to that character's
            /// CharacterAnimController or SolanumAnimController.
            /// </summary>
            public string pathToAnimController;

            /// <summary>
            /// When you enter into dialogue, you will look here.
            /// </summary>
            public MVector3 position;

            /// <summary>
            /// Radius of the spherical collision volume where you get the "talk to" prompt when looking at. If you use a
            /// remoteTriggerPosition, you can set this to 0 to make the dialogue only trigger remotely.
            /// </summary>
            public float radius = 1f;

            /// <summary>
            /// Allows you to trigger dialogue from a distance when you walk into an area.
            /// </summary>
            public MVector3 remoteTriggerPosition;

            /// <summary>
            /// Distance from radius the prompt appears
            /// </summary>
            [DefaultValue(2f)] public float range = 2f;

            /// <summary>
            /// The radius of the remote trigger volume.
            /// </summary>
            public float remoteTriggerRadius;

            /// <summary>
            /// Relative path to the xml file defining the dialogue.
            /// </summary>
            public string xmlFile;
        }

        [JsonObject]
        public class EntryLocationInfo
        {
            /// <summary>
            /// Whether this location is cloaked
            /// </summary>
            public bool cloaked;

            /// <summary>
            /// ID of the entry this location relates to
            /// </summary>
            public string id;

            /// <summary>
            /// The position of this entry location
            /// </summary>
            public MVector3 position;
        }

        [JsonObject]
        public class NomaiTextInfo
        {
            [JsonConverter(typeof(StringEnumConverter))]
            public enum NomaiTextType
            {
                [EnumMember(Value = @"wall")] Wall = 0,

                [EnumMember(Value = @"scroll")] Scroll = 1,

                [EnumMember(Value = @"computer")] Computer = 2,

                [EnumMember(Value = @"cairn")] Cairn = 3,

                [EnumMember(Value = @"recorder")] Recorder = 4,

                [EnumMember(Value = @"preCrashRecorder")] PreCrashRecorder = 5,

                [EnumMember(Value = @"preCrashComputer")] PreCrashComputer = 6,

                [EnumMember(Value = @"trailmarker")] Trailmarker = 7,
            }

            [JsonConverter(typeof(StringEnumConverter))]
            public enum NomaiTextLocation
            {
                [EnumMember(Value = @"unspecified")] UNSPECIFIED = 0,

                [EnumMember(Value = @"a")] A = 1,

                [EnumMember(Value = @"b")] B = 2
            }

            /// <summary>
            /// Additional information about each arc in the text
            /// </summary>
            public NomaiTextArcInfo[] arcInfo;

            /// <summary>
            /// The normal vector for this object. Used for writing on walls and positioning computers.
            /// </summary>
            public MVector3 normal;

            /// <summary>
            /// Position of the root of this text
            /// </summary>
            public MVector3 position;

            /// <summary>
            /// The euler angle rotation of this object. Not required if setting the normal. Computers and cairns will orient
            /// themselves to the surface of the planet automatically.
            /// </summary>
            public MVector3 rotation;

            /// <summary>
            /// The random seed used to pick what the text arcs will look like.
            /// </summary>
            public int seed; // For randomizing arcs

            /// <summary>
            /// The type of object this is.
            /// </summary>
            [DefaultValue("wall")] public NomaiTextType type = NomaiTextType.Wall;

            /// <summary>
            /// The location of this object. 
            /// </summary>
            [DefaultValue("unspecified")] public NomaiTextLocation location = NomaiTextLocation.UNSPECIFIED;

            /// <summary>
            /// The relative path to the xml file for this object.
            /// </summary>
            public string xmlFile;

            /// <summary>
            /// The relative path from the planet to the parent of this object. Optional (will default to the root sector).
            /// </summary>
            public string parentPath;

            /// <summary>
            /// An optional rename of this object
            /// </summary>
            public string rename;
        }

        [JsonObject]
        public class NomaiTextArcInfo
        {
            [JsonConverter(typeof(StringEnumConverter))]
            public enum NomaiTextArcType
            {
                [EnumMember(Value = @"adult")] Adult = 0,

                [EnumMember(Value = @"child")] Child = 1,

                [EnumMember(Value = @"stranger")] Stranger = 2
            }

            /// <summary>
            /// Whether to flip the spiral from left-curling to right-curling or vice versa.
            /// </summary>
            public bool mirror;

            /// <summary>
            /// The local position of this object on the wall.
            /// </summary>
            public MVector2 position;

            /// <summary>
            /// The type of text to display.
            /// </summary>
            [DefaultValue("adult")] public NomaiTextArcType type = NomaiTextArcType.Adult;

            /// <summary>
            /// Which variation of the chosen type to place. If not specified, a random variation will be selected based on the seed provided in the parent module.
            /// </summary>
            [DefaultValue(-1)] public int variation = -1;

            /// <summary>
            /// The z euler angle for this arc.
            /// </summary>
            [Range(0f, 360f)] public float zRotation;
        }

        [JsonObject]
        public class ProjectionInfo
        {
            [JsonConverter(typeof(StringEnumConverter))]
            public enum SlideShowType
            {
                [EnumMember(Value = @"slideReel")] SlideReel = 0,

                [EnumMember(Value = @"autoProjector")] AutoProjector = 1,

                [EnumMember(Value = @"visionTorchTarget")] VisionTorchTarget = 2,

                [EnumMember(Value = @"standingVisionTorch")] StandingVisionTorch = 3,

            }

            /// <summary>
            /// The position of this slideshow.
            /// </summary>
            public MVector3 position;

            /// <summary>
            /// The ship log entries revealed after finishing this slide reel.
            /// </summary>
            public string[] reveals;

            /// <summary>
            /// The rotation of this slideshow.
            /// </summary>
            public MVector3 rotation;

            /// <summary>
            /// The list of slides for this object.
            /// </summary>
            public SlideInfo[] slides;

            /// <summary>
            /// The type of object this is.
            /// </summary>
            [DefaultValue("slideReel")] public SlideShowType type = SlideShowType.SlideReel;

            /// <summary>
            /// The relative path from the planet to the parent of this slideshow. Optional (will default to the root sector).
            /// </summary>
            public string parentPath;
        }

        [JsonObject]
        public class SlideInfo
        {
            /// <summary>
            /// Ambient light colour when viewing this slide.
            /// </summary>
            public MColor ambientLightColor;


            // SlideAmbientLightModule

            /// <summary>
            /// Ambient light intensity when viewing this slide.
            /// </summary>
            public float ambientLightIntensity;

            /// <summary>
            /// Ambient light range when viewing this slide.
            /// </summary>
            public float ambientLightRange;

            // SlideBackdropAudioModule

            /// <summary>
            /// The name of the AudioClip that will continuously play while watching these slides
            /// </summary>
            public string backdropAudio;

            /// <summary>
            /// The time to fade into the backdrop audio
            /// </summary>
            public float backdropFadeTime;

            // SlideBeatAudioModule

            /// <summary>
            /// The name of the AudioClip for a one-shot sound when opening the slide.
            /// </summary>
            public string beatAudio;

            /// <summary>
            /// The time delay until the one-shot audio
            /// </summary>
            public float beatDelay;


            // SlideBlackFrameModule

            /// <summary>
            /// Before viewing this slide, there will be a black frame for this many seconds.
            /// </summary>
            public float blackFrameDuration;

            /// <summary>
            /// The path to the image file for this slide.
            /// </summary>
            public string imagePath;


            // SlidePlayTimeModule

            /// <summary>
            /// Play-time duration for auto-projector slides.
            /// </summary>
            public float playTimeDuration;


            // SlideShipLogEntryModule

            /// <summary>
            /// Ship log entry revealed when viewing this slide
            /// </summary>
            public string reveal;

            /// <summary>
            /// Spotlight intensity modifier when viewing this slide.
            /// </summary>
            public float spotIntensityMod;
        }




        [JsonConverter(typeof(StringEnumConverter))]
        public enum QuantumGroupType
        {
            [EnumMember(Value = @"sockets")] Sockets = 0,

            [EnumMember(Value = @"states")] States = 1,

            FailedValidation = 10
        }

        [JsonObject]
        public class QuantumGroupInfo
        {
            /// <summary>
            /// What type of group this is: does it define a list of states a single quantum object could take or a list of sockets one or more quantum objects could share?
            /// </summary>
            public QuantumGroupType type;

            /// <summary>
            /// A unique string used by props (that are marked as quantum) use to refer back to this group
            /// </summary>
            public string id;

            /// <summary>
            /// Only required if type is `sockets`. This lists all the possible locations for any props assigned to this group.
            /// </summary>
            public QuantumSocketInfo[] sockets;

            /// <summary>
            /// Optional. Only used if type is `states`. If this is true, then the first prop made part of this group will be used to construct a visibility box for an empty game object, which will be considered one of the states.
            /// </summary>
            public bool hasEmptyState;

            /// <summary>
            /// Optional. Only used if type is `states`. If this is true, then the states will be presented in order, rather than in a random order
            /// </summary>
            public bool sequential;

            /// <summary>
            /// Optional. Only used if type is `states` and `sequential` is true. If this is false, then after the last state has appeared, the object will no longer change state
            /// </summary>
            [DefaultValue(true)] public bool loop = true;
        }

        [JsonObject]
        public class QuantumSocketInfo
        {
            /// <summary>
            /// The location of this socket
            /// </summary>
            public MVector3 position;

            /// <summary>
            /// The rotation the quantum object will take if it's occupying this socket
            /// </summary>
            public MVector3 rotation;

            /// <summary>
            /// The probability any props that are part of this group will occupy this socket
            /// </summary>
            [DefaultValue(1f)] public float probability = 1f;
        }

        [JsonObject]
        public class RemoteInfo
        {
            /// <summary>
            /// The unique remote id
            /// </summary>
            public string id;

            /// <summary>
            /// Icon that the will show on the stone, pedastal of the whiteboard, and pedastal of the platform.
            /// </summary>
            public string decalPath;

            /// <summary>
            /// Whiteboard that the stones can put text onto
            /// </summary>
            public WhiteboardInfo whiteboard;

            /// <summary>
            /// Camera platform that the stones can project to and from
            /// </summary>
            public PlatformInfo platform;

            /// <summary>
            /// Projection stones
            /// </summary>
            public StoneInfo[] stones;

            [JsonObject]
            public class WhiteboardInfo
            {
                /// <summary>
                /// The text for each stone
                /// </summary>
                public SharedNomaiTextInfo[] nomaiText;

                /// <summary>
                /// The location of this platform.
                /// </summary>
                public MVector3 position;

                /// <summary>
                /// The rotation of this platform.
                /// </summary>
                public MVector3 rotation;

                /// <summary>
                /// The relative path from the planet to the parent of this object. Optional (will default to the root sector).
                /// </summary>
                public string parentPath;

                /// <summary>
                /// An optional rename of this object
                /// </summary>
                public string rename;

                /// <summary>
                /// Disable the wall, leaving only the pedestal and text.
                /// </summary>
                public bool disableWall;

                [JsonObject]
                public class SharedNomaiTextInfo
                {
                    /// <summary>
                    /// The id of the stone this text will appear for
                    /// </summary>
                    public string id;

                    /// <summary>
                    /// Additional information about each arc in the text
                    /// </summary>
                    public NomaiTextArcInfo[] arcInfo;

                    /// <summary>
                    /// The random seed used to pick what the text arcs will look like.
                    /// </summary>
                    public int seed; // For randomizing arcs

                    /// <summary>
                    /// The location of this object. 
                    /// </summary>
                    [DefaultValue("unspecified")] public NomaiTextInfo.NomaiTextLocation location = NomaiTextInfo.NomaiTextLocation.UNSPECIFIED;

                    /// <summary>
                    /// The relative path to the xml file for this object.
                    /// </summary>
                    public string xmlFile;

                    /// <summary>
                    /// An optional rename of this object
                    /// </summary>
                    public string rename;
                }
            }

            [JsonObject]
            public class PlatformInfo
            {
                /// <summary>
                /// The location of this platform.
                /// </summary>
                public MVector3 position;

                /// <summary>
                /// The rotation of this platform.
                /// </summary>
                public MVector3 rotation;

                /// <summary>
                /// The relative path from the planet to the parent of this object. Optional (will default to the root sector).
                /// </summary>
                public string parentPath;

                /// <summary>
                /// An optional rename of this object
                /// </summary>
                public string rename;

                /// <summary>
                /// A ship log fact to reveal when the platform is connected to.
                /// </summary>
                [DefaultValue("")] public string reveals = "";

                /// <summary>
                /// Disable the structure, leaving only the pedestal.
                /// </summary>
                public bool disableStructure;

                /// <summary>
                /// Disable the pool that rises when you place a stone.
                /// </summary>
                public bool disablePool;
            }

            [JsonObject]
            public class StoneInfo
            {
                /// <summary>
                /// The location of this stone.
                /// </summary>
                public MVector3 position;

                /// <summary>
                /// The rotation of this stone.
                /// </summary>
                public MVector3 rotation;

                /// <summary>
                /// The relative path from the planet to the parent of this object. Optional (will default to the root sector).
                /// </summary>
                public string parentPath;

                /// <summary>
                /// An optional rename of this object
                /// </summary>
                public string rename;
            }
        }
    }
}