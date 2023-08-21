using Functional.Option;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using L = LocalizationString;
using LC = LocalizationConstant;
using LE = ILocalizationExpression;
using LF = LocalizationFormat;
using LI = LocalizationInt;

[RequireComponent(typeof(RectTransform))]
[DisallowMultipleComponent]
[ExecuteInEditMode]
public sealed class InvertFitter : UIBehaviour, ILayoutSelfController, ILayoutController {
    private bool delayedSetDirty;
    private DrivenRectTransformTracker tracker;

    [SerializeField]
    private Vector2 sizeDelta;

    public Vector2 SizeDelta {
        get {
            return sizeDelta;
        }
        set {
            sizeDelta = value;
            delayedSetDirty = true;
        }
    }

    [NonSerialized]
    private RectTransform rectTransform;

    private RectTransform RectTransform {
        get {
            if (rectTransform == null) {
                rectTransform = GetComponent<RectTransform>();
            }
            return rectTransform;
        }
    }

    protected override void OnEnable() {
        base.OnEnable();
        delayedSetDirty = true;
    }

    /// <summary>
    ///   <para>See MonoBehaviour.OnDisable.</para>
    /// </summary>
    protected override void OnDisable() {
        tracker.Clear();
        LayoutRebuilder.MarkLayoutForRebuild(RectTransform);
        base.OnDisable();
    }

    private void Update() {
        if (delayedSetDirty) {
            delayedSetDirty = false;
            SetDirty();
        }
    }

    protected override void OnRectTransformDimensionsChange() {
        UpdateRect();
    }

    private void UpdateRect() {
        if (IsActive()) {
            var parentSize = GetParentSize();
            tracker.Clear();
            tracker.Add(this, RectTransform, DrivenTransformProperties.SizeDelta);
            RectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, parentSize.y + SizeDelta.x);
            RectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, parentSize.x + SizeDelta.y);
        }
    }

    private Vector2 GetParentSize() {
        var rectTransform = RectTransform.parent as RectTransform;
        Vector2 result;
        if (!rectTransform) {
            result = Vector2.zero;
        } else {
            result = rectTransform.rect.size;
        }
        return result;
    }

    /// <summary>
    ///   <para>Method called by the layout system.</para>
    /// </summary>
    public void SetLayoutHorizontal() {
    }

    /// <summary>
    ///   <para>Method called by the layout system.</para>
    /// </summary>
    public void SetLayoutVertical() {
    }

    /// <summary>
    ///   <para>Mark the AspectRatioFitter as dirty.</para>
    /// </summary>
    private void SetDirty() {
        UpdateRect();
    }
}
