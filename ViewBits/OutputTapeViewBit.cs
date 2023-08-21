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

public class OutputTapeViewBit : MonoBehaviour {
#pragma warning disable CS0649
    [SerializeField]
    private RectTransform tapeValueContainer;
    [SerializeField]
    private RectTransform lengthRect;
    [SerializeField]
    private Tween tween;
    [SerializeField]
    private ExecutionContextHeightTween executionContextTween;
#pragma warning restore CS0649

    public int Length {
        get => (int)(lengthRect.anchorMax.y - lengthRect.anchorMin.y);
        set => lengthRect.anchorMin = lengthRect.anchorMin.WithY(lengthRect.anchorMax.y - value);
    }

    public RectTransform TapeValueContainer => tapeValueContainer;

    public IDisposable Unlock() => tween.Unlock();

    public event Action<Tween> OnTweenComplete {
        add => executionContextTween.OnComplete += value;
        remove => executionContextTween.OnComplete -= value;
    }
}
