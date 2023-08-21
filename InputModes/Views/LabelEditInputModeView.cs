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

partial class LabelEditInputMode : IModel<LabelEditInputMode.LabelEditInputModeView> {
    private ViewContainer<LabelEditInputModeView, LabelEditInputMode> view;
    public Option<LabelEditInputModeView> View => view.View;
    public LabelEditInputModeView MakeView() => view.MakeView(this);
    public void ClearView() => view.ClearView();

    public sealed class LabelEditInputModeView : MonoBehaviour, IView<LabelEditInputMode> {
        public LabelEditInputMode Model { get; set; }

        void IView.StartNow() {
            if (Model.highlight != null) {
                CreateHighlight(Model.highlight);
            }
        }

        void IView.OnDestroyNow() { }

        internal void OnUpdateHighlight(TextHighlight oldHighlight, TextHighlight newHighlight) {
            if (oldHighlight != null) {
                oldHighlight.ClearView();
            }
            if (newHighlight != null) {
                CreateHighlight(newHighlight);
            }
        }

        private void CreateHighlight(TextHighlight highlight) {
            var view = highlight.MakeView();
            view.transform.SetParent(transform, false);
            view.SetTMP(Model.Label.MakeView().TMP);
        }
    }
}
