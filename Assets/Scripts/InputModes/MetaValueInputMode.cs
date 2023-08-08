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

public sealed partial class MetaValueInputMode : InputMode {
    private SelectValueInputMode selectMode;

    public MetaValueInputMode(InputManager manager, SelectValueInputMode selectMode) : base(manager) {
        this.selectMode = selectMode;
    }

    public override bool AlwaysActive => true;

    public override bool Mouse0(State state, Modifiers modifiers, Vector3 position, Lazy<RaycastHit2D[]> hits) {
        switch (state) {
            case State.Press when !modifiers.HasCtrl():
                bool select = modifiers.HasShift();
                foreach (var hit in hits.Value) {
                    if (hit.collider.GetComponent<TapeValueModifyPoint>() != null) {
                        select = true;
                        break;
                    }
                }
                if (select) {
                    selectMode.SetDragStart(position);
                    InputManager.Select(selectMode);
                }
                return select;
        }
        return false;
    }
}

public sealed partial class SelectValueInputMode : InputMode {
    private struct TapeValueId {
        public readonly Cryptex cryptex;
        public readonly TapeSelectionPoint index;

        public TapeValueId(Cryptex cryptex, TapeSelectionPoint index) {
            this.cryptex = cryptex;
            this.index = index;
        }
    }

    private SingleValueInputMode singleMode;
    private MultiValueInputMode multiMode;

    private Option<Vector3> dragStart;

    public SelectValueInputMode(InputManager manager, SingleValueInputMode singleMode, MultiValueInputMode multiMode) : base(manager) {
        this.singleMode = singleMode;
        this.multiMode = multiMode;
    }

    public override bool AlwaysActive => false;

    public override void OnSelect() {
        MakeView();
    }
    public override void OnDeselect() {
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
                foreach (var view in View) {
                    view.UpdatePosition(modifiers, position);
                }
                return true;
            case State.Release:
                Vector3 dragStartValue = dragStart.ValueOr(position);
                dragStart = Option.None;
                if (modifiers.HasShift()) {
                    var enclosed = GetEnclosed(GetBounds(dragStartValue, position), dragStartValue, position);

                    foreach (var enclosedValue in enclosed) {
                        var start = enclosedValue.Item1;
                        var end = enclosedValue.Item2;

                        multiMode.Cryptex = start.cryptex;
                        multiMode.TapePointStart = start.index;
                        multiMode.TapePointEnd = end.index;
                        InputManager.Select(multiMode);

                        return true;
                    }
                } else {
                    foreach (var hit in hits.Value) {
                        var insertPoint = hit.collider.GetComponent<TapeValueInsertPoint>();
                        if (insertPoint != null && insertPoint.enabled) {
                            // Don't allow single selection of locked values
                            if (!insertPoint.Tape.Lock.HasFlag(LockType.Edit)) {
                                singleMode.Cryptex = insertPoint.Tape.Cryptex;
                                // This conversion is a bit of a mess
                                singleMode.TapePoint = new TapeSelectionPoint(insertPoint.Tape.Cryptex.Tapes.IndexOf(insertPoint.Tape), insertPoint.Offset + insertPoint.Tape.ShiftOffset);
                                InputManager.Select(singleMode);
                            }

                            return true;
                        }
                    }
                }
                
                InputManager.Deselect();
                // I guess this should always be true, because even if nothing winds up being selected, the event should be consumed to Deselect this InputMode
                return true;
        }
        return false;
    }

    public void SetDragStart(Vector3 dragStart) {
        this.dragStart = dragStart;
    }

    private Bounds GetBounds(Vector3 start, Vector3 end) {
        start = Camera.main.ScreenToWorldPoint(start);
        end = Camera.main.ScreenToWorldPoint(end);
        var width = Mathf.Abs(end.x - start.x);
        var height = Mathf.Abs(end.y - start.y);
        var x = Mathf.Min(end.x, start.x);
        var y = Mathf.Min(end.y, start.y);

        return new Bounds(new Vector3(x + width / 2f, y + height / 2f, 0f), new Vector3(width, height, 2f));
    }

    private Option<(TapeValueId, TapeValueId)> GetEnclosed(Bounds bounds, Vector3 start, Vector3 end) {
        start = Camera.main.ScreenToWorldPoint(start);
        end = Camera.main.ScreenToWorldPoint(end);
        var hits = Physics2D.OverlapBoxAll(bounds.center, bounds.size, 0f).Where(c => c.GetComponent<TapeValueModifyPoint>() != null).ToArray();

        switch (hits.Length) {
            case 0:
                return Option.None;
            case 1: {
                    var tvmp = hits[0].GetComponent<TapeValueModifyPoint>();
                    var cryptex = tvmp.Tape.Cryptex;
                    var tapeIndex = cryptex.Tapes.IndexOf(tvmp.Tape);
                    return (new TapeValueId(cryptex, new TapeSelectionPoint(tapeIndex, tvmp.Offset + tvmp.Tape.ShiftOffset)), new TapeValueId(cryptex, new TapeSelectionPoint(tapeIndex, tvmp.Offset + tvmp.Tape.ShiftOffset)));
                }
            default: {
                    Collider2D closest = hits[0];
                    float closestDist = Vector3.Distance(start, hits[0].transform.position);
                    foreach (var hit in hits.Skip(1)) {
                        var dist = Vector3.Distance(start, hit.transform.position);
                        if (dist < closestDist) {
                            closest = hit;
                            closestDist = dist;
                        }
                    }
                    var tvmp0 = closest.GetComponent<TapeValueModifyPoint>();
                    var cryptex = tvmp0.Tape.Cryptex;
                    var tapeIndex0 = cryptex.Tapes.IndexOf(tvmp0.Tape);
                    Collider2D farthest = hits[0];
                    float farthestDist = float.PositiveInfinity;
                    foreach (var hit in hits) {
                        var tvip = hit.GetComponent<TapeValueModifyPoint>();
                        if (tvip.Tape.Cryptex == cryptex) {
                            var dist = Vector3.Distance(end, hit.transform.position);
                            if (dist < farthestDist) {
                                farthest = hit;
                                farthestDist = dist;
                            }
                        }
                    }
                    var tvmp1 = farthest.GetComponent<TapeValueModifyPoint>();
                    var tapeIndex1 = cryptex.Tapes.IndexOf(tvmp1.Tape);
                    return (new TapeValueId(cryptex, new TapeSelectionPoint(tapeIndex0, tvmp0.Offset + tvmp0.Tape.ShiftOffset)), new TapeValueId(cryptex, new TapeSelectionPoint(tapeIndex1, tvmp1.Offset + tvmp1.Tape.ShiftOffset)));
                }
        }
    }
}