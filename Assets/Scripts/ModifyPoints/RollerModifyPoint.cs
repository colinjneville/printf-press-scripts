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

public sealed class RollerModifyPoint : ModifyPoint<RollerInsertPoint, Roller> {
    public Roller Roller { get; set; }

    public override bool AllowDrag => true;

    protected override bool DeleteInternal(bool forMove, bool checkOnly) {
        if ((Roller.Lock.HasFlag(LockType.Move) && forMove) || (Roller.Lock.HasFlag(LockType.Delete) && !forMove)) {
            return false;
        }

        if (!checkOnly) {
            Workspace.ApplyModificationRecord(Roller.Cryptex.RemoveRoller(Roller));
        }
        return true;
    }

    public override bool Edit() {
        // TODO once all colors are used, it becomes impossible to change colors
        for (ColorId color = Roller.Color.Next(); color != Roller.Color; color = color.Next()) {
            if (!Roller.Cryptex.Workspace.GetRoller(color, Roller.IsPrimary).HasValue) {
                var record = Roller.ChangeColor(color);
                Workspace.ApplyModificationRecord(record);
                return true;
            }
        }
        return false;
    }

    protected override Inserter<RollerInsertPoint, Roller> Inserter => RollerInserter.Instance;

    public override Roller Model => Roller;
}
