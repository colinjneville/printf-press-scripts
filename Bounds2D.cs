using Functional.Option;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using L = LocalizationString;
using LC = LocalizationConstant;
using LE = ILocalizationExpression;
using LF = LocalizationFormat;
using LI = LocalizationInt;

// TODO wrap more of Bounds
public struct Bounds2D : IEquatable<Bounds2D> {
    private Bounds bounds;

    public Bounds2D(Vector2 center, Vector2 size) {
        bounds = new Bounds(center, size);
    }

    public Vector2 Center {
        get => bounds.center;
        set => bounds.center = value;
    }

    public Vector2 Extents {
        get => bounds.extents;
        set => bounds.extents = value;
    }

    public Vector2 Max {
        get => bounds.max;
        set => bounds.max = value;
    }

    public Vector2 Min {
        get => bounds.min;
        set => bounds.min = value;
    }

    public Vector2 Size {
        get => bounds.size;
        set => bounds.size = value;
    }

    public void Encapsulate(Vector2 point) => bounds.Encapsulate(point);
    public void Encapsulate(Bounds2D bounds) => this.bounds.Encapsulate(bounds.bounds);

    public bool Equals(Bounds2D other) => bounds.Equals(other.bounds);
}
