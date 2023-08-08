﻿using Functional.Option;
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

public sealed class RollerInsertPoint : InsertPoint {
    public Tape Tape { get; set; }
    public int Offset { get; set; }

    public override Vector3 Origin => base.Origin - transform.localPosition.WithZ(0f);
}
