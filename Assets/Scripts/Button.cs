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

// UNITY BUG
//[RequireComponent(typeof(BoxCollider2D))]
public abstract class Button : MonoBehaviour, IApertureTarget {
    private void OnEnable() {
        RegisterKeyCode(KeyCode);
    }

    private void OnDisable() {
        UnregisterKeyCode(KeyCode);
    }

    private void RegisterKeyCode(Option<KeyCode> keyCode) {
        foreach (var keyCodeValue in keyCode) {
            bool result = Overseer.InputManager.RegisterMiscKey(keyCodeValue, Mouse0PressRelease);
            Assert.True(result);
        }
    }

    private void UnregisterKeyCode(Option<KeyCode> keyCode) {
        foreach (var keyCodeValue in KeyCode) {
            bool result = Overseer.InputManager.UnregisterMiscKey(keyCodeValue);
            Assert.True(result);
        }
    }

    protected void ChangeKeyCode(Option<KeyCode> oldKeyCode) {
        UnregisterKeyCode(oldKeyCode);
        RegisterKeyCode(KeyCode);
    }

    public virtual bool AllowMouse0 => false;
    public virtual bool AllowMouse1 => false;
    public virtual bool AllowMouse2 => false;

    public virtual bool Mouse0(InputMode.State state, InputMode.Modifiers modifiers, Vector3 position, Lazy<RaycastHit2D[]> hits, bool overButton) => true;

    public virtual bool Mouse1(InputMode.State state, InputMode.Modifiers modifiers, Vector3 position, Lazy<RaycastHit2D[]> hits, bool overButton) => true;

    public virtual bool Mouse2(InputMode.State state, InputMode.Modifiers modifiers, Vector3 position, Lazy<RaycastHit2D[]> hits, bool overButton) => true;

    public virtual Option<KeyCode> KeyCode => Option.None;

    private bool Mouse0PressRelease(InputMode.State state, InputMode.Modifiers modifiers) {
        switch (state) {
            case InputMode.State.Press:
                if (Mouse0(InputMode.State.Press, modifiers, transform.position, new Lazy<RaycastHit2D[]>(Array.Empty<RaycastHit2D>), true)) {
                    // We at least partially consumed the event, so always return true here?
                    Mouse0(InputMode.State.Release, modifiers, transform.position, new Lazy<RaycastHit2D[]>(Array.Empty<RaycastHit2D>), true);
                    return true;
                }
                break;
        }
        return false;
    }

    Bounds2D IApertureTarget.Bounds => GetComponent<RectTransform>().GetWorldBounds();
}
