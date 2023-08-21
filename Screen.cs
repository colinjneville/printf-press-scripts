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

[RequireComponent(typeof(RectTransform))]
public sealed class Screen : MonoBehaviour, IApertureTarget {
#pragma warning disable CS0649
    [SerializeField]
    private RectTransform fixedRect;
    [SerializeField]
    private TransformProxy fixedProxy;
    [SerializeField]
    private RectTransform viewRect;
    [SerializeField]
    private TransformProxy viewProxy;
    [SerializeField]
    private LayeredCanvas canvas;
#pragma warning restore CS0649

    [HideInInspector]
    [SerializeField]
    private RectTransform rt;

    private void Start() {
        rt = GetComponent<RectTransform>();
    }

    private void LateUpdate() {
        if (Camera.main != null) {
            float orthoSize = Camera.main.orthographicSize * 2f;
            rt.sizeDelta = new Vector2(orthoSize * Camera.main.aspect, orthoSize);
            viewRect.localPosition = Camera.main.transform.position.WithZ(0f);
        }
    }

    public TransformProxy FixedProxy => fixedProxy;
    public TransformProxy ViewProxy => viewProxy;

    public LayeredCanvas Canvas => canvas;

    Bounds2D IApertureTarget.Bounds => GetComponent<RectTransform>().GetWorldBounds();

    private static Dictionary<UnityEngine.SceneManagement.Scene, Screen> screenCache;

    public static Screen Active => Get(UnityEngine.SceneManagement.SceneManager.GetActiveScene());

    public static Screen Get(UnityEngine.SceneManagement.Scene scene) {
        if (screenCache == null) {
            screenCache = new Dictionary<UnityEngine.SceneManagement.Scene, Screen>();
        }
        Screen screen;
        if (!screenCache.TryGetValue(scene, out screen)) {
            foreach (var go in scene.GetRootGameObjects()) {
                screen = go.GetComponent<Screen>();
                if (screen != null) {
                    break;
                }
            }
            if (screen == null) {
                var activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
                UnityEngine.SceneManagement.SceneManager.SetActiveScene(scene);
                screen = Utility.Instantiate(Overseer.GlobalAssets.ScreenPrefab, null);
                UnityEngine.SceneManagement.SceneManager.SetActiveScene(activeScene);
            }
            screenCache.Add(scene, screen);
        }
        return screen;
    }

    public static Screen Get(GameObject go) => Get(go.scene);

    public static Screen Get(Component c) => Get(c.gameObject);
}
