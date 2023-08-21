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

public sealed class TapeInsertPoint : InsertPoint {
    public Cryptex Cryptex { get; set; }
    public Option<Tape> Tape { get; set; }
    public int Index { get; set; }
    public int Offset { get; set; }

    public override void OnStartSnap() {
        MoveTapesAfter(-1.5f);
    }

    public override void OnEndSnap() {
        MoveTapesAfter(1.5f);
    }

    private void MoveTapesAfter(float delta) {
        // TODO UISCALE
        bool isAfter = !Tape.HasValue;
        foreach (var tape in Cryptex.Tapes) {
            if (isAfter) {
                foreach (var view in tape.View) {
                    view.transform.position += new Vector3(0f, delta, 0f);
                }
            } else {
                isAfter |= Tape.Equals(tape);
            }
        }
    }
}
