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

partial class LevelMenu : IModel<LevelMenu.LevelMenuView> {
    [JsonIgnore]
    private ViewContainer<LevelMenuView, LevelMenu> view;
    public Option<LevelMenuView> View => view.View;
    public LevelMenuView MakeView() => view.MakeView(this);
    public void ClearView() => view.ClearView();

    [RequireComponent(typeof(RectTransform))]
    public sealed class LevelMenuView : MonoBehaviour, IView<LevelMenu> {
        public LevelMenu Model { get; set; }

        private Dictionary<CampaignLevel, LevelButtonViewBit> buttons;

        private void Awake() {
            buttons = new Dictionary<CampaignLevel, LevelButtonViewBit>();
        }

        void IView.StartNow() {
            foreach (var level in Model.chapter.Levels) {
                var button = Utility.Instantiate(Overseer.GlobalAssets.LevelButtonPrefab, transform);
                button.Button.Init(Model, level);

                int starCount = 0;
                foreach (var userData in Overseer.UserDataManager.Active) {
                    foreach (var levelData in userData.TryGetLevelData(level.Level)) {
                        starCount = levelData.StarCount(level.Level);
                    }
                }

                
                for (int i = 0; i < level.Level.StarThresholds.Count; ++i) {
                    var star = Utility.Instantiate(Overseer.GlobalAssets.StarPrefab, button.StarGroup.transform);
                    if (i < starCount) {
                        star.Earned = true;
                    }
                }

                buttons.Add(level, button);

                var buttonRt = button.GetComponent<RectTransform>();
                buttonRt.anchorMin = new Vector2(0f, -buttons.Count);
                buttonRt.anchorMax = new Vector2(1f, -buttons.Count + 1f);
                //button.Action = (Action)(() => Overseer.LoadLevel(level.Level));
                //button.Enabled = Overseer.UserData.ValueOrAssert().IsUnlocked(level);
            }

            // HACK
            // Everything in Unity is garbage and broken, so we need to force a layout refresh in the next frame
            Utility.ExecuteNextFrame(() => { gameObject.SetActive(false); gameObject.SetActive(true); });
        }

        void IView.OnDestroyNow() { }

        internal void UpdateSelection(Option<CampaignLevel> prevSelection, Option<CampaignLevel> selection) {
            foreach (var prevSelectionValue in prevSelection) {
                buttons[prevSelectionValue].Button.Close();
            }
            foreach (var selectionValue in selection) {
                buttons[selectionValue].Button.Open();
            }
        }
    }
}
