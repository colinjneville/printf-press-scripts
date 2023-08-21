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

public class BasicTween : MonoBehaviour, ITweenController {
    [SerializeField]
    private IntegralFloat initialVelocity;
    [HideInInspector]
    [SerializeField]
    private IntegralFloat velocity;

    private TweenSteps<Vector2> dimPosition;
    private TweenStrategyVector2MoveToward strPosition;

    private void Reset() {
        initialVelocity = new IntegralFloat(1f, 10f);
    }

    private void Start() {
        strPosition = ScriptableObject.CreateInstance<TweenStrategyVector2MoveToward>();
    }

    void ITweenController.RegisterDimensions(Tween tween) {
        dimPosition = tween.RegisterDimension(new TweenDimensionPosition());
    }

    void ITweenController.StartStep(Tween tween) {
        velocity = initialVelocity;
    }

    void ITweenController.ContinueStep(Tween tween, float timeDelta) {
        var rt = tween.RectTransform;
        dimPosition.MoveToward(rt, ref velocity, strPosition, timeDelta);

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