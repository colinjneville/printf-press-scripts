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

public sealed class LevelButtonViewBit : MonoBehaviour {
#pragma warning disable CS0649
    [SerializeField]
    private LevelButton levelButton;
    [SerializeField]
    private UnityEngine.UI.LayoutGroup starGroup;
#pragma warning restore CS0649

    public LevelButton Button => levelButton;

    public UnityEngine.UI.LayoutGroup StarGroup => starGroup;
}
