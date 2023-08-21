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
    public static readonly LD Ch01Lvl01_Name = new LD("Sorting");

    public static readonly LD Ch01Lvl01_00 = new LD("Sort the inputs from least to greatest. Inputs will range from 0 to 99, inclusive.");
}
