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

public sealed class FrameTime : MonoBehaviour {
    private DateTime frameStart;

    private void Awake() {
        RecordTime();
    }

    private void Update() {
        RecordTime();
    }

    private void RecordTime() {
        frameStart = DateTime.UtcNow;
    }

    public TimeSpan Elapsed => DateTime.UtcNow - frameStart;

    public bool TimeSliceExceeded(TimeSpan buffer = default) {
        //float frameLimit = 1f / Application.targetFrameRate;
        // TODO Currently hardcoding a min framerate of 30fps
        var frameLimit = TimeSpan.FromSeconds(1f / 30f);
        var frameTime = Elapsed + buffer;
        return frameTime >= frameLimit;
    }
}
