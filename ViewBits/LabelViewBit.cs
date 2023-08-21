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

public sealed class LabelViewBit : MonoBehaviour, IApertureTarget {
#pragma warning disable CS0649
    [SerializeField]
    private Image image;
    [SerializeField]
    private LabelModifyPoint modifyPoint;
    [SerializeField]
    private TMPro.TMP_Text tmp;
#pragma warning restore CS0649
    [HideInInspector]
    [SerializeField]
    private bool highlight;

    public WorkspaceFull Workspace {
        get => modifyPoint.Workspace;
        set => modifyPoint.Workspace = value;
    }

    public Cryptex Cryptex {
        get => modifyPoint.Cryptex;
        set => modifyPoint.Cryptex = value;
    }

    public Label Label {
        get => modifyPoint.Label;
        set => modifyPoint.Label = value;
    }

    public string Text {
        get => tmp.text;
        set => tmp.text = value;
    }

    public TMPro.TMP_Text TMP => tmp;

    public const float Height = 3f;

    Bounds2D IApertureTarget.Bounds => GetComponent<RectTransform>().GetWorldBounds();
}
