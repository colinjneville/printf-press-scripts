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

public sealed class MenuScene : MonoBehaviour {
    //private LevelMenu menu;
    private ChapterMenu menu;

    private QuitButton quitButton;

    private ButtonInputMode buttonInput;
    //private MetaButtonInputMode metaButtonInput;

    private static MenuScene instance;

    private void Awake() {
        buttonInput = new ButtonInputMode(Overseer.InputManager);
        //metaButtonInput = new MetaButtonInputMode(Overseer.InputManager, buttonInput);
    }

    private void OnEnable() {
        Overseer.InputManager.RegisterInputMode(buttonInput);
        //Overseer.InputManager.RegisterInputMode(metaButtonInput);

        Assert.Null(instance);
        instance = this;
    }

    private void Start() {
        var screen = this.GetScreen();
        // TEMP
        var campaign = Campaign.All.First();
        //var chapter = campaign.Chapters.First();
        //var menu = new LevelMenu(chapter);
        var menu = new ChapterMenu(campaign);
        var view = menu.MakeView();
        var rt = view.GetComponent<RectTransform>();
        rt.SetParent(Layer.LevelSelect(screen), false);
        screen.ViewProxy.CreateReceiver(rt.gameObject);
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(0f, 1f);
        rt.sizeDelta = new Vector2(4f, 4f);
        rt.pivot = new Vector2(0f, 1f);

        this.menu = menu;

        quitButton = Utility.Instantiate(Overseer.GlobalAssets.QuitButtonPrefab, Layer.Quit(screen));
        screen.ViewProxy.CreateReceiver(quitButton.gameObject);
        rt = quitButton.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(4f, 4f);
        rt.anchorMin = new Vector2(1f, 1f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot = new Vector2(1f, 1f);
    }

    private void OnDisable() {
        Assert.NotNull(instance);
        instance = null;

        Overseer.InputManager.UnregisterInputMode(buttonInput);
        //Overseer.InputManager.UnregisterInputMode(metaButtonInput);
    }

    private void OnDestroy() {
        quitButton?.DestroyGameObject();

        // TEMP
        menu.ClearView();
    }

    public static class Layer {
        private static RectTransform Get(Screen screen, int layer) {
            Assert.NotNull(instance);
            return screen.Canvas[layer];
        }

        public static RectTransform LevelSelect(Screen screen) => Get(screen, 1);
        public static RectTransform Quit(Screen screen) => Get(screen, 2);
    }
}
