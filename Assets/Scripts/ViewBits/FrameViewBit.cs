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

public sealed class FrameViewBit : MonoBehaviour, IApertureTarget {
#pragma warning disable CS0649
    [SerializeField]
    private FrameModifyPoint modifyPoint;
    [SerializeField]
    private FrameInsertPoint insertPoint;
    [SerializeField]
    private Image frameSprite;
    [SerializeField]
    private Image bridgeSprite;
#pragma warning restore CS0649

    public WorkspaceFull Workspace {
        get => modifyPoint.Workspace;
        set {
            modifyPoint.Workspace = value;
            insertPoint.Workspace = value;
        }
    }

    public Roller Roller {
        get => modifyPoint.Roller;
        set {
            modifyPoint.Roller = value;
            insertPoint.Roller = value;
        }
    }

    public int Index {
        get => modifyPoint.Index;
        set {
            modifyPoint.Index = value;
            insertPoint.Index = value + 1;
        }
    }

    public Sprite FrameSprite {
        get => frameSprite.sprite;
        set => frameSprite.sprite = value;
    }

    public Sprite BridgeSprite {
        get => bridgeSprite.sprite;
        set => bridgeSprite.sprite = value;
    }

    public bool IsBridgeVisible {
        get => bridgeSprite.enabled;
        set => bridgeSprite.enabled = value;
    }

    Bounds2D IApertureTarget.Bounds {
        get {
            var bounds = frameSprite.GetComponent<RectTransform>().GetWorldBounds();
            if (IsBridgeVisible) {
                bounds.Encapsulate(bridgeSprite.GetComponent<RectTransform>().GetWorldBounds());
            }
            return bounds;
        }
    }
}
