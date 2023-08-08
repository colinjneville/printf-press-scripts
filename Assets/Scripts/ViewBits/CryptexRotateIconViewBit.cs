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

public sealed class CryptexRotateIconViewBit : MonoBehaviour {
#pragma warning disable CS0649
    [SerializeField]
    private Image targetImage;
    [SerializeField]
    private Image image;
    [SerializeField]
    private Tween tween;
    [SerializeField]
    [HideInInspector]
    private bool rotate;
#pragma warning restore CS0649

    public bool Rotate {
        get => rotate;
        set {
            rotate = value;
            image.sprite = rotate ? Overseer.GlobalAssets.CryptexRotateTrueIcon : Overseer.GlobalAssets.CryptexRotateFalseIcon;
        }
    }

    public float TargetAlpha {
        get => targetImage.color.a;
        set => targetImage.color = targetImage.color.WithA(value);
    }

    public float Alpha {
        get => image.color.a;
        set => image.color = image.color.WithA(value);
    }

    // HACK
    public void RegisterDimensions() => tween.RegisterDimensions();

    public IDisposable Unlock() => tween.Unlock();
}
