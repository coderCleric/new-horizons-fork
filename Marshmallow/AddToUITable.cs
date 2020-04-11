﻿using OWML.ModHelper.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Marshmallow.UI
{
    static class AddToUITable
    {
        public static int Add(string text)
        {
            TextTranslation.TranslationTable instance = GameObject.FindObjectOfType<TextTranslation>().GetValue<TextTranslation.TranslationTable>("m_table");

            instance.Insert_UI(instance.theUITable.Keys.Max() + 1, text);

            Main.Log("Added [" + text + "] to UI table with key [" + instance.theUITable.Keys.Max() + "]");

            return instance.theUITable.Keys.Max();
        }
    }
}
