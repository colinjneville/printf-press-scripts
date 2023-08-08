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

public sealed class FrameModifyPoint : ModifyPoint<FrameInsertPoint, Frame> {
    public Roller Roller { get; set; }
    public int Index { get; set; }

    public override bool AllowDrag => true;

    protected override bool DeleteInternal(bool forMove, bool checkOnly) {
        if (Roller.Lock.HasFlag(LockType.Edit)) {
            return false;
        }
        if (Model.Lock.HasFlag(LockType.Delete) && !forMove) {
            return false;
        }

        if (!checkOnly) {
            Workspace.ApplyModificationRecord(Roller.RemoveFrame(Index));
        }
        return true;
    }

    public override bool Edit() {
        if ((Roller.Lock | Model.Lock).HasFlag(LockType.Edit)) {
            return false;
        }
        var frame = Roller.Frames[Index];
        // TODO will need to be updated for more frame types
        FrameFlags newFlags;
        if (frame.AllowWrite) {
            newFlags = FrameFlags.FrameRead;
        } else {
            newFlags = FrameFlags.FrameReadWrite;
        }
        Workspace.ApplyModificationRecord(Roller.ModifyFrame(Index, newFlags));

        return true;
    }

    protected override Inserter<FrameInsertPoint, Frame> Inserter => FrameInserter.Instance;

    public override Frame Model => Roller.Frames[Index];

    public override void OnStartDrag() {
        MoveFramesAfter(1.5f);
    }

    public override void OnEndDrag() {
        MoveFramesAfter(-1.5f);
    }

    private void MoveFramesAfter(float delta) {
        // TODO UISCALE
        for (int i = Index + 1; i < Roller.Frames.Count; ++i) {
            foreach (var view in Roller.Frames[i].View) {
                view.transform.position += new Vector3(0f, delta, 0f);
            }
        }
    }
}
