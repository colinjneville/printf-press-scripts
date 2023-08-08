using Functional.Option;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.UI;
using L = LocalizationString;
using LC = LocalizationConstant;
using LE = ILocalizationExpression;
using LF = LocalizationFormat;
using LI = LocalizationInt;

public class HopArrowSegmentViewBit : MonoBehaviour {
#pragma warning disable CS0649
    [SerializeField]
    private Graphic start;
    [SerializeField]
    private Graphic middle;
    [SerializeField]
    private Graphic endCap;
#pragma warning restore CS0649

    public Color Color {
        get => endCap.color;
        set {
            start.color = value;
            middle.color = value;
            endCap.color = value;
        }
    }
}
