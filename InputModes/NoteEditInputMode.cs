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

public sealed partial class NoteEditInputMode : InputMode {
    private WorkspaceFull workspace;
    private Tape tape;
    private int index;
    private Note note;

    private TextHighlight highlight;

    public NoteEditInputMode(InputManager manager) : base(manager) { }

    public void SetNote(WorkspaceFull workspace, Tape tape, int index, Note note) {
        this.workspace = workspace;
        this.tape = tape;
        this.index = index;
        this.note = note;
        CreateHighlight();
    }

    public WorkspaceFull Workspace => workspace;

    public Tape Tape => tape;

    public int Index => index;

    public Note Note => note;

    public override bool AlwaysActive => false;

    public override void OnSelect() {
        // TODO put this somewhere, even though it owns no visible components
        var view = MakeView();

        //view.transform.parent = Screen.Active.Canvas[4];
        //Screen.Active.FixedProxy.CreateReceiver(view.gameObject);

    }
    public override void OnDeselect() {
        Save();
        Note.MakeView().TextOverride = Option.None;
        ClearView();

        foreach (var view in Workspace.View) {
            view.OnUpdateNote(Option.None);
        }
    }

    private void CreateHighlight() {
        var oldHighlight = highlight;
        highlight = new TextHighlight(Note.Text.ToString());
        highlight.SelectAll();
        foreach (var view in View) {
            view.OnUpdateHighlight(oldHighlight, highlight);
        }
    }

    private void ReadText() {
        highlight.Text = Note.Text.ToString();
        highlight.SelectAll();
    }

    public override bool Mouse0(State state, Modifiers modifiers, Vector3 position, Lazy<RaycastHit2D[]> hits) {
        switch (state) {
            case State.Press:
                foreach (var view in View) {
                    foreach (var noteView in Note.View) {
                        foreach (var hit in hits.Value) {
                            // HACK
                            var nvb = hit.collider.GetComponent<NoteViewBit>();
                            if (nvb != null && nvb.TMP == noteView.TMP) {
                                highlight.Mouse0Press(modifiers, position);
                                return true;
                            }
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
        highlight.Char(state, modifiers, c);
        return true;
    }

    public override bool Left(State state, Modifiers modifiers) {
        highlight.Left(state, modifiers);
        return true;
    }

    public override bool Down(State state, Modifiers modifiers) {
        return Left(state, modifiers);
    }

    public override bool Right(State state, Modifiers modifiers) {
        highlight.Right(state, modifiers);
        return true;
    }

    public override bool Up(State state, Modifiers modifiers) {
        return Right(state, modifiers);
    }

    public override bool Home(State state, Modifiers modifiers) {
        highlight.Home(state, modifiers);
        return true;
    }
    public override bool End(State state, Modifiers modifiers) {
        highlight.End(state, modifiers);
        return true;
    }

    public override bool Escape(State state, Modifiers modifiers) {
        switch (state) {
            case State.Press:
                ReadText();
                InputManager.Deselect(this);
                break;
        }
        return true;
    }

    public override bool Return(State state, Modifiers modifiers) {
        switch (state) {
            case State.Press:
                if (modifiers.HasShift()) {
                    highlight.Char(state, modifiers & ~Modifiers.Shift, '\n');
                } else {
                    InputManager.Deselect(this);
                }
                break;
        }
        return true;
    }

    public override bool Space(State state, Modifiers modifiers) {
        highlight.Char(state, modifiers, ' ');
        return true;
    }

    public override bool Backspace(State state, Modifiers modifiers) {
        highlight.Backspace(state, modifiers);
        return true;
    }
    public override bool Delete(State state, Modifiers modifiers) {
        highlight.Delete(state, modifiers);
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
        highlight.Cut(modifiers);
        return true;
    }

    public override bool Copy(Modifiers modifiers) {
        highlight.Copy(modifiers);
        return true;
    }

    public override bool Paste(Modifiers modifiers) {
        highlight.Paste(modifiers);
        return true;
    }

    private void Save() {
        if (highlight.Text != Note.Text.ToString()) {
            Record record;
            if (string.IsNullOrWhiteSpace(highlight.Text)) {
                if (!Tape.Note(Index).HasValue) {
                    // There was no note here to begin with, and we decided not to add one; exit early
                    return;
                }
                record = Tape.RemoveNote(Index);
            } else {
                record = Tape.AddNote(Index, LC.User(highlight.Text));
            }
            
            Workspace.ApplyModificationRecord(record);
        }
    }
}
