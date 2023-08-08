using Functional.Option;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

using LE = ILocalizationExpression;
using L = LocalizationString;
using LC = LocalizationConstant;
using LF = LocalizationFormat;
using LI = LocalizationInt;

public sealed partial class Note {
    private LE text;

    public Note(LE text) {
        this.text = text;
    }

    public LE Text {
        get => text;
        set {
            text = value;
            foreach (var view in View) {
                view.OnUpdateText();
            }
        }
    }
}
