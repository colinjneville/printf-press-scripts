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

using LD = LocalizationDefault;

partial class LocalizationDefault {
    public static readonly LD Concat = new LD("{0}{1}");

    public static readonly LD ConcatSpace = new LD("{0} {1}");

    public static readonly LD PositiveInfinity = new LD(@"Infinity");
    public static readonly LD NegativeInfinity = new LD(@"-Infinity");
    public static readonly LD NaN = new LD(@"NaN");
}

