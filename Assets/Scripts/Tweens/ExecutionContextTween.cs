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

public abstract class ExecutionContextTween : MonoBehaviour, ITweenController {
    private IDisposable ticket;

    private void OnDestroy() {
        ticket?.Dispose();
    }

    void ITweenController.RegisterDimensions(Tween tween) => RegisterDimensions(tween);
    protected virtual void RegisterDimensions(Tween tween) { }

    void ITweenController.StartStep(Tween tween) => StartStep(tween);
    protected virtual void StartStep(Tween tween) {
        foreach (var workspace in Overseer.Workspace) {
            foreach (var ec in workspace.ExecutionContext) {
                ticket?.Dispose();
                ticket = ec.NotifyOnNextStep(tween.Skip);
            }
        }
    }

    void ITweenController.ContinueStep(Tween tween, float timeDelta) => ContinueStep(tween, timeDelta);
    protected virtual void ContinueStep(Tween tween, float timeDelta) { }

    void ITweenController.EndStep(Tween tween) => EndStep(tween);
    protected virtual void EndStep(Tween tween) { }

    void ITweenController.Complete(Tween tween) {
        OnComplete?.Invoke(tween);
    }

    public event Action<Tween> OnComplete;
}
