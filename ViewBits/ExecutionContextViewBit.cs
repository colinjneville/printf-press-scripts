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

public sealed class ExecutionContextViewBit : MonoBehaviour {
#pragma warning disable CS0649
    //[SerializeField]
    //private TMPro.TMP_Text costTmp;
    [SerializeField]
    private TMPro.TMP_Text energyTmp;
#pragma warning restore CS0649

    //public string CostText {
    //    get => costTmp.text;
    //    set => costTmp.text = value;
    //}

    public string EnergyText {
        get => energyTmp.text;
        set => energyTmp.text = value;
    }
}
