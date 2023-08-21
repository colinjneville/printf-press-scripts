using Functional.Option;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using TMPro;
using UnityEngine;

using L = LocalizationString;
using LC = LocalizationConstant;
using LE = ILocalizationExpression;
using LF = LocalizationFormat;
using LI = LocalizationInt;

partial class SingleValueInputMode : IModel<SingleValueInputMode.SingleValueInputModeView> {
    private ViewContainer<SingleValueInputModeView, SingleValueInputMode> view;
    public Option<SingleValueInputModeView> View => view.View;
    public SingleValueInputModeView MakeView() => view.MakeView(this);
    public void ClearView() => view.ClearView();

    [RequireComponent(typeof(RectTransform))]
    public sealed class SingleValueInputModeView : MonoBehaviour, IView<SingleValueInputMode> {
        public SingleValueInputMode Model { get; set; }

        private SingleValueInputModeViewBit bit;

        private void Awake() {
            bit = Utility.Instantiate(Overseer.GlobalAssets.SingleValueInputModePrefab, transform);
        }

        private void Update() {
            bit.Locked = Model.Locked;
            foreach (var view in Model.Cryptex.Tapes[Model.TapePoint.TapeIndex].View) {
                bit.transform.position = view.transform.position + new Vector3(Cryptex.CryptexView.GetValueX(Model.TapePoint.OffsetIndex), 0f, -0.5f);
            }
        }

        void IView.StartNow() {
            // Only set the Text here once - after this point the TextHighlightView is responsible for updating it
            bit.Text = Model.highlight.Text;
        }

        void IView.OnDestroyNow() { }

        internal void OnUpdateHighlight(TextHighlight oldHighlight, TextHighlight newHighlight) {
            if (oldHighlight != null) {
                oldHighlight.ClearView();
            }
            if (newHighlight != null) {
                var view = newHighlight.MakeView();
                view.transform.SetParent(transform, false);
                view.SetTMP(bit.TMP);
            }
        }

        public bool IsViewBit(SingleValueInputModeViewBit bit) => this.bit == bit;
    }
}
