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
    public static readonly LD UICurrencyCost = new LD(@"{0:\$#;-\$#;\$0}");

    public static readonly LD SettingOptionsPage = new LD(@"Options");
    public static readonly LD SettingGraphicsPage = new LD(@"Graphics");
    public static readonly LD SettingAudioPage = new LD(@"Audio");

    public static readonly LD SettingPlaySpeed = new LD(@"Play Speed");
    public static readonly LD SettingCameraSpeed = new LD(@"Camera Speed");

    public static readonly LD SettingResolution = new LD(@"Resolution");
    public static readonly LD SettingFullScreen = new LD(@"Full Screen");
    public static readonly LD SettingWindowedFullScreen = new LD(@"Windowed Full Screen");

    public static readonly LD SettingResolutionFormat = new LD(@"{0}x{1}");
}
