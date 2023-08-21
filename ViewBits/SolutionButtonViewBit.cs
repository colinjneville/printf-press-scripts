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

public sealed class SolutionButtonViewBit : MonoBehaviour {
#pragma warning disable CS0649
    [SerializeField]
    private SolutionButton solutionButton;
    [SerializeField]
    private TMPro.TMP_Text costText;
#pragma warning restore CS0649

    public SolutionButton Button => solutionButton;

    public string CostText {
        get => costText.text;
        set => costText.text = value;
    }
}
