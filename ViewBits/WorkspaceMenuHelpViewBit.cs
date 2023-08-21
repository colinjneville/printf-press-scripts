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

public sealed class WorkspaceMenuHelpViewBit : MonoBehaviour {
#pragma warning disable CS0649
    [SerializeField]
    private RuntimeButton settingsButton;
#pragma warning restore CS0649

    private void Awake() {
        settingsButton.Mouse0Action = (Action)MakeSettings;
    }

    private void MakeSettings() {
        gameObject.SetActive(false);
        Overseer.Instance.StartCoroutine(CoWaitForSettings());
    }

    private System.Collections.IEnumerator CoWaitForSettings() {
        var settings = Utility.Instantiate(Overseer.GlobalAssets.SettingsPrefab, transform.parent);
        while (settings != null && this != null) {
            yield return null;
        }

        if (this != null) {
            gameObject.SetActive(true);
        }
    }
}
