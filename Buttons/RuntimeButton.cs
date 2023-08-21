using Functional.Option;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

using LE = ILocalizationExpression;
using L = LocalizationString;
using LC = LocalizationConstant;
using LF = LocalizationFormat;
using LI = LocalizationInt;

public abstract class RuntimeButtonShim : SimpleButton {
    public override Option<KeyCode> KeyCode => KeyCodeInternal;

    protected abstract Option<KeyCode> KeyCodeInternal { get; }
}

public sealed class RuntimeButton : RuntimeButtonShim {
    [SerializeField]
    private bool mouse0Enabled;
    [SerializeField]
    private bool mouse1Enabled;
    [SerializeField]
    private bool mouse2Enabled;
    [SerializeField]
    private KeyCode keyCode;

    private Option<Action> mouse0Action;
    private Option<Action> mouse1Action;
    private Option<Action> mouse2Action;

    public bool Mouse0Enabled {
        get => mouse0Enabled;
        set {
            if (mouse0Enabled != value) {
                mouse0Enabled = value;
                UpdateAll();
            }
        }
    }

    public bool Mouse1Enabled {
        get => mouse1Enabled;
        set {
            if (mouse1Enabled != value) {
                mouse1Enabled = value;
                UpdateAll();
            }
        }
    }

    public bool Mouse2Enabled {
        get => mouse2Enabled;
        set {
            if (mouse2Enabled != value) {
                mouse2Enabled = value;
                UpdateAll();
            }
        }
    }

    public override bool AllowMouse0 => Mouse0Enabled;
    public override bool AllowMouse1 => Mouse1Enabled;
    public override bool AllowMouse2 => Mouse2Enabled;

    public Option<Action> Mouse0Action {
        get => mouse0Action;
        set => mouse0Action = value;
    }

    public Option<Action> Mouse1Action {
        get => mouse1Action;
        set => mouse1Action = value;
    }

    public Option<Action> Mouse2Action {
        get => mouse2Action;
        set => mouse2Action = value;
    }

    public Option<Sprite> EnabledSprite {
        get => EnabledSpriteInternal;
        set => EnabledSpriteInternal = value.ValueOrDefault;
    }

    public Option<Sprite> DisabledSprite {
        get => DisabledSpriteInternal;
        set => DisabledSpriteInternal = value.ValueOrDefault;
    }

    public LE Text {
        get => TextInternal;
        set => TextInternal = value;
    }

    public Color EnabledTextColor {
        get => EnabledTextColorInternal;
        set => EnabledTextColorInternal = value;
    }

    public Color DisabledTextColor {
        get => DisabledTextColorInternal;
        set => DisabledTextColorInternal = value;
    }

    protected override Option<KeyCode> KeyCodeInternal => KeyCode;

    public new Option<KeyCode> KeyCode {
        get => keyCode == UnityEngine.KeyCode.None ? Option.None : keyCode.ToOption();
        set {
            var oldKeyCode = KeyCode;
            keyCode = value.ValueOrDefault;
            ChangeKeyCode(oldKeyCode);
        }
    }

    protected override void Mouse0(InputMode.Modifiers modifiers, Vector3 position, Lazy<RaycastHit2D[]> hits, bool overButton) {
        foreach (var mouse0Action in Mouse0Action) {
            mouse0Action();
        }
    }

    protected override void Mouse1(InputMode.Modifiers modifiers, Vector3 position, Lazy<RaycastHit2D[]> hits, bool overButton) {
        foreach (var mouse1Action in Mouse1Action) {
            mouse1Action();
        }
    }

    protected override void Mouse2(InputMode.Modifiers modifiers, Vector3 position, Lazy<RaycastHit2D[]> hits, bool overButton) {
        foreach (var mouse2Action in Mouse1Action) {
            mouse2Action();
        }
    }
}
