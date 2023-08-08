using Functional.Option;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

using LE = ILocalizationExpression;
using LD = LocalizationDefault;
using L = LocalizationString;
using LC = LocalizationConstant;
using LF = LocalizationFormat;
using LI = LocalizationInt;

partial class ExecutionContext {
    private ViewContainer<ExecutionContextFull.ExecutionContextFullView, ExecutionContextFull> view;

    protected Option<ExecutionContextFull.ExecutionContextFullView> View => view.View;
    protected ExecutionContextFull.ExecutionContextFullView MakeView(ExecutionContextFull self) => view.MakeView(self);
    protected void ClearView() => view.ClearView();
}

partial class ExecutionContextFull : IModel<ExecutionContextFull.ExecutionContextFullView> {
    public new Option<ExecutionContextFullView> View => base.View;
    public ExecutionContextFullView MakeView() => base.MakeView(this);
    public new void ClearView() => base.ClearView();

    [RequireComponent(typeof(RectTransform))]
    public sealed class ExecutionContextFullView : MonoBehaviour, IView<ExecutionContextFull> {
        public ExecutionContextFull Model { get; set; }

        private ExecutionContextViewBit bit;

        private OutputTape outputTape;
        private OutputTape expectedTape;

        private bool isInit;

        private void Awake() {
            outputTape = new OutputTape();
            expectedTape = new OutputTape();

            bit = Utility.Instantiate(Overseer.GlobalAssets.ExecutionContextPrefab, transform);
        }

        void IView.StartNow() {
            Init();

            var outputView = outputTape.MakeView();
            var expectedView = expectedTape.MakeView();
            var outputViewRt = outputView.GetComponent<RectTransform>();
            var expectedViewRt = expectedView.GetComponent<RectTransform>();

            outputViewRt.SetParent(transform, false);
            expectedViewRt.SetParent(transform, false);
            // TEMP
            outputView.transform.localPosition = new Vector3(8f, 16f, 0f);
            //outputViewRt.anchorMin = Vector2.zero;
            //outputViewRt.anchorMax = Vector2.one;
            outputViewRt.sizeDelta = new Vector2(1.25f, 0f);

            expectedView.transform.localPosition = new Vector3(11f, 16f, 0f);
            //expectedViewRt.anchorMin = Vector2.zero;
            //expectedViewRt.anchorMax = Vector2.one;
            expectedViewRt.sizeDelta = new Vector2(1.25f, 0f);
            //outputView.transform.localScale = new Vector3(2f, 2f, 2f);
            //expectedView.transform.localScale = new Vector3(2f, 2f, 2f);
        }

        void IView.OnDestroyNow() {
            outputTape.ClearView();
            expectedTape.ClearView();
        }

        private void Init() {
            if (!isInit) {
                isInit = true;

                outputTape.PushRange(Model.TestCase.ExpectedResult.Take(Model.TestCaseResultIndex));
                //for (int i = 0; i < Model.TestCaseResultIndex; ++i) {
                //    outputTape.Push(Model.TestCase.ExpectedResult[i]);
                //}
                expectedTape.PushRange(Model.TestCase.ExpectedResult.Skip(Model.TestCaseResultIndex).Reverse());
                //for (int i = Model.TestCase.ExpectedResult.Count - 1; i >= Model.TestCaseResultIndex; --i) {
                //    expectedTape.Push(Model.TestCase.ExpectedResult[i]);
                //}
            }
        }

        public void OnUpdateOutput(Option<TapeValue> newOutput = default) {
            Init();

            TapeValue value;
            if (newOutput.TryGetValue(out value)) {
                expectedTape.Pop();
                outputTape.Push(value);
            } else {
                outputTape.Pop();
                expectedTape.Push(Model.TestCase.ExpectedResult[outputTape.Values.Count]);
            }
        }

        //internal void OnUpdateCost() {
        //    bit.CostText = new LF(nameof(LD.UICurrencyCost)).Format((LI)Model.Workspace.ModificationCost).String;
        //}

        internal void OnUpdateEnergy() {
            bit.EnergyText = FormatEnergy(Model.energyUse).ToString();
        }

        internal void OnOutputComplete(bool success) {
            if (success) {
                Overseer.AudioManager.PlayOneShot(Overseer.GlobalAssets.BellClip);
            } else {

            }
        }

        private static LE FormatEnergy(int energy) {
            LC suffix;
            if (energy >= 1_000_000_000) {
                suffix = LC.Temp("G");
                energy /= 1_000_000_000;
            } else if (energy >= 1_000_000) {
                suffix = LC.Temp("M");
                energy /= 1_000_000;
            } else if (energy >= 1_000) {
                suffix = LC.Temp("K");
                energy /= 1_000;
            } else {
                suffix = LC.Empty;
            }
            return LF.Inline(nameof(LD.Concat)).Format((LI)energy, suffix);
        }
    }

}
