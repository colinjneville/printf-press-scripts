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

public abstract class ValueInputMode : InputMode {
    private Guid cryptexId;

    protected ValueInputMode(InputManager manager) : base(manager) { }

    public Cryptex Cryptex {
        get => Overseer.Workspace.ValueOrAssert().GetCryptex(cryptexId).ValueOrAssert();
        set => cryptexId = value.Id;
    }
    protected Tape GetTape(TapeSelectionPoint selection) => Cryptex.Tapes[selection.TapeIndex];
}


public abstract class TextEditInputMode : InputMode {
    protected TextEditInputMode(InputManager manager) : base(manager) { }

    private string text;
    private int startIndex;
    private int endIndex;

    private bool isSelectAll;

    // HACK we don't need the Workspace in this case (SetText/CursorRecord are the only Records used here), so null is fine
    private UndoLog undoLog = new UndoLog(null);

    protected void ClearUndoLog() {
        undoLog.Clear();
    }

    protected UndoLog.IBatchFrame NewUndoBatchFrame() => undoLog.NewBatchFrame();

    protected bool IsSelectAll => isSelectAll;

    protected string Text {
        get => text;
        set {
            if (text != value) {
                undoLog.AddAndApply(new SetTextRecord(this, value));
            }
        }
    }

    public int StartIndex {
        get => startIndex;
        set {
            if (startIndex != value) {
                undoLog.AddAndApply(new SetCursorRecord(this, value, endIndex, false));
            }
        }
    }

    public int EndIndex {
        get => endIndex;
        set {
            if (endIndex != value) {
                undoLog.AddAndApply(new SetCursorRecord(this, startIndex, value, false));
            }
        }
    }

    public void SetIndices(int startIndex, int endIndex) => SetIndices(startIndex, endIndex, false);

    private void SetIndices(int startIndex, int endIndex, bool isSelectAll) {
        if (this.startIndex != startIndex || this.endIndex != endIndex || this.isSelectAll != isSelectAll) {
            undoLog.AddAndApply(new SetCursorRecord(this, startIndex, endIndex, isSelectAll));
        }
    }

    public void SelectAll() {
        SetIndices(0, Text.Length, true);
    }

    protected void Deselect() {
        StartIndex = EndIndex;
    }

    protected bool HasSelection => StartIndex != EndIndex;

    protected void DeselectIfNoShift(Modifiers modifiers) {
        if (!modifiers.HasShift()) {
            Deselect();
        }
    }

    protected void DeleteSelection() {
        if (HasSelection) {
            int minIndex = Mathf.Min(StartIndex, EndIndex);
            int maxIndex = Mathf.Max(StartIndex, EndIndex);
            Text = Text.Remove(minIndex, maxIndex - minIndex);
            SetIndices(minIndex, minIndex);
        }
    }

    protected virtual void OnUpdateStartIndex() { }
    protected virtual void OnUpdateEndIndex() { }
    protected virtual void OnUpdateText() { }

    protected abstract class BaseRecord : Record {
        private TextEditInputMode input;

        protected BaseRecord(TextEditInputMode input) {
            this.input = input;
        }

        protected TextEditInputMode Input => input;
    }

    protected sealed class SetCursorRecord : BaseRecord {
        private int startIndex;
        private int endIndex;
        private bool isSelectAll;

        public SetCursorRecord(TextEditInputMode input, int startIndex, int endIndex, bool isSelectAll) : base(input) {
            this.startIndex = startIndex;
            this.endIndex = endIndex;
            this.isSelectAll = isSelectAll;
        }

        public override IEnumerable<CostType> GetCosts(Workspace workspace) => Array.Empty<CostType>();

        public override Record Apply(Workspace workspace, bool invertOnly = false) {
            int oldStartIndex = Input.startIndex;
            int oldEndIndex = Input.endIndex;
            bool oldIsSelectAll = Input.isSelectAll;
            if (!invertOnly) {
                Input.startIndex = startIndex;
                Input.endIndex = endIndex;
                Input.isSelectAll = isSelectAll;
                Input.OnUpdateStartIndex();
                Input.OnUpdateEndIndex();
            }
            return new SetCursorRecord(Input, oldStartIndex, oldEndIndex, oldIsSelectAll);
        }
    }

    protected sealed class SetTextRecord : BaseRecord {
        private string text;

        public SetTextRecord(TextEditInputMode input, string text) : base(input) {
            this.text = text;
        }

        public override IEnumerable<CostType> GetCosts(Workspace workspace) => Array.Empty<CostType>();

        public override Record Apply(Workspace workspace, bool invertOnly = false) {
            var oldText = Input.text;
            if (!invertOnly) {
                Input.text = text;
                Input.OnUpdateText();
            }
            return new SetTextRecord(Input, oldText);
        }
    }

    public override bool Undo(Modifiers modifiers) {
        if (undoLog.CanUndo) {
            undoLog.Undo();
            return true;
        }
        return false;
    }
    public override bool Redo(Modifiers modifiers) {
        if (undoLog.CanRedo) {
            undoLog.Redo();
            return true;
        }
        return false;
    }

    public override bool Copy(Modifiers modifiers) {
        if (HasSelection) {
            int minIndex = Mathf.Min(StartIndex, EndIndex);
            int maxIndex = Mathf.Max(StartIndex, EndIndex);
            GUIUtility.systemCopyBuffer = Text.Substring(minIndex, maxIndex - minIndex);
        }
        return true;
    }
    public override bool Cut(Modifiers modifiers) {
        using (undoLog.NewBatchFrame()) {
            Copy(modifiers);
            DeleteSelection();
            return true;
        }
    }
    public override bool Paste(Modifiers modifiers) {
        using (undoLog.NewBatchFrame()) {
            DeleteSelection();
            Text = Text.Insert(StartIndex, GUIUtility.systemCopyBuffer);
            return true;
        }
    }

    public override bool Char(State state, Modifiers modifiers, char c) {
        if (modifiers.HasCtrl()) {
            return false;
        }

        switch (state) {
            case State.Press:
                using (NewUndoBatchFrame()) {
                    if (HasSelection) {
                        DeleteSelection();
                    } else if (!modifiers.HasInsert() && EndIndex < Text.Length) {
                        Text = Text.Remove(EndIndex, 1);
                    }
                    Text = Text.Insert(EndIndex, c.ToString());
                    SetIndices(EndIndex + 1, EndIndex + 1);
                }
                break;
        }
        return true;
    }
}