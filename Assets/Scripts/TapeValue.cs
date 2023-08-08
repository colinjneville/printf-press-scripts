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

public sealed partial class TapeValueWrapper {
    private TapeValue tapeValue;

    public TapeValueWrapper(TapeValue tapeValue) {
        this.tapeValue = tapeValue;
    }

    public TapeValue Value => tapeValue;

    public static implicit operator TapeValue(TapeValueWrapper self) => self.tapeValue;
}

partial class TapeValueWrapper : IModel<TapeValueWrapper.TapeValueWrapperView> {
    private ViewContainer<TapeValueWrapperView, TapeValueWrapper> view;
    public Option<TapeValueWrapperView> View => view.View;
    public TapeValueWrapperView MakeView() => view.MakeView(this);
    public void ClearView() => view.ClearView();

    public class TapeValueWrapperView : MonoBehaviour, IView<TapeValueWrapper> {
        public TapeValueWrapper Model { get; set; }

        private PooledRef<TapeValueView> viewRef;

        void IView.StartNow() {
            viewRef = TapeValueView.GetWith(Model.tapeValue);
            foreach (var view in viewRef) {
                view.transform.SetParent(transform, false);
            }
        }

        void IView.OnDestroyNow() {
            viewRef.Return();
        }
    }
}

public abstract partial class TapeValue {
    public abstract TapeValueType Type { get; }

    public bool IsNull => Type == TapeValueType.Null;
    public bool IsCut => Type == TapeValueType.Cut;

    public virtual T To<T>() {
        throw RtlAssert.NotReached($"Attempted to convert {Type} to {typeof(T)}");
    }

    public abstract LE GetText();

    public static bool operator ==(TapeValue a, TapeValue b) {
        return ReferenceEquals(a, b) || (!(a is null) && a.Equals((object)b));
    }

    public static bool operator !=(TapeValue a, TapeValue b) {
        return !(a == b);
    }

    public override bool Equals(object obj) {
        var tv = obj as TapeValue;
        if (tv != null) {
            if (Type == tv.Type) {
                return Equals(tv);
            }
        }
        return false;
    }

    public override int GetHashCode() => Type.GetHashCode() ^ GetHashValue();

    public abstract bool Equals(TapeValue tv);

    protected abstract int GetHashValue();

    public TapeValueWrapper ToWrapper() => new TapeValueWrapper(this);

    public static TapeValue FromString(string str) {
        if (str.Length == 0) {
            return TapeValueNull.Instance;
        }
        char c0 = str[0];
        if (char.IsDigit(c0) || c0 == '-' || c0 == '+') {
            if (int.TryParse(str, out int value)) {
                if (value >= TapeValueInt.MinValue && value <= TapeValueInt.MaxValue) {
                    return new TapeValueInt(value);
                } else {
                    return new TapeValueInvalid(str);
                }
            } else {
                return new TapeValueInvalid(str);
            }
        } else {
            if (str.Length == 1 && char.IsLetter(c0)) {
                return new TapeValueChar(c0);
            }

            for (int i = 0; i < (int)ColorId.Count; ++i) {
                if (str == TapeValueColor.GetName((ColorId)i).ToString()) {
                    return new TapeValueColor((ColorId)i);
                }
            }
            
            if (c0 == TapeValueFrameRead.Prefix) {
                if (str.Length == 3 && str[1] == TapeValueFrame.Prefix && char.IsDigit(str[2])) {
                    // char digit to int conversion
                    return new TapeValueFrameRead(str[2] - '0');
                } else {
                    return new TapeValueInvalid(str);
                }
            }
            if (c0 == TapeValueFrame.Prefix && char.IsDigit(str[1])) {
                return new TapeValueFrame(str[1] - '0');
            }
            if (c0 == TapeValueChannel.Prefix && char.IsDigit(str[1])) {
                return new TapeValueChannel(str[1] - '0');
            }
            if (c0 == TapeValueLabel.Prefix) {
                return new TapeValueLabel(str.Substring(1));
            }

            if (Statement.TryLookup(str).HasValue) {
                return new TapeValueOpcode(str);
            }

            return new TapeValueInvalid(str);
        }
    }
}

public sealed class TapeValueInvalid : TapeValue {
    public override TapeValueType Type => TapeValueType.Invalid;

    private LE text;

    public TapeValueInvalid(string text) : this((LE)LC.User(text)) { }

    public TapeValueInvalid(LE text) {
        this.text = text;
    }

    public override LE GetText() => text;

    public override string ToString() => "invalid";

    public override bool Equals(TapeValue tv) => false;
    protected override int GetHashValue() => 0;
}

public sealed class TapeValueNull : TapeValue {
    public override TapeValueType Type => TapeValueType.Null;

    public override LE GetText() => LC.Empty;

    public override string ToString() => "null";

    public override bool Equals(TapeValue tv) => tv.IsNull;
    protected override int GetHashValue() => 0;

    public static TapeValueNull Instance { get; } = new TapeValueNull();
}

public abstract class TapeValue<T> : TapeValue {
    private T value;

    protected TapeValue(T value) {
        this.value = value;
    }

    public T Value => value;

    public override TT To<TT>() {
        if (typeof(T) == typeof(TT)) {
            return (TT)(object)Value;
        }
        return base.To<TT>();
    }

    public override string ToString() {
        return String.Format("{0}: {1}", Type, Value);
    }

    public override bool Equals(TapeValue tv) => Type == tv.Type && Value.Equals(tv.To<T>());
    protected override int GetHashValue() => value.GetHashCode();
}

public sealed class TapeValueInt : TapeValue<int> {
    public static int MinValue => -999;
    public static int MaxValue => 999;

    public TapeValueInt(int value) : base(Mathf.Clamp(value, MinValue, MaxValue)) { }

    public override LE GetText() => (LI)Value;

    public override TapeValueType Type => TapeValueType.Int;

    public static IEnumerable<TapeValueInt> Batch(params int[] values) => Batch((IEnumerable<int>)values);
    public static IEnumerable<TapeValueInt> Batch(IEnumerable<int> values) => values.Select(i => new TapeValueInt(i));
}

public sealed class TapeValueFrame : TapeValue<int> {
    public TapeValueFrame(int value) : base(value) {
        Assert.Within(value, Constants.FrameIndexMin, Constants.FrameIndexMax);
    }

    public override LE GetText() => LC.Temp(Prefix + Value.ToString());

    public override TapeValueType Type => TapeValueType.Frame;

    public const char Prefix = 'f';
}

public sealed class TapeValueFrameRead : TapeValue<int> {
    public TapeValueFrameRead(int value) : base(value) {
        Assert.Within(value, Constants.FrameIndexMin, Constants.FrameIndexMax);
    }

    public override LE GetText() => LC.Temp(Prefix + (TapeValueFrame.Prefix + Value.ToString()));

    public override TapeValueType Type => TapeValueType.FrameRead;

    public const char Prefix = '@';
}

public sealed class TapeValueChannel : TapeValue<int> {
    public TapeValueChannel(int value) : base(value) { }

    public override LE GetText() => LC.Temp(Prefix + Value.ToString());

    public override TapeValueType Type => TapeValueType.Channel;

    public const char Prefix = 'c';
}

public sealed class TapeValueCut : TapeValue {
    public override TapeValueType Type => TapeValueType.Cut;

    public override LE GetText() => LC.Empty;

    public override string ToString() => "X";

    public override bool Equals(TapeValue tv) => tv.IsCut;
    protected override int GetHashValue() => 1;

    public static TapeValueCut Instance { get; } = new TapeValueCut();
}

public enum ColorId {
    First = 0,

    Red = First,
    Green,
    Blue,
    Yellow,

    Count,
}

public static class ColorIdExtensions {
    public static ColorId Next(this ColorId self) => (ColorId)(((int)self + 1) % (int)ColorId.Count);
}

public sealed class TapeValueColor : TapeValue<ColorId> {
    public TapeValueColor(ColorId value) : base(value) { }
    public TapeValueColor(int value) : this((ColorId)value) { }

    public override TapeValueType Type => TapeValueType.Color;

    public override LE GetText() => GetName(Value);

    public static Color32 GetColor(ColorId id) {
        RtlAssert.Equal((int)ColorId.Count, colors.Count());
        RtlAssert.Within((int)id, 0, colors.Count());
        return colors[(int)id];
    }

    public static LE GetName(ColorId id) {
        RtlAssert.Equal((int)ColorId.Count, colors.Count());
        RtlAssert.Within((int)id, 0, colors.Count());
        return names[(int)id];
    }

    private static Color32[] colors = new Color32[] {
        Color.red,
        Color.green,
        Color.blue,
        Color.yellow,
    };

    private static LE[] names = new LE[] {
        LC.Temp("red"),
        LC.Temp("green"),
        LC.Temp("blue"),
        LC.Temp("yellow"),
    };

    public static TapeValueColor Red => new TapeValueColor(ColorId.Red);
    public static TapeValueColor Green => new TapeValueColor(ColorId.Green);
    public static TapeValueColor Blue => new TapeValueColor(ColorId.Blue);
    public static TapeValueColor Yellow => new TapeValueColor(ColorId.Yellow);
}

public sealed class TapeValueChar : TapeValue<char> {
    public TapeValueChar(char value) : base(value) { }

    public override TapeValueType Type => TapeValueType.Char;

    public override LE GetText() => LC.User(Value.ToString());

    public static IEnumerable<TapeValue> Batch(params char[] values) => Batch((IEnumerable<char>)values);

    public static IEnumerable<TapeValue> Batch(IEnumerable<char> values) => values.Select(v => new TapeValueChar(v));
}

public sealed class TapeValueOpcode : TapeValue<string> {
    public TapeValueOpcode(string value) : base(value) { }

    public override TapeValueType Type => TapeValueType.Opcode;

    public override LE GetText() => LC.User(Value);

    public Statement GetStatement() => Statement.Lookup(Value);
}

public sealed class TapeValueLabel : TapeValue<string> {
    public TapeValueLabel(string value) : base(value) { }

    public override TapeValueType Type => TapeValueType.Label;

    // TODO should be an LF expression
    public override LE GetText() => LC.User(Prefix + Value);

    public const char Prefix = ':';
}