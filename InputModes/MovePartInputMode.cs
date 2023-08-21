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

public sealed partial class MovePartInputMode : InputMode {
    private Option<DragOperation> dragOperation;

    public MovePartInputMode(InputManager manager) : base(manager) { }

    public Option<DragOperation> DragOperation {
        get => dragOperation;
        set => dragOperation = value;
    }

    public override bool AlwaysActive => false;

    public override void OnSelect() {
        MakeView();
    }

    public override void OnDeselect() {
        ClearView();
        foreach (var dragOperationValue in dragOperation) {
            dragOperationValue.Cancel();
        }
        dragOperation = Option.None;
    }

    public override bool Escape(State state, Modifiers modifiers) {
        switch (state) {
            case State.Press:
                InputManager.Deselect(this);
                break;
        }
        return true;
    }

    public override bool Mouse(Modifiers modifiers, Vector3 position, Lazy<RaycastHit2D[]> hits) => true;

    public override bool Mouse0(State state, Modifiers modifiers, Vector3 position, Lazy<RaycastHit2D[]> hits) {
        switch (state) {
            case State.Held:
                // TODO highlight InsertPoints
                foreach (var dragOperationValue in dragOperation) {
                    dragOperationValue.Mouse0Held(position, hits);
                    return true;
                }
                break;
            case State.Release:
                foreach (var dragOperationValue in dragOperation) {
                    dragOperationValue.Mouse0Release(position, hits);
                    dragOperation = Option.None;
                    InputManager.Deselect();
                    return true;
                }
                break;
        }
        return false;
    }
}
