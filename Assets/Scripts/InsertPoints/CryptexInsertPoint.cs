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

public sealed class CryptexInsertPoint : InsertPoint {
    private Vector2 cursorXYHack;

    public override Vector3 Origin => XY;

    public override bool AllowSnap => true;

    public Vector2 XY => cursorXYHack;

    public override void OnHoverHack(Vector2 xy) {
        // For now, only change the y coordinate to keep horizontal alignment
        cursorXYHack = xy.WithX(0f);
    }
}
