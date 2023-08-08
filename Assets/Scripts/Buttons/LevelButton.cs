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

public sealed class LevelButton : SimpleButton {
    private LevelMenu menu;
    private CampaignLevel level;

    private SolutionMenu solutionMenu;

    public void Init(LevelMenu menu, CampaignLevel level) {
        Utility.AssignOnce(ref this.menu, menu);
        Utility.AssignOnce(ref this.level, level);
        TextInternal = level.Level.Name;
        solutionMenu = new SolutionMenu(level);
    }

    public override bool AllowMouse0 => true;

    private bool IsOpen => solutionMenu.View.HasValue;

    public void Open() {
        var view = solutionMenu.MakeView();
        var viewRt = view.GetComponent<RectTransform>();
        viewRt.SetParent(transform, false);
        viewRt.MatchParent();
        
        /*
        foreach (var data in Overseer.UserDataManager.Active) {
            solutions = new List<SolutionButtonViewBit>();
            var levelData = data.GetLevelData(level);
            foreach (var solution in levelData.Solutions) {
                var button = Utility.Instantiate(Overseer.GlobalAssets.SolutionButtonPrefab, transform);
                button.Level = level;
                button.SolutionData = solution;
                solutions.Add(button);
                var rt = button.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(solutions.Count, 0f);
                rt.anchorMax = new Vector2(solutions.Count + 1f, 1f);
            }

            newSolution = Utility.Instantiate(Overseer.GlobalAssets.NewSolutionButtonPrefab, transform);
            newSolution.Level = level;
            var newRt = newSolution.GetComponent<RectTransform>();
            newRt.anchorMin = new Vector2(solutions.Count + 1f, 0f);
            newRt.anchorMax = new Vector2(solutions.Count + 2f, 1f);
        }
        */
    }

    public void Close() {
        solutionMenu.ClearView();
    }

    protected override void Mouse0(InputMode.Modifiers modifiers, Vector3 position, Lazy<RaycastHit2D[]> hits, bool overButton) {
        if (IsOpen) {
            menu.Deselect();
        } else {
            menu.Select(level);
        }
    }

    public void Refresh() {
        if (IsOpen) {
            Close();
            Open();
        }
    }
}
