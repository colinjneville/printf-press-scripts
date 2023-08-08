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

public sealed class WorkspaceMenuViewBit : MonoBehaviour {
#pragma warning disable CS0649
    [SerializeField]
    private RuntimeButton returnButton;
    [SerializeField]
    private RuntimeButton continueButton;
#pragma warning restore CS0649

    private void Awake() {
        continueButton.Mouse0Action = (Action)(() => Utility.DestroyGameObject(this));
        returnButton.Mouse0Action = (Action)(() => Overseer.ReturnToMenu());
    }

    private void OnEnable() {
        WorkspaceScene.SetButtonInputOnly(true);
    }

    private void OnDisable() {
        WorkspaceScene.SetButtonInputOnly(false);
    }
}
