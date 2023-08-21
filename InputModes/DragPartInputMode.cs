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

public sealed partial class DragPartInputMode : PartInputMode {
    private Option<DragOperation> dragOperation;

    public DragPartInputMode(InputManager manager) : base(manager) { }

    public Option<DragOperation> DragOperation {
        get {
            return dragOperation;
        }
        set {
            dragOperation = value;
        }
    }

    public override bool AlwaysActive => false;

    public override void OnSelect() {
        MakeView();
    }

    public override void OnDeselect() {
        foreach (var dragOperation in DragOperation) {
            dragOperation.Cancel();
            DragOperation = Option.None;
        }
        ClearView();
    }

    public override bool Escape(State state, Modifiers modifiers) {
        switch (state) {
            case State.Press:
                InputManager.Deselect(this);
                break;
        }
        return true;
    }

    public override bool Mouse0(State state, Modifiers modifiers, Vector3 position, Lazy<RaycastHit2D[]> hits) {
        switch (state) {
            case State.Held:
                // TODO highlight InsertPoints
                foreach (var dragOperation in DragOperation) {
                    dragOperation.Mouse0Held(position, hits);
                    return true;
                }
                break;
            case State.Release:
                foreach (var dragOperation in DragOperation) {
                    var result = dragOperation.Mouse0Release(position, hits);
                    InputManager.Deselect();
                    DragOperation = Option.None;
                    return result;
                }
                break;
        }
        return false;
    }
}
