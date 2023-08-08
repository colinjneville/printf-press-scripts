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

public sealed partial class ChapterMenu {
    private Campaign campaign;
    private Option<CampaignChapter> selection;

    public ChapterMenu(Campaign campaign) {
        this.campaign = campaign;
    }

    public Option<CampaignChapter> Selection {
        get => selection;
        private set {
            var prevSelection = selection;
            selection = value;
            foreach (var view in View) {
                view.UpdateSelection(prevSelection, selection);
            }
        }
    }

    public void Select(CampaignChapter chapter) {
        Selection = chapter;
    }

    public void Deselect() {
        Selection = Option.None;
    }
}
