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

partial class OutputTape : IModel<OutputTape.OutputTapeView> {
    [JsonIgnore]
    private ViewContainer<OutputTapeView, OutputTape> view;
    public Option<OutputTapeView> View => view.View;
    public OutputTapeView MakeView() => view.MakeView(this);
    public void ClearView() => view.ClearView();

    [RequireComponent(typeof(RectTransform))]
    public sealed class OutputTapeView : MonoBehaviour, IView<OutputTape>, IApertureTarget {
        public OutputTape Model { get; set; }

        private OutputTapeViewBit bit;

        private List<PooledRef<TapeValueView>> values;

        private void Awake() {
            bit = Utility.Instantiate(Overseer.GlobalAssets.OutputTapePrefab, transform);

            values = new List<PooledRef<TapeValueView>>();

            bit.OnTweenComplete += RemoveOldValues;
        }

        void IView.StartNow() {
            foreach (var value in Model.Values) {
                MakeValueView(value);
            }
            UpdateTapeLength();
        }

        void IView.OnDestroyNow() {
            bit.OnTweenComplete -= RemoveOldValues;

            while (values.Count > 0) {
                var value = values.Pop();
                value.Return();
            }
        }

        private void RemoveOldValues(Tween tween) {
            while (values.Count > Model.Values.Count) {
                var value = values.Pop();
                value.Return();
                //UpdateTapeLength();
            }
        }

        public void OnPushValues() {
            MakeValueView(Model.Values[Model.Values.Count - 1]);

            using (bit.Unlock()) {
                UpdateTapeLength();
            }
        }

        public void OnPopValues() {
            using (bit.Unlock()) {
                UpdateTapeLength();
            }
        }

        private void UpdateTapeLength() {
            bit.Length = Model.Values.Count;
        }

        private void MakeValueView(TapeValue value) {
            int index = values.Count;
            var viewRef = TapeValueView.Get();
            foreach (var view in viewRef) {
                view.TapeValue = value;
                var viewRt = view.GetComponent<RectTransform>();
                viewRt.SetParent(bit.TapeValueContainer.transform, false);
                viewRt.anchorMin = new Vector2(0.125f, 0.125f + index);
                viewRt.anchorMax = new Vector2(0.875f, 0.875f + index);
                viewRt.sizeDelta = Vector2.zero;
            }

            values.Add(viewRef);
        }

        Bounds2D IApertureTarget.Bounds => GetComponent<RectTransform>().GetWorldBounds();
    }
}
