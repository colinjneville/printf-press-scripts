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

public sealed class InvalidArrowViewBit : ArrowViewBit {
#pragma warning disable CS0649
    [SerializeField]
    private Graphic x;
#pragma warning restore CS0649

    protected override Color Color {
        get => x.color;
        set => x.color = value;
    }
}