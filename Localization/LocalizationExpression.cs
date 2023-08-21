using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using UnityEngine;
using Newtonsoft.Json;

using LE = ILocalizationExpression;
using L = LocalizationString;
using LC = LocalizationConstant;
using LF = LocalizationFormat;
using LI = LocalizationInt;
using LK = LocalizationKeycode;


public interface ILocalizationExpression {
    string SerialString { get; }
}

public static class LocalizationExpression {
    public static LE Parse(string text) {
        var le = ParseSubexpression(ref text);
        Assert.Zero(text.Length);
        return le;
    }

    public static string Serialize(LE expression) => expression.SerialString;

    private static LE GetConstant(ref string text) {
        var sb = new StringBuilder();
        bool isEscaped = false;
        int i;
        for (i = 0; i < text.Length; ++i) {
            var c = text[i];
            if (isEscaped) {
                sb.Append(c);
                isEscaped = false;
            } else {
                if (c == '\\') {
                    isEscaped = true;
                } else if (c == ',' || c == ']') {
                    break;
                } else {
                    sb.Append(c);
                }
            }
        }
        Assert.False(isEscaped);
        text = text.Substring(i);
        return LC.Inline(sb.ToString());
    }

    private static LE GetLocalized(ref string text) {
        string innerText;
        char c = GetBracketedText(ref text, out innerText);
        if (innerText.Length == 0) {
            return LC.Empty;
        }
        switch (c) {
            case ']':
                if (char.IsDigit(innerText[0])) {
                    int innerInt;
                    if (!int.TryParse(innerText, out innerInt)) {
                        Assert.NotReached();
                    }
                    return (LI)innerInt;
                } else if (innerText[0] == '[') {
                    if (text.Length > 0) {
                        Assert.Equal(text[0], ']');
                        text = text.Substring(1);
                    } else {
                        Assert.NotReached();
                    }
                    int innerInt;
                    if (!int.TryParse(innerText.Substring(1), out innerInt)) {
                        Assert.NotReached();
                    }
                    return LK.Inline((KeyCode)innerInt);
                } else {
                    return L.Inline(innerText);
                }
            case ':':
                var les = Enumerable.Empty<LE>();
                while (text.Length > 0) {
                    var le = ParseSubexpression(ref text);
                    les = les.Append(le);
                    if (text.Length > 0) {
                        char cc = text[0];
                        Assert.True(cc == ']' || cc == ',');
                        text = text.Substring(1);
                        if (cc == ']') {
                            break;
                        }
                    } else {
                        Assert.NotReached();
                    }
                }
                return LF.Inline(innerText).Format(les.ToArray());
            default:
                return LC.Empty;
        }
    }

    private static char GetBracketedText(ref string text, out string innerText) {
        Assert.Equal(text[0], '[');
        for (int i = 1; i < text.Length; ++i) {
            char c = text[i];
            switch (c) {
                case ']':
                case ':':
                    innerText = text.Substring(1, i - 1);
                    text = text.Substring(i + 1);
                    return c;
            }
        }
        Assert.NotReached("Found no closing bracket or colon");
        innerText = "";
        return '\0';
    }

    private static LE ParseSubexpression(ref string text) {
        if (text.Length > 0) {
            if (text[0] == '[') {
                return GetLocalized(ref text);
            } else {
                return GetConstant(ref text);
            }
        } else {
            Assert.NotReached();
            return LC.Empty;
        }
    }
}
