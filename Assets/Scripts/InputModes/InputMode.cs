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

public static class InputModeExtensions {
    public static bool HasShift(this InputMode.Modifiers modifiers) => modifiers.HasFlag(InputMode.Modifiers.Shift);
    public static bool HasCtrl(this InputMode.Modifiers modifiers) => modifiers.HasFlag(InputMode.Modifiers.Ctrl);
    public static bool HasAlt(this InputMode.Modifiers modifiers) => modifiers.HasFlag(InputMode.Modifiers.Alt);
    public static bool HasInsert(this InputMode.Modifiers modifiers) => modifiers.HasFlag(InputMode.Modifiers.Insert);
    public static bool HasRepeat(this InputMode.Modifiers modifiers) => modifiers.HasFlag(InputMode.Modifiers.Repeat);
    public static bool HasShiftCtrlAlt(this InputMode.Modifiers modifiers) => HasShift(modifiers) || HasCtrl(modifiers) || HasAlt(modifiers);
}

public abstract class InputMode {
    private readonly InputManager manager;

    protected InputMode(InputManager manager) {
        this.manager = manager;
    }

    public enum State {
        Up = 0,
        Press,
        Held,
        Release,
    }

    public enum Modifiers {
        None = 0x0,
        Shift = 0x1,
        Ctrl = 0x2,
        Alt = 0x4,
        Insert = 0x8,
        Repeat = 0x10,
    }

    protected InputManager InputManager => manager;
    public abstract bool AlwaysActive { get; }
    public virtual bool ActiveDuringExecution => false;

    public virtual void OnRegister() { }
    public virtual void OnUnregister() { }

    public virtual void OnSelect() { }
    public virtual void OnDeselect() { }

    public virtual bool Mouse(Modifiers modifiers, Vector3 position, Lazy<RaycastHit2D[]> hits) => false;

    public virtual bool Mouse0(State state, Modifiers modifiers, Vector3 position, Lazy<RaycastHit2D[]> hits) => false;

    public virtual bool Mouse1(State state, Modifiers modifiers, Vector3 position, Lazy<RaycastHit2D[]> hits) => false;

    public virtual bool Mouse2(State state, Modifiers modifiers, Vector3 position, Lazy<RaycastHit2D[]> hits) => false;

    public virtual bool MouseWheel(Modifiers modifiers, float delta) => false;

    public virtual bool Char(State state, Modifiers modifiers, char c) => false;

    public virtual bool Backspace(State state, Modifiers modifiers) => false;
    public virtual bool Delete(State state, Modifiers modifiers) => false;
    public virtual bool Escape(State state, Modifiers modifiers) => false;

    public virtual bool Space(State state, Modifiers modifiers) => false;
    public virtual bool Return(State state, Modifiers modifiers) => false;

    public virtual bool Tab(State state, Modifiers modifiers) => false;
    public virtual bool Left(State state, Modifiers modifiers) => false;
    public virtual bool Right(State state, Modifiers modifiers) => false;
    public virtual bool Up(State state, Modifiers modifiers) => false;
    public virtual bool Down(State state, Modifiers modifiers) => false;

    public virtual bool Home(State state, Modifiers modifiers) => false;
    public virtual bool End(State state, Modifiers modifiers) => false;

    public virtual bool Undo(Modifiers modifiers) => false;
    public virtual bool Redo(Modifiers modifiers) => false;
    public virtual bool Copy(Modifiers modifiers) => false;
    public virtual bool Cut(Modifiers modifiers) => false;
    public virtual bool Paste(Modifiers modifiers) => false;
    public virtual bool SelectAll(Modifiers modifiers) => false;
}
