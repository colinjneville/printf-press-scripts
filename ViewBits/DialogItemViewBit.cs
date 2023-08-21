using Functional.Option;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using L = LocalizationString;
using LC = LocalizationConstant;
using LE = ILocalizationExpression;
using LF = LocalizationFormat;
using LI = LocalizationInt;

public sealed class DialogItemViewBit : MonoBehaviour {
#pragma warning disable CS0649
    [SerializeField]
    private TMP_Text tmp;
    [SerializeField]
    private Image leftCharacter;
    [SerializeField]
    private Image rightCharacter;
#pragma warning restore CS0649

    public string Text {
        get => tmp.text;
        set => tmp.text = value;
    }

    public Option<Sprite> LeftCharacter {
        get => leftCharacter.sprite;
        set {
            leftCharacter.sprite = value.ValueOrDefault;
            leftCharacter.enabled = value.HasValue;
        }
    }

    public Option<Sprite> RightCharacter {
        get => rightCharacter.sprite;
        set {
            rightCharacter.sprite = value.ValueOrDefault;
            rightCharacter.enabled = value.HasValue;
        }
    }
}
