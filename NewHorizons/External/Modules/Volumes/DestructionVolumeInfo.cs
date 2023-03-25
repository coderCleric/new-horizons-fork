using Newtonsoft.Json;
using System.ComponentModel;

namespace NewHorizons.External.Modules.Volumes
{
    [JsonObject]
    public class DestructionVolumeInfo : VanishVolumeInfo
    {
        /// <summary>
        /// The type of death the player will have if they enter this volume.
        /// </summary>
        [DefaultValue("default")] public DeathType deathType = DeathType.Default;
    }

}
