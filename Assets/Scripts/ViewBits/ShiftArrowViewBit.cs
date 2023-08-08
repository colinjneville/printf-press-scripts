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

public class ShiftArrowViewBit : ArrowViewBit {
#pragma warning disable CS0649
    [SerializeField]
    private Image arrowStart;
    [SerializeField]
    private Image arrowMiddle;
    [SerializeField]
    private Image arrowEndCap;
#pragma warning restore CS0649

    protected override Color Color {
        get => arrowStart.color;
        set {
            arrowStart.color = value;
            arrowMiddle.color = value;
            arrowEndCap.color = value;
        }
    }
}
