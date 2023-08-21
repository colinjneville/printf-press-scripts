using Functional.Option;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

using L = LocalizationString;
using LC = LocalizationConstant;
using LE = ILocalizationExpression;
using LF = LocalizationFormat;
using LI = LocalizationInt;

[RequireComponent(typeof(RectTransform))]
public sealed class ToolboxTabViewBit : MonoBehaviour {
#pragma warning disable CS0649
    [SerializeField]
    private TMPro.TMP_Text text;
    [SerializeField]
    private ToolboxTabButton button;
#pragma warning restore CS0649

    private RectTransform rt;

    public string Text {
        get => text.text;
        set => text.text = value;
    }

    public Toolbox Toolbox {
        get => button.Toolbox;
        set => button.Toolbox = value;
    }

    public int PageIndex {
        get => button.PageIndex;
        set => button.PageIndex = value;
    }

    public TMPro.TMP_Text TextMeshPro => text;

    public RectTransform RectTransform => rt ?? (rt = GetComponent<RectTransform>());
}
