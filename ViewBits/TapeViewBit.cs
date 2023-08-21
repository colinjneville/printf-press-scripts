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

public sealed class TapeViewBit : MonoBehaviour {
#pragma warning disable CS0649
    [SerializeField]
    private RectTransform spriteRect;
    [SerializeField]
    private RectTransform valueContainer;
    [SerializeField]
    private Image image;
    [SerializeField]
    private Tween tween;
#pragma warning restore CS0649

    public IDisposable Unlock() => tween.Unlock();

    public RectTransform SpriteRect => spriteRect;

    public RectTransform ValueContainer => valueContainer;
}
