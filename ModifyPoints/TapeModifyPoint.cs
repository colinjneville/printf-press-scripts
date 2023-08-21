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

public sealed class TapeModifyPoint : ModifyPoint<TapeInsertPoint, Tape> {
    public Tape Tape { get; set; }
    public int Offset { get; set; }

    public override bool AllowDrag => true;

    protected override bool DeleteInternal(bool forMove, bool checkOnly) {
        if ((Tape.Lock.HasFlag(LockType.Move) && forMove) || (Tape.Lock.HasFlag(LockType.Delete) && !forMove)) {
            return false;
        }

        if (!checkOnly) {
            Workspace.ApplyModificationRecord(Tape.Cryptex.RemoveTape(Tape));
        }
        return true;
    }

    protected override Inserter<TapeInsertPoint, Tape> Inserter => new TapeInserter(Offset);

    public override Tape Model => Tape;

    public override Vector3 Origin => transform.position;

    public override void OnStartDrag() {
        MoveTapesAfter(1.5f);
    }

    public override void OnEndDrag() {
        MoveTapesAfter(-1.5f);
    }

    private void MoveTapesAfter(float delta) {
        bool isAfter = false;
        foreach (var tape in Model.Cryptex.Tapes) {
            if (isAfter) {
                foreach (var view in tape.View) {
                    // TODO UISCALE
                    view.transform.position += new Vector3(0f, delta, 0f);
                }
            } else {
                if (tape == Model) {
                    isAfter = true;
                }
            }
        }
    }
}
