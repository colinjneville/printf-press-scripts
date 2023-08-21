using Functional.Option;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

using L = LocalizationString;
using LC = LocalizationConstant;
using LE = ILocalizationExpression;
using LF = LocalizationFormat;
using LI = LocalizationInt;

public enum CustomStrategyType {
    None,
    MoveToward,
    Logistic,
    Halflife,
}

public interface ICustomDimension {

    void RegisterDimension(Tween tween);

    void StartStep(Tween tween);

    void ContinueStep(RectTransform rt, float timeDelta);

    void SetStrategyType(CustomStrategyType strategyType);

    ITweenStrategy Strategy { get; }
}

[Serializable]
public sealed class CustomDimension<T> : ICustomDimension {
    [SerializeField]
    [HideInInspector]
    private ICustomStrategyConverter<T> converter;

    [SerializeField]
    [HideInInspector]
    private ICustomDimensionStrategy<T> strategy;

    private TweenDimension<T> dimension;
    private TweenSteps<T> steps;


    public CustomDimension(TweenDimension<T> dimension, ICustomStrategyConverter<T> converter) {
        this.dimension = dimension;
        this.converter = converter;
    }

    public void SetStrategyType(CustomStrategyType strategyType) {
        if (converter.Convert(strategyType).TryGetValue(out var strategy)) {
            this.strategy = strategy;
        } else {
            this.strategy = null;
        }
    }

    public void RegisterDimension(Tween tween) {
        if (strategy != null) {
            steps = tween.RegisterDimension(dimension);
        }
    }

    public void StartStep(Tween tween) {
        if (strategy != null) {
            strategy.Reset();
        }
    }

    public void ContinueStep(RectTransform rt, float timeDelta) {
        if (strategy != null) {
            strategy.MoveToward(rt, steps, timeDelta);
        }
    }

    ITweenStrategy ICustomDimension.Strategy => strategy?.Strategy;
}

public interface ICustomDimensionStrategy {
    void Reset();

    ITweenStrategy Strategy { get; }
}

public interface ICustomDimensionStrategy<T> : ICustomDimensionStrategy {
    void MoveToward(RectTransform rt, TweenSteps<T> steps, float timeDelta);
}

public static class CustomDimensionStrategy {
    public static CustomDimensionStrategy<T, TState> Make<T, TState>(ITweenStrategy<T, TState> strategy) => new CustomDimensionStrategy<T, TState>(strategy);
}

public sealed class CustomDimensionStrategy<T, TState> : ICustomDimensionStrategy<T> {
    [SerializeField]
    [HideInInspector]
    private ITweenStrategy<T, TState> strategy;

    [SerializeField]
    private TState initialState;
    [SerializeField]
    [HideInInspector]
    private TState state;

    public CustomDimensionStrategy(ITweenStrategy<T, TState> strategy) {
        this.strategy = strategy;
        initialState = strategy.DefaultState;
        state = initialState;
    }

    public void Reset() {
        state = initialState;
    }

    public void MoveToward(RectTransform rt, TweenSteps<T> steps, float timeDelta) {
        steps.MoveToward(rt, ref state, strategy, timeDelta);
    }

    ITweenStrategy ICustomDimensionStrategy.Strategy => strategy;
}

public interface ICustomStrategyConverter {
    bool SupportsStrategyType(CustomStrategyType strategyType);

    Option<ICustomDimensionStrategy> Convert(CustomStrategyType strategyType);
}

public interface ICustomStrategyConverter<T> : ICustomStrategyConverter {
    new Option<ICustomDimensionStrategy<T>> Convert(CustomStrategyType strategyType);
}

public abstract class CustomStrategyConverter<T> : ICustomStrategyConverter<T> {
    public abstract bool SupportsStrategyType(CustomStrategyType strategyType);

    Option<ICustomDimensionStrategy> ICustomStrategyConverter.Convert(CustomStrategyType strategyType) => Convert(strategyType).Cast<ICustomDimensionStrategy<T>, ICustomDimensionStrategy>();
    public abstract Option<ICustomDimensionStrategy<T>> Convert(CustomStrategyType strategyType);
}

public class CustomStrategyConverterVector2 : CustomStrategyConverter<Vector2> {
    public override bool SupportsStrategyType(CustomStrategyType strategyType) {
        switch (strategyType) {
            case CustomStrategyType.None:
            case CustomStrategyType.MoveToward:
            case CustomStrategyType.Logistic:
            case CustomStrategyType.Halflife:
                return true;
            default:
                return false;
        }
    }

    public override Option<ICustomDimensionStrategy<Vector2>> Convert(CustomStrategyType strategyType) {
        switch (strategyType) {
            case CustomStrategyType.MoveToward:
                return CustomDimensionStrategy.Make(TweenStrategyVector2MoveToward.Make());
            case CustomStrategyType.Logistic:
                return CustomDimensionStrategy.Make(TweenStrategyVector2Logistic.Make(5, 0.5f));
            case CustomStrategyType.Halflife:
                return CustomDimensionStrategy.Make(TweenStrategyVector2Halflife.Make());
            default:
                return Option.None;
        }
    }
}

public class CustomTween : MonoBehaviour, ITweenController {
    [Serializable]
    public class DimensionWrapper : ISerializationCallbackReceiver {
        [SerializeField]
        private CustomStrategyType type;
        private ICustomDimension dimension;
        [SerializeField]
        private string stateSerialization;

        public DimensionWrapper() {

        }

        public void SetDimension(ICustomDimension dimension) {
            this.dimension = dimension;
            SetStrategyType();
            DeserializeState();
        }

        public CustomStrategyType StrategyType {
            get => type;
            set {
                type = value;
                SetStrategyType();
            }
        }

        public ICustomDimension Dimension => dimension;

        void ISerializationCallbackReceiver.OnBeforeSerialize() {
            if (dimension == null || dimension.Strategy == null) {
                stateSerialization = null;
            } else {
                var scratchField = GetScratchField();
                var scratchValue = scratchField.GetValue(dimension.Strategy);

                stateSerialization = JsonConvert.SerializeObject(scratchValue, scratchField.FieldType, SerializationUtility.Settings);
            }
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize() {
            //StrategyType = StrategyType;
            //DeserializeState();
        }

        private void Awake() {
            StrategyType = StrategyType;
            DeserializeState();
        }

        private void DeserializeState() {
            if (!string.IsNullOrEmpty(stateSerialization) && dimension != null) {
                var scratchField = GetScratchField();
                var scratchValue = JsonConvert.DeserializeObject(stateSerialization, scratchField.FieldType, SerializationUtility.Settings);

                stateSerialization = null;
            }
        }

        private void SetStrategyType() {
            if (dimension != null) {
                dimension.SetStrategyType(StrategyType);
            }
        }

        private FieldInfo GetScratchField() {
            var strategyType = dimension.Strategy.GetType();
            FieldInfo scratchField = null;
            while (strategyType != null && scratchField == null) {
                scratchField = strategyType.GetField("scratchState", BindingFlags.NonPublic | BindingFlags.Instance);
                strategyType = strategyType.BaseType;
            }
            RtlAssert.NotNull(scratchField);
            return scratchField;
        }
    }

    [SerializeField]
    private DimensionWrapper position;
    [SerializeField]
    private DimensionWrapper scale;

    private DimensionWrapper[] dimensions;

    private void Reset() {
        position = new DimensionWrapper();
        scale = new DimensionWrapper();
    }

    private void Awake() {
        InitializeDimensions();
        dimensions = new[] { position, scale };
    }

    private void Start() {
        //InitializeDimensions();
    }

    public void InitializeDimensions() {
        if (position.Dimension == null) {
            position.SetDimension(new CustomDimension<Vector2>(new TweenDimensionPosition(), new CustomStrategyConverterVector2()));
            scale.SetDimension(new CustomDimension<Vector2>(new TweenDimensionScale(), new CustomStrategyConverterVector2()));
        }
    }

    void ITweenController.RegisterDimensions(Tween tween) {
        foreach (var dimension in dimensions) {
            dimension.Dimension.RegisterDimension(tween);
        }
    }

    void ITweenController.StartStep(Tween tween) {
        foreach (var dimension in dimensions) {
            dimension.Dimension.StartStep(tween);
        }
    }

    void ITweenController.ContinueStep(Tween tween, float timeDelta) {
        foreach (var dimension in dimensions) {
            var rt = tween.RectTransform;
            dimension.Dimension.ContinueStep(rt, timeDelta);
        }

        /*
        foreach (var target in tween.Target) {
            float positionDelta;
            velocity = velocity.Advance(timeDelta, out positionDelta);
            var rt = tween.RectTransform;
            dimPosition.Dimension.Set(rt, Vector2.MoveTowards(dimPosition.Dimension.Get(rt), dimPosition.Next, positionDelta));
        }
        */
    }

    void ITweenController.EndStep(Tween tween) { }

    void ITweenController.Complete(Tween tween) { }
}