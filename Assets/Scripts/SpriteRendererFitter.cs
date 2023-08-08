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

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(SpriteRenderer))]
public sealed class SpriteRendererFitter : UnityEngine.EventSystems.UIBehaviour {
    private RectTransform rt;
    private SpriteRenderer spriteRenderer;
    private bool dirty;

    protected override void Awake() {
        dirty = true;
    }

    protected override void Start() {
        rt = GetComponent<RectTransform>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void LateUpdate() {
        if (dirty) {
            spriteRenderer.size = rt.rect.size;
            dirty = false;
        }
    }

    protected override void OnRectTransformDimensionsChange() {
        dirty = true;
    }
}
