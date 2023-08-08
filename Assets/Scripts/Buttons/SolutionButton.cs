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

public sealed class SolutionButton : SimpleButton {
    private SolutionMenu menu;
    private Level level;
    private SolutionData solutionData;

    public void Init(SolutionMenu menu, Level level, SolutionData solutionData) {
        Utility.AssignOnce(ref this.menu, menu);
        Utility.AssignOnce(ref this.level, level);
        Utility.AssignOnce(ref this.solutionData, solutionData);
        TextInternal = solutionData.Name;
    }

    public override bool AllowMouse0 => true;
    public override bool AllowMouse1 => true;
    public override bool AllowMouse2 => true;

    private bool IsOpen => false;

    public void Open() {
        // TODO
        Overseer.LoadLevel(level, solutionData);
    }

    public void Close() {
        // TODO
    }

    protected override void Mouse0(InputMode.Modifiers modifiers, Vector3 position, Lazy<RaycastHit2D[]> hits, bool overButton) {
        if (IsOpen) {
            // TODO
        } else {
            menu.Select(solutionData);
        }
    }
    protected override void Mouse1(InputMode.Modifiers modifiers, Vector3 position, Lazy<RaycastHit2D[]> hits, bool overButton) {
        foreach (var data in Overseer.UserDataManager.Active) {
            var levelData = data.GetLevelData(level);
            levelData.DeleteSolution(levelData.Solutions.IndexOf(solutionData));
            data.SetDirty();
            menu.Refresh();
        }
    }

    protected override void Mouse2(InputMode.Modifiers modifiers, Vector3 position, Lazy<RaycastHit2D[]> hits, bool overButton) {
        foreach (var data in Overseer.UserDataManager.Active) {
            var levelData = data.GetLevelData(level);
            levelData.CloneSolution(levelData.Solutions.IndexOf(solutionData));
            data.SetDirty();
            menu.Refresh();
        }
    }
}
