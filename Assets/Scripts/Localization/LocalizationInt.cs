using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using L = LocalizationString;
using LI = LocalizationInt;
using LE = ILocalizationExpression;

public partial class LocalizationInt : LE {
    private int value;

    private LocalizationInt(int value) {
        this.value = value;
    }

    public string SerialString => $"[{value}]";

    public int Value => value;

    public override string ToString() => value.ToString();

    public static implicit operator LI(int integer) => new LI(integer);

    public override bool Equals(object obj) => obj is LI && value == ((LI)obj).value;

    public override int GetHashCode() => value.GetHashCode();

    public static bool operator ==(LI a, LI b) => ReferenceEquals(a, b) || (a is object && a.Equals(b));

    public static bool operator !=(LI a, LI b) => !(a == b);
}
