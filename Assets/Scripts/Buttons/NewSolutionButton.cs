using Functional.Option;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

using LE = ILocalizationExpression;
using L = LocalizationString;
using LC = LocalizationConstant;
using LF = LocalizationFormat;
using LI = LocalizationInt;

public sealed class NewSolutionButton : SimpleButton {
    private SolutionMenu menu;

    public void Init(SolutionMenu menu) {
        Utility.AssignOnce(ref this.menu, menu);
        TextInternal = LC.Temp("New Solution");
    }

    public override bool AllowMouse0 => true;

    protected override void Mouse0(InputMode.Modifiers modifiers, Vector3 position, Lazy<RaycastHit2D[]> hits, bool overButton) {
        foreach (var data in Overseer.UserDataManager.Active) {
            var levelData = data.GetLevelData(menu.Level.Level);
            var solutionData = levelData.NewSolution();
            data.SetDirty();
            // Refresh for when we return
            menu.Refresh();
            Overseer.LoadLevel(menu.Level.Level, solutionData);
        }
    }
}
