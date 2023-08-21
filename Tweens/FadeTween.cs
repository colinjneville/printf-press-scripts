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

public class FadeTween : MonoBehaviour, ITweenController {
    [SerializeField]
    private IntegralFloat initialVelocity;
    [HideInInspector]
    [SerializeField]
    private IntegralFloat velocity;

    private TweenSteps<float> dimAlpha;

    private void Reset() {
        initialVelocity = new IntegralFloat(1f, 10f);
    }

    void ITweenController.RegisterDimensions(Tween tween) {
        dimAlpha = tween.RegisterDimension(new TweenDimensionImageAlpha());
    }

    void ITweenController.StartStep(Tween tween) {
        velocity = initialVelocity;
    }

    void ITweenController.ContinueStep(Tween tween, float timeDelta) {
        foreach (var target in tween.Target) {
            float positionDelta;
            velocity = velocity.Advance(timeDelta, out positionDelta);
            dimAlpha.Dimension.Set(tween.RectTransform, Mathf.MoveTowards(dimAlpha.Dimension.Get(tween.RectTransform), dimAlpha.Next, positionDelta));
        }
    }

    void ITweenController.EndStep(Tween tween) {
    }

    void ITweenController.Complete(Tween tween) {
        Utility.DestroyGameObject(this);
    }
}