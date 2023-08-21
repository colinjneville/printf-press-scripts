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

public sealed class SettingsViewBit : MonoBehaviour {
#pragma warning disable CS0649
    [SerializeField]
    private VerticalLayoutGroup nameGroup;
    [SerializeField]
    private RectTransform pageParent;
    [SerializeField]
    private RuntimeButton closeButton;
#pragma warning restore CS0649
    private SortedDictionary<SettingsPage, (SettingsPageNameViewBit, SettingsPageViewBit)> pages;
    private SettingsPage selected;

    private void Start() {
        closeButton.Mouse0Action = (Action)Close;

        pages = new SortedDictionary<SettingsPage, (SettingsPageNameViewBit, SettingsPageViewBit)>();
        foreach (var page in Enum.GetValues(typeof(SettingsPage)).Cast<SettingsPage>()) {
            var nameObject = Utility.Instantiate(Overseer.GlobalAssets.SettingsPageNamePrefab, nameGroup.transform);
            nameObject.Page = page;
            nameObject.Settings = this;
            var pageObject = Utility.Instantiate(Overseer.GlobalAssets.SettingsPagePrefab, pageParent);
            pageObject.Page = page;
            pageObject.gameObject.SetActive(false);
            pages.Add(page, (nameObject, pageObject));
        }
        Select(pages.First().Key, false);
    }

    private void OnEnable() {
        WorkspaceScene.SetButtonInputOnly(true);
    }

    private void OnDisable() {
        WorkspaceScene.SetButtonInputOnly(false);
    }

    public void Select(SettingsPage page) => Select(page, true);

    private void Select(SettingsPage page, bool deselectPrevious) {
        if (deselectPrevious) {
            var oldPageObjects = pages[selected];
            oldPageObjects.Item1.Selected = false;
            oldPageObjects.Item2.gameObject.SetActive(false);
        }
        selected = page;
        var pageObjects = pages[selected];
        pageObjects.Item1.Selected = true;
        pageObjects.Item2.gameObject.SetActive(true);
    }

    private void Close() {
        foreach (var userData in Overseer.UserDataManager.Active) {
            userData.SetDirty();
        }
        Overseer.UserDataManager.Save();

        Utility.DestroyGameObject(this);
    }
}
