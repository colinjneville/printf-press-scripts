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

public sealed partial class LabelEditInputMode : InputMode {
    private WorkspaceFull workspace;
    private Cryptex cryptex;
    private Label label;

    private TextHighlight highlight;

    public LabelEditInputMode(InputManager manager) : base(manager) { }

    public void SetLabel(WorkspaceFull workspace, Cryptex cryptex, Label label) {
        this.workspace = workspace;
        this.cryptex = cryptex;
        this.label = label;
        CreateHighlight();
    }

    public Cryptex Cryptex => cryptex;

    public Label Label => label;

    public override bool AlwaysActive => false;

    public override void OnSelect() {
        var view = MakeView();
        view.transform.parent = WorkspaceScene.Layer.EditHighlight(Screen.Active);
        Screen.Active.FixedProxy.CreateReceiver(view.gameObject);
    }
    public override void OnDeselect() {
        Save();
        Label.MakeView().TextOverride = Option.None;
        ClearView();
    }

    private void CreateHighlight() {
        var oldHighlight = highlight;
        highlight = new TextHighlight(Label.Name.ToString());
        highlight.SelectAll();
        foreach (var view in View) {
            view.OnUpdateHighlight(oldHighlight, highlight);
        }
    }

    private void ReadText() {
        highlight.Text = Label.Name.ToString();
        highlight.SelectAll();
    }

    public override bool Mouse0(State state, Modifiers modifiers, Vector3 position, Lazy<RaycastHit2D[]> hits) {
        switch (state) {
            case State.Press:
                foreach (var view in View) {
                    foreach (var hit in hits.Value) {
                        // HACK
                        var lmp = hit.collider.GetComponent<LabelModifyPoint>();
                        if (lmp != null && lmp.Label == Label) {
                            highlight.Mouse0Press(modifiers, position);
                            return true;
                        }
                    }
                }
                break;
            case State.Held:
                highlight.Mouse0Held(modifiers, position);
                return true;
            case State.Release:
                highlight.Mouse0Release(modifiers, position);
                return true;
        }
        return false;
    }

    public override bool Char(State state, Modifiers modifiers, char c) {
        highlight.Char(state, modifiers, c);
        return true;
    }

    public override bool Left(State state, Modifiers modifiers) {
        highlight.Left(state, modifiers);
        return true;
    }

    public override bool Down(State state, Modifiers modifiers) {
        return Left(state, modifiers);
    }

    public override bool Right(State state, Modifiers modifiers) {
        highlight.Right(state, modifiers);
        return true;
    }

    public override bool Up(State state, Modifiers modifiers) {
        return Right(state, modifiers);
    }

    public override bool Home(State state, Modifiers modifiers) {
        highlight.Home(state, modifiers);
        return true;
    }
    public override bool End(State state, Modifiers modifiers) {
        highlight.End(state, modifiers);
        return true;
    }

    public override bool Escape(State state, Modifiers modifiers) {
        switch (state) {
            case State.Press:
                ReadText();
                InputManager.Deselect(this);
                break;
        }
        return true;
    }

    public override bool Return(State state, Modifiers modifiers) {
        switch (state) {
            case State.Press:
                InputManager.Deselect(this);
                break;
        }
        return true;
    }

    public override bool Backspace(State state, Modifiers modifiers) {
        highlight.Backspace(state, modifiers);
        return true;
    }
    public override bool Delete(State state, Modifiers modifiers) {
        highlight.Delete(state, modifiers);
        return true;
    }

    public override bool Undo(Modifiers modifiers) {
        highlight.Undo(modifiers);
        return true;
    }

    public override bool Redo(Modifiers modifiers) {
        highlight.Redo(modifiers);
        return true;
    }

    public override bool Cut(Modifiers modifiers) {
        highlight.Cut(modifiers);
        return true;
    }

    public override bool Copy(Modifiers modifiers) {
        highlight.Copy(modifiers);
        return true;
    }

    public override bool Paste(Modifiers modifiers) {
        highlight.Paste(modifiers);
        return true;
    }

    private void Save() {
        if (highlight.Text != Label.Name.ToString()) {
            var record = Cryptex.RenameLabel(Label.Id, LC.User(highlight.Text));
            workspace.ApplyModificationRecord(record);
        }
    }
}
