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

public sealed class ChapterButton : SimpleButton {
    private CampaignChapter chapter;
    private ChapterMenu menu;

    private LevelMenu levelMenu;

    public void Init(ChapterMenu menu, CampaignChapter chapter) {
        Utility.AssignOnce(ref this.menu, menu);
        Utility.AssignOnce(ref this.chapter, chapter);
        TextInternal = chapter.Name;
        levelMenu = new LevelMenu(chapter);
    }

    public override bool AllowMouse0 => true;

    private bool IsOpen => levelMenu.View.HasValue;

    public void Open() {
        var view = levelMenu.MakeView();
        var viewRt = view.GetComponent<RectTransform>();
        viewRt.SetParent(transform, false);
        viewRt.MatchParent();
    }

    public void Close() {
        levelMenu.ClearView();
    }

    protected override void Mouse0(InputMode.Modifiers modifiers, Vector3 position, Lazy<RaycastHit2D[]> hits, bool overButton) {
        if (IsOpen) {
            menu.Deselect();
        } else {
            menu.Select(chapter);
        }
    }

    public void Refresh() {
        if (IsOpen) {
            Close();
            Open();
        }
    }
}
