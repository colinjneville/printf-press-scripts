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
    public static readonly LD Ch01Lvl00_Name = new LD("Multiplication");

    public static readonly LD Ch01Lvl00_00 = new LD("Multiply the pairs of numbers and output the results (note: the numbers will never be negative).");
}
