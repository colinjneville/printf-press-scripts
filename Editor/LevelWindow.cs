using Functional.Option;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

using LE = ILocalizationExpression;
using L = LocalizationString;
using LC = LocalizationConstant;
using LF = LocalizationFormat;
using LI = LocalizationInt;

public class LevelWindow : EditorWindow {
    [SerializeField]
    private LE levelName;
    [SerializeField]
    private int version;
    [SerializeField]
    private Option<WorkspaceLayer.Serial> baseLayer;
    [SerializeField]
    private int cutFrequency;
    [SerializeField]
    private Option<ReplayLog.Serial> referenceSolution;
    [SerializeField]
    private List<TestCaseComponents> testCases;
    [SerializeField]
    private Option<Guid> levelId;
    [SerializeField]
    private Option<string> filePath;
    [SerializeField]
    private List<int> starThresholds;
    [SerializeField]
    private List<DialogItem> dialogItems;

    [Serializable]
    private class TestCaseComponents {
        private Option<ReplayLog.Serial> init;
        private string expectedValues;

        public TestCaseComponents(TestCase testCase) : this(testCase.Initialization.Empty ? Option.None : testCase.Initialization.Serialize().ToOption(), ValuesToString(testCase.ExpectedResult)) { }

        public TestCaseComponents() : this(Option.None, "") { }

        private TestCaseComponents(Option<ReplayLog.Serial> init, string expectedValues) {
            this.init = init;
            this.expectedValues = expectedValues;
        }

        public Option<ReplayLog.Serial> Init {
            get {
                return init;
            }
            set {
                init = value;
            }
        }

        public string ExpectedValues {
            get {
                return expectedValues;
            }
            set {
                expectedValues = value;
            }
        }
    }

    private enum State {
        Default,
        RecordingReferenceSolution,

        LoadingWorkspace,

        RecordingTestCaseInit,
        // RecordingTestCaseInit is used as the base for indexing, no values can come after it
    }
    [SerializeField]
    private State state;

    private State RecordingTestCaseInit(int index) => (State)((int)State.RecordingTestCaseInit + index);

    [MenuItem("Window/Level Editor")]
    private static void MakeWindow() {
        GetWindow<LevelWindow>().Show();
    }

    private void Awake() {
        name = "Level Editor";
        Init(PlayModeStateChange.EnteredEditMode);
    }

    private void OnEnable() {
        EditorApplication.playModeStateChanged += Init;
    }

    private void OnDisable() {
        EditorApplication.playModeStateChanged -= Init;
    }

    private void Init(PlayModeStateChange stateChange) {
        testCases = new List<TestCaseComponents>();
        starThresholds = new List<int>();
        dialogItems = new List<DialogItem>();
        state = State.Default;
        Clear();
    }

    private void OnGUI() {
        if (Application.isPlaying) {
            using (new EditorGUI.DisabledScope(state != State.Default)) {
                if (GUILayout.Button("Load Base Workspace")) {
                    bool confirm = !Overseer.Workspace.HasValue;
                    if (!confirm) {
                        confirm = EditorUtility.DisplayDialog("Load Base Workspace?", "This will clear all unsaved changes", "OK", "Cancel");
                    }
                    if (confirm) {
                        Clear();
                        // HACK
                        FindFirstObjectByType<Controls>()?.Stop();
                        Overseer.InputManager.Deselect();
                        // Don't pollute our user data with anything from the editor
                        Overseer.UserDataManager.ClearActive();
                        global::Test.EditorLevel();
                    }
                }
            }

            foreach (var workspace in Overseer.Workspace) {
                var name = EditorGUILayout.TextField("Level Name", levelName.ToString());
                if (name != null) {
                    levelName = LC.Temp(name);
                }

                version = EditorGUILayout.IntField("Version", version);

                int newCutFrequency = EditorGUILayout.IntField("Cut Frequency", cutFrequency);
                if (cutFrequency != newCutFrequency) {
                    cutFrequency = newCutFrequency;
                    foreach (var baseLayer in baseLayer) {
                        Apply(baseLayer);
                    }
                }

                GUILayout.Label("Base Layer");
                using (new EditorGUILayout.HorizontalScope()) {
                    using (new EditorGUI.DisabledGroupScope(true)) {
                        EditorGUILayout.Toggle(baseLayer.HasValue, GUILayout.ExpandWidth(false));
                    }

                    using (new EditorGUI.DisabledScope(state != State.Default)) {
                        if (GUILayout.Button("Set")) {
                            baseLayer = workspace.SerializeBaseLayer();
                        }
                    }
                    using (new EditorGUI.DisabledScope(state != State.Default))
                    using (new EditorGUI.DisabledScope(!baseLayer.HasValue)) {
                        if (GUILayout.Button("Revert")) {
                            foreach (var baseLayer in baseLayer) {
                                workspace.ClearModificationLog();
                                workspace.LoadBaseLayer(baseLayer);
                            }
                        }
                    }
                }

                GUILayout.Label("Reference Solution");
                using (new EditorGUILayout.HorizontalScope()) {
                    using (new EditorGUI.DisabledGroupScope(true)) {
                        EditorGUILayout.Toggle(referenceSolution.HasValue, GUILayout.ExpandWidth(false));
                    }
                    if (state == State.RecordingReferenceSolution) {
                        if (GUILayout.Button("Cancel")) {
                            state = State.Default;
                        }
                    } else {
                        using (new EditorGUI.DisabledGroupScope(state != State.Default)) {
                            if (GUILayout.Button("Start")) {
                                workspace.ClearModificationLog();
                                state = State.RecordingReferenceSolution;
                            }
                        }
                    }

                    using (new EditorGUI.DisabledGroupScope(state != State.RecordingReferenceSolution)) {
                        if (GUILayout.Button("Set")) {
                            var solution = workspace.SerializeModifications();
                            referenceSolution = solution.Empty ? Option.None : solution.ToOption();
                            state = State.Default;
                        }
                    }
                }

                GUILayout.Label("Star Thresholds");
                using (new EditorGUILayout.HorizontalScope()) {
                    for (int i = 0; i < starThresholds.Count; ++i) {
                        starThresholds[i] = EditorGUILayout.IntField(starThresholds[i]);
                    }
                    if (GUILayout.Button("+", GUILayout.ExpandWidth(false))) {
                        starThresholds.Add(0);
                    }
                    if (GUILayout.Button("-", GUILayout.ExpandWidth(false))) {
                        starThresholds.RemoveAt(starThresholds.Count - 1);
                    }
                }

                GUILayout.Label("Test Cases");
                for (int i = 0; i < testCases.Count; ++i) {
                    var testCase = testCases[i];
                    bool doInsert, doRemove;
                    using (new EditorGUILayout.HorizontalScope()) {
                        using (new EditorGUI.DisabledGroupScope(state != State.Default)) {
                            doInsert = GUILayout.Button("+", GUILayout.ExpandWidth(false));
                            doRemove = GUILayout.Button("-", GUILayout.ExpandWidth(false));
                        }

                        string controlName = $"ExpectedValues{i}";
                        GUI.SetNextControlName(controlName);
                        testCase.ExpectedValues = EditorGUILayout.TextField(testCase.ExpectedValues);
                    }
                    using (new EditorGUILayout.HorizontalScope()) {
                        using (new EditorGUI.DisabledGroupScope(true)) {
                            EditorGUILayout.Toggle(testCase.Init.HasValue, GUILayout.ExpandWidth(false));
                        }

                        if (state == RecordingTestCaseInit(i)) {
                            if (GUILayout.Button("Cancel")) {
                                state = State.Default;
                            }
                        } else {
                            using (new EditorGUI.DisabledGroupScope(state != State.Default)) {
                                if (GUILayout.Button("Start")) {
                                    workspace.ClearModificationLog();
                                    state = RecordingTestCaseInit(i);
                                }
                            }
                        }

                        using (new EditorGUI.DisabledGroupScope(state != RecordingTestCaseInit(i))) {
                            if (GUILayout.Button("Set")) {
                                var init = workspace.SerializeModifications();
                                testCase.Init = init.Empty ? Option.None : init.ToOption();
                                state = State.Default;
                            }
                        }
                    }

                    if (doInsert) {
                        testCases.Insert(i, new TestCaseComponents());
                        ++i;
                    }
                    if (doRemove) {
                        testCases.RemoveAt(i);
                        --i;
                    }
                }
                using (new EditorGUI.DisabledScope(state != State.Default)) {
                    if (GUILayout.Button("+")) {
                        testCases.Add(new TestCaseComponents());
                    }
                }

                EditorGUILayout.Separator();

                using (new EditorGUI.DisabledScope(state != State.Default)) {
                    for (var i = 0; i < dialogItems.Count; ++i) {
                        bool changed = false;
                        var item = dialogItems[i];
                        var oldTextStr = LEToString(item.Text);
                        var newTextStr = EditorGUILayout.TextField(oldTextStr);
                        changed |= newTextStr != oldTextStr;

                        if (changed) {
                            dialogItems[i] = new DialogItem(StringToLE(newTextStr));
                        }
                    }

                    if (GUILayout.Button("+")) {
                        dialogItems.Add(new DialogItem(LC.Empty));
                    }
                }

                EditorGUILayout.Separator();

                using (new EditorGUI.DisabledScope(state != State.Default))
                using (new EditorGUI.DisabledScope(!baseLayer.HasValue)) {
                    if (GUILayout.Button("Apply")) {
                        foreach (var baseLayer in baseLayer) {
                            Apply(baseLayer);
                        }
                    }
                }

                var fp = filePath.ValueOr("");
                GUILayout.Label(fp);

                using (new EditorGUI.DisabledScope(state != State.Default))
                using (new EditorGUI.DisabledScope(!CanSave)) {
                    using (new EditorGUI.DisabledScope(!filePath.HasValue)) {
                        if (GUILayout.Button("Save")) {
                            foreach (var baseLayer in baseLayer) {
                                foreach (var filePath in filePath) {
                                    Save(workspace, baseLayer, filePath);
                                }
                            }
                        }
                    }

                    if (GUILayout.Button("Save As")) {
                        foreach (var baseLayer in baseLayer) {
                            var filePath = EditorUtility.SaveFilePanel("Save level", "", levelName.ToString().ToLower().Replace(' ', '_'), "level.json");
                            if (filePath.Length > 0) {
                                Save(workspace, baseLayer, filePath);
                            }
                        }
                    }
                }
            }

            using (new EditorGUI.DisabledScope(state != State.Default)) {
                if (GUILayout.Button("Load")) {
                    var filePath = EditorUtility.OpenFilePanel("Load level", "", "level.json");
                    if (filePath.Length > 0) {
                        Load(filePath);
                    }
                }
            }

            foreach (var workspace in Overseer.Workspace) {
                using (new EditorGUI.DisabledScope(state != State.Default)) {
                    if (GUILayout.Button("Clear")) {
                        Clear();
                    }
                }

                EditorGUILayout.Separator();

                using (new EditorGUI.DisabledScope(state != State.Default)) {
                    if (GUILayout.Button("Create Campaign")) {
                        var folderPath = EditorUtility.OpenFolderPanel("Include levels", "", "");
                        if (folderPath.Length > 0) {
                            ExportCampaign(folderPath);
                        }
                    }
                }

                EditorGUILayout.Separator();

                GUILayout.Label("Preset GUID");
                var oldGString = Overseer.PresetGuid.Select(g => g.ToString()).ValueOr("");
                var gString = EditorGUILayout.TextField(oldGString);
                if (!string.IsNullOrWhiteSpace(gString)) {
                    if (Guid.TryParse(gString, out Guid guid)) {
                        Overseer.SetPresetGuid(guid);
                    } else {
                        // TODO make TextField red?
                    }
                }
                GUILayout.Label("Lock Type");
                var locked = (LockType)EditorGUILayout.EnumFlagsField(Overseer.LockType);
                Overseer.LockType = locked;
            }
        }
    }

    private void Clear() {
        levelId = Option.None;
        levelName = LC.Empty;
        version = 1;
        baseLayer = Option.None;
        referenceSolution = Option.None;
        testCases.Clear();
        starThresholds.Clear();
        cutFrequency = 0;

        state = State.Default;
        filePath = Option.None;
    }

    private static string ValuesToString(IEnumerable<TapeValue> values) => string.Join(" ", values.Select(tv => tv.GetText()));

    private static IEnumerable<TapeValue> StringToValues(string str) => str.Split(' ').Select(s => TapeValue.FromString(s));

    private bool CanSave => !string.IsNullOrWhiteSpace(levelName.ToString()) && baseLayer.HasValue;

    private LE StringToLE(string str) {
        // TODO
        return LC.Temp(str);
    }

    private string LEToString(LE le) {
        // TODO
        return le.ToString();
    }

    private void Apply(WorkspaceLayer.Serial baseLayer) {
        try {
            if (!CanSave) {
                return;
            }
            var oldFilePath = filePath;
            var levelJson = Serialize(baseLayer);
            var level = Deserialize(levelJson);
            Mirror(level);
            filePath = oldFilePath;
        } catch (Exception e) {
            Debug.LogError($"Applying level failed: {e}");
        }
    }

    private string Serialize(WorkspaceLayer.Serial baseLayer) {
        var levelId = this.levelId.ValueOr(Overseer.NewGuid());
        this.levelId = levelId;
        var testSuite = new ExampleTestSuite(testCases.Select(tc => new TestCase(tc.Init.Select(i => i.Deserialize(null)).ValueOr(new ReplayLog()), StringToValues(tc.ExpectedValues))));
        var referenceSolution = this.referenceSolution.ValueOr(new ReplayLog());
        var dialogSequence = dialogItems.Count > 0 ? new DialogSequence(dialogItems).ToOption() : Option.None;
        var level = new Level(levelId, version, levelName, baseLayer, cutFrequency, testSuite, referenceSolution, starThresholds, dialogSequence);
        return level.AsJson();
    }

    private Level Deserialize(string levelJson) {
        return levelJson.AsSerial<Level>();
    }

    private void Mirror(Level level) {
        // We are about to load into the Workspace, so this is the point of no return
        Clear();

        // Copy all the level details to the editor
        this.baseLayer = level.BaseLayer;
        cutFrequency = level.CutFrequency;
        referenceSolution = level.ReferenceSolution.Empty ? Option.None : level.ReferenceSolution.ToOption();
        levelName = level.Name;
        levelId = level.Id;
        // Auto-increment version here
        version = level.Version + 1;
        foreach (var testCase in level.TestSuite.Deserialize(null).TestCases) {
            testCases.Add(new TestCaseComponents(testCase));
        }
        dialogItems = level.Dialog.SelectMany(d => d.Items).ToList();

        if (this.baseLayer.TryGetValue(out WorkspaceLayer.Serial baseLayer)) {
            var newLevel = new Level(level.Id, version, levelName, baseLayer, cutFrequency, level.TestSuite, level.ReferenceSolution, starThresholds);
            if (Overseer.Workspace.HasValue) {
                Overseer.ReturnToMenu();
            }
            state = State.LoadingWorkspace;
            var asyncOp = Overseer.LoadLevel(newLevel, SolutionData.Dummy);
            asyncOp.completed += LoadMirror;
        } else {
            Debug.LogWarning("Level contains no base layer");
        }

        void LoadMirror(AsyncOperation op) {
            state = State.Default;
        }
    }

    private void Save(WorkspaceFull workspace, WorkspaceLayer.Serial baseLayer, string filePath) {
        if (!CanSave) {
            return;
        }
        try {
            var levelJson = Serialize(baseLayer);
            File.WriteAllText(filePath, levelJson);
            this.filePath = filePath;
        } catch (Exception e) {
            Debug.LogError($"Saving level failed: {e}");
        }
    }

    private void Load(string filePath) {
        var levelJson = File.ReadAllText(filePath);

        var level = Deserialize(levelJson);
        
        this.filePath = filePath;
        Mirror(level);
    }

    private void Test(WorkspaceFull workspace) {
        // TODO
    }

    private void ExportCampaign(string folderPath) {
        var campaign = MakeCampaign(folderPath);
        var json = campaign.AsJson();
        var directoryName = new DirectoryInfo(folderPath).Name.ToLower().Replace(' ', '_');
        File.WriteAllText(Path.Combine(folderPath, directoryName + ".campaign.json"), json);
    }

    private Campaign MakeCampaign(string folderPath) {
        var levels = new List<CampaignLevel>();
        foreach (var levelFile in Directory.EnumerateFiles(folderPath, "*.level.json")) {
            var json = File.ReadAllText(levelFile);
            var level = Deserialize(json);
            var campaignLevel = new CampaignLevel(level, UnlockRequirement.None);
            levels.Add(campaignLevel);
        }
        var chapter = new CampaignChapter(LC.Temp("Chapter Name"), levels, UnlockRequirement.None);
        var campaign = new Campaign(Overseer.NewGuid(), LC.Temp("Campaign Name"), chapter.Yield());
        return campaign;
    }
}
