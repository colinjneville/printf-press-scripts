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

public sealed class WorkspaceScene : MonoBehaviour {
    private SingleValueInputMode singleValueInput;
    private MultiValueInputMode multiValueInput;
    private SelectValueInputMode selectValueInput;
    private MetaValueInputMode metaValueInput;
    private DragPartInputMode dragPartInput;
    private PickPartInputMode pickPartInput;
    private MovePartInputMode movePartInput;
    private MetaMovePartInputMode metaMovePartInput;
    private LabelEditInputMode labelEditInput;
    private NoteEditInputMode noteEditInput;
    private ButtonInputMode buttonInput;
    private DialogInputMode dialogInput;
    private WorkspaceInputMode workspaceInput;
    private CameraInputMode cameraInput;


    private bool buttonOnly;

    private static WorkspaceScene instance;

    private void Awake() {
        multiValueInput = new MultiValueInputMode(Overseer.InputManager);
        singleValueInput = new SingleValueInputMode(Overseer.InputManager, multiValueInput);
        selectValueInput = new SelectValueInputMode(Overseer.InputManager, singleValueInput, multiValueInput);
        metaValueInput = new MetaValueInputMode(Overseer.InputManager, selectValueInput);

        dragPartInput = new DragPartInputMode(Overseer.InputManager);
        pickPartInput = new PickPartInputMode(Overseer.InputManager);

        movePartInput = new MovePartInputMode(Overseer.InputManager);
        metaMovePartInput = new MetaMovePartInputMode(Overseer.InputManager, movePartInput);

        labelEditInput = new LabelEditInputMode(Overseer.InputManager);
        noteEditInput = new NoteEditInputMode(Overseer.InputManager);

        buttonInput = new ButtonInputMode(Overseer.InputManager);

        dialogInput = new DialogInputMode(Overseer.InputManager);

        workspaceInput = new WorkspaceInputMode(Overseer.InputManager);
        cameraInput = new CameraInputMode(Overseer.InputManager, Camera.main);
    }

    private void Start() {

    }

    private void OnEnable() {
        Overseer.InputManager.RegisterInputMode(buttonInput);
        if (!buttonOnly) {
            Overseer.InputManager.RegisterInputMode(dragPartInput);
            Overseer.InputManager.RegisterInputMode(pickPartInput);
            Overseer.InputManager.RegisterInputMode(singleValueInput);
            Overseer.InputManager.RegisterInputMode(multiValueInput);
            Overseer.InputManager.RegisterInputMode(selectValueInput);
            Overseer.InputManager.RegisterInputMode(metaValueInput);
            Overseer.InputManager.RegisterInputMode(movePartInput);
            Overseer.InputManager.RegisterInputMode(metaMovePartInput);
            Overseer.InputManager.RegisterInputMode(labelEditInput);
            Overseer.InputManager.RegisterInputMode(noteEditInput);
            Overseer.InputManager.RegisterInputMode(dialogInput);
            Overseer.InputManager.RegisterInputMode(workspaceInput);
            Overseer.InputManager.RegisterInputMode(cameraInput);
        }
        Assert.Null(instance);
        instance = this;
    }

    private void OnDisable() {
        Assert.NotNull(instance);
        instance = null;
        Overseer.InputManager.UnregisterInputMode(buttonInput);
        if (!buttonOnly) {
            Overseer.InputManager.UnregisterInputMode(dragPartInput);
            Overseer.InputManager.UnregisterInputMode(pickPartInput);
            Overseer.InputManager.UnregisterInputMode(singleValueInput);
            Overseer.InputManager.UnregisterInputMode(multiValueInput);
            Overseer.InputManager.UnregisterInputMode(selectValueInput);
            Overseer.InputManager.UnregisterInputMode(metaValueInput);
            Overseer.InputManager.UnregisterInputMode(movePartInput);
            Overseer.InputManager.UnregisterInputMode(metaMovePartInput);
            Overseer.InputManager.UnregisterInputMode(labelEditInput);
            Overseer.InputManager.UnregisterInputMode(noteEditInput);
            Overseer.InputManager.UnregisterInputMode(dialogInput);
            Overseer.InputManager.UnregisterInputMode(workspaceInput);
            Overseer.InputManager.UnregisterInputMode(cameraInput);
        }
    }

    public void SetPickPart(ToolboxItem item) {
        pickPartInput.Item = item;
        Overseer.InputManager.Select(pickPartInput);
    }

    public void ClearPickPart() {
        pickPartInput.Item = Option.None;
        Overseer.InputManager.Deselect(pickPartInput);
    }

    // HACK
    public static void SetButtonInputOnly(bool buttonOnly) {
        if (instance != null && instance.buttonOnly != buttonOnly) {
            instance.buttonOnly = buttonOnly;
            Action<InputMode> action = buttonOnly ? (Action<InputMode>)Overseer.InputManager.UnregisterInputMode : Overseer.InputManager.RegisterInputMode;
            //action(instance.buttonInput);
            action(instance.dragPartInput);
            action(instance.pickPartInput);
            action(instance.singleValueInput);
            action(instance.multiValueInput);
            action(instance.selectValueInput);
            action(instance.metaValueInput);
            action(instance.movePartInput);
            action(instance.metaMovePartInput);
            action(instance.labelEditInput);
            action(instance.noteEditInput);
            action(instance.dialogInput);
            action(instance.workspaceInput);
            action(instance.cameraInput);
        }
    }

    // HACK
    public static Option<LabelEditInputMode> LabelEditInputMode => instance == null ? Option.None : instance.labelEditInput.ToOption();

    // HACK
    public static Option<NoteEditInputMode> NoteEditInputMode => instance == null ? Option.None : instance.noteEditInput.ToOption();

    public static class Layer {
        private static RectTransform Get(Screen screen, int layer) {
            Assert.NotNull(instance);
            return screen.Canvas[layer];
        }

        public static RectTransform WorkspaceLayer(Screen screen) => Get(screen, 0);
        public static RectTransform Cryptex(Screen screen) => Get(screen, 1);
        public static RectTransform Drag(Screen screen) => Get(screen, 2);
        public static RectTransform Edit(Screen screen) => Get(screen, 3);
        public static RectTransform EditHighlight(Screen screen) => Get(screen, 4);
        // This layer may need to be split up (cost display, toolbox, cryptex insert)
        public static RectTransform WorkspaceTools(Screen screen) => Get(screen, 5);
        public static RectTransform ControlBar(Screen screen) => Get(screen, 6);
        public static RectTransform OutputTape(Screen screen) => Get(screen, 7);
        public static RectTransform Aperture(Screen screen) => Get(screen, 8);
        public static RectTransform ModalMenu(Screen screen) => Get(screen, 9);
    }
}
