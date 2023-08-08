using Functional.Option;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

using L = LocalizationString;
using LC = LocalizationConstant;
using LE = ILocalizationExpression;
using LF = LocalizationFormat;
using LI = LocalizationInt;
using LD = LocalizationDefault;

public class SettingWheelButton : SimpleButton {
    private enum Direction {
        Left,
        Right,
    }

#pragma warning disable CS0649
    [SerializeField]
    private SettingWheelViewBit bit;
#pragma warning restore CS0649
    [SerializeField]
    private Direction direction;

    public override bool AllowMouse0 => true;

    public override bool Mouse0(InputMode.State state, InputMode.Modifiers modifiers, Vector3 position, Lazy<RaycastHit2D[]> hits, bool overButton) {
        return base.Mouse0(state, modifiers, position, hits, overButton);
    }
}
