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
using LD = LocalizationDefault;

partial class SolutionMenu : IModel<SolutionMenu.SolutionMenuView> {
    private ViewContainer<SolutionMenuView, SolutionMenu> view;
    public Option<SolutionMenuView> View => view.View;
    public SolutionMenuView MakeView() => view.MakeView(this);
    public void ClearView() => view.ClearView();

    [RequireComponent(typeof(RectTransform))]
    public sealed class SolutionMenuView : MonoBehaviour, IView<SolutionMenu> {
        public SolutionMenu Model { get; set; }

        private Dictionary<SolutionData, SolutionButtonViewBit> buttons;
        private NewSolutionButtonViewBit newSolutionButton;

        private void Awake() {
            buttons = new Dictionary<SolutionData, SolutionButtonViewBit>();
        }

        void IView.StartNow() {
            MakeButtons();
        }

        void IView.OnDestroyNow() { }

        private void ClearButtons() {
            foreach (var button in buttons.Values) {
                button.DestroyGameObject();
            }

            buttons.Clear();

            newSolutionButton.DestroyGameObject();
        }

        private void MakeButtons() {
            foreach (var userData in Overseer.UserDataManager.Active) {
                foreach (var solution in userData.GetLevelData(Model.level.Level).Solutions) {
                    var button = Utility.Instantiate(Overseer.GlobalAssets.SolutionButtonPrefab, transform);
                    button.Button.Init(Model, Model.level.Level, solution);
                    foreach (var score in solution.Score) {
                        button.CostText = LF.Inline(nameof(LD.UICurrencyCost)).Format((LI)score).ToString();
                    }
                    buttons.Add(solution, button);

                    var buttonRt = button.GetComponent<RectTransform>();
                    buttonRt.anchorMin = new Vector2(buttons.Count, 0f);
                    buttonRt.anchorMax = new Vector2(buttons.Count + 1f, 1f);
                }

                newSolutionButton = Utility.Instantiate(Overseer.GlobalAssets.NewSolutionButtonPrefab, transform);
                newSolutionButton.Button.Init(Model);

                var newSolutionButtonRt = newSolutionButton.GetComponent<RectTransform>();
                newSolutionButtonRt.anchorMin = new Vector2(buttons.Count + 1f, 0f);
                newSolutionButtonRt.anchorMax = new Vector2(buttons.Count + 2f, 1f);
            }
        }

        internal void UpdateSelection(Option<SolutionData> prevSelection, Option<SolutionData> selection) {
            foreach (var prevSelectionValue in prevSelection) {
                buttons[prevSelectionValue].Button.Close();
            }
            foreach (var selectionValue in selection) {
                buttons[selectionValue].Button.Open();
            }
        }

        internal void Refresh() {
            ClearButtons();
            MakeButtons();
        }
    }
}
