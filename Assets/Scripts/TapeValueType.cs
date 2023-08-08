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


/// <summary>
/// No longer just values from Tapes, represents all in-game types
/// </summary>
public sealed partial class TapeValueType {
    private LE name;
    private Guid id;

    private TapeValueType(LE name, Guid id) {
        this.name = name;
        this.id = id;
    }

    public LE Name => name;

    public override bool Equals(object obj) {
        var other = obj as TapeValueType;
        return other != null && Equals(other);
    }

    public static bool operator ==(TapeValueType a, TapeValueType b) {
        return ReferenceEquals(a, b) || (!(a is null) && a.Equals(b));
    }

    public static bool operator !=(TapeValueType a, TapeValueType b) {
        return !(a == b);
    }

    public bool Equals(TapeValueType other) => other != null && id == other.id;

    public override int GetHashCode() => id.GetHashCode();

    public override string ToString() => name.ToString();

    private static TapeValueType @null = new TapeValueType(LC.Temp("Null"), new Guid("712D7073-FB8A-4668-82CB-14107BA66CB2"));
    public static TapeValueType Null => @null;
    private static TapeValueType @int = new TapeValueType(LC.Temp("Int"), new Guid("F521FEBD-4809-4AD4-8B79-51D408F3EE95"));
    public static TapeValueType Int => @int;
    private static TapeValueType frame = new TapeValueType(LC.Temp("Frame"), new Guid("00B3BE3E-4F34-4E72-99F4-F8E7EB2E126B"));
    public static TapeValueType Frame => frame;
    private static TapeValueType channel = new TapeValueType(LC.Temp("Channel"), new Guid("84EABC49-2E46-40DA-BF67-8B3782CEE3B0"));
    public static TapeValueType Channel => channel;
    private static TapeValueType color = new TapeValueType(LC.Temp("Color"), new Guid("B55B564E-21DC-4AEE-A471-C725C0A02335"));
    public static TapeValueType Color => color;
    private static TapeValueType @char = new TapeValueType(LC.Temp("Char"), new Guid("7F686455-AC99-44BE-BDC4-7D3872BEECEC"));
    public static TapeValueType Char => @char;

    private static TapeValueType invalid = new TapeValueType(LC.Temp("Invalid"), new Guid("CCA726FA-3519-426B-AF02-A9EC4B2EC635"));
    public static TapeValueType Invalid => invalid;
    private static TapeValueType frameRead = new TapeValueType(LC.Temp("FrameRead"), new Guid("D087E333-231D-4384-9FA2-0F0CE59D95E8"));
    public static TapeValueType FrameRead => frameRead;
    private static TapeValueType opcode = new TapeValueType(LC.Temp("Opcode"), new Guid("B13A9B58-5DF1-4C5A-86B1-CEE0975724AD"));
    public static TapeValueType Opcode => opcode;
    private static TapeValueType label = new TapeValueType(LC.Temp("Label"), new Guid("E1210333-62A6-4AC4-90A4-0C6712188D7B"));
    public static TapeValueType Label => label;
    private static TapeValueType cut = new TapeValueType(LC.Temp("Cut"), new Guid("29C6F7D1-7892-41C1-BF56-555B85A77838"));
    public static TapeValueType Cut => cut;

    private static TapeValueType[] allTypes = new[] {
        @null, @int, frame, channel, color, @char, invalid, frameRead, opcode, label, cut,
    };
    public static TapeValueType Lookup(Guid id) {
        for (int i = 0; i < allTypes.Length; ++i) {
            if (allTypes[i].id == id) {
                return allTypes[i];
            }
        }
        throw RtlAssert.NotReached();
    }

    private static Dictionary<Serial, Color32> highlightColors;

    private static Dictionary<Serial, Color32> defaultHighlightColors = new Dictionary<Serial, Color32> {
        //{ Null, new Color32(0, 0, 0, 0) },
        { Int, new Color32(0, 0, 0, 255) },
        { Frame, new Color32(128, 96, 64, 255) },
        { Channel, new Color32(96, 64, 128, 255) },
        //{ Color, new Color32(0, 0, 0, 0) },
        { Char, new Color32(64, 96, 64, 255) },
        { Invalid, new Color32(255, 0, 0, 255) },
        { FrameRead, new Color32(160, 128, 96, 255) },
        { Opcode, new Color32(0, 0, 0, 255) },
        { Label, new Color32(0, 0, 0, 255) },
        //{ Cut, new Color32(0, 0, 0, 0) },
    };

    static TapeValueType() {
        LoadHighlightColors();
    }

    private const string highlightColorsFile = "highlightcolors.json";

    private static void LoadHighlightColors() {
        var filePath = System.IO.Path.Combine(Application.persistentDataPath, highlightColorsFile);
        if (System.IO.File.Exists(filePath)) {
            try {
                var json = System.IO.File.ReadAllText(filePath);
                highlightColors = JsonConvert.DeserializeObject<Dictionary<Serial, Color32>>(json, SerializationUtility.Settings);

                return;
            } catch (Exception e) {
                Debug.LogWarning($"Failed to load highlight colors: {e}");
            }
        } else {
            var json = JsonConvert.SerializeObject(defaultHighlightColors, SerializationUtility.Settings);
            try {
                System.IO.File.WriteAllText(filePath, json);
            } catch (Exception e) {
                Debug.LogWarning($"Failed to create highlight colors file: {e}");
            }
        }
        highlightColors = defaultHighlightColors;
    }


    public Color HighlightColor => highlightColors.GetOrNone(this).ValueOr(UnityEngine.Color.black);

    public CostType CostType {
        get {
            if (this == Null) {
                return CostType.WriteBlank;
            } else if (this == Int || this == Label) {
                return CostType.WriteNumber;
            } else if (this == Frame) {
                return CostType.WriteFrame;
            } else if (this == Channel) {
                return CostType.WriteChannel;
            } else if (this == Color) {
                return CostType.WriteColor;
            } else if (this == Char) {
                return CostType.WriteLetter;
            } else if (this == Invalid) {
                return CostType.WriteInvalid;
            } else if (this == FrameRead) {
                return CostType.WriteFrameRead;
            } else if (this == Opcode) {
                return CostType.WriteOpcode;
            } else {
                Assert.NotReached();
                return CostType.WriteInvalid;
            }
        }
    }
}
