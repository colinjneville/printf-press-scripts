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

public sealed class LabelInsertPoint : InsertPoint {
    public Cryptex Cryptex { get; set; }
    public int Index { get; set; }

    //public override Vector3 Origin => base.Origin + new Vector3(0f, -LabelViewBit.Height / 2f, 0f);
    //public override Vector3 Origin => base.Origin + new Vector3(0f, -0.5f, 0f);
}
