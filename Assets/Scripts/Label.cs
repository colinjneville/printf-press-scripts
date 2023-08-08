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

public sealed partial class Label {
    private Guid id;
    private LE name;

    public Label(Guid id, LE name) {
        this.id = id;
        this.name = name ?? LC.Empty;
    }

    public Guid Id => id;

    public LE Name {
        get => name;
        set {
            name = value;
            foreach (var view in View) {
                view.OnUpdateName();
            }
        }
    }
}
