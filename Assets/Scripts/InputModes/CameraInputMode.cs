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

// Somewhat a misnomer, this is all things active in WorkspaceScene during edit and execution
public sealed class CameraInputMode : InputMode {
    private Camera camera;
    private Option<Roller> prevRoller;

    public CameraInputMode(InputManager manager, Camera camera) : base(manager) {
        this.camera = camera;
    }

    public override bool AlwaysActive => true;
    public override bool ActiveDuringExecution => true;

    private const float minCameraSize = 4f;
    private const float maxCameraSize = 32f;

    public override bool MouseWheel(Modifiers modifiers, float delta) {
        camera.orthographicSize = Mathf.Clamp(camera.orthographicSize - delta, minCameraSize, maxCameraSize);
        return true;
    }

    public override bool Escape(State state, Modifiers modifiers) {
        switch (state) {
            case State.Press:
                var screen = camera.GetScreen();
                var menu = Utility.Instantiate(Overseer.GlobalAssets.HelpMenuPrefab, WorkspaceScene.Layer.ModalMenu(screen));
                screen.ViewProxy.CreateReceiver(menu.gameObject);
                break;
        }
        return true;
    }

    public override bool Char(State state, Modifiers modifiers, char c) {
        if (!modifiers.HasShiftCtrlAlt()) {
            switch (c) {
                case 'w':
                    return Up(state, modifiers);
                case 'a':
                    return Left(state, modifiers);
                case 's':
                    return Down(state, modifiers);
                case 'd':
                    return Right(state, modifiers);
            }
        }
        return false;
    }

    public override bool Left(State state, Modifiers modifiers) {
        switch (state) {
            case State.Held:
                camera.transform.localPosition += new Vector3(Time.deltaTime * -GetSpeed(modifiers), 0f, 0f);
                break;
        }
        return true;
    }
    public override bool Right(State state, Modifiers modifiers) {
        switch (state) {
            case State.Held:
                camera.transform.localPosition += new Vector3(Time.deltaTime * GetSpeed(modifiers), 0f, 0f);
                break;
        }
        return true;
    }
    public override bool Up(State state, Modifiers modifiers) {
        switch (state) {
            case State.Held:
                camera.transform.localPosition += new Vector3(0f, Time.deltaTime * GetSpeed(modifiers), 0f);
                break;
        }
        return true;
    }
    public override bool Down(State state, Modifiers modifiers) {
        switch (state) {
            case State.Held:
                camera.transform.localPosition += new Vector3(0f, Time.deltaTime * -GetSpeed(modifiers), 0f);
                break;
        }
        return true;
    }

    public override bool Space(State state, Modifiers modifiers) {
        switch (state) {
            case State.Press:
                camera.transform.localPosition = Vector3.zero.WithZ(camera.transform.localPosition.z);
                break;
        }
        return true;
    }

    private static float GetSpeed(Modifiers modifiers) {
        var speed = Overseer.UserDataManager.Active.ValueOrAssert().Settings.CameraPanSpeed;
        if (modifiers.HasShift()) {
            speed *= 2;
        }
        return speed;
    }

    public override bool Tab(State state, Modifiers modifiers) {
        switch (state) {
            case State.Press:
                foreach (var workspace in Overseer.Workspace) {
                    var rollers = workspace.Rollers;
                    if (modifiers.HasShift()) {
                        rollers = rollers.Reverse();
                    }

                    foreach (var prevRoller in prevRoller) {
                        bool useNext = false;
                        foreach (var roller in rollers) {
                            if (useNext) {
                                this.prevRoller = roller;
                                SnapToRoller(roller);
                                return true;
                            }
                            if (roller == prevRoller) {
                                useNext = true;
                            }
                        }
                    }

                    {
                        Roller roller = rollers.FirstOrDefault();
                        if (roller is object) {
                            SnapToRoller(roller);
                            prevRoller = roller;
                        }
                        return true;
                    }
                }
                return false;
        }
        return false;
    }

    private void SnapToRoller(Roller roller) {
        foreach (var view in roller.View) {
            var center = ((IApertureTarget)view).Bounds.Center;
            Vector3 point;
            if (IsRollerInVerticalView(view)) {
                point = camera.transform.localPosition.WithX(center.x);
            } else {
                point = center.WithZ(camera.transform.localPosition.z);
            }
            camera.transform.localPosition = point;
        }
    }

    private bool IsRollerInVerticalView(Roller.RollerView roller) {
        const float margin = 0.05f;
        var y = camera.WorldToScreenPoint(roller.transform.position).y;
        return y >= margin && y <= 1.0f - margin;
    }
}
