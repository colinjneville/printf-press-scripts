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

public sealed partial class PickPartInputMode : PartInputMode {
    private Option<ToolboxItem> item;

    public PickPartInputMode(InputManager manager) : base(manager) { }

    public Option<ToolboxItem> Item {
        get {
            return item;
        }
        set {
            item = value;
        }
    }

    public override bool AlwaysActive => false;

    public override void OnSelect() {
        MakeView();
    }

    public override void OnDeselect() {
        Item = Option.None;
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
            case State.Press:
                foreach (var item in Item) {
                    foreach (var workspace in Overseer.Workspace) {
                        // Create a new drag operation so we have a new object with a new id
                        var dragOperation = item.CreateDragOperation(workspace, Camera.main.ScreenToWorldPoint(position));
                        return dragOperation.Mouse0Release(position, hits);
                    }
                    return false;
                }
                Assert.NotReached();
                break;
        }
        return false;
    }
}
