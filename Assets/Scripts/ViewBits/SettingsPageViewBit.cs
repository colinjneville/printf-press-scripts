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

public sealed class SettingsPageViewBit : MonoBehaviour {
#pragma warning disable CS0649
    [SerializeField]
    private VerticalLayoutGroup wheelGroup;
#pragma warning restore CS0649
    private SettingsPage page;

    private void Start() {
        foreach (var userData in Overseer.UserDataManager.Active) {
            var wheels = new SortedDictionary<int, SettingWheelViewBit>();

            foreach (var property in userData.Settings.GetType().GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)) {
                foreach (var attribute in property.GetCustomAttributes(typeof(SettingValuesAttribute), true).Take(1).Cast<SettingValuesAttribute>()) {
                    if (attribute.Page == page) {
                        var wheel = Utility.Instantiate(Overseer.GlobalAssets.SettingWheelPrefab, wheelGroup.transform);
                        wheel.SettingPropertyName = property.Name;
                        wheels.Add(attribute.LineNumber, wheel);
                    }
                }
            }

            foreach (var wheel in wheels) {
                wheel.Value.transform.SetParent(wheelGroup.transform, false);
            }
        }
    }

    public SettingsPage Page {
        get => page;
        set => page = value;
    }
}
