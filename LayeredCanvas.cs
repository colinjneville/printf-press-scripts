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

[RequireComponent(typeof(Canvas))]
public sealed class LayeredCanvas : MonoBehaviour {
    [HideInInspector]
    [SerializeField]
    private List<RectTransform> layers;

    private void Awake() {
        layers = new List<RectTransform>();
    }

    public RectTransform this[int index] => Get(index);

    private RectTransform Get(int index) {
        Assert.Within(index, 0, 100);
        if (layers.Count <= index) {
            layers.AddRange(Enumerable.Repeat(default(RectTransform), index - layers.Count + 1));
        }

        var rt = layers[index];
        if (rt == null) {
            var go = new GameObject($"Layer {index:D2}");
            rt = go.AddComponent<RectTransform>();
            rt.SetParent(transform, false);
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;
            // This might not be the most efficient way to do this, but making new layers is a very infrequent event
            for (int i = index + 1; i < layers.Count; ++i) {
                var layer = layers[i];
                layer?.SetAsLastSibling();
            }
            layers[index] = rt;
        }

        return rt;
    }

    [EasyButtons.Button("Add Layer")]
    private void AddLayer() {
        Get(layers.Count);
    }
}
