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

public sealed class TapeValueViewBit : MonoBehaviour, IApertureTarget {
#pragma warning disable CS0649
    [SerializeField]
    private Image backdrop;
    [SerializeField]
    private Image image;
    [SerializeField]
    private TMPro.TMP_Text tmp;
#pragma warning restore CS0649

    private TapeValue value;
    private Option<FontSizeSync> fss;

    public TapeValue Value {
        get {
            return value;
        }
        set {
            this.value = value;
            UpdateVisual(value);
        }
    }

    public Option<FontSizeSync> FontSizeSync {
        get {
            return fss;
        }
        set {
            foreach (var fss in fss) {
                fss.RemoveText(tmp);
            }
            fss = value;
            foreach (var fss in fss) {
                fss.AddText(tmp);
            }
        }
    }

    private void Start() {
        value = TapeValueNull.Instance;
    }

    private void DisableAll() {
        image.enabled = false;
        tmp.enabled = false;
    }

    private void UpdateVisual(TapeValue tv) {
        DisableAll();
        var type = tv.Type;

        if (type == TapeValueType.Int) {
            tmp.enabled = true;
            tmp.text = tv.To<int>().ToString();
        } else if (type == TapeValueType.Color) {
            image.enabled = true;
            image.sprite = Overseer.GlobalAssets.ColorValueSprite;
            image.color = TapeValueColor.GetColor(tv.To<ColorId>());
        } else if (type == TapeValueType.Char) {
            tmp.enabled = true;
            tmp.text = tv.To<char>().ToString();
        } else if (type == TapeValueType.Opcode
                || type == TapeValueType.Frame
                || type == TapeValueType.FrameRead
                || type == TapeValueType.Channel
                || type == TapeValueType.Label) {
            tmp.enabled = true;
            tmp.text = tv.GetText().ToString();
        } else if (type == TapeValueType.Null) {

        } else if (type == TapeValueType.Cut) {
            image.enabled = true;
            image.sprite = Overseer.GlobalAssets.CutSprite;
            image.color = Color.black;
        } else if (type == TapeValueType.Invalid) {
            tmp.enabled = true;
            tmp.text = "?";
        }

        tmp.color = tv.Type.HighlightColor;

        foreach (var fss in fss) {
            fss.RequireUpdate();
        }
    }

    Bounds2D IApertureTarget.Bounds => GetComponent<RectTransform>().GetWorldBounds();
}
