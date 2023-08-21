using Functional.Option;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

using LE = ILocalizationExpression;
using L = LocalizationString;
using LC = LocalizationConstant;
using LF = LocalizationFormat;
using LI = LocalizationInt;
using TMPro;

public sealed class NoteViewBit : MonoBehaviour {
#pragma warning disable CS0649
    [SerializeField]
    private TMPro.TMP_Text tmp;
#pragma warning restore CS0649

    public string Text {
        get => tmp.text ?? "";
        set {
            tmp.text = value;
            UpdateMaxTextSize();
        }
    }

    public TMP_Text TMP => tmp;

    private void UpdateMaxTextSize() {
        string text = tmp.text;
        
        tmp.fontSizeMax = 10000;
        // Size for a minimum of 4 lines
        tmp.text = "a\nb\nc\nd";
        tmp.enableAutoSizing = true;
        tmp.ForceMeshUpdate(ignoreActiveState: true);
        var size = tmp.fontSize;
        tmp.text = text;
        tmp.fontSizeMax = size;
    }
}
