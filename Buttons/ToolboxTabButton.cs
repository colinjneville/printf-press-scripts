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

public sealed class ToolboxTabButton : Button {
    private Toolbox toolbox;
    private int pageIndex;

    public Toolbox Toolbox {
        get => toolbox;
        set => toolbox = value;
    }

    public int PageIndex {
        get => pageIndex;
        set => pageIndex = value;
    }

    public void Init(Toolbox toolbox, int pageIndex) {
        Assert.Null(this.toolbox);
        this.toolbox = toolbox;
        this.pageIndex = pageIndex;
    }

    public override bool AllowMouse0 => true;

    public override bool Mouse0(InputMode.State state, InputMode.Modifiers modifiers, Vector3 position, Lazy<RaycastHit2D[]> hits, bool overButton) {
        switch (state) {
            case InputMode.State.Release:
                if (overButton) {
                    toolbox.SelectPage(pageIndex);
                }
                break;
        }
        return true;
    }
}
