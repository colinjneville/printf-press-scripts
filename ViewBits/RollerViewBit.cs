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

public sealed class RollerViewBit : MonoBehaviour, IApertureTarget {
#pragma warning disable CS0649
    [SerializeField]
    private RollerModifyPoint modifyPoint;
    [SerializeField]
    private FrameInsertPoint frameInsertPoint;
    [SerializeField]
    private Image topSprite;
    [SerializeField]
    private Image lightSprite;
    [SerializeField]
    private Tween tween;
    [SerializeField]
    private RectTransform frameContainer;
#pragma warning restore CS0649

    public WorkspaceFull Workspace {
        get => modifyPoint.Workspace;
        set {
            modifyPoint.Workspace = value;
            frameInsertPoint.Workspace = value;
        }
    }

    public Roller Roller {
        get => modifyPoint.Roller;
        set {
            modifyPoint.Roller = value;
            frameInsertPoint.Roller = value;
        }
    }

    public Sprite TopSprite {
        get => topSprite.sprite;
        set => topSprite.sprite = value;
    }

    public Color LightColor {
        get => lightSprite.color;
        set => lightSprite.color = value;
    }

    public IDisposable Unlock() => tween.Unlock();

    public RectTransform FrameContainer => frameContainer;

    Bounds2D IApertureTarget.Bounds => topSprite.GetComponent<RectTransform>().GetWorldBounds();
}
