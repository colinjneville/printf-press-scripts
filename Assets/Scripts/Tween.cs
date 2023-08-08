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


public interface ITweenStrategy { }

public interface ITweenStrategy<T> : ITweenStrategy { }

public interface ITweenStrategy<T, TState> : ITweenStrategy<T> {
    T MoveToward(ref TState state, T current, RectTransform target, TweenSteps<T> steps, float timeDelta);

    TState DefaultState { get; }
}

[AttributeUsage(AttributeTargets.Class)]
public sealed class TweenStrategyAttribute : Attribute {
    private bool hideState;

    public TweenStrategyAttribute(bool fixedState = false) {
        this.hideState = fixedState;
    }

    public bool HideState => hideState;

    private static TweenStrategyAttribute @default = new TweenStrategyAttribute();

    public static TweenStrategyAttribute Default => @default;
}

public abstract class TweenStrategy<T, TState> : ScriptableObject, ITweenStrategy<T, TState> {
    [SerializeField]
    private TState scratchState;

    protected TweenStrategy() {
        scratchState = DefaultState;
    }

    public abstract T MoveToward(ref TState state, T current, RectTransform target, TweenSteps<T> steps, float timeDelta);

    public abstract TState DefaultState { get; }
}

public sealed class TweenStrategyFloatMoveToward : TweenStrategy<float, IntegralFloat> {
    private TweenStrategyFloatMoveToward() { }

    public override float MoveToward(ref IntegralFloat state, float current, RectTransform target, TweenSteps<float> steps, float timeDelta) {
        float delta;
        state = state.Advance(timeDelta, out delta);
        return Mathf.MoveTowards(current, steps.Next, delta);
    }

    public override IntegralFloat DefaultState => new IntegralFloat(1f, 10f);

    public static TweenStrategyFloatMoveToward Make() => CreateInstance<TweenStrategyFloatMoveToward>();
}

public sealed class TweenStrategyVector2MoveToward : TweenStrategy<Vector2, IntegralFloat> {
    private TweenStrategyVector2MoveToward() { }

    public override Vector2 MoveToward(ref IntegralFloat state, Vector2 current, RectTransform target, TweenSteps<Vector2> steps, float timeDelta) {
        float delta;
        state = state.Advance(timeDelta, out delta);
        return Vector2.MoveTowards(current, steps.Next, delta);
    }

    public override IntegralFloat DefaultState => new IntegralFloat(1f, 10f);

    public static TweenStrategyVector2MoveToward Make() => CreateInstance<TweenStrategyVector2MoveToward>();
}

public sealed class TweenStrategyVector2Halflife : TweenStrategy<Vector2, float> {
    private TweenStrategyVector2Halflife() { }

    public override Vector2 MoveToward(ref float state, Vector2 current, RectTransform target, TweenSteps<Vector2> steps, float timeDelta) {
        Vector2 delta = steps.Next - current;
        Vector2 newDelta = delta * Mathf.Pow(2f, -timeDelta / state);

        return steps.Next - newDelta;
    }

    public override float DefaultState => 0.5f;

    public static TweenStrategyVector2Halflife Make() => CreateInstance<TweenStrategyVector2Halflife>();
}

[TweenStrategy(fixedState: true)]
public sealed class TweenStrategyVector2Logistic : TweenStrategy<Vector2, float> {
    [SerializeField]
    private float k;
    [SerializeField]
    private float t1;

    private TweenStrategyVector2Logistic() { }

    public override Vector2 MoveToward(ref float state, Vector2 current, RectTransform target, TweenSteps<Vector2> steps, float timeDelta) {
        //Vector2 y0 = steps.NextDelta / (1f + Mathf.Exp(k / 2f));
        var prev = Evaluate(state);
        state += timeDelta / t1;
        var next = Evaluate(state);
        var yDelta = next - prev;
        var diff = (steps.Next - current) / (1f - prev) * new Vector2(yDelta, yDelta);
        return current + diff;
    }

    private float Evaluate(float x) => 1f / (1f + Mathf.Exp(-k * (x - 0.5f)));

    public override float DefaultState => 0f;

    public static TweenStrategyVector2Logistic Make(float k, float t1) {
        var o = CreateInstance<TweenStrategyVector2Logistic>();
        o.k = k;
        o.t1 = t1;
        return o;
    }
}

public interface ITweenController {
    void RegisterDimensions(Tween tween);

    void StartStep(Tween tween);
    void ContinueStep(Tween tween, float timeDelta);
    void EndStep(Tween tween);

    void Complete(Tween tween);
}

public abstract class TweenDimension {
}

public abstract class TweenDimension<T> : TweenDimension {
    public abstract T Get(RectTransform rt);
    public abstract void Set(RectTransform rt, T value);

    public virtual T Identity => default;

    public abstract T Combine(T a, T b);
    public abstract T Invert(T a);

    public abstract bool Equal(T self, T other);

    public override bool Equals(object obj) => !ReferenceEquals(obj, null) && GetType() == obj.GetType();

    public override int GetHashCode() => GetType().GetHashCode();
}

public abstract class TweenDimensionFloat : TweenDimension<float> {
    public override bool Equal(float self, float other) => Mathf.Approximately(self, other);
}

public abstract class TweenDimensionFloatLinear : TweenDimensionFloat {
    public override float Combine(float a, float b) => a + b;
    public override float Invert(float a) => -a;
}

public abstract class TweenDimensionFloatGeometric : TweenDimensionFloat {
    public override float Identity => 1f;
    public override float Combine(float a, float b) => a * b;
    public override float Invert(float a) => 1f / a;
}

public abstract class TweenDimensionVector2 : TweenDimension<Vector2> {
    protected virtual float Threshold => 0.01f;

    public override bool Equal(Vector2 self, Vector2 other) => (self - other).sqrMagnitude <= Threshold;
}

public abstract class TweenDimensionVector2Linear : TweenDimensionVector2 {
    public override Vector2 Combine(Vector2 a, Vector2 b) => a + b;
    public override Vector2 Invert(Vector2 a) => -a;
}

public abstract class TweenDimensionVector2Geometric : TweenDimensionVector2 {
    public override Vector2 Identity => Vector2.one;
    public override Vector2 Combine(Vector2 a, Vector2 b) => a * b;
    public override Vector2 Invert(Vector2 a) => new Vector2(1f / a.x, 1f / a.y);
}

public sealed class TweenDimensionPosition : TweenDimensionVector2Linear {
    public override Vector2 Get(RectTransform rt) => rt.position;
    public override void Set(RectTransform rt, Vector2 value) => rt.position = value;
}

public sealed class TweenDimensionImageAlpha : TweenDimensionFloatLinear {
    public override float Get(RectTransform rt) => rt.GetComponent<Image>().color.a;
    public override void Set(RectTransform rt, float value) {
        var image = rt.GetComponent<Image>();
        image.color = image.color.WithA(value);
    }
}

public sealed class TweenDimensionHeight : TweenDimensionFloatLinear {
    /*
    public override float Get(RectTransform rt) => rt.anchorMax.y - rt.anchorMin.y;
    public override void Set(RectTransform rt, float value) {
        float existing = Get(rt);
        float scale = value / existing;
        rt.anchorMax = rt.anchorMax.WithY(rt.anchorMax.y * scale);
        rt.anchorMin = rt.anchorMin.WithY(rt.anchorMin.y * scale);
    }
    */
    public override float Get(RectTransform rt) => rt.rect.height;
    public override void Set(RectTransform rt, float value) {
        float diff = value - Get(rt);
        rt.sizeDelta += new Vector2(0f, diff);
    }
}

public sealed class TweenDimensionScale : TweenDimensionVector2Geometric {
    public override Vector2 Get(RectTransform rt) => rt.rect.size;
    public override void Set(RectTransform rt, Vector2 value) {
        Vector2 diff = value - Get(rt);
        rt.sizeDelta += diff;
    }
}

public abstract class TweenSteps {
    public abstract void AddStep();
    public abstract void SetAtTarget();
    public abstract void Discard();

    public abstract void Apply();

    public abstract bool Met();
}

public sealed class TweenSteps<T> : TweenSteps, IEnumerable<T> {
    private Tween tween;
    private List<T> values;
    private TweenDimension<T> dimension;
    private T valueBase;

    public TweenSteps(Tween tween, TweenDimension<T> dimension) {
        this.tween = tween;
        this.dimension = dimension;
        values = new List<T>();
        valueBase = dimension.Identity;
    }

    public TweenDimension<T> Dimension => dimension;

    public override void AddStep() {
        foreach (var target in tween.Target) {
            T newBase = Dimension.Combine(Dimension.Get(tween.RectTransform), Dimension.Invert(Dimension.Get(target)));
            for (int i = 0; i < values.Count; ++i) {
                values[i] = Dimension.Combine(Dimension.Combine(values[i], Dimension.Invert(valueBase)), newBase);
            }

            values.Add(Dimension.Identity);
            valueBase = newBase;
        }
    }

    public override void SetAtTarget() {
        if (values.Count > 0) {
            foreach (var target in tween.Target) {
                dimension.Set(tween.RectTransform, dimension.Combine(dimension.Get(target), values[0]));
            }
        }
    }

    public override void Discard() {
        if (values.Count > 0) {
            values.RemoveAt(0);
        }
    }

    public override void Apply() {
        SetAtTarget();
        Discard();
    }

    public override bool Met() {
        if (values.Count > 0) {
            foreach (var target in tween.Target) {
                return dimension.Equal(dimension.Get(tween.RectTransform), dimension.Combine(dimension.Get(target), values[0]));
            }
        }
        return true;
    }

    public T Next {
        get {
            foreach (var target in tween.Target) {
                return dimension.Combine(dimension.Get(target), NextDelta);
            }
            return dimension.Get(tween.RectTransform);
        }
    }

    public T NextDelta => values.Count == 0 ? Dimension.Identity : values[0];

    public void MoveToward<TState>(RectTransform rt, ref TState state, ITweenStrategy<T, TState> strategy, float timeDelta) {
        var newValue = strategy.MoveToward(ref state, Dimension.Get(rt), tween.RectTransform, this, timeDelta);
        Dimension.Set(rt, newValue);
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    public IEnumerator<T> GetEnumerator() => values.GetEnumerator();
}

[Serializable]
public struct IntegralFloat {
    [SerializeField]
    private float n0;
    [SerializeField]
    private float n1;
    [SerializeField]
    private float n2;
    [SerializeField]
    private float n3;

    public IntegralFloat(float n0, float n1 = 0f, float n2 = 0f, float n3 = 0f) {
        this.n0 = n0;
        this.n1 = n1;
        this.n2 = n2;
        this.n3 = n3;
    }

    public float N0 => n0;
    public float N1 => n1;
    public float N2 => n2;
    public float N3 => n3;

    public IntegralFloat Advance(float timeDelta, out float delta) {
        float n2_ = n2 + Integrate(n3, n3, timeDelta);
        float n1_ = n1 + Integrate(n2, n2_, timeDelta);
        float n0_ = n0 + Integrate(n1, n1_, timeDelta);
        delta = Integrate(n0, n0_, timeDelta);
        return new IntegralFloat(n0_, n1_, n2_, n3);
    }

    public IntegralFloat WithN0(float n0) => new IntegralFloat(n0, n1, n2, n3);
    public IntegralFloat WithN1(float n1) => new IntegralFloat(n0, n1, n2, n3);
    public IntegralFloat WithN2(float n2) => new IntegralFloat(n0, n1, n2, n3);
    public IntegralFloat WithN3(float n3) => new IntegralFloat(n0, n1, n2, n3);

    private static float Integrate(float y0, float y1, float dx) => (y0 + y1) * dx / 2f;
}

public sealed class Tween : MonoBehaviour {
    private sealed class Relocker : IDisposable {
        private Tween tween;
        private Transform parent;

        public Relocker(Tween tween) {
            Assert.NotNull(tween);
            this.tween = tween;
            parent = tween.transform.parent;

            tween.UnlockInternal();
            tween.transform.SetParent(null, true);
        }

        ~Relocker() {
            Assert.RefNull(tween);
        }

        public void Dispose() {
            if (tween != null) {
                if (!ReferenceEquals(parent, null)) {
                    Assert.RefNull(tween.transform.parent, "Unlocked Tween was reparented");
                    if (parent == null) {
                        // parent was destroyed during the unlock, this should be destroyed as well
                        tween.DestroyGameObject();
                    } else {
                        tween.transform.SetParent(parent, true);
                    }
                }
                tween.RelockInternal();
            }
            tween = null;
        }
    }

    public enum AddMode {
        Skip,
        Continue,
        Restart,
        Sequence,
        Signal,
    }

    [SerializeField]
    private AddMode mode;

    [SerializeField]
    [HideInInspector]
    private float time;

    [SerializeField]
    [HideInInspector]
    private RectTransform rectTransform;

    private Dictionary<TweenDimension, TweenSteps> dimensions;
    private int stepCount;
    private bool didRegister;

    private int lockCount;


    public RectTransform RectTransform {
        get {
            if (rectTransform == null) {
                rectTransform = GetComponent<RectTransform>();
            }
            return rectTransform;
        }
    }

    public Option<RectTransform> Target => (transform.parent?.GetComponent<RectTransform>()).ToOption();

    public float Time => time;

    public AddMode Mode {
        get => mode;
        set => mode = value;
    }

    private void Awake() {
        dimensions = new Dictionary<TweenDimension, TweenSteps>();
    }

    private void Start() {
        RegisterDimensions();
    }

    private void Update() {
        float timeDelta = UnityEngine.Time.deltaTime;
        float oldTime = time;
        time += timeDelta;

        if (stepCount > 0) {
            ContinueStep(timeDelta);
            // ContinueStep may have skipped; if it didn't, check if we have completed this step
            // BUG? may not work correctly when callback skips
            // BUG this should be inside ContinueStep, as written it does not work correctly when there are multiple controllers
            bool activeDimension = false;
            foreach (var dimension in dimensions) {
                if (!dimension.Value.Met()) {
                    activeDimension = true;
                    break;
                }
            }

            if (!activeDimension) {
                Skip();
            }
        }
    }

    public IEnumerable<TweenSteps> Dimensions => dimensions.Values;

    public void Skip() {
        if (stepCount > 0) {
            foreach (var dimension in dimensions) {
                dimension.Value.Apply();
                EndStep();
            }
            if (--stepCount == 0) {
                foreach (var controller in GetComponents<ITweenController>()) {
                    controller.Complete(this);
                }
            } else {
                StartStep();
            }
        }
    }

    public IDisposable Unlock() => didRegister ? new Relocker(this) : Utility.NoopDisposable;

    private void UnlockInternal() {
        ++lockCount;
    }

    private void RelockInternal() {
        if (--lockCount == 0) {
            switch (mode) {
                case AddMode.Skip:
                    Skip();
                    AddStep();
                    break;
                case AddMode.Continue:
                    if (stepCount == 0) {
                        AddStep();
                    } else {
                        // TODO recalc progress
                    }
                    break;
                case AddMode.Restart:
                    // TODO this is weird
                    foreach (var dimension in dimensions) {
                        for (int i = 0; i < stepCount; ++i) {
                            dimension.Value.Discard();
                        }
                    }
                    stepCount = 0;
                    AddStep();
                    break;
                case AddMode.Sequence:
                    AddStep();
                    break;
                case AddMode.Signal:
                // TODO
                default:
                    throw RtlAssert.NotReached();
            }
        }
    }

    private void AddStep() {
        foreach (var dimension in dimensions) {
            dimension.Value.AddStep();
        }

        if (++stepCount == 1) {
            StartStep();
        }
    }

    public TweenSteps<T> RegisterDimension<T>(TweenDimension<T> dimension) {
        if (dimensions.TryGetValue(dimension, out TweenSteps steps)) {
            return (TweenSteps<T>)steps;
        } else {
            var newSteps = new TweenSteps<T>(this, dimension);
            dimensions.Add(dimension, newSteps);
            foreach (var target in Target) {
                dimension.Set(RectTransform, dimension.Get(target));
            }

            return newSteps;
        }
    }

    private void ClearDimensions() {
        dimensions.Clear();
        didRegister = false;
    }

    public void RegisterDimensions() {
        if (!didRegister) {
            didRegister = true;
            foreach (var controller in GetComponents<ITweenController>()) {
                controller.RegisterDimensions(this);
            }
        }
    }

    private void StartStep() {
        foreach (var controller in GetComponents<ITweenController>()) {
            controller.StartStep(this);
        }
    }

    private void ContinueStep(float timeDelta) {
        foreach (var controller in GetComponents<ITweenController>()) {
            controller.ContinueStep(this, timeDelta);
        }
    }

    private void EndStep() {
        foreach (var controller in GetComponents<ITweenController>()) {
            controller.EndStep(this);
        }
    }
}
