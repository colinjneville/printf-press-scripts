using Functional.Option;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using L = LocalizationString;
using LC = LocalizationConstant;
using LE = ILocalizationExpression;
using LF = LocalizationFormat;
using LI = LocalizationInt;
public abstract class ArrowViewBit : MonoBehaviour {
#pragma warning disable CS0649
#pragma warning restore CS0649
    [SerializeField]
    [HideInInspector]
    private Color baseColor;
    [SerializeField]
    [HideInInspector]
    private float delay;

    public Color BaseColor {
        get => baseColor;
        set => baseColor = value;
    }

    public float Delay {
        get => delay;
        set {
            delay = value;
            float r = BaseColor.r;
            float g = BaseColor.g;
            float b = BaseColor.b;
            float luma = r * 0.3f + g * 0.6f + b * 0.1f;
            float desat = delay;
            Color = new Color(r + desat * (luma - r), g + desat * (luma - g), b + desat * (luma - b));
        }
    }

    protected abstract Color Color { get; set; }
}
