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

public abstract class SimpleButton : Button {
#pragma warning disable CS0649
    [SerializeField]
    private Image image;
    [SerializeField]
    private TMPro.TMP_Text tmp;
#pragma warning restore CS0649

    [SerializeField]
    private Sprite enabledSprite;
    [SerializeField]
    private Sprite disabledSprite;

    [SerializeField]
    private LE text;
    [SerializeField]
    private Color enabledTextColor = Color.black;
    [SerializeField]
    private Color disabledTextColor = Color.gray;

    [SerializeField]
    [HideInInspector]
    private bool initialized;

    protected virtual void Start() {
        if (text == null) {
            if (tmp == null) {
                text = LC.Empty;
            } else {
                text = LC.User(tmp.text);
            }
        }
        initialized = true;
        UpdateAll();
    }

    protected Sprite EnabledSpriteInternal {
        get => enabledSprite;
        set {
            enabledSprite = value;
            UpdateImage();
        }
    }

    protected Sprite DisabledSpriteInternal {
        get => disabledSprite;
        set {
            disabledSprite = value;
            UpdateImage();
        }
    }

    protected LE TextInternal {
        get => text;
        set {
            text = value;
            UpdateTMP();
        }
    }

    protected Color EnabledTextColorInternal {
        get => enabledTextColor;
        set {
            enabledTextColor = value;
            UpdateTMP();
        }
    }

    protected Color DisabledTextColorInternal {
        get => disabledTextColor;
        set {
            disabledTextColor = value;
            UpdateTMP();
        }
    }

    protected void UpdateAll() {
        UpdateImage();
        UpdateTMP();
    }

    protected void UpdateImage() {
        if (initialized && image != null) {
            image.sprite = Enabled ? enabledSprite : disabledSprite;
        }
    }

    protected void UpdateTMP() {
        if (initialized && tmp != null) {
            tmp.text = text.ToString();
            tmp.color = Enabled ? enabledTextColor : disabledTextColor;
        }
    }

    protected bool Enabled => AllowMouse0 || AllowMouse1 || AllowMouse2;

    public override bool Mouse0(InputMode.State state, InputMode.Modifiers modifiers, Vector3 position, Lazy<RaycastHit2D[]> hits, bool overButton) {
        if (state == InputMode.State.Release) {
            Mouse0(modifiers, position, hits, overButton);
        }
        return true;
    }
    public override bool Mouse1(InputMode.State state, InputMode.Modifiers modifiers, Vector3 position, Lazy<RaycastHit2D[]> hits, bool overButton) {
        if (state == InputMode.State.Release) {
            Mouse1(modifiers, position, hits, overButton);
        }
        return true;
    }
    public override bool Mouse2(InputMode.State state, InputMode.Modifiers modifiers, Vector3 position, Lazy<RaycastHit2D[]> hits, bool overButton) {
        if (state == InputMode.State.Release) {
            Mouse2(modifiers, position, hits, overButton);
        }
        return true;
    }

    protected virtual void Mouse0(InputMode.Modifiers modifiers, Vector3 position, Lazy<RaycastHit2D[]> hits, bool overButton) { }
    protected virtual void Mouse1(InputMode.Modifiers modifiers, Vector3 position, Lazy<RaycastHit2D[]> hits, bool overButton) { }
    protected virtual void Mouse2(InputMode.Modifiers modifiers, Vector3 position, Lazy<RaycastHit2D[]> hits, bool overButton) { }
}
