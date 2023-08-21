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

public sealed class TapeValueModifyPoint : ModifyPoint<TapeValueInsertPoint, TapeValueWrapper> {
    public Tape Tape { get; set; }
    public int Offset { get; set; }

    // HACK
    private bool inHover = false;

    public override bool AllowDrag => false;

    public override bool Edit() {
        // TODO PERF For simplicity's sake, end the hover and clear the note view here, then immediately recreate it under NoteEditInputMode's "control"
        OnEndHover();

        var workspace = (WorkspaceFull)Tape.Cryptex.Workspace;
        // We need to make a new note if one does not exist, but don't officially add it to the tape (let NoteEditInputMode do that once a non-blank note is saved)
        Note note = Tape.Note(Offset).ValueOr(new Note(LC.Empty));
        workspace.View.ValueOrAssert().OnUpdateNote(note);

        var noteInput = WorkspaceScene.NoteEditInputMode.ValueOrAssert();
        noteInput.SetNote(workspace, Tape, Offset, note);
        Overseer.InputManager.Select(noteInput);
        
        return true;
    }

    public override bool EditAlt() {
        Tape.ToggleBreakpoint(Offset);
        return true;
    }

    protected override bool DeleteInternal(bool forMove, bool checkOnly) {
        if (Tape.Lock.HasFlag(LockType.Edit)) {
            return false;
        }

        if (Tape.IsModified(Offset)) {
            if (!checkOnly) {
                Workspace.ApplyModificationRecord(Tape.Unwrite(Tape.ShiftOffset + Offset));
            }
            return true;
        }
        return false;
    }

    public override void OnStartHover() {
        // HACK it's possible to hover over the TVMP when placing a new Tape (because of snapping). At this point we don't have Workspace set, so skip this
        if (Workspace is object) {
            foreach (var view in Workspace.View) {
                if (!view.Note.HasValue) {
                    inHover = true;
                    view.OnUpdateNote(Tape.Note(Offset));
                }
            }
        }
    }

    public override void OnEndHover() {
        if (inHover) {
            foreach (var view in Workspace.View) {
                if (view.Note == Tape.Note(Offset)) {
                    view.OnUpdateNote(Option.None);
                }
            }
            inHover = false;
        }
    }

    protected override Inserter<TapeValueInsertPoint, TapeValueWrapper> Inserter => TapeValueInserter.Instance;

    public override TapeValueWrapper Model => Tape.Read(Offset).ToWrapper();
}
