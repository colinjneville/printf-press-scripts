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

public sealed class SettingsPageNameViewBit : MonoBehaviour {
#pragma warning disable CS0649
    [SerializeField]
    private RuntimeButton button;
    [SerializeField]
    private TMPro.TMP_Text text;
#pragma warning restore CS0649
    private SettingsViewBit settings;
    private SettingsPage page;

    private void Start() {
        button.Mouse0Action = (Action)Select;
    }

    public SettingsViewBit Settings {
        get => settings;
        set => settings = value;
    }

    public SettingsPage Page {
        get => page;
        set {
            page = value;
            var memberInfo = typeof(SettingsPage).GetMember(page.ToString(), System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static).Single();
            foreach (var attribute in memberInfo.GetCustomAttributes(typeof(SettingsPageAttribute), true).Take(1).Cast<SettingsPageAttribute>()) {
                button.Text = attribute.Name;
            }
        }
    }

    public bool Selected {
        get => !button.Mouse0Enabled;
        set => button.Mouse0Enabled = !value;
    }

    private void Select() {
        settings.Select(page);
    }
}
