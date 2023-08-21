using Functional.Option;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using L = LocalizationString;
using LC = LocalizationConstant;
using LE = ILocalizationExpression;
using LF = LocalizationFormat;
using LI = LocalizationInt;

[RequireComponent(typeof(GlobalAssets))]
//[RequireComponent(typeof(Screen))]
public sealed class Overseer : MonoBehaviour {
#pragma warning disable CS0649
    [SerializeField]
    private GlobalAssets globalAssets;
    [SerializeField]
    private InputManager inputManager;
    [SerializeField]
    private UserDataManager userDataManager;
    [SerializeField]
    private AudioManager audioManager;
    [SerializeField]
    private FrameTime frameTime;
    [SerializeField]
    private Transform hideaway;
    [SerializeField]
    [HideInInspector]
    private static Overseer instance;
#pragma warning restore CS0649

    private Option<WorkspaceFull> workspace;

    private Option<Scene> menuScene;

    private Option<DialogSequence> dialogSequence;

    private bool applicationQuitting;


#if       DEBUG
    private Option<Guid> presetGuid;

    private LockType lockType;
#endif // DEBUG

    private void Awake() {
        DontDestroyOnLoad(gameObject);
    }

    private void Update() {
        TapeValueView.FillPool(2);
        //TapeValueRootViewBit.FillPool(2);
    }

    private void OnApplicationQuit() {
        applicationQuitting = true;

        for (int i = SceneManager.sceneCount - 1; i >= 0; --i) {
            var scene = SceneManager.GetSceneAt(i);
            DepoolScene(scene);
            //foreach (var obj in scene.GetRootGameObjects()) {
            //    obj.SetActive(true);
            //}
        }
    }

    public static bool Quitting => Instance.applicationQuitting;

    private static void DepoolScene(Scene scene) {
        foreach (var obj in scene.GetRootGameObjects()) {
            foreach (var pooled in obj.GetComponentsInChildren<IPooled>()) {
                pooled.ForceReturn();
            }
        }
    }

    public static Overseer Instance {
        get {
            if (ReferenceEquals(instance, null)) {
                instance = FindAnyObjectByType<Overseer>();
            }

            return instance;
        }
    }

    public static GlobalAssets GlobalAssets => Instance.globalAssets;

    public static Option<WorkspaceFull> Workspace => Instance.workspace;

    public static InputManager InputManager => Instance.inputManager;

    public static UserDataManager UserDataManager => Instance.userDataManager;

    public static AudioManager AudioManager => Instance.audioManager;

    public static FrameTime FrameTime => Instance.frameTime;

    public static Transform Hideaway => Instance.hideaway;

    private static void RegisterWorkspace(WorkspaceFull workspace) {
        RtlAssert.NotHasValue(Instance.workspace);
        Instance.workspace = workspace;
        var view = workspace.MakeView();
        var viewRt = view.GetComponent<RectTransform>();
        viewRt.SetParent(WorkspaceScene.Layer.WorkspaceTools(Screen.Active), false);
        viewRt.anchorMin = Vector2.zero;
        viewRt.anchorMax = Vector2.one;

        Screen.Active.ViewProxy.CreateReceiver(view.gameObject);
        view.transform.localPosition = Vector3.zero;
        UserDataManager.OnSave += workspace.FlushSolutionData;
    }

    private static void ClearWorkspace() {
        foreach (var workspace in Instance.workspace) {
            UserDataManager.OnSave -= workspace.FlushSolutionData;

            workspace.Close();
            workspace.ClearView();
            Instance.workspace = Option.None;
        }
    }

    public static Option<DialogSequence> Dialog {
        get => Instance.dialogSequence;
        private set {
            Instance.dialogSequence = value;
            OnDialogChange(value);
        }
    }

    public static event Action<Option<DialogSequence>> OnDialogChange;

    public static void StartDialog(DialogSequence dialogSequence, Action<DialogSequence, bool> callback = null) {
        Assert.NotHasValue(Dialog);
        ClearDialog();
        Dialog = dialogSequence;
        var view = dialogSequence.MakeView();
        var rt = view.GetComponent<RectTransform>();
        // TODO this needs to be changed if using dialog in any other scenes
        rt.SetParent(WorkspaceScene.Layer.ModalMenu(view.GetScreen()), false);
        view.GetScreen().ViewProxy.CreateReceiver(rt.gameObject);
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;

        if (callback != null) {
            view.OnComplete += callback;
        }
    }

    public static void ClearDialog() {
        foreach (var dialog in Dialog) {
            dialog.ClearView();
            Dialog = Option.None;
        }
    }

    public static AsyncOperation LoadLevel(Level level, SolutionData solution) {
        Assert.False(instance.menuScene.HasValue);
        return instance.LoadLevelInternal(level, solution);
    }

    public static AsyncOperation ReturnToMenu() {
        Assert.True(instance.menuScene.HasValue);
        return instance.ReturnToMenuInternal();
    }

    private AsyncOperation LoadLevelInternal(Level level, SolutionData solution) {
        InputManager.Deselect();

        var workspace = new WorkspaceFull(level, solution);

        var oldScene = SceneManager.GetActiveScene();
        menuScene = oldScene;

        var objs = oldScene.GetRootGameObjects();
        foreach (var obj in objs) {
            // TODO this assumes all root objects will be active
            obj.SetActive(false);
        }

        var asyncOp = SceneManager.LoadSceneAsync(GlobalAssets.WorkspaceScene, LoadSceneMode.Additive);
        asyncOp.completed += OnSceneLoaded;
        return asyncOp;

        void OnSceneLoaded(AsyncOperation op) {
            var scene = SceneManager.GetSceneByName(GlobalAssets.WorkspaceScene);
            SceneManager.SetActiveScene(scene);

            // Now the scene is loaded, set the Workspace
            RegisterWorkspace(workspace);

            // If the Level has associated dialog (and it hasn't been played before) play it now
            foreach (var dialog in level.Dialog) {
                foreach (var userData in UserDataManager.Active) {
                    foreach (var levelData in userData.TryGetLevelData(level)) {
                        if (!levelData.SkipText) {
                            StartDialog(dialog, OnDialogComplete);

                            // Once the user has completed the dialog, mark it as read in the LevelData
                            void OnDialogComplete(DialogSequence dialogSequence, bool skipped) {
                                levelData.SetSkipText();
                                userData.SetDirty();
                            }
                        }
                    }
                }
            }
        }
    }

    private AsyncOperation ReturnToMenuInternal() {
        InputManager.Deselect();
        ClearWorkspace();
        UserDataManager.Save();
        var menuScene = this.menuScene.ValueOrAssert();
        var thisScene = SceneManager.GetActiveScene();
        DepoolScene(thisScene);
        var asyncOp = SceneManager.UnloadSceneAsync(thisScene);
        asyncOp.completed += OnSceneUnloaded;
        // TODO BUG potentially a race condition here if we return twice before unload completes
        SceneManager.SetActiveScene(menuScene);
        this.menuScene = Option.None;
        return asyncOp;

        void OnSceneUnloaded(AsyncOperation op) {
            foreach (var obj in menuScene.GetRootGameObjects()) {
                obj.SetActive(true);
            }
        }
    }

#if       DEBUG
    public static Option<Guid> PresetGuid => instance.presetGuid;

    public static void SetPresetGuid(Guid guid) {
        instance.presetGuid = guid;
    }
#endif // DEBUG

    public static Guid NewGuid() {
        var guid = Guid.NewGuid();

#if       DEBUG
        foreach (var pg in instance.presetGuid) {
            Debug.Log($"Consuming GUID {pg}");
            guid = pg;
            instance.presetGuid = Option.None;
        }
#endif // DEBUG

        return guid;
    }


#if       DEBUG
    public static LockType LockType {
        get => instance.lockType;
        set => instance.lockType = value;
    }
#endif // DEBUG



}
