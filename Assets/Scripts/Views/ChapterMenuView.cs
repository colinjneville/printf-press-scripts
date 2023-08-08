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

partial class ChapterMenu : IModel<ChapterMenu.ChapterMenuView> {
    private ViewContainer<ChapterMenuView, ChapterMenu> view;
    public Option<ChapterMenuView> View => view.View;
    public ChapterMenuView MakeView() => view.MakeView(this);
    public void ClearView() => view.ClearView();

    [RequireComponent(typeof(RectTransform))]
    public sealed class ChapterMenuView : MonoBehaviour, IView<ChapterMenu> {
        public ChapterMenu Model { get; set; }

        private Dictionary<CampaignChapter, ChapterButtonViewBit> buttons;

        private void Awake() {
            buttons = new Dictionary<CampaignChapter, ChapterButtonViewBit>();
        }

        void IView.StartNow() {
            foreach (var chapter in Model.campaign.Chapters) {
                var button = Utility.Instantiate(Overseer.GlobalAssets.ChapterButtonPrefab, transform);
                var buttonRt = button.GetComponent<RectTransform>();
                buttonRt.anchorMin = new Vector2(buttons.Count, 0f);
                buttonRt.anchorMax = new Vector2(buttons.Count + 1f, 1f);
                button.Button.Init(Model, chapter);
                //button.Action = (Action)(() => Overseer.LoadLevel(level.Level));
                //button.Enabled = Overseer.UserData.ValueOrAssert().IsUnlocked(level);

                buttons.Add(chapter, button);
            }
        }

        void IView.OnDestroyNow() { }

        internal void UpdateSelection(Option<CampaignChapter> prevSelection, Option<CampaignChapter> selection) {
            foreach (var prevSelectionValue in prevSelection) {
                buttons[prevSelectionValue].Button.Close();
            }
            foreach (var selectionValue in selection) {
                buttons[selectionValue].Button.Open();
            }
        }
    }
}
