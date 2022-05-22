﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using NewHorizons.Utility;
using OWML.Common;

namespace NewHorizons.Handlers
{
    class SubtitlesHandler : MonoBehaviour
    {
        public static int SUBTITLE_HEIGHT = 97;
        public static int SUBTITLE_WIDTH = 669; // nice

        Graphic graphic;
        Image image;

        public float fadeSpeed = 0.01f;
        float fade = 1;
        bool fadingAway = true;

        static List<Sprite> possibleSubtitles = new List<Sprite>();
        static bool eoteSubtitleHasBeenInserted = false;
        int subtitleIndex;

        System.Random randomizer;

        public void Start()
        {
            randomizer = new System.Random();

            graphic = GetComponent<Graphic>();
            image =   GetComponent<UnityEngine.UI.Image>();

            if (!eoteSubtitleHasBeenInserted)
            {
                possibleSubtitles.Insert(0, image.sprite); // ensure that the Echoes of the Eye subtitle always appears first
                eoteSubtitleHasBeenInserted = true;
            }
        }

        public static void AddSubtitle(IModBehaviour mod, string filepath)
        {
            
            var tex = ImageUtilities.GetTexture(mod, filepath);
            if (tex == null) return;

            // var sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);
            var sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, SUBTITLE_HEIGHT), new Vector2(0.5f, 0.5f), 100.0f);
            AddSubtitle(sprite);
        }

        public static void AddSubtitle(Sprite sprite)
        {
            possibleSubtitles.Add(sprite);
        }

        public void Update()
        {
            // don't fade transition subtitles if there's only one subtitle
            if (possibleSubtitles.Count <= 1) return;

            if (fadingAway)
            {
                fade -= fadeSpeed;
            
                if (fade <= 0)
                {
                    fade = 0;
                    ChangeSubtitle();
                    fadingAway = false;
                }
            }
            else
            {
                fade += fadeSpeed;
            
                if (fade >= 1)
                {
                    fade = 1;
                    fadingAway = true;
                }
            }

            graphic.color = new Color(1, 1, 1, fade);
        }

        public void ChangeSubtitle()
        {
            // to pick a new random subtitle without requiring retries, we generate a random offset less than the length of the possible subtitles array
            // we then add that offset to the current index, modulo NUMBER_OF_POSSIBLE_SUBTITLES
            // since the offset can never be NUMBER_OF_POSSIBLE_SUBTITLES, it will never wrap all the way back around to the initial subtitleIndex

            // note, this makes the code more confusing, but Random.Next(min, max) generates a random number on the range [min, max)
            // that is, the below code will generate numbers up to and including Count-1, not Count.
            var newIndexOffset = randomizer.Next(1, possibleSubtitles.Count);
            subtitleIndex = (subtitleIndex % newIndexOffset) % possibleSubtitles.Count;
            
            image.sprite = possibleSubtitles[subtitleIndex];
        }
    }
}
