using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

using L = LocalizationString;
using LC = LocalizationConstant;
using LE = ILocalizationExpression;
using LF = LocalizationFormat;
using LI = LocalizationInt;
using LK = LocalizationKeycode;

public sealed partial class LocalizationString : LE {
    private string name;

    private LocalizationString(string name) {
        this.name = name;
    }

    public string Name => name;

    public override string ToString() => Language.Current[Name];

    public string SerialString => $"[{Name}]";

    public override bool Equals(object obj) => obj is L && name == ((L)obj).name;

    public override int GetHashCode() => name.GetHashCode();

    public static bool operator ==(L a, L b) => a.Equals(b);

    public static bool operator !=(L a, L b) => !(a == b);

    public static L Inline(string name) => new L(name);

}
