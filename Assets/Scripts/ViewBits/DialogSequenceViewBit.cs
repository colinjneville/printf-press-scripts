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

public sealed class DialogSequenceViewBit : MonoBehaviour {
#pragma warning disable CS0649
    [SerializeField]
    private ApertureViewBit aperture;
#pragma warning restore CS0649
    [SerializeField]
    [HideInInspector]
    private IApertureTarget apertureTarget;

    private void Start() {
        aperture.transform.SetParent(WorkspaceScene.Layer.Aperture(this.GetScreen()), false);
        this.GetScreen().ViewProxy.CreateReceiver(aperture.gameObject);
    }

    private void OnDestroy() {
        Utility.DestroyGameObject(aperture);
    }

    public Option<IApertureTarget> ApertureTarget {
        get => apertureTarget.ToOption();
        set {
            bool wasInactive = this.apertureTarget == null;
            aperture.gameObject.SetActive(value.HasValue);

            if (value.TryGetValue(out var apertureTarget)) {
                aperture.SetTarget(apertureTarget, snapPosition: wasInactive);
            } else {
                aperture.SetTarget(this.GetScreen());
                aperture.Skip();
            }

            this.apertureTarget = value.ValueOrDefault;
        }
    }
}
