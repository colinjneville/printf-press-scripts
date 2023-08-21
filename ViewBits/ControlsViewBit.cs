using Functional.Option;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

using L = LocalizationString;
using LC = LocalizationConstant;
using LE = ILocalizationExpression;
using LF = LocalizationFormat;
using LI = LocalizationInt;

public sealed class ControlsViewBit : MonoBehaviour {
#pragma warning disable CS0649
    [SerializeField]
    private BlockerButton panelButton;
    [SerializeField]
    private RuntimeButton runButton;
    [SerializeField]
    private RuntimeButton stepButton;
    [SerializeField]
    private RuntimeButton unstepButton;
    [SerializeField]
    private RuntimeButton breakButton;
    [SerializeField]
    private RuntimeButton stopButton;
    [SerializeField]
    private RuntimeButton decreaseRunSpeedButton;
    [SerializeField]
    private RuntimeButton increaseRunSpeedButton;
#pragma warning restore CS0649

    public bool RunEnabled {
        get => runButton.Mouse0Enabled;
        set => runButton.Mouse0Enabled = value;
    }

    public bool StepEnabled {
        get => stepButton.Mouse0Enabled;
        set => stepButton.Mouse0Enabled = value;
    }

    public bool UnstepEnabled {
        get => unstepButton.Mouse0Enabled;
        set => unstepButton.Mouse0Enabled = value;
    }

    public bool BreakEnabled {
        get => breakButton.Mouse0Enabled;
        set => breakButton.Mouse0Enabled = value;
    }

    public bool StopEnabled {
        get => stopButton.Mouse0Enabled;
        set => stopButton.Mouse0Enabled = value;
    }

    public bool DecreaseRunSpeedEnabled {
        get => decreaseRunSpeedButton.Mouse0Enabled;
        set => decreaseRunSpeedButton.Mouse0Enabled = value;
    }

    public bool IncreaseRunSpeedEnabled {
        get => increaseRunSpeedButton.Mouse0Enabled;
        set => increaseRunSpeedButton.Mouse0Enabled = value;
    }

    public Option<Action> RunAction {
        get => runButton.Mouse0Action;
        set => runButton.Mouse0Action = value;
    }

    public Option<Action> StepAction {
        get => stepButton.Mouse0Action;
        set => stepButton.Mouse0Action = value;
    }

    public Option<Action> UnstepAction {
        get => unstepButton.Mouse0Action;
        set => unstepButton.Mouse0Action = value;
    }

    public Option<Action> BreakAction {
        get => breakButton.Mouse0Action;
        set => breakButton.Mouse0Action = value;
    }

    public Option<Action> StopAction {
        get => stopButton.Mouse0Action;
        set => stopButton.Mouse0Action = value;
    }

    public Option<Action> DecreaseRunSpeedAction {
        get => decreaseRunSpeedButton.Mouse0Action;
        set => decreaseRunSpeedButton.Mouse0Action = value;
    }

    public Option<Action> IncreaseRunSpeedAction {
        get => increaseRunSpeedButton.Mouse0Action;
        set => increaseRunSpeedButton.Mouse0Action = value;
    }

    public IApertureTarget RunTarget => runButton;
    public IApertureTarget StepTarget => stepButton;
    public IApertureTarget UnstepTarget => unstepButton;
    public IApertureTarget BreakTarget => breakButton;
    public IApertureTarget StopTarget => stopButton;

    public IApertureTarget PanelTarget => panelButton;
}
