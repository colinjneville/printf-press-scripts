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

public sealed class FrameInsertPoint : InsertPoint {
    public Roller Roller { get; set; }
    public int Index { get; set; }

    public override void OnStartSnap() {
        if (Index == 0 && Roller.Frames.Count > 0) {
            foreach (var view in Roller.Frames[0].View) {
                view.OnUpdateBridge(true);
            }
        }
        MoveFramesAfter(-1.5f);
    }

    public override void OnEndSnap() {
        if (Index == 0 && Roller.Frames.Count > 0) {
            foreach (var view in Roller.Frames[0].View) {
                view.OnUpdateBridge(false);
            }
        }
        MoveFramesAfter(1.5f);
    }

    private void MoveFramesAfter(float delta) {
        // TODO UISCALE
        for (int i = Index; i < Roller.Frames.Count; ++i) {
            foreach (var view in Roller.Frames[i].View) {
                view.transform.position += new Vector3(0f, delta, 0f);
            }
        }
    }
}
