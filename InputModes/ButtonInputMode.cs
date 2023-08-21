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

public class ButtonInputMode : InputMode {
    private Option<Button> heldButton;

    public ButtonInputMode(InputManager manager) : base(manager) { }

    public override bool AlwaysActive => true;
    public override bool ActiveDuringExecution => true;

    public override bool Mouse0(State state, Modifiers modifiers, Vector3 position, Lazy<RaycastHit2D[]> hits) => MouseX(b => b.Mouse0, b => b.AllowMouse0, state, modifiers, position, hits);
    public override bool Mouse1(State state, Modifiers modifiers, Vector3 position, Lazy<RaycastHit2D[]> hits) => MouseX(b => b.Mouse1, b => b.AllowMouse1, state, modifiers, position, hits);
    public override bool Mouse2(State state, Modifiers modifiers, Vector3 position, Lazy<RaycastHit2D[]> hits) => MouseX(b => b.Mouse2, b => b.AllowMouse2, state, modifiers, position, hits);

    private bool MouseX(Func<Button, Func<State, Modifiers, Vector3, Lazy<RaycastHit2D[]>, bool, bool>> func, Func<Button, bool> allowFunc, State state, Modifiers modifiers, Vector3 position, Lazy<RaycastHit2D[]> hits) {
        switch (state) {
            case State.Press:
                // HACK this isn't perfect, but this should reduce the posibility of heldButton getting 'stuck,' if another InputMode takes MouseXRelease (preventing heldButton clear)
                heldButton = Option.None;
                // TODO should this only check the nearest hit?
                foreach (var hit in hits.Value) {
                    var button = hit.collider.GetComponent<Button>();
                    if (button != null) {
                        if (allowFunc(button)) {
                            if (func(button)(state, modifiers, position, hits, true)) {
                                heldButton = button;
                                return true;
                            }
                            return false;
                        }
                        return true;
                    }
                }
                break;
            case State.Held:
                foreach (var button in heldButton) {
                    return func(button)(state, modifiers, position, hits, ButtonFound(modifiers, position, hits));
                }
                break;
            case State.Release:
                foreach (var button in heldButton) {
                    var result = func(button)(state, modifiers, position, hits, ButtonFound(modifiers, position, hits));
                    InputManager.Deselect(this);
                    return result;
                }
                break;
        }
        return false;
    }

    private bool ButtonFound(Modifiers modifiers, Vector3 position, Lazy<RaycastHit2D[]> hits) {
        foreach (var button in heldButton) {
            foreach (var hit in hits.Value) {
                if (heldButton == hit.collider.GetComponent<Button>()) {
                    return true;
                }
            }
            return false;
        }
        // If this InputMode is active, heldButton must have a value
        throw RtlAssert.NotReached();
    }
}
