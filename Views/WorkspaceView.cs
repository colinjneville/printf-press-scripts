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


partial class WorkspaceFull : IModel<WorkspaceFull.WorkspaceFullView> {
    [JsonIgnore]
    private ViewContainer<WorkspaceFullView, WorkspaceFull> view;
    public Option<WorkspaceFullView> View => view.View;
    public WorkspaceFullView MakeView() => view.MakeView(this);
    public void ClearView() => view.ClearView();

    [RequireComponent(typeof(RectTransform))]
    public sealed class WorkspaceFullView : MonoBehaviour, IView<WorkspaceFull> {
        public WorkspaceFull Model { get; set; }

        private WorkspaceViewBit bit;
        private CryptexInsertViewBit insertPoint;

        private OutputTape testCasePreviewExpected;

        private ExecutionContextPreview preview;

        private Option<WorkspaceLayer> previousLayer;

        private Option<WorkspaceLayer> hiddenLayer;

        private void Awake() {
            testCasePreviewExpected = new OutputTape();

            bit = Utility.Instantiate(Overseer.GlobalAssets.WorkspacePrefab, transform);

            insertPoint = Utility.Instantiate(Overseer.GlobalAssets.CryptexInsertPrefab, transform);
        }

        private void LateUpdate() {
            ForceLayerRefresh();
        }

        void IView.StartNow() {
            var testCasePreviewExpectedView = testCasePreviewExpected.MakeView();
            var testCasePreviewExpectedViewRt = testCasePreviewExpectedView.GetComponent<RectTransform>();
            //testCasePreviewExpectedViewRt.SetParent(this.GetScreen().transform, false);
            testCasePreviewExpectedViewRt.SetParent(WorkspaceScene.Layer.OutputTape(this.GetScreen()), false);
            this.GetScreen().ViewProxy.CreateReceiver(testCasePreviewExpectedViewRt.gameObject);
            testCasePreviewExpectedViewRt.localPosition = new Vector3(11f, 16f, 0f);
            testCasePreviewExpectedViewRt.sizeDelta = new Vector2(1.25f, 1.25f);

            insertPoint.Workspace = Model;
            var insertPointRt = insertPoint.GetComponent<RectTransform>();
            //insertPointRt.SetParent(this.GetScreen().Canvas[0], false);
            //this.GetScreen().ViewProxy.CreateReceiver(insertPointRt.gameObject);
            insertPointRt.SetParent(transform, false);
            insertPointRt.anchorMin = Vector2.zero;
            insertPointRt.anchorMax = Vector2.one;
            this.GetScreen().ViewProxy.CreateReceiver(insertPointRt.gameObject);

            /*
            var toolboxView = Model.Toolbox.MakeView();
            var toolboxRt = toolboxView.GetComponent<RectTransform>();
            toolboxRt.SetParent(this.GetScreen().Canvas[5], false);
            this.GetScreen().ViewProxy.CreateReceiver(toolboxRt.gameObject);
            toolboxRt.anchorMin = new Vector2(0f, 0f);
            toolboxRt.anchorMax = new Vector2(1f, 0.25f);
            toolboxRt.sizeDelta = Vector3.zero;
            */
            bit.Toolbox = Model.Toolbox;

            MakeLayerView(Model.VisibleLayer);

            OnUpdateCost();

            UpdatePreview();
        }

        void IView.OnDestroyNow() {
            preview?.Dispose();

            insertPoint.DestroyGameObject();
            testCasePreviewExpected.ClearView();
            Model.Toolbox.ClearView();
        }

        private void MakeLayerView(WorkspaceLayer layer) {
            layer.MakeView().transform.SetParent(WorkspaceScene.Layer.WorkspaceLayer(this.GetScreen()), false);
            this.GetScreen().FixedProxy.CreateReceiver(layer.MakeView().gameObject);

            previousLayer = layer;
        }

        private void UpdatePreview() {
            // TODO PERF It is pretty inefficient to teardown and rebuild the preview on every change, but interacting with the log to do incremental changes is messy
            ClearPreview();
            preview = new ExecutionContextPreview(Model);
        }

        private void ClearPreview() {
            if (preview != null) {
                preview.Dispose();
                preview = null;
            }
        }

        internal void OnCreateTestCasePreview() {
            testCasePreviewExpected.PushRange(Model.testSuite.TestCases.First().ExpectedResult.Reverse());
        }

        internal void OnClearTestCasePreview() {
            testCasePreviewExpected.Clear();
        }

        internal void OnUpdateCost() {
            bit.CostText = LF.Inline(nameof(LD.UICurrencyCost)).Format((LI)Model.modificationCost).ToString();
        }

        internal void OnTestsPassed(int maxCost) {
            var menu = Utility.Instantiate(Overseer.GlobalAssets.ResultsMenuPrefab, WorkspaceScene.Layer.ModalMenu(this.GetScreen()));
            this.GetScreen().ViewProxy.CreateReceiver(menu.gameObject);
            menu.TotalStars = Model.Level.StarThresholds.Count;
            menu.EarnedStars = Model.Level.StarsEarned(maxCost);
            menu.Cost = maxCost;
            // HACK Unity bug workaround
            Utility.ExecuteNextFrame(() => { menu.gameObject.SetActive(false); menu.gameObject.SetActive(true); });
        }

        internal void OnModificationLogChange() {
            UpdatePreview();
        }

        internal void OnUpdateNote(Option<Note> note) {
            bit.Note = note;
        }

        internal Option<Note> Note => bit.Note;

        internal void ForceLayerRefresh() {
            var visibleLayer = Model.VisibleLayer;
            if (previousLayer != visibleLayer) {
                foreach (var previousLayer in previousLayer) {
                    previousLayer.ClearView();
                    //previousLayer.HideView();
                }

                MakeLayerView(visibleLayer);
            }
        }

        internal void OnStartExecution() {
            ClearPreview();
            Model.VisibleLayer.HideView();
            hiddenLayer = Model.VisibleLayer;
            bit.ToolboxEnabled = false;
        }

        internal void OnEndExecution() {
            foreach (var hiddenLayer in hiddenLayer) {
                hiddenLayer.UnhideView();
            }
            bit.ToolboxEnabled = true;
        }

        public IApertureTarget ExpectedOutputTarget => testCasePreviewExpected.View.Cast<OutputTape.OutputTapeView, IApertureTarget>().ValueOr(ApertureTarget.None);
    }
}
