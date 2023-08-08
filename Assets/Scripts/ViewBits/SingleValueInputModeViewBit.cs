using Functional.Option;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using L = LocalizationString;
using LC = LocalizationConstant;
using LE = ILocalizationExpression;
using LF = LocalizationFormat;
using LI = LocalizationInt;

public sealed class SingleValueInputModeViewBit : MonoBehaviour {
#pragma warning disable CS0649
    [SerializeField]
    private TMP_Text tmp;
    [SerializeField]
    private Image lockedIcon;
#pragma warning restore CS0649

    public string Text {
        get => tmp.text;
        set => tmp.text = value;
    }

    public bool Locked {
        get => lockedIcon.enabled;
        set => lockedIcon.enabled = value;
    }

    public TMP_TextInfo TextInfo => string.IsNullOrEmpty(Text) ? tmp.GetTextInfo(" ") : tmp.textInfo;

    public TMP_Text TMP => tmp;
}
