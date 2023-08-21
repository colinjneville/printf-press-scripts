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

public sealed partial class LevelMenu {
    private CampaignChapter chapter;
    private Option<CampaignLevel> selection;

    public LevelMenu(CampaignChapter chapter) {
        this.chapter = chapter;
    }

    public Option<CampaignLevel> Selection {
        get => selection;
        private set {
            var prevSelection = selection;
            selection = value;
            foreach (var view in View) {
                view.UpdateSelection(prevSelection, selection);
            }
        }
    }

    public void Select(CampaignLevel level) {
        Selection = level;
    }

    public void Deselect() {
        Selection = Option.None;
    }
}
