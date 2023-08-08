using Functional.Option;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

using L = LocalizationString;
using LC = LocalizationConstant;
using LE = ILocalizationExpression;
using LF = LocalizationFormat;
using LI = LocalizationInt;
using LD = LocalizationDefault;
using System.Reflection;

public interface ISettingValues<T> {
    LE PropertyName { get; }

    T Previous(T current, bool wrap = true);
    T Next(T current, bool wrap = true);

    LE ValueText(T Value);

    SettingAdapter MakeAdapter(SettingsData data, System.Reflection.PropertyInfo propertyInfo);
}

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public sealed class SettingsPageAttribute : Attribute {
    private LE name;

    public SettingsPageAttribute(string lName) {
        name = L.Inline(lName);
    }

    public LE Name => name;
}

public enum SettingsPage {
    [SettingsPage(nameof(LD.SettingOptionsPage))]
    Options,
    [SettingsPage(nameof(LD.SettingGraphicsPage))]
    Graphics,
    [SettingsPage(nameof(LD.SettingAudioPage))]
    Audio,
}

[AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
public abstract class SettingValuesAttribute : Attribute {
    private SettingsPage page;
    private LE name;
    private int lineNumber;

    protected SettingValuesAttribute(SettingsPage page, string lName, int lineNumber) {
        this.page = page;
        name = L.Inline(lName);
        this.lineNumber = lineNumber;
        Assert.Positive(lineNumber);
    }

    public SettingsPage Page => page;
    public LE PropertyName => name;
    public int LineNumber => lineNumber;

    protected static T Previous<T>(T[] all, T current, bool wrap = true) where T : IEquatable<T> {
        int index = GetIndex(all, current) - 1;
        if (index < 0) {
            if (wrap) {
                index = all.Length - 1;
            } else {
                index = 0;
            }
        }
        return all[index];
    }

    protected static T Next<T>(T[] all, T current, bool wrap = true) where T : IEquatable<T> {
        int index = GetIndex(all, current) + 1;
        if (index >= all.Length) {
            if (wrap) {
                index = 0;
            } else {
                index = all.Length - 1;
            }
        }
        return all[index];
    }

    protected static int GetIndex<T>(T[] all, T current) where T : IEquatable<T> {
        for (int i = 0; i < all.Length; ++i) {
            if (current.Equals(all[i])) {
                return i;
            }
        }
        return -1;
    }

    public abstract SettingAdapter MakeAdapter(SettingsData data, PropertyInfo propertyInfo);
}

public class BoolSettingValuesAttribute : SettingValuesAttribute, ISettingValues<bool> {
    public BoolSettingValuesAttribute(SettingsPage page, string lName, [System.Runtime.CompilerServices.CallerLineNumber]int lineNumber = 0) : base(page, lName, lineNumber) { }

    public bool Previous(bool current, bool wrap = true) => Previous(values, current, wrap);
    public bool Next(bool current, bool wrap = true) => Next(values, current, wrap);

    public virtual LE ValueText(bool value) => value ? LC.Temp("true") : LC.Temp("false");

    public override SettingAdapter MakeAdapter(SettingsData data, PropertyInfo propertyInfo) => new SettingAdapter<bool>(data, propertyInfo, this);

    private static bool[] values = new[] { false, true };
}

public sealed class CustomBoolSettingValuesAttribute : BoolSettingValuesAttribute {
    private LE falseName;
    private LE trueName;

    public CustomBoolSettingValuesAttribute(SettingsPage page, string lName, string falseLName, string trueLName, [System.Runtime.CompilerServices.CallerLineNumber]int lineNumber = 0) : base(page, lName, lineNumber) {
        falseName = L.Inline(falseLName);
        trueName = L.Inline(trueLName);
    }

    public override LE ValueText(bool value) => value ? trueName : falseName;
}

public class IntSettingValuesAttribute : SettingValuesAttribute, ISettingValues<int> {
    private int[] values;

    public IntSettingValuesAttribute(SettingsPage page, string lName, [System.Runtime.CompilerServices.CallerLineNumber]int lineNumber = 0, params int[] values) : base(page, lName, lineNumber) {
        this.values = values;
    }

    public int Previous(int current, bool wrap = true) => Previous(values, current, wrap);
    public int Next(int current, bool wrap = true) => Next(values, current, wrap);

    public virtual LE ValueText(int value) => (LI)value;

    public override SettingAdapter MakeAdapter(SettingsData data, PropertyInfo propertyInfo) => new SettingAdapter<int>(data, propertyInfo, this);
}

public class FloatSettingValuesAttribute : SettingValuesAttribute, ISettingValues<float> {
    private float[] values;

    public FloatSettingValuesAttribute(SettingsPage page, string lName, [System.Runtime.CompilerServices.CallerLineNumber]int lineNumber = 0, params float[] values) : base(page, lName, lineNumber) {
        this.values = values;
    }

    public float Previous(float current, bool wrap = true) => Previous(values, current, wrap);
    public float Next(float current, bool wrap = true) => Next(values, current, wrap);

    public virtual LE ValueText(float value) {
        switch (value) {
            case float.PositiveInfinity:
                return L.Inline(nameof(LD.PositiveInfinity));
            case float.NegativeInfinity:
                return L.Inline(nameof(LD.NegativeInfinity));
            case float.NaN:
                return L.Inline(nameof(LD.NaN));
            default:
                return (LI)value;
        }
    }

    public override SettingAdapter MakeAdapter(SettingsData data, PropertyInfo propertyInfo) => new SettingAdapter<float>(data, propertyInfo, this);
}

public sealed class ResolutionSettingValuesAttribute : SettingValuesAttribute, ISettingValues<Vector2Int> {
    public ResolutionSettingValuesAttribute(SettingsPage page, string lName, [System.Runtime.CompilerServices.CallerLineNumber]int lineNumber = 0) : base(page, lName, lineNumber) { }

    public Vector2Int Next(Vector2Int current, bool wrap = true) {
        var resolutions = UnityEngine.Screen.resolutions;
        int index = Utility.Mod(((GetIndex(resolutions, current) + 1) / 2), resolutions.Length);
        if (!wrap && index == 0) {
            index = resolutions.Length - 1;
        }
        return ResolutionToVector2Int(resolutions[index]);
    }

    public Vector2Int Previous(Vector2Int current, bool wrap = true) {
        var resolutions = UnityEngine.Screen.resolutions;
        // Right shift is required here to round toward negative infinity (integer division will round toward zero)
        int index = Utility.Mod(((GetIndex(resolutions, current) - 2) >> 1), resolutions.Length);
        if (!wrap && index == resolutions.Length - 1) {
            index = 0;
        }
        return ResolutionToVector2Int(resolutions[index]);
    }

    public LE ValueText(Vector2Int value) => LF.Inline(nameof(LD.SettingResolutionFormat)).Format((LI)value.x, (LI)value.y);

    public override SettingAdapter MakeAdapter(SettingsData data, PropertyInfo propertyInfo) => new SettingAdapter<Vector2Int>(data, propertyInfo, this);

    // Returns a 'spaced' index of the resolution.
    // Odd indices indicate actual supported resolutions
    // Even indicies mean an unsupported resolution between the two consecutive odd indices (or on either extreme)
    private static int GetIndex(Resolution[] resolutions, Vector2Int current) {
        for (int i = 0; i < resolutions.Length; ++i) {
            var resolution = ResolutionToVector2Int(resolutions[i]);
            if (resolution == current) {
                return i * 2 + 1;
            } else if (resolution.x > current.x || (resolution.x == current.x && resolution.y > current.y)) {
                return i * 2;
            }
        }
        return resolutions.Length * 2;
    }

    private static Vector2Int ResolutionToVector2Int(Resolution resolution) => new Vector2Int(resolution.width, resolution.height);
}

public abstract class SettingAdapter {
    private SettingsData data;
    private PropertyInfo propertyInfo;

    public SettingAdapter(SettingsData data, PropertyInfo propertyInfo) {
        this.data = data;
        this.propertyInfo = propertyInfo;
    }

    protected SettingsData Data => data;
    protected PropertyInfo PropertyInfo => propertyInfo;

    public abstract void SetPrevious(bool wrap = true);
    public abstract void SetNext(bool wrap = true);

    public abstract LE PropertyName { get; }
    public abstract LE CurrentValueText { get; }
}

public sealed class SettingAdapter<T> : SettingAdapter {
    private ISettingValues<T> values;

    public SettingAdapter(SettingsData data, PropertyInfo propertyInfo, ISettingValues<T> values) : base(data, propertyInfo) {
        this.values = values;
    }

    public override void SetPrevious(bool wrap = true) {
        Set(values.Previous(Get(), wrap));
    }

    public override void SetNext(bool wrap = true) {
        Set(values.Next(Get(), wrap));
    }

    private T Get() {
        return (T)PropertyInfo.GetValue(Data);
    }

    private void Set(T value) {
        PropertyInfo.SetValue(Data, value);
    }

    public override LE PropertyName => values.PropertyName;
    public override LE CurrentValueText => values.ValueText(Get());
}
