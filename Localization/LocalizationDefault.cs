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

public sealed partial class LocalizationDefault {
    private string defaultValue;

    private LocalizationDefault(string defaultValue) {
        this.defaultValue = defaultValue;
    }

    public string DefaultValue => defaultValue;

    private const string startTermTag = "<b>";
    private const string endTermTag = "</b>";

    private static Word platen => "platen";
    private static Word tape => "tape";
    private static Word head => "head";
    private static Word control => "control";
    private static Word auxiliary => "auxiliary";
    private static Word frame => "frame";
    private static Word nit => "nit";
    private static Word label => "label";

    private static Word toolbox => "toolbox";

    [Flags]
    private enum WordForm {
        Plural = 0x1,
        Capitalized = 0x2,
        Term = 0x4,
        Possessive = 0x8,
    }

    private static WordForm P => WordForm.Plural;
    private static WordForm C => WordForm.Capitalized;
    private static WordForm T => WordForm.Term;
    private static WordForm O => WordForm.Possessive;

    private struct Word {
        private string str;

        public Word(string str) {
            Assert.False(string.IsNullOrEmpty(str));
            this.str = str;
        }

        public string this[WordForm form] {
            get {
                var str = this.str;
                if (form.HasFlag(WordForm.Plural)) {
                    // TODO
                    str = str + 's';
                } 
                if (form.HasFlag(WordForm.Possessive)) {
                    str = str + "'s";
                }
                if (form.HasFlag(WordForm.Capitalized)) {
                    str = char.ToUpper(str[0]) + str.Substring(1);
                }
                if (form.HasFlag(WordForm.Term)) {
                    str = startTermTag + str + endTermTag;
                }
                return str;
            }
        }

        public static implicit operator Word(string str) => new Word(str);
        public static implicit operator string(Word word) => word.ToString();

        public override string ToString() => str;
    }
}
