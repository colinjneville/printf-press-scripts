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

public sealed partial class OutputTape {
    private ImmutableList<TapeValue> values;

    public OutputTape() {
        values = ImmutableList<TapeValue>.Empty;
    }

    public OutputTape(IEnumerable<TapeValue> values) {
        this.values = values.ToImmutableList();
    }

    public IImmutableList<TapeValue> Values => values;

    public void Push(TapeValue value) => Push(value, updateView: true);

    private void Push(TapeValue value, bool updateView) {
        values = values.Add(value);
        foreach (var view in View) {
            view.OnPushValues();
        }
    }

    public TapeValue Pop() => Pop(updateView: true);

    private TapeValue Pop(bool updateView) {
        Assert.NotEmpty(values);
        var value = values[values.Count - 1];
        values = values.RemoveAt(values.Count - 1);
        if (updateView) {
            foreach (var view in View) {
                view.OnPopValues();
            }
        }

        return value;
    }

    public void PushRange(IEnumerable<TapeValue> values) {
        foreach (var value in values) {
            Push(value, updateView: false);
        }
        foreach (var view in View) {
            view.OnPushValues();
        }
    }

    public void Clear() {
        while (values.Count > 0) {
            Pop(updateView: false);
        }
        foreach (var view in View) {
            view.OnPopValues();
        }
    }

    public bool IsSubsetOf(OutputTape other) {
        if (values.Count() > other.values.Count()) {
            return false;
        }
        return IsPartialMatch(other);
    }

    public bool IsMatch(OutputTape other) {
        if (values.Count() == other.values.Count()) {
            return false;
        }
        return IsPartialMatch(other);
    }

    private bool IsPartialMatch(OutputTape other) {
        for (int i = 0; i < values.Count(); ++i) {
            if (values[i] != other.values[i]) {
                return false;
            }
        }
        return true;
    }
}
