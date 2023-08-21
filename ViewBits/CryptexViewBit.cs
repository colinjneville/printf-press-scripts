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

public sealed class CryptexViewBit : MonoBehaviour, IApertureTarget {
#pragma warning disable CS0649
    [SerializeField]
    private CryptexModifyPoint modifyPoint;
    [SerializeField]
    private RectTransform offsetRect;
    [SerializeField]
    private RectTransform labelInsertPointContainer;
    [SerializeField]
    private RectTransform labelContainer;
    [SerializeField]
    private RectTransform tapeContainer;
    [SerializeField]
    private RectTransform rollerContainer;
    [SerializeField]
    private RectTransform widthContainer;
    [SerializeField]
    private RectTransform backdrop;
    [SerializeField]
    private Image image;
    [SerializeField]
    private LookaheadView lookahead;
#pragma warning restore CS0649

    public WorkspaceFull Workspace {
        get => modifyPoint.Workspace;
        set => modifyPoint.Workspace = value;
    }

    public Cryptex Cryptex {
        get => modifyPoint.Cryptex;
        set => modifyPoint.Cryptex = value;
    }

    public RectTransform LabelInsertPointContainer => labelInsertPointContainer;

    public RectTransform LabelContainer => labelContainer;

    public RectTransform TapeContainer => tapeContainer;

    public RectTransform RollerContainer => rollerContainer;

    public RectTransform OffsetRect => offsetRect;

    public float Width {
        get => widthContainer.sizeDelta.x;
        set => widthContainer.sizeDelta = widthContainer.sizeDelta.WithX(value);
    }

    public int TapeCount {
        set => backdrop.anchorMin = backdrop.anchorMin.WithY(backdrop.anchorMax.y - Mathf.Max(value * 1.5f, 0.5f));
    }

    public void AddArrow(ArrowInfo arrow) => lookahead.AddArrow(arrow);
    public void RemoveArrow(ArrowInfo arrow) => lookahead.RemoveArrow(arrow);
    public void UpdateArrows(int currentTime, int lookaheadCount) => lookahead.UpdateArrowViews(currentTime, lookaheadCount);

    Bounds2D IApertureTarget.Bounds => widthContainer.GetWorldBounds();
}
