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

public sealed class TapeWindAudioTween : MonoBehaviour, ITweenController {
    private bool isPlaying;

    private void OnDestroy() {
        StopPlaying();
    }

    void ITweenController.RegisterDimensions(Tween tween) { }

    void ITweenController.StartStep(Tween tween) => StartPlaying();

    void ITweenController.ContinueStep(Tween tween, float timeDelta) { }

    void ITweenController.EndStep(Tween tween) { }

    void ITweenController.Complete(Tween tween) => StopPlaying();

    private void StartPlaying() {
        if (!isPlaying) {
            isPlaying = true;
            Overseer.AudioManager.StartWind();
        }
    }

    private void StopPlaying() {
        if (isPlaying) {
            isPlaying = false;
            Overseer.AudioManager.StopWind();
        }
    }
}
