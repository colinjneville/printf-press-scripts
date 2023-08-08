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
public sealed class ScaleBinding : MonoBehaviour {
    private RectTransform rt;
    private Option<float> prevWidth;
    private Option<float> prevHeight;

    private void Start() {
        rt = GetComponent<RectTransform>();
    }

    private void LateUpdate() {
        var rect = rt.rect;
        if (prevWidth != rect.width || prevHeight != rect.height) {
            prevWidth = rect.width;
            prevHeight = rect.height;

            rt.localScale = new Vector3(rect.width, rect.height, 1f);
            rt.sizeDelta = Vector2.zero;
        }
    }
}
