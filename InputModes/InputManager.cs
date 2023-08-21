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

public sealed class InputManager : MonoBehaviour {
    private List<InputMode> inputModes;
    private Option<int> currentInputIndex;
    private Dictionary<KeyCode, Func<InputMode.State, InputMode.Modifiers, bool>> miscKeys;

    private bool insert;

    private static Dictionary<KeyCode, Func<InputMode, Func<InputMode.State, InputMode.Modifiers, bool>>> internalKeys = new Dictionary<KeyCode, Func<InputMode, Func<InputMode.State, InputMode.Modifiers, bool>>> {
        { KeyCode.RightArrow, im => im.Right },
        { KeyCode.UpArrow, im => im.Up },
        { KeyCode.LeftArrow, im => im.Left },
        { KeyCode.DownArrow, im => im.Down },
        { KeyCode.Tab, im => im.Tab },
        { KeyCode.Space, im => im.Space },
        { KeyCode.Return, im => im.Return },
        { KeyCode.Escape, im => im.Escape },
        { KeyCode.Backspace, im => im.Backspace },
        { KeyCode.Delete, im => im.Delete },
        { KeyCode.Home, im => im.Home },
        { KeyCode.End, im => im.End }
    };

    private static Dictionary<KeyCode, Func<InputMode, Func<InputMode.State, InputMode.Modifiers, Vector3, Lazy<RaycastHit2D[]>, bool>>> internalButtons = new Dictionary<KeyCode, Func<InputMode, Func<InputMode.State, InputMode.Modifiers, Vector3, Lazy<RaycastHit2D[]>, bool>>> {
        { KeyCode.Mouse0, im => im.Mouse0 },
        { KeyCode.Mouse1, im => im.Mouse1 },
        { KeyCode.Mouse2, im => im.Mouse2 }
    };

    private static KeyCode LetterToKeyCode(char letter) {
        Assert.True(char.IsLetter(letter));
        letter = char.ToLowerInvariant(letter);
        return LetterIndexToKeyCode(letter - 'a');
    }

    private static KeyCode LetterIndexToKeyCode(int index) {
        Assert.Within(index, 0, 26);
        return KeyCode.A + index;
    }

    private static char LetterIndexToChar(int index) {
        Assert.Within(index, 0, 26);
        return (char)('a' + index);
    }

    private void Awake() {
        inputModes = new List<InputMode>();
        miscKeys = new Dictionary<KeyCode, Func<InputMode.State, InputMode.Modifiers, bool>>();

        insert = true;
    }

    private void Update() {
        var lazyHit = new Lazy<RaycastHit2D[]>(Raycast);

        var camera = Camera.main;
        bool mouseOverWindow = false;
        if (camera is object) {
            Vector2 mousePos = camera.ScreenToViewportPoint(Input.mousePosition);
            mouseOverWindow = mousePos.x >= 0 && mousePos.x < 1 && mousePos.y >= 0 && mousePos.y < 1;
        }

        if (Input.GetKeyDown(KeyCode.Insert)) {
            insert = !insert;
        }

        var modifiers = InputMode.Modifiers.None;

        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) {
            modifiers |= InputMode.Modifiers.Shift;
        }
        // The Editor does not pass ctrl to play mode...
        if (
#if UNITY_EDITOR
            Input.GetKey(KeyCode.F11)
#else
            Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)
#endif
            ) {
            modifiers |= InputMode.Modifiers.Ctrl;
        }
        if (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)) {
            modifiers |= InputMode.Modifiers.Alt;
        }
        if (insert) {
            modifiers |= InputMode.Modifiers.Insert;
        }

        Broadcast(im => (s, m) => im.Mouse(m, Input.mousePosition, lazyHit), InputMode.State.Held, modifiers);

        var mouseWheelDelta = Input.mouseScrollDelta.y;
        if (mouseWheelDelta != 0 && mouseOverWindow) {
            Broadcast(im => (s, m) => im.MouseWheel(m, mouseWheelDelta), InputMode.State.Press, modifiers);
        }

        InputMode.State state;
        foreach (var internalButton in internalButtons) {
            if ((state = GetKeyState(internalButton.Key)) != InputMode.State.Up) {
                Broadcast(im => (s, m) => internalButton.Value(im)(s, m, Input.mousePosition, lazyHit), state, modifiers);
            }
        }

        foreach (var internalKey in internalKeys) {
            if ((state = GetKeyState(internalKey.Key)) != InputMode.State.Up) {
                if (!Broadcast(internalKey.Value, state, modifiers)) {
                    Func<InputMode.State, InputMode.Modifiers, bool> value;
                    if (miscKeys.TryGetValue(internalKey.Key, out value)) {
                        value(state, modifiers);
                    }
                }
            }
        }

        //foreach (var ch in previousInputString) {
        //    var letterState = GetKeyState(LetterToKeyCode(ch));
        //    if (letterState == InputMode.State.Held || letterState == InputMode.State.Release) {
        //        Broadcast(im => (s, m) => im.Char(s, m, ch), letterState, modifiers);
        //        if (letterState == InputMode.State.Release) {
        //
        //        }
        //    }
        //}

        if (modifiers.HasCtrl()) {
            if (Input.GetKeyDown(KeyCode.Z)) {
                if (modifiers.HasShift()) {
                    Broadcast(im => (s, m) => im.Redo(m & ~(InputMode.Modifiers.Ctrl | InputMode.Modifiers.Shift)), InputMode.State.Press, modifiers);
                } else {
                    Broadcast(im => (s, m) => im.Undo(m & ~InputMode.Modifiers.Ctrl), InputMode.State.Press, modifiers);
                }
                
            } else if (Input.GetKeyDown(KeyCode.Y)) {
                Broadcast(im => (s, m) => im.Redo(m & ~InputMode.Modifiers.Ctrl), InputMode.State.Press, modifiers);
            } else if (Input.GetKeyDown(KeyCode.X)) {
                Broadcast(im => (s, m) => im.Cut(m & ~InputMode.Modifiers.Ctrl), InputMode.State.Press, modifiers);
            } else if (Input.GetKeyDown(KeyCode.C)) {
                Broadcast(im => (s, m) => im.Copy(m & ~InputMode.Modifiers.Ctrl), InputMode.State.Press, modifiers);
            } else if (Input.GetKeyDown(KeyCode.V)) {
                Broadcast(im => (s, m) => im.Paste(m & ~InputMode.Modifiers.Ctrl), InputMode.State.Press, modifiers);
            } else if (Input.GetKeyDown(KeyCode.A)) {
                Broadcast(im => (s, m) => im.SelectAll(m & ~InputMode.Modifiers.Ctrl), InputMode.State.Press, modifiers);
            }
        } else {
            for (int i = 0; i < 26; ++i) {
                var kc = LetterIndexToKeyCode(i);
                if ((state = GetKeyState(kc)) != InputMode.State.Up) {
                    var ch = LetterIndexToChar(i);
                    Broadcast(im => (s, m) => im.Char(s, m, ch), state, modifiers);
                }
            }

            //foreach (var ch in Input.inputString) {
            //    if (char.IsLetter(ch)) {
            //        Broadcast(im => (s, m) => im.Char(s, m, ch), InputMode.State.Press, modifiers);
            //    }
            //}
        }

        foreach (var kvp in miscKeys) {
            if (!internalKeys.ContainsKey(kvp.Key)) {
                if ((state = GetKeyState(kvp.Key)) != InputMode.State.Up) {
                    // TODO don't step on Char's toes
                    kvp.Value(state, modifiers);
                }
            }
        }
    }

    //public void Clear() {
    //    Assert.Empty(miscKeys);
    //    inputModes.Clear();
    //    currentInputIndex = Option.None;
    //}
    

    private InputMode.State GetKeyState(KeyCode keycode) {
        if (Input.GetKeyDown(keycode)) {
            return InputMode.State.Press;
        } else if (Input.GetKeyUp(keycode)) {
            return InputMode.State.Release;
        } else if (Input.GetKey(keycode)) {
            return InputMode.State.Held;
        } else {
            return InputMode.State.Up;
        }
    }

    public void RegisterInputMode(InputMode inputMode) {
        Assert.NotNull(inputMode);
        inputModes.Add(inputMode);
        inputMode.OnRegister();
    }

    public void UnregisterInputMode(InputMode inputMode) {
        int index = inputModes.IndexOf(inputMode);

        if (currentInputIndex.Equals(index)) {
            currentInputIndex = Option.None;
        }
        inputModes.RemoveAt(index);
        inputMode.OnUnregister();
    }

    public bool RegisterMiscKey(KeyCode keycode, Func<InputMode.State, InputMode.Modifiers, bool> func) {
        if (miscKeys.ContainsKey(keycode)) {
            return false;
        } else {
            miscKeys.Add(keycode, func);
            return true;
        }
    }

    public bool UnregisterMiscKey(KeyCode keycode) {
        return miscKeys.Remove(keycode);
    }

    public void Select(InputMode mode) {
        for (int i = 0; i < inputModes.Count; ++i) {
            if (inputModes[i] == mode) {
                if (currentInputIndex != i) {
                    SelectIndex(i);
                }
                return;
            }
        }
        RtlAssert.NotReached();
    }

    private void SelectIndex(Option<int> currentInputIndex) {
        foreach (var currentInputIndexValue in this.currentInputIndex) {
            inputModes[currentInputIndexValue].OnDeselect();
        }
        this.currentInputIndex = currentInputIndex;
        foreach (var currentInputIndexValue in this.currentInputIndex) {
            inputModes[currentInputIndexValue].OnSelect();
        }
    }

    private Option<InputMode> CurrentInputMode {
        get {
            foreach (var currentInputIndexValue in currentInputIndex) {
                return inputModes[currentInputIndexValue];
            }
            return Option.None;
        }
    }

    public void Deselect() {
        SelectIndex(Option.None);
    }

    public void Deselect(InputMode modeToDeselect) {
        if (CurrentInputMode == modeToDeselect) {
            Deselect();
        }
    }

    private bool Broadcast(Func<InputMode, Func<InputMode.State, InputMode.Modifiers, bool>> func, InputMode.State state, InputMode.Modifiers modifiers) {
        bool notExecuting = true;
        foreach (var workspace in Overseer.Workspace) {
            notExecuting = !workspace.ExecutionContext.HasValue;
        }

        foreach (var currentInputIndexValue in currentInputIndex) {
            var inputMode = inputModes[currentInputIndexValue];
            if ((notExecuting || inputMode.ActiveDuringExecution) && func(inputMode)(state, modifiers)) {
                return true;
            }
        }

        for (int i = 0; i < inputModes.Count; ++i) {
            var inputMode = inputModes[i];
            if (inputMode.AlwaysActive) {
                foreach (var currentInputIndexValue in currentInputIndex) {
                    if (i == currentInputIndexValue) {
                        continue;
                    }
                }
                if ((notExecuting || inputMode.ActiveDuringExecution) && func(inputMode)(state, modifiers)) {
                    return true;
                }
            }
        }
        return false;
    }

    private RaycastHit2D[] Raycast() => Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition));

    /// <summary>
    /// Raycasts using the InputManager raycast settings. Not really InputManager specific, but needed for DragDefinition also, and left here out of laziness
    /// </summary>
    public static RaycastHit2D[] Raycast(Vector2 worldPosition) {
        // Apparently Vector2.zero is a valid direction for raycasting a single point
        return Physics2D.RaycastAll(worldPosition, Vector2.zero, float.PositiveInfinity).OrderBy(rh => rh.transform.position.z).ToArray();
    }
}
