using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using LE = ILocalizationExpression;
using LD = LocalizationDefault;
using L = LocalizationString;
using LF = LocalizationFormat;

public sealed partial class LocalizationFormat {
    private string name;

    private LocalizationFormat(string name) {
        this.name = name;
    }

    public string Name => name;

    public LE Format(params LE[] les) {
        return new LocalizationFormatted(this, les);
    }

    public override string ToString() => Language.Current[Name];

    public override bool Equals(object obj) => obj is LF && name == ((LF)obj).name;

    public override int GetHashCode() => name.GetHashCode();

    public static bool operator ==(LF a, LF b) => ReferenceEquals(a, b) || (!ReferenceEquals(a, null) && a.Equals(b));

    public static bool operator !=(LF a, LF b) => !(a == b);

    public static LF Inline(string name) => new LF(name);
}


public class LocalizationFormatted : LE {
    public LocalizationFormatted(LF format, params LE[] subexpressions) {
        this.format = format;
        this.subexpressions = subexpressions ?? new LE[] { };
    }

    private LF format;
    private IEnumerable<LE> subexpressions;

    public override string ToString() => string.Format(format.ToString(), subexpressions.ToArray());
    public string SerialString => $"[{format.Name}:{string.Join(",", subexpressions.Select(se => se.SerialString))}]";

    public override bool Equals(object obj) {
        var other = obj as LocalizationFormatted;
        if (other != null) {
            return format == other.format && subexpressions.SequenceEqual(other.subexpressions);
        }
        return false;
    }

    public override int GetHashCode() {
        return Utility.GetCompositeHashCode(format, Utility.GetCompositeHashCode(subexpressions));
    }

    public static bool operator ==(LocalizationFormatted a, LocalizationFormatted b) => ReferenceEquals(a, b) || (a is object && a.Equals(b));

    public static bool operator !=(LocalizationFormatted a, LocalizationFormatted b) => !(a == b);
}
