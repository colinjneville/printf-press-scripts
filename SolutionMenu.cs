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

public sealed partial class SolutionMenu {
    private CampaignLevel level;
    private Option<SolutionData> selection;

    public SolutionMenu(CampaignLevel level) {
        this.level = level;
    }

    public CampaignLevel Level => level;

    public Option<SolutionData> Selection {
        get => selection;
        private set {
            var prevSelection = selection;
            selection = value;
            foreach (var view in View) {
                view.UpdateSelection(prevSelection, selection);
            }
        }
    }

    public void Select(SolutionData solutionData) {
        Selection = solutionData;
    }

    public void Deselect() {
        Selection = Option.None;
    }

    public void Refresh() {
        foreach (var selection in selection) {
            // Check to see if our selection still exists. Assume it doesn't and clear it, but if we find it iterating UserData, reassign it
            bool selectionExists = false;
            foreach (var data in Overseer.UserDataManager.Active) {
                foreach (var solutionData in data.GetLevelData(level.Level).Solutions) {
                    if (selection == solutionData) {
                        selectionExists = true;
                        break;
                    }
                }
            }
            if (!selectionExists) {
                Deselect();
            }
        }

        foreach (var view in View) {
            view.Refresh();
        }
    }
}
