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

public sealed class ExecutionContextPositionTween : ExecutionContextTween {
    [SerializeField]
    private IntegralFloat initialVelocity;
    [HideInInspector]
    [SerializeField]
    private IntegralFloat velocity;

    private TweenSteps<Vector2> dimPosition;

    private void Reset() {
        initialVelocity = new IntegralFloat(1f, 10f);
    }

    protected override void RegisterDimensions(Tween tween) {
        base.RegisterDimensions(tween);
        dimPosition = tween.RegisterDimension(new TweenDimensionPosition());
    }

    protected override void StartStep(Tween tween) {
        base.StartStep(tween);
        velocity = initialVelocity;
    }

    protected override void ContinueStep(Tween tween, float timeDelta) {
        base.ContinueStep(tween, timeDelta);
        foreach (var target in tween.Target) {
            float positionDelta;
            velocity = velocity.Advance(timeDelta, out positionDelta);
            tween.RectTransform.position = Vector2.MoveTowards(tween.RectTransform.position, dimPosition.Next, positionDelta);
        }
    }
}
