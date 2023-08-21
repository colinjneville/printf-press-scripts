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
    public static readonly LD Platen = new LD(@"platen");
    public static readonly LD Tape = new LD(@"tape");
    public static readonly LD Head = new LD(@"head");
    public static readonly LD Control = new LD(@"control");
    public static readonly LD Auxiliary = new LD(@"auxiliary");
    public static readonly LD Frame = new LD(@"frame");
    public static readonly LD Nit = new LD(@"nit");
    public static readonly LD Label = new LD(@"label");
    public static readonly LD Toolbox = new LD(@"toolbox");
}
