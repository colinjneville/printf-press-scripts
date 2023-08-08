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


partial class WorkspaceLayer : IModel<WorkspaceLayer.WorkspaceLayerView> {
    [JsonIgnore]
    private ViewContainer<WorkspaceLayerView, WorkspaceLayer> view;
    public Option<WorkspaceLayerView> View => view.View;
    public WorkspaceLayerView MakeView() => view.MakeView(this);
    public void ClearView() => view.ClearView();

    public sealed class WorkspaceLayerView : MonoBehaviour, IView<WorkspaceLayer> {
        public WorkspaceLayer Model { get; set; }

        void IView.StartNow() {
            foreach (var cryptex in Model.Cryptexes) {
                OnAddCryptex(cryptex);
            }
        }

        void IView.OnDestroyNow() {
            foreach (var cryptex in Model.Cryptexes) {
                cryptex.ClearView();
            }
        }

        public void OnAddCryptex(Cryptex cryptex) {
            var view = cryptex.MakeView();
            var rt = view.GetComponent<RectTransform>();
            rt.SetParent(WorkspaceScene.Layer.Cryptex(this.GetScreen()), false);
            this.GetScreen().FixedProxy.CreateReceiver(rt.gameObject);
            rt.sizeDelta = Vector2.zero;
        }

        public void OnRemoveCryptex(Cryptex cryptex) {
            cryptex.ClearView();
        }
    }
}
