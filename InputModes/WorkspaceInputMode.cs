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

public sealed class WorkspaceInputMode : InputMode {
    public WorkspaceInputMode(InputManager manager) : base(manager) { }

    public override bool AlwaysActive => true;

    public override bool Mouse0(State state, Modifiers modifiers, Vector3 position, Lazy<RaycastHit2D[]> hits) {
        switch (state) {
            case State.Release when modifiers.HasCtrl():
                return ForModifyPoint(hits, Edit);
        }
        return false;
    }

    public override bool Mouse1(State state, Modifiers modifiers, Vector3 position, Lazy<RaycastHit2D[]> hits) {
        switch (state) {
            case State.Release when !modifiers.HasCtrl():
                return ForModifyPoint(hits, Delete);
            case State.Release when modifiers.HasCtrl():
                return ForModifyPoint(hits, EditAlt);
        }
        return false;
    }

    public override bool Mouse2(State state, Modifiers modifiers, Vector3 position, Lazy<RaycastHit2D[]> hits) {
        switch (state) {
            case State.Release when !modifiers.HasCtrl():
                return ForModifyPoint(hits, Edit);
        }
        return false;
    }

    public override bool Char(State state, Modifiers modifiers, char c) {
        if (modifiers.HasCtrl() && c == 's') {
            switch (state) {
                case State.Press:
                    Overseer.UserDataManager.Save();
                    break;
            }
            return true;
        }
        return false;
    }

    public override bool Escape(State state, Modifiers modifiers) {
        switch (state) {
            case State.Press:
                var menu = Utility.Instantiate(Overseer.GlobalAssets.HelpMenuPrefab, WorkspaceScene.Layer.ModalMenu(Camera.main.GetScreen()));
                Camera.main.GetScreen().ViewProxy.CreateReceiver(menu.gameObject);
                break;
        }
        return true;
    }

    public override bool Undo(Modifiers modifiers) {
        foreach (var workspace in Overseer.Workspace) {
            workspace.UndoModification();
            return true;
        }
        return false;
    }
    public override bool Redo(Modifiers modifiers) {
        foreach (var workspace in Overseer.Workspace) {
            workspace.RedoModification();
            return true;
        }
        return false;
    }

    private bool ForModifyPoint(Lazy<RaycastHit2D[]> hits, Func<ModifyPoint, bool> func) {
        foreach (var hit in hits.Value) {
            var modifyPoint = hit.collider.GetComponent<ModifyPoint>();
            if (modifyPoint != null) {
                return func(modifyPoint);
            }
        }
        return false;
    }

    private bool Delete(ModifyPoint mp) {
        if (mp.CanDelete(forMove: false)) {
            InputManager.Deselect();
            // Technically Deselect could make us unable to perform the Delete?
            if (mp.Delete(forMove: false)) {
                return true;
            }
        }
        return false;
    }

    private bool Edit(ModifyPoint mp) => mp.Edit();
    private bool EditAlt(ModifyPoint mp) => mp.EditAlt();
}
