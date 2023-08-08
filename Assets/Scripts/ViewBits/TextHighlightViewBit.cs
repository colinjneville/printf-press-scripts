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

public sealed class TextHighlightViewBit : MonoBehaviour {
#pragma warning disable CS0649
    [SerializeField]
    private Image image;
#pragma warning restore CS0649

    public Color Color {
        get => image.color;
        set => image.color = value;
    }

    public Vector2 Position {
        get => GetComponent<RectTransform>().position;
        set => GetComponent<RectTransform>().position = value;
    }

    public Vector2 Size {
        get => GetComponent<RectTransform>().sizeDelta;
        set => GetComponent<RectTransform>().sizeDelta = value;
    }

    public Quaternion Rotation {
        get => GetComponent<RectTransform>().rotation;
        set => GetComponent<RectTransform>().rotation = value;
    }
}