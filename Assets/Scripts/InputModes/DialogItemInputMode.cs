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

public sealed class DialogInputMode : InputMode {
    public DialogInputMode(InputManager manager) : base(manager) { }

    public override bool AlwaysActive => false;

    public override void OnRegister() {
        Overseer.OnDialogChange += OnDialogChange;
    }

    public override void OnUnregister() {
        Overseer.OnDialogChange -= OnDialogChange;
    }

    public override bool Mouse0(State state, Modifiers modifiers, Vector3 position, Lazy<RaycastHit2D[]> hits) {
        switch (state) {
            case State.Release:
                Advance();
                break;
        }
        return true;
    }

    public override bool Mouse1(State state, Modifiers modifiers, Vector3 position, Lazy<RaycastHit2D[]> hits) => true;

    public override bool Space(State state, Modifiers modifiers) {
        switch (state) {
            case State.Press:
                Advance();
                break;
        }
        return true;
    }

    public override bool Return(State state, Modifiers modifiers) {
        switch (state) {
            case State.Press:
                Advance();
                break;
        }
        return true;
    }

    public override bool Escape(State state, Modifiers modifiers) {
        switch (state) {
            case State.Press:
                Skip();
                break;
        }
        return true;
    }

    private void Advance() {
        foreach (var dialog in Overseer.Dialog) {
            dialog.MakeView().Advance();
        }
    }

    private void Skip() {
        foreach (var dialog in Overseer.Dialog) {
            dialog.MakeView().Skip();
        }
    }

    private void OnDialogChange(Option<DialogSequence> dialogSequence) {
        if (dialogSequence.TryGetValue(out var dialogSequenceValue)) {
            InputManager.Select(this);
        } else {
            InputManager.Deselect(this);
        }
    }
}
