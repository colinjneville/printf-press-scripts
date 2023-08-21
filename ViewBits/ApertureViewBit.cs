using Functional.Option;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.UI;
using L = LocalizationString;
using LC = LocalizationConstant;
using LE = ILocalizationExpression;
using LF = LocalizationFormat;
using LI = LocalizationInt;

public interface IApertureTarget {
    Bounds2D Bounds { get; }
}

public sealed class ApertureTarget : IApertureTarget {
    private ApertureTarget() { }

    Bounds2D IApertureTarget.Bounds => new Bounds2D();

    private static IApertureTarget none;

    public static IApertureTarget None {
        get {
            if (none == null) {
                none = new ApertureTarget();
            }
            return none;
        }
    }
}

public sealed class ApertureViewBit : MonoBehaviour {
#pragma warning disable CS0649
    [SerializeField]
    private TransformProxy proxy;
    [SerializeField]
    private RectTransform mask;
    [SerializeField]
    private RectTransform overlay;
    [SerializeField]
    private Tween tween;
#pragma warning restore CS0649

    private void Start() {
        // Allow the overlay to match the grandparent of the transform (screen size), instead of the parent (cutout size)
        proxy.CreateReceiver(overlay.gameObject);
    }

    public void SetTarget(IApertureTarget target, bool snapPosition = false) {
        var bounds = target.Bounds;
        float innerSize = Mathf.Min(Mathf.Max(bounds.Size.x, bounds.Size.y), bounds.Size.x * maxRatio, bounds.Size.y * maxRatio);
        float outerSize = innerSize * Mathf.Sqrt(2f);

        // Snap by moving outside of the Unlock
        if (snapPosition) {
            mask.position = bounds.Center;
        }

        using (tween.Unlock()) {
            mask.position = bounds.Center;
            mask.sizeDelta = new Vector2(outerSize, outerSize);
        }
    }

    public void Skip() => tween.Skip();

    private const float maxRatio = 2f;
}
