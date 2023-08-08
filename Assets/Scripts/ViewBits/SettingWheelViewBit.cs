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

public sealed class SettingWheelViewBit : MonoBehaviour {
#pragma warning disable CS0649
    [SerializeField]
    private RuntimeButton leftButton;
    [SerializeField]
    private RuntimeButton rightButton;
    [SerializeField]
    private TMPro.TMP_Text propertyNameText;
    [SerializeField]
    private TMPro.TMP_Text propertyValueText;
#pragma warning restore CS0649
    [SerializeField]
    private string settingPropertyName;
    private SettingAdapter adapter;

    private void Awake() {
        leftButton.Mouse0Action = (Action)Left;
        rightButton.Mouse0Action = (Action)Right;
    }

    private void Start() {
        var userData = Overseer.UserDataManager.Active.ValueOrAssert();
        
        adapter = SettingsData.GetAdapter(userData, settingPropertyName);

        propertyNameText.text = adapter.PropertyName.ToString();
        UpdateText();
    }

    public string SettingPropertyName {
        get => settingPropertyName;
        set => settingPropertyName = value;
    }

    private void Left() {
        adapter.SetPrevious();
        UpdateText();
    }

    private void Right() {
        adapter.SetNext();
        UpdateText();
    }

    private void UpdateText() {
        propertyValueText.text = adapter.CurrentValueText.ToString();
    }
}
