using NewHorizons.External.Modules;
using NewHorizons.Utility;
using UnityEngine;
using Logger = NewHorizons.Utility.Logger;

namespace NewHorizons.Builder.Props
{
    public static class GeyserBuilder
    {
        private static GameObject _geyserPrefab;

        internal static void InitPrefab()
        {
            if (_geyserPrefab == null) _geyserPrefab = SearchUtilities.Find("TimberHearth_Body/Sector_TH/Interactables_TH/Geysers/Geyser_Village").InstantiateInactive().Rename("Prefab_TH_Geyser").DontDestroyOnLoad();
        }

        public static void Make(GameObject planetGO, Sector sector, PropModule.GeyserInfo info)
        {
            InitPrefab();

            var geyserGO = GeneralPropBuilder.MakeFromPrefab(_geyserPrefab, "Geyser", sector?.transform ?? planetGO.transform, info);
            
            var pos = planetGO.transform.InverseTransformPoint(geyserGO.transform.position);

            // Offset height, default -97.5 pushes it underground so the spout is at the surface
            var length = pos.magnitude + info.offset;

            // About 130 high, bubbles start at 10, shaft starts at 67, spout starts at 97.5
            geyserGO.transform.position = planetGO.transform.TransformPoint(pos.normalized * length);

            geyserGO.transform.localScale = Vector3.one;

            // Geyser alignment is technically incorrect due to inheriting the prefab's rotation but we need backwards compat so explicitly applying it here
            geyserGO.transform.rotation = _geyserPrefab.transform.rotation;
            var up = planetGO.transform.TransformPoint(pos) - planetGO.transform.position;
            geyserGO.transform.rotation = Quaternion.FromToRotation(geyserGO.transform.up, up) * _geyserPrefab.transform.rotation;

            var bubbles = geyserGO.FindChild("GeyserParticles/GeyserBubbles");
            var shaft = geyserGO.FindChild("GeyserParticles/GeyserShaft");
            var spout = geyserGO.FindChild("GeyserParticles/GeyserSpout");

            if (info.tint != null)
            {
                var tint = info.tint.ToColor();
                bubbles.GetComponent<ParticleSystemRenderer>().material.color = new Color(tint.r, tint.g, tint.b, Mathf.LerpUnclamped(0.5f, 1f, tint.a)); //bubbles disappear at 0.5 alpha
                shaft.GetComponent<ParticleSystemRenderer>().material.color = tint;
                spout.GetComponent<ParticleSystemRenderer>().material.color = tint;
            }

            if (info.disableBubbles) bubbles.SetActive(false);
            if (info.disableShaft) shaft.SetActive(false);
            if (info.disableSpout) spout.SetActive(false);

            var geyserController = geyserGO.GetComponent<GeyserController>();
            geyserController._activeDuration = info.activeDuration;
            geyserController._inactiveDuration = info.inactiveDuration;

            geyserGO.SetActive(true);

            var geyserFluidVolume = geyserGO.GetComponentInChildren<GeyserFluidVolume>();

            // Do this after awake
            Delay.FireOnNextUpdate(() => geyserFluidVolume._maxHeight = 1);

            if (info.force == 0f) geyserFluidVolume.enabled = false;
            else
            {
                geyserFluidVolume.enabled = true; // why do we enable this? idk
                geyserFluidVolume.GetComponent<CapsuleShape>().enabled = true; // i think this is already enabled but wtv

                geyserFluidVolume._attractionalFlowSpeed *= info.force / 55f;
                geyserFluidVolume._directionalFlowSpeed = info.force;
            }

            geyserGO.GetComponent<GeyserAudioController>().SetSector(sector);
            var oneShotAudio = geyserGO.FindChild("Geyser_OneShotAudioSrc");
            var loopAudio = geyserGO.FindChild("Geyser_LoopAudioSrc");
            oneShotAudio.GetComponent<AudioSpreadController>().SetSector(sector);
            loopAudio.GetComponent<AudioSpreadController>().SetSector(sector);

            Delay.FireOnNextUpdate(() => {
                if (info.volume == 0)
                {
                    oneShotAudio.SetActive(false);
                    loopAudio.SetActive(false);
                }
                else
                {
                    oneShotAudio.GetComponent<OWAudioSource>().SetMaxVolume(info.volume);
                    loopAudio.GetComponent<OWAudioSource>().SetMaxVolume(info.volume);
                }
            });

            // If it starts at the shaft, move the start/end sounds to it
            if ((info.disableSpout && !info.disableShaft) || info.offset == -67f)
            {
                oneShotAudio.transform.SetLocalPositionY(67f);
            }
        }
    }
}
