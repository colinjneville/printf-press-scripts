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

public sealed class StarViewBit : MonoBehaviour {
#pragma warning disable CS0649
    [SerializeField]
    private Image image;
#pragma warning restore CS0649

    public bool Earned {
        get => image.sprite == Overseer.GlobalAssets.StarEarnedIcon;
        set {
            image.sprite = value
                ? Overseer.GlobalAssets.StarEarnedIcon
                : Overseer.GlobalAssets.StarUnearnedIcon;
        }
    }
}
