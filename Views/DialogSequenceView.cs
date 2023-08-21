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

partial class DialogSequence : IModel<DialogSequence.DialogSequenceView> {
    [JsonIgnore]
    private ViewContainer<DialogSequenceView, DialogSequence> view;
    public Option<DialogSequenceView> View => view.View;
    public DialogSequenceView MakeView() => view.MakeView(this);
    public void ClearView() => view.ClearView();

    [RequireComponent(typeof(RectTransform))]
    public sealed class DialogSequenceView : MonoBehaviour, IView<DialogSequence> {
        public DialogSequence Model { get; set; }

        private DialogSequenceViewBit bit;
        private IEnumerator<DialogItem> itemEnumerator;

        void IView.StartNow() {
            bit = Utility.Instantiate(Overseer.GlobalAssets.DialogSequencePrefab, transform);

            itemEnumerator = Model.items.GetEnumerator();
            if (itemEnumerator.MoveNext()) {
                MakeItemView();
            } else {
                Debug.LogWarning("Empty DialogSequence!");
                End(false);
            }
        }

        void IView.OnDestroyNow() {
            itemEnumerator?.Current.ClearView();
        }

        public event Action<DialogSequence, bool> OnComplete;

        public void Advance() {
            itemEnumerator.Current.ClearView();
            if (itemEnumerator.MoveNext()) {
                MakeItemView();
            } else {
                End(false);
            }
        }

        public void Skip() {
            itemEnumerator.Current.ClearView();
            End(true);
        }

        private void End(bool skipped) {
            itemEnumerator = null;
            OnComplete?.Invoke(Model, skipped);
            Overseer.ClearDialog();
        }

        private void MakeItemView() {
            var view = itemEnumerator.Current.MakeView();
            var rt = view.GetComponent<RectTransform>();
            rt.SetParent(transform, false);
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;

            bit.ApertureTarget = view.ApertureTarget;
        }
    }
}
