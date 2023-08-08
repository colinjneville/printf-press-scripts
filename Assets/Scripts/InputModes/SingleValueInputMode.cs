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

public sealed partial class SingleValueInputMode : InputMode {
    private enum InputMode {
        Instructions,
        Data,
    }

    private Cryptex cryptex;

    private TapeSelectionPoint tapePoint;

    private TextHighlight highlight;

    private MultiValueInputMode multiInputMode;

    public SingleValueInputMode(InputManager manager, MultiValueInputMode multiInputMode) : base(manager) {
        this.multiInputMode = multiInputMode;
    }

    public override bool AlwaysActive => false;

    public Cryptex Cryptex {
        get => cryptex;
        set {
            cryptex = value;
            TapePoint = new TapeSelectionPoint(0, 0);
        }
    }

    public TapeSelectionPoint TapePoint {
        get => tapePoint;
        set {
            tapePoint = value;
            OpenValue();
        }
    }

    private Tape Tape => Cryptex.Tapes[TapePoint.TapeIndex];

    private TapeValue TapeValue => Tape.Read(TapePoint.OffsetIndex);

    private void Save() {
        var value = TapeValue.FromString(highlight.Text);
        if (value != TapeValue) {
            var record = Tape.Write(TapePoint.OffsetIndex, value);
            Overseer.Workspace.ValueOrAssert().ApplyModificationRecord(record);
        }
    }

    private InputMode Mode => Cryptex.Rotated ? InputMode.Data : InputMode.Instructions;

    private void OpenValue() {
        var oldHighlight = highlight;
        highlight = new TextHighlight(ReadText());
        highlight.SelectAll();
        foreach (var view in View) {
            view.OnUpdateHighlight(oldHighlight, highlight);
        }
    }

    private string ReadText() => TapeValue.GetText().ToString();

    private bool CanNextTape => TapePoint.TapeIndex < Cryptex.Tapes.Count - 1;

    private bool NextTape() {
        if (CanNextTape) {
            TapePoint = TapePoint.NextTape;
            return true;
        } else {
            return false;
        }
    }

    private bool CanPrevTape => TapePoint.TapeIndex > 0;

    private bool PrevTape() {
        if (CanPrevTape) {
            TapePoint = TapePoint.PrevTape;
            return true;
        } else {
            return false;
        }
    }

    private void NextOffset() {
        TapePoint = TapePoint.NextOffset;
    }

    private void PrevOffset() {
        TapePoint = TapePoint.PrevOffset;
    }
   

    private void NextPrimary() {
        switch (Mode) {
            case InputMode.Data:
                NextOffset();
                break;
            case InputMode.Instructions:
                if (!NextTape()) {
                    NextOffset();
                    TapePoint = TapePoint.WithTape(0);
                }
                break;
        }
    }

    private void PrevPrimary() {
        switch (Mode) {
            case InputMode.Data:
                PrevOffset();
                break;
            case InputMode.Instructions:
                if (!PrevTape()) {
                    PrevOffset();
                    TapePoint = TapePoint.WithTape(Cryptex.Tapes.Count - 1);
                }
                break;
        }
    }

    private void NextSecondary() {
        switch (Mode) {
            case InputMode.Data:
                if (!NextTape()) {
                    TapePoint = TapePoint.WithTape(0);
                }
                break;
            case InputMode.Instructions:
                NextOffset();
                TapePoint = TapePoint.WithTape(0);
                break;
        }
    }

    private void PrevSecondary() {
        switch (Mode) {
            case InputMode.Data:
                if (!PrevTape()) {
                    TapePoint = TapePoint.WithTape(Cryptex.Tapes.Count - 1);
                }
                break;
            case InputMode.Instructions:
                PrevOffset();
                TapePoint = TapePoint.WithTape(Cryptex.Tapes.Count - 1);
                while (TapePoint.TapeIndex > 0 && Tape.Read(TapePoint.OffsetIndex).IsNull) {
                    TapePoint = TapePoint.PrevTape;
                }

                break;
        }
    }

    private void InsertColumn(int index) {
        var record = Cryptex.InsertColumn(index);
        Overseer.Workspace.ValueOrAssert().ApplyModificationRecord(record);
    }

    private void RemoveColumn(int index) {
        var record = Cryptex.RemoveColumn(index);
        Overseer.Workspace.ValueOrAssert().ApplyModificationRecord(record);
    }

    public override void OnSelect() {
        var view = MakeView();
        view.transform.SetParent(WorkspaceScene.Layer.EditHighlight(view.GetScreen()), true);
        view.GetScreen().FixedProxy.CreateReceiver(view.gameObject);
        OpenValue();
    }
    public override void OnDeselect() {
        Save();
        ClearView();
    }

    public override bool Mouse0(State state, Modifiers modifiers, Vector3 position, Lazy<RaycastHit2D[]> hits) {
        switch (state) {
            case State.Press:
                foreach (var view in View) {
                    foreach (var hit in hits.Value) {
                        // HACK
                        var tvmp = hit.collider.GetComponent<SingleValueInputModeViewBit>();
                        if (view.IsViewBit(tvmp)) {
                            highlight.Mouse0Press(modifiers, position);
                            return true;
                        }
                    }
                }
                break;
            case State.Held:
                highlight.Mouse0Held(modifiers, position);
                return true;
            case State.Release:
                highlight.Mouse0Release(modifiers, position);
                return true;
        }
        
        return false;
    }

    public override bool Char(State state, Modifiers modifiers, char c) {
        if (CanEdit) {
            highlight.Char(state, modifiers, c);
        }
        return true;
    }

    public override bool SelectAll(Modifiers modifiers) {
        highlight.SelectAll();
        return true;
    }

    public override bool Backspace(State state, Modifiers modifiers) {
        if (modifiers.HasAlt()) {
            switch (state) {
                case State.Press:
                    RemoveColumn(TapePoint.OffsetIndex);
                    PrevOffset();
                    break;
            }
        } else {
            if (highlight.HasSelection || highlight.StartIndex > 0 && CanEdit) {
                highlight.Backspace(state, modifiers);
            } else {
                switch (state) {
                    case State.Press:
                        Save();
                        PrevPrimary();
                        break;
                }
            }
        }
        
        return true;
    }

    public override bool Delete(State state, Modifiers modifiers) {
        if (modifiers.HasAlt()) {
            switch (state) {
                case State.Press:
                    RemoveColumn(TapePoint.OffsetIndex + 1);
                    break;
            }
        } else if (CanEdit) {
            highlight.Delete(state, modifiers);
        }
        return true;
    }

    public override bool Escape(State state, Modifiers modifiers) {
        switch (state) {
            case State.Press:
                // Explicitly discard changes so OnDeselect does not save them
                highlight.Text = ReadText();
                InputManager.Deselect();
                break;
        }
        return true;
    }

    public override bool Space(State state, Modifiers modifiers) {
        switch (state) {
            case State.Press:
                if (modifiers.HasCtrl()) {
                    // TODO ???
                } else {
                    Save();
                    if (modifiers.HasShift()) {
                        PrevPrimary();
                    } else {
                        NextPrimary();
                    }
                }
                break;
        }
        return true;
    }
    public override bool Return(State state, Modifiers modifiers) {
        switch (state) {
            case State.Press:
                if (modifiers.HasAlt()) {
                    InsertColumn(TapePoint.OffsetIndex + 1);
                    NextOffset();
                } else {
                    Save();
                    if (modifiers.HasShift()) {
                        PrevSecondary();
                    } else {
                        NextSecondary();
                    }
                }
                break;
        }
        return true;
    }

    public override bool Tab(State state, Modifiers modifiers) {
        switch (state) {
            case State.Press:
                var cryptexes = Cryptex.Workspace.Cryptexes.OrderBy(c => c.XY.y);
                if (modifiers.HasShift()) {
                    cryptexes.Reverse();
                }

                bool useNext = false;
                Cryptex next = null;
                foreach (var c in cryptexes) {
                    if (next == null) {
                        // If we loop through and the current cryptex is last (or not found), default to the first in order
                        next = c;
                    }

                    if (c == Cryptex) {
                        useNext = true;
                    } else if (useNext) {
                        useNext = false;
                        next = c;
                        break;
                    }
                }
                
                if (Cryptex != next) {
                    OnDeselect();
                    Cryptex = next;
                    OnSelect();
                }
                
                break;
        }
        return true;
    }

    public override bool Left(State state, Modifiers modifiers) {
        if (highlight.EndIndex == 0 || highlight.IsSelectAll) {
            switch (state) {
                case State.Press:
                    if (modifiers.HasShift()) {
                        multiInputMode.TapePointStart = tapePoint;
                        multiInputMode.TapePointEnd = tapePoint.PrevOffset;
                        InputManager.Select(multiInputMode);
                    } else {
                        Save();
                        PrevOffset();
                    }
                    break;
            }
        } else {
            highlight.Left(state, modifiers);
        }
        return true;
    }

    public override bool Right(State state, Modifiers modifiers) {
        if (highlight.EndIndex == highlight.FinalIndex || highlight.IsSelectAll) {
            switch (state) {
                case State.Press:
                    if (modifiers.HasShift()) {
                        multiInputMode.TapePointStart = tapePoint;
                        multiInputMode.TapePointEnd = tapePoint.NextOffset;
                        InputManager.Select(multiInputMode);
                    } else {
                        Save();
                        NextOffset();
                    }
                    break;
            }
        } else {
            highlight.Right(state, modifiers);
        }
        return true;
    }

    public override bool Up(State state, Modifiers modifiers) {
        switch (state) {
            case State.Press:
                if (CanPrevTape) {
                    if (modifiers.HasShift()) {
                        multiInputMode.TapePointStart = tapePoint;
                        multiInputMode.TapePointEnd = tapePoint.PrevTape;
                        InputManager.Select(multiInputMode);
                    } else {
                        Save();
                        PrevTape();
                    }
                }
                break;
        }
        return true;
    }

    public override bool Down(State state, Modifiers modifiers) {
        switch (state) {
            case State.Press:
                if (CanNextTape) {
                    if (modifiers.HasShift()) {
                        multiInputMode.Cryptex = Cryptex;
                        multiInputMode.TapePointStart = tapePoint;
                        multiInputMode.TapePointEnd = tapePoint.NextTape;
                        InputManager.Select(multiInputMode);
                    } else {
                        Save();
                        NextTape();
                    }
                }
                break;
        }
        return true;
    }

    public override bool Home(State state, Modifiers modifiers) {
        highlight.Home(state, modifiers);
        return true;
    }
    public override bool End(State state, Modifiers modifiers) {
        highlight.End(state, modifiers);
        return true;
    }

    public override bool Undo(Modifiers modifiers) {
        highlight.Undo(modifiers);
        return true;
    }

    public override bool Redo(Modifiers modifiers) {
        highlight.Redo(modifiers);
        return true;
    }

    public override bool Cut(Modifiers modifiers) {
        if (CanEdit) {
            highlight.Cut(modifiers);
        } else {
            highlight.Copy(modifiers);
        }
        return true;
    }

    public override bool Copy(Modifiers modifiers) {
        highlight.Copy(modifiers);
        return true;
    }

    public override bool Paste(Modifiers modifiers) {
        if (CanEdit) {
            highlight.Paste(modifiers);
        }
        return true;
    }

    private bool Locked => Tape.Lock.HasFlag(LockType.Edit);

    private bool CanEdit => !TapeValue.IsCut && !Locked;
}
