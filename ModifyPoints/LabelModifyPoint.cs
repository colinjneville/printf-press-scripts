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

public sealed class LabelModifyPoint : ModifyPoint<LabelInsertPoint, Label> {
    public Cryptex Cryptex { get; set; }
    public Label Label { get; set; }

    private Option<Transform> labelViewParent;
    private Option<TransformProxy> proxy;
    private Option<TransformProxyReceiver> receiver;

    public override bool AllowDrag => true;

    protected override bool DeleteInternal(bool forMove, bool checkOnly) {
        foreach (var index in Cryptex.GetLabelIndex(Label.Name.ToString())) {
            if (!checkOnly) {
                Workspace.ApplyModificationRecord(Cryptex.RemoveLabel(index));
            }
            return true;
        }
        return false;
    }

    public override bool Edit() {
        foreach (var labelEdit in WorkspaceScene.LabelEditInputMode) {
            labelEdit.SetLabel(Workspace, Cryptex, Label);
            Overseer.InputManager.Select(labelEdit);
        }
        
        return true;
    }

    public override void OnStartHover() {
        // HACK don't bring to front while dragging
        if (Cryptex is object) {
            foreach (var view in Label.View) {
                var proxyGo = new GameObject("Label Proxy");
                var proxy = proxyGo.AddComponent<TransformProxy>();
                this.proxy = proxy;
                proxy.ForwardTransformation = true;
                proxy.ForwardDestruction = true;
                proxy.ForwardActive = true;

                var proxyRt = proxyGo.AddComponent<RectTransform>();
                proxyGo.transform.SetParent(view.transform.parent, false);
                proxyRt.MatchParent();

                labelViewParent = view.transform.parent;
                view.transform.SetParent(WorkspaceScene.Layer.Drag(view.GetScreen()), false);
                receiver = proxy.CreateReceiver(view.gameObject);
            }
        }
    }
    public override void OnEndHover() {
        foreach (var view in Label.View) {
            foreach (var labelViewParent in labelViewParent) {
                view.transform.SetParent(labelViewParent, false);
                this.labelViewParent = Option.None;
            }
            foreach (var receiver in receiver) {
                receiver.DestroyGameObject();
                this.receiver = Option.None;
            }
            foreach (var proxy in proxy) {
                proxy.DestroyGameObject();
                this.proxy = Option.None;
            }
        }
    }

    protected override Inserter<LabelInsertPoint, Label> Inserter => LabelInserter.Instance;

    public override Label Model => Label;
}
