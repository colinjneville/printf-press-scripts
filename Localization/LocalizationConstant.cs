using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

using L = LocalizationString;
using LC = LocalizationConstant;
using LE = ILocalizationExpression;
using LF = LocalizationFormat;
using LK = LocalizationKeycode;

public sealed class LocalizationConstant : LE {
    private string text;

    private LocalizationConstant(string text) {
        this.text = text;
    }

    public override string ToString() => text;

    public string SerialString => Escape(text);

    public override bool Equals(object obj) => obj is LC && text.Equals(((LC)obj).text);

    public override int GetHashCode() => text.GetHashCode();

    public static bool operator ==(LC a, LC b) => ReferenceEquals(a, b) || (!(a is null) && a.Equals(b));

    public static bool operator !=(LC a, LC b) => !(a == b);

    private static string Escape(string text) {
        return text.Replace(@"\", @"\\").Replace(@"[", @"\[").Replace(@"]", @"\]").Replace(@",", @"\,");
    }

    public static LC Inline(string text, params object[] args) => new LC(string.Format(text, args));
    // TEMP
    public static LC Temp(string str, params object[] args) => Inline(str, args);
    public static LC User(string str) => Inline(str);
    public static LC Empty => Inline(string.Empty);
}

public sealed class LocalizationKeycode : LE {
    private KeyCode keyCode;

    private LocalizationKeycode(KeyCode keyCode) {
        this.keyCode = keyCode;
    }

    public override string ToString() => keyCode.ToString();

    public string SerialString => $"[[{(int)keyCode}]]";

    public override bool Equals(object obj) => obj is LK && keyCode.Equals(((LK)obj).keyCode);

    public override int GetHashCode() => keyCode.GetHashCode();

    public static bool operator ==(LK a, LK b) => ReferenceEquals(a, b) || (!(a is null) && a.Equals(b));

    public static bool operator !=(LK a, LK b) => !(a == b);

    
    public static LK Inline(KeyCode data) => new LK(data);
}
