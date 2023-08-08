using Functional.Option;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using TMPro;
using UnityEngine;

using L = LocalizationString;
using LC = LocalizationConstant;
using LE = ILocalizationExpression;
using LF = LocalizationFormat;
using LI = LocalizationInt;


public class FontSizeSync {
    private HashSet<TMP_Text> texts;
    private bool needUpdate;
    private bool allowGrow;
    private float prevSize;
    private float minSize;
    private float maxSize;

    public FontSizeSync(float minSize = 1f, float maxSize = 10000f) {
        texts = new HashSet<TMP_Text>();
        allowGrow = true;
        this.minSize = minSize;
        this.maxSize = maxSize;
        ClearHistory();
    }

    public float MinSize {
        get {
            return minSize;
        }
        set {
            minSize = value;
        }
    }

    public float MaxSize {
        get {
            return maxSize;
        }
        set {
            maxSize = value;
        }
    }

    public bool AllowGrow {
        get {
            return allowGrow;
        }
        set {
            allowGrow = value;
        }
    }

    public void ClearHistory() {
        prevSize = maxSize;
    }

    public void AddText(TMP_Text text) {
        texts.Add(text);
        needUpdate = true;
    }

    public void RemoveText(TMP_Text text) {
        texts.Remove(text);
        text.enableAutoSizing = true;

        needUpdate |= allowGrow;
    }

    public void RequireUpdate() {
        needUpdate = true;
    }

    public bool Update() {
        if (needUpdate) {
            float size = MaxSize;

            foreach (var text in texts) {
                text.fontSizeMin = 0f;
                text.fontSizeMax = MaxSize;
                text.enableAutoSizing = true;
                text.ForceMeshUpdate();
                size = Mathf.Min(text.fontSize, size);
            }

            if (!AllowGrow) {
                size = Mathf.Min(size, prevSize);
            }

            size = Mathf.Max(size, MinSize);

            foreach (var text in texts) {
                text.enableAutoSizing = false;
                text.fontSize = size;
            }

            prevSize = size;

            needUpdate = false;
            return true;
        }
        return false;
    }
}
