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

partial class NoteEditInputMode : IModel<NoteEditInputMode.NoteEditInputModeView> {
    private ViewContainer<NoteEditInputModeView, NoteEditInputMode> view;
    public Option<NoteEditInputModeView> View => view.View;
    public NoteEditInputModeView MakeView() => view.MakeView(this);
    public void ClearView() => view.ClearView();

    public sealed class NoteEditInputModeView : MonoBehaviour, IView<NoteEditInputMode> {
        public NoteEditInputMode Model { get; set; }

        void IView.StartNow() {
            if (Model.highlight != null) {
                CreateHighlight(Model.highlight);
            }
        }

        void IView.OnDestroyNow() {
            if (Model.highlight != null) {
                Model.highlight.ClearView();
            }
            Model.Note.ClearView();
        }

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
            view.transform.SetParent(Model.Note.MakeView().transform, false);
            view.SetTMP(Model.Note.MakeView().TMP);
        }
    }
}
