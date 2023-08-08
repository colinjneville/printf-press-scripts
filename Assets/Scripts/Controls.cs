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


public class Controls : MonoBehaviour {
    private ControlsViewBit bit;

    private Option<ExecutionContextFull> ec;
    private bool inPreview;

    private void Start() {
        bit = Utility.Instantiate(Overseer.GlobalAssets.ControlsPrefab, WorkspaceScene.Layer.ControlBar(this.GetScreen()));
        var proxy = this.GetScreen().ViewProxy.CreateReceiver(bit.gameObject);
        var rt = bit.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 1f);
        rt.anchorMax = new Vector2(0.5f, 1f);
        bit.RunAction = (Action)Run;
        bit.StepAction = (Action)Step;
        bit.UnstepAction = (Action)Unstep;
        bit.BreakAction = (Action)Break;
        bit.StopAction = (Action)Stop;
        bit.DecreaseRunSpeedAction = (Action)DecreaseRunSpeed;
        bit.IncreaseRunSpeedAction = (Action)IncreaseRunSpeed;
    }

    private void OnEnable() {
        Overseer.InputManager.RegisterMiscKey(KeyCode.F1, MakeOnKeyDownFunc(Intro));
        //Overseer.InputManager.RegisterMiscKey(KeyCode.F5, MakeOnKeyDownFunc(Run));
        //Overseer.InputManager.RegisterMiscKey(KeyCode.F6, MakeOnKeyDownFunc(Step));
        //Overseer.InputManager.RegisterMiscKey(KeyCode.F7, MakeOnKeyDownFunc(Unstep));
        //Overseer.InputManager.RegisterMiscKey(KeyCode.F8, MakeOnKeyDownFunc(Break));
        //Overseer.InputManager.RegisterMiscKey(KeyCode.F9, MakeOnKeyDownFunc(Stop));
    }

    private void OnDisable() {
        Overseer.InputManager.UnregisterMiscKey(KeyCode.F1);
        //Overseer.InputManager.UnregisterMiscKey(KeyCode.F5);
        //Overseer.InputManager.UnregisterMiscKey(KeyCode.F6);
        //Overseer.InputManager.UnregisterMiscKey(KeyCode.F7);
        //Overseer.InputManager.UnregisterMiscKey(KeyCode.F8);
        //Overseer.InputManager.UnregisterMiscKey(KeyCode.F9);
    }

    private void OnDestroy() {
        bit.DestroyGameObject();
    }

    private void Update() {
        bool allowRun = false, allowStep = false, allowUnstep = false, allowBreak = false, allowStop = false, allowDecreaseRunSpeed = false, allowIncreaseRunSpeed = false;
        if (ec.TryGetValue(out ExecutionContextFull ecValue)) {
            ecValue.Update();
            switch (ecValue.CurrentState) {
                case ExecutionContext.State.Break:
                    allowRun = true;
                    allowStep = true;
                    allowUnstep = ecValue.CanUndo;
                    allowStop = true;
                    break;
                case ExecutionContext.State.Running:
                    allowBreak = true;
                    allowStop = true;
                    break;
                case ExecutionContext.State.Stopped:
                    allowUnstep = true;
                    allowStop = true;
                    break;
            }
            allowDecreaseRunSpeed = true;
            allowIncreaseRunSpeed = true;
        } else {
            if (!inPreview) {
                allowRun = true;
                allowStep = true;
            }
        }

        bit.RunEnabled = allowRun;
        bit.StepEnabled = allowStep;
        bit.UnstepEnabled = allowUnstep;
        bit.BreakEnabled = allowBreak;
        bit.StopEnabled = allowStop;
        bit.DecreaseRunSpeedEnabled = allowDecreaseRunSpeed;
        bit.IncreaseRunSpeedEnabled = allowIncreaseRunSpeed;

#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.Pause)) {
            Debug.Break();
        }

#endif // UNITY_EDITOR

        // TEST
        if (Input.GetKeyDown(KeyCode.F10)) {
            foreach (var workspace in Overseer.Workspace) {
                workspace.Level.ApplyReferenceSolution(workspace);
            }
        }

        if (Input.GetKeyDown(KeyCode.F12) && !inPreview) {
            foreach (var workspace in Overseer.Workspace) {
                workspace.CreateTestCasePreview();
                inPreview = true;
            }
        } else if (!Input.GetKey(KeyCode.F12) && inPreview) {
            foreach (var workspace in Overseer.Workspace) {
                workspace.ClearTestCasePreview();
            }
            inPreview = false;
        }
    }

    private static Func<InputMode.State, InputMode.Modifiers, bool> MakeOnKeyDownFunc(Action action) => (state, modifiers) => OnKeyDown(state, modifiers, action);

    private static bool OnKeyDown(InputMode.State state, InputMode.Modifiers modifiers, Action action) {
        if (state == InputMode.State.Press) {
            action();
            return true;
        }
        return false;
    }

    private ExecutionContextFull GetOrCreateContext() {
        if (!ec.TryGetValue(out ExecutionContextFull ecValue)) {
            foreach (var workspace in Overseer.Workspace) {
                ecValue = workspace.CreateExecutionContext();
                InitializeContextView(ecValue);
                ec = ecValue;
                ecValue.Start();
            }
        }
        return ecValue;
    }

    private void InitializeContextView(ExecutionContextFull context) {
        var view = context.MakeView();
        var viewRt = view.GetComponent<RectTransform>();
        viewRt.SetParent(context.Workspace.MakeView().transform, false);
        viewRt.anchorMin = Vector2.zero;
        viewRt.anchorMax = Vector2.one;
    }

    public void InjectContext(ExecutionContextFull context) {
        ClearContext();
        InitializeContextView(context);
        ec = context;
    }

    public void ClearContext() {
        foreach (var ec in ec) {
            ec.ClearView();
        }
        ec = Option.None;
    }

    public void Intro() {
        if (!ec.HasValue && !inPreview) {
            Overseer.InputManager.Deselect();
            foreach (var workspace in Overseer.Workspace) {
                foreach (var dialog in workspace.Level.Dialog) {
                    if (!Overseer.Dialog.HasValue) {
                        Overseer.StartDialog(dialog);
                    }
                }
            }
        }
    }

    public void Run() {
        if (bit.RunEnabled) {
            Overseer.InputManager.Deselect();
            try {
                var ecValue = GetOrCreateContext();
                ecValue.Continue();
            } catch (UserException e) {
                Debug.Log(e);
            }
        }
    }

    public void Step() {
        if (bit.StepEnabled) {
            try {
                var ecValue = GetOrCreateContext();
                ecValue.Step();
            } catch (UserException e) {
                Debug.Log(e);
            }
        }
    }

    public void Unstep() {
        if (bit.UnstepEnabled) {
            foreach (var ecValue in ec) {
                ecValue.Unstep();
            }
        }
    }

    public void Break() {
        if (bit.BreakEnabled) {
            foreach (var ecValue in ec) {
                ecValue.Break();
            }
        }
    }

    public void Stop() {
        if (bit.StopEnabled) {
            foreach (var workspace in Overseer.Workspace) {
                workspace.StopExecution();
            }

            foreach (var ecValue in ec) {
                if (ecValue.CurrentState != ExecutionContext.State.Stopped) {
                    ecValue.Stop();
                }
                ecValue.ClearView();
                ec = Option.None;
            }
        }
    }

    public void DecreaseRunSpeed() {
        if (bit.DecreaseRunSpeedEnabled) {
            foreach (var userData in Overseer.UserDataManager.Active) {
                var adapter = SettingsData.GetAdapter(userData, nameof(userData.Settings.StepsPerSecond));
                adapter.SetPrevious(wrap: false);
                userData.SetDirty();
            }
        }
    }

    public void IncreaseRunSpeed() {
        if (bit.IncreaseRunSpeedEnabled) {
            foreach (var userData in Overseer.UserDataManager.Active) {
                var adapter = SettingsData.GetAdapter(userData, nameof(userData.Settings.StepsPerSecond));
                adapter.SetNext(wrap: false);
                userData.SetDirty();
            }
        }
    }
}
