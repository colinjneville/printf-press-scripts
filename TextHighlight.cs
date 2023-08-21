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

public sealed partial class TextHighlight {
    private string text;
    private int startIndex;
    private int endIndex;
    private bool isSelectAll;

    // HACK we don't need the Workspace in this case (SetText/CursorRecord are the only Records used here), so null is fine
    private UndoLog undoLog = new UndoLog(null);

    private Option<Action<string>> textCallback;

    public TextHighlight(string text) : this(text, Option.None) { }

    public TextHighlight(string text, Action<string> textCallback) : this(text, textCallback.ToOption()) {
        Assert.NotNull(textCallback);
    }

    private TextHighlight(string text, Option<Action<string>> textCallback) {
        this.text = text;
        this.textCallback = textCallback;
    }

    public string Text {
        get => text;
        set => SetText(value);
    }

    public int StartIndex {
        get => startIndex;
        set => SetIndices(value, EndIndex);
    }

    public int EndIndex {
        get => endIndex;
        set => SetIndices(StartIndex, value);
    }

    public bool IsSelectAll => isSelectAll;

    public bool HasSelection => StartIndex != EndIndex;

    public int FinalIndex => Text.Length;

    public void SelectAll() {
        SetIndices(0, Text.Length, true);
    }

    public void Deselect() {
        StartIndex = EndIndex;
    }

    public void SetIndices(int startIndex, int endIndex) => SetIndices(startIndex, endIndex, false);

    public void Backspace(InputMode.State state, InputMode.Modifiers modifiers) {
        switch (state) {
            case InputMode.State.Press:
                using (var frame = NewUndoBatchFrame()) {
                    if (HasSelection) {
                        DeleteSelection();
                    } else {
                        if (StartIndex > 0) {
                            SetIndices(StartIndex - 1, EndIndex - 1);
                            Text = Text.Remove(StartIndex, 1);
                        }
                    }
                }
                break;
        }
    }

    public void Delete(InputMode.State state, InputMode.Modifiers modifiers) {
        switch (state) {
            case InputMode.State.Press:
                using (NewUndoBatchFrame()) {
                    if (HasSelection) {
                        DeleteSelection();
                    } else {
                        if (StartIndex < Text.Length) {
                            Text = Text.Remove(StartIndex, 1);
                        }
                    }
                }
                break;
        }
    }

    public void Left(InputMode.State state, InputMode.Modifiers modifiers) {
        switch (state) {
            case InputMode.State.Press:
                if (IsSelectAll) {
                    EndIndex = 0;
                    DeselectIfNoShift(modifiers);
                } else if (EndIndex > 0) {
                    --EndIndex;
                    DeselectIfNoShift(modifiers);
                }
                break;
        }
    }

    public void Right(InputMode.State state, InputMode.Modifiers modifiers) {
        switch (state) {
            case InputMode.State.Press:
                if (IsSelectAll) {
                    EndIndex = FinalIndex;
                    DeselectIfNoShift(modifiers);
                } else if (EndIndex < FinalIndex) {
                    ++EndIndex;
                    DeselectIfNoShift(modifiers);
                }
                break;
        }
    }

    public void Home(InputMode.State state, InputMode.Modifiers modifiers) {
        switch (state) {
            case InputMode.State.Press:
                EndIndex = 0;
                DeselectIfNoShift(modifiers);
                break;
        }
    }
    public void End(InputMode.State state, InputMode.Modifiers modifiers) {
        switch (state) {
            case InputMode.State.Press:
                EndIndex = Text.Length;
                DeselectIfNoShift(modifiers);
                break;
        }
    }

    public bool Undo(InputMode.Modifiers modifiers) {
        if (undoLog.CanUndo) {
            undoLog.Undo();
            return true;
        }
        return false;
    }
    public bool Redo(InputMode.Modifiers modifiers) {
        if (undoLog.CanRedo) {
            undoLog.Redo();
            return true;
        }
        return false;
    }

    public void Copy(InputMode.Modifiers modifiers) {
        if (HasSelection) {
            int minIndex = Mathf.Min(StartIndex, EndIndex);
            int maxIndex = Mathf.Max(StartIndex, EndIndex);
            GUIUtility.systemCopyBuffer = Text.Substring(minIndex, maxIndex - minIndex);
        }
    }
    public void Cut(InputMode.Modifiers modifiers) {
        using (undoLog.NewBatchFrame()) {
            Copy(modifiers);
            DeleteSelection();
        }
    }
    public void Paste(InputMode.Modifiers modifiers) {
        using (undoLog.NewBatchFrame()) {
            DeleteSelection();
            Text = Text.Insert(StartIndex, GUIUtility.systemCopyBuffer);
        }
    }

    public void Char(InputMode.State state, InputMode.Modifiers modifiers, char c) {
        switch (state) {
            case InputMode.State.Press:
                using (NewUndoBatchFrame()) {
                    if (HasSelection) {
                        DeleteSelection();
                    } else if (!modifiers.HasInsert() && EndIndex < FinalIndex) {
                        Text = Text.Remove(EndIndex, 1);
                    }
                    Text = Text.Insert(EndIndex, c.ToString());
                    SetIndices(EndIndex + 1, EndIndex + 1);
                }
                break;
        }
    }

    private void DeselectIfNoShift(InputMode.Modifiers modifiers) {
        if (!modifiers.HasShift()) {
            Deselect();
        }
    }

    private void SetIndices(int startIndex, int endIndex, bool isSelectAll) {
        Assert.NotNegative(startIndex);
        Assert.NotNegative(endIndex);
        Assert.LessOrEqual(startIndex, Text.Length);
        Assert.LessOrEqual(endIndex, Text.Length);

        if (this.startIndex != startIndex || this.endIndex != endIndex || this.isSelectAll != isSelectAll) {
            undoLog.AddAndApply(new SetCursorRecord(this, startIndex, endIndex, isSelectAll));
        }
        UpdateView();
    }

    private void SetIndicesInternal(int startIndex, int endIndex, bool isSelectAll) {
        this.startIndex = startIndex;
        this.endIndex = endIndex;
        this.isSelectAll = isSelectAll;
        UpdateView();
    }

    private void SetText(string text) {
        if (this.text != text) {
            undoLog.AddAndApply(new SetTextRecord(this, text));
        }
    }

    private bool WillChangeIndicies(string text) => text.Length < StartIndex || text.Length < EndIndex;

    private void SetTextInternal(string text) {
        this.text = text;
        if (StartIndex > FinalIndex) {
            startIndex = FinalIndex;
        }
        if (EndIndex > FinalIndex) {
            endIndex = FinalIndex;
        }

        UpdateView();
        foreach (var textCallback in textCallback) {
            textCallback(text);
        }
    }

    private UndoLog.IBatchFrame NewUndoBatchFrame() => undoLog.NewBatchFrame();

    private void DeleteSelection() {
        if (HasSelection) {
            int minIndex = Mathf.Min(StartIndex, EndIndex);
            int maxIndex = Mathf.Max(StartIndex, EndIndex);
            Text = Text.Remove(minIndex, maxIndex - minIndex);
            SetIndices(minIndex, minIndex);
        }
    }

    public void Mouse0Press(InputMode.Modifiers modifiers, Vector3 position) {
        foreach (var view in View) {
            var index = view.IndexAtPoint(position);
            if (modifiers.HasShift()) {
                EndIndex = index;
            } else {
                SetIndices(index, index);
            }
        }
    }

    public void Mouse0Held(InputMode.Modifiers modifiers, Vector3 position) {
        foreach (var view in View) {
            var index = view.IndexAtPoint(position);
            EndIndex = index;
        }
    }

    public void Mouse0Release(InputMode.Modifiers modifiers, Vector3 position) {
        foreach (var view in View) {
            var index = view.IndexAtPoint(position);
            EndIndex = index;
        }
    }

    private void UpdateView() {
        foreach (var view in View) {
            view.OnUpdate();
        }
    }

    private abstract class BaseRecord : Record {
        private TextHighlight highlight;

        protected BaseRecord(TextHighlight highlight) {
            this.highlight = highlight;
        }

        protected TextHighlight Highlight => highlight;
    }

    private sealed class SetCursorRecord : BaseRecord {
        private int startIndex;
        private int endIndex;
        private bool isSelectAll;

        public SetCursorRecord(TextHighlight highlight, int startIndex, int endIndex, bool isSelectAll) : base(highlight) {
            this.startIndex = startIndex;
            this.endIndex = endIndex;
            this.isSelectAll = isSelectAll;
        }

        public override IEnumerable<CostType> GetCosts(Workspace workspace) => Array.Empty<CostType>();

        public override Record Apply(Workspace workspace, bool invertOnly = false) {
            bool oldIsSelectAll = Highlight.IsSelectAll;
            int oldStartIndex = Highlight.StartIndex;
            int oldEndIndex = Highlight.EndIndex;
            if (!invertOnly) {
                Highlight.SetIndicesInternal(startIndex, endIndex, isSelectAll);
                
            }
            return new SetCursorRecord(Highlight, oldStartIndex, oldEndIndex, oldIsSelectAll);
        }
    }

    private sealed class SetTextRecord : BaseRecord {
        private string text;

        public SetTextRecord(TextHighlight highlight, string text) : base(highlight) {
            this.text = text;
        }

        public override IEnumerable<CostType> GetCosts(Workspace workspace) => Array.Empty<CostType>();

        public override Record Apply(Workspace workspace, bool invertOnly = false) {
            string oldText = Highlight.Text;
            int oldStartIndex = Highlight.StartIndex;
            int oldEndIndex = Highlight.EndIndex;
            bool changedIndicies = Highlight.WillChangeIndicies(text);
            if (!invertOnly) {
                Highlight.SetTextInternal(text);
            }
            return changedIndicies ? (Record)new SetTextAndCursorRecord(Highlight, oldText, oldStartIndex, oldEndIndex) : new SetTextRecord(Highlight, oldText);
        }
    }

    private sealed class SetTextAndCursorRecord : BaseRecord {
        private string text;
        private int startIndex;
        private int endIndex;

        public SetTextAndCursorRecord(TextHighlight highlight, string text, int startIndex, int endIndex) : base(highlight) {
            this.text = text;
            this.startIndex = startIndex;
            this.endIndex = endIndex;
        }

        public override IEnumerable<CostType> GetCosts(Workspace workspace) => Array.Empty<CostType>();

        public override Record Apply(Workspace workspace, bool invertOnly = false) {
            string oldText = Highlight.Text;
            int oldStartIndex = Highlight.StartIndex;
            int oldEndIndex = Highlight.EndIndex;

            if (!invertOnly) {
                Highlight.SetIndicesInternal(startIndex, endIndex, Highlight.IsSelectAll);
                Highlight.SetTextInternal(text);
            }

            return new SetTextAndCursorRecord(Highlight, oldText, oldStartIndex, oldEndIndex);
        }
    }
}
