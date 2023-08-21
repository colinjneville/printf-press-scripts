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

public sealed class HopArrowViewBit : ArrowViewBit {
#pragma warning disable CS0649
    [SerializeField]
    private Graphic arrowStart;
    [SerializeField]
    private Graphic arrowMiddle;
    [SerializeField]
    private Graphic arrowEndCap;
    [SerializeField]
    private LayoutGroup segmentGroup;
#pragma warning restore CS0649
    [SerializeField]
    [HideInInspector]
    private List<HopArrowSegmentViewBit> segments;

    private void Awake() {
        segments = new List<HopArrowSegmentViewBit>();
    }

    public int Distance {
        get => segments.Count + 1;
        set {
            Assert.Positive(value);

            arrowMiddle.gameObject.SetActive(value == 1);

            while (segments.Count >= value) {
                var segment = segments[segments.Count - 1];
                segments.RemoveAt(segments.Count - 1);
                Utility.DestroyGameObject(segment);
            }
            while (segments.Count < value - 1) {
                var segment = Utility.Instantiate(Overseer.GlobalAssets.HopArrowSegmentPrefab, segmentGroup.transform);
                segment.transform.SetAsFirstSibling();
                segment.Color = Color;
                segments.Add(segment);
            }
        }
    }

    protected override Color Color {
        get => arrowEndCap.color;
        set {
            arrowStart.color = value;
            arrowMiddle.color = value;
            arrowEndCap.color = value;
            foreach (var segment in segments) {
                segment.Color = value;
            }
        }
    }
}
