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

[Serializable]
public sealed partial class DialogSequence {
    private List<DialogItem> items;

    public DialogSequence(params DialogItem[] items) : this((IEnumerable<DialogItem>)items) { }

    public DialogSequence(IEnumerable<DialogItem> items) {
        this.items = items.ToList();
    }

    public IReadOnlyList<DialogItem> Items => items;
}
