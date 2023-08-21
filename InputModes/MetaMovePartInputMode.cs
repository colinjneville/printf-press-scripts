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

public sealed class MetaMovePartInputMode : InputMode {
    private MovePartInputMode movePartInputMode;
    private Option<ModifyPoint> previousPoint;

    public MetaMovePartInputMode(InputManager manager, MovePartInputMode movePartInputMode) : base(manager) {
        this.movePartInputMode = movePartInputMode;
    }

    public override bool AlwaysActive => true;

    public override bool Mouse(Modifiers modifiers, Vector3 position, Lazy<RaycastHit2D[]> hits) {
        foreach (var hit in hits.Value) {
            var modify = hit.collider.GetComponent<ModifyPoint>();
            if (modify != previousPoint) {
                EndHover();

                if (modify != null) {
                    StartHover(modify);
                }
                
                return true;
            }

            // Even if we are hovering over the same ModifyPoint, consume the event
            if (modify != null) {
                return true;
            }
        }

        return false;
    }

    public override bool Mouse0(State state, Modifiers modifiers, Vector3 position, Lazy<RaycastHit2D[]> hits) {
        switch (state) {
            case State.Press:
                EndHover();
                foreach (var hit in hits.Value) {
                    var modify = hit.collider.GetComponent<ModifyPoint>();
                    if (modify != null) {
                        if (modify.AllowDrag && modify.CanDelete(true)) {
                            InputManager.Select(movePartInputMode);
                            movePartInputMode.DragOperation = modify.CreateDragOperation(position);
                        }
                        return true;
                    }
                }
                break;
        }
        return false;
    }

    private void StartHover(ModifyPoint point) {
        point.OnStartHover();
        previousPoint = point;
    }

    private void EndHover() {
        foreach (var previousPoint in previousPoint) {
            previousPoint.OnEndHover();
            this.previousPoint = Option.None;
        }
    }
}
