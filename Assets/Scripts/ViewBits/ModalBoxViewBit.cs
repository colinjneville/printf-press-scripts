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

public sealed class ModalBoxViewBit : MonoBehaviour {
#pragma warning disable CS0649
    [SerializeField]
    private LayoutGroup buttonGroup;
    [SerializeField]
    private LayoutElement content;
    [SerializeField]
    private List<Button> buttons;
#pragma warning restore CS0649

    public LayoutElement Content {
        get => content;
        set => content = value;
    }

    public IEnumerable<Button> Buttons {
        get => buttons;
        set {
            buttonGroup.transform.DetachChildren();
            buttons = value.ToList();
            foreach (var button in value) {
                button.transform.parent = buttonGroup.transform;
            }
        }
    }
}
