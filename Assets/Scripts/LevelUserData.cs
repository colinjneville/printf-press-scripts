using Functional.Option;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using UnityEngine;

using L = LocalizationString;
using LC = LocalizationConstant;
using LE = ILocalizationExpression;
using LF = LocalizationFormat;
using LI = LocalizationInt;
using LD = LocalizationDefault;

[Serializable]
public sealed class SolutionData : IDeserializeTo {
    public SolutionData(LE name, ReplayLog.Serial log, Option<int> score) {
        this.name = name;
        this.log = log;
        Score = score;
    }

    private LE name;
    private ReplayLog.Serial log;
    private int? score;

    public LE Name {
        get => name;
        set => name = value;
    }
    public ReplayLog.Serial Log {
        get => log;
        set => log = value;
    }
    public Option<int> Score {
        get => score.HasValue ? score.Value.ToOption() : Option.None;
        set => score = value.Cast<int?>().SingleOrDefault();
    }

    public static SolutionData Dummy => new SolutionData(LC.Empty, new ReplayLog.Serial(Enumerable.Empty<Record>()), Option.None);
}

[Serializable]
public sealed class LevelData : IDeserializeTo {
    private Guid levelId;
    private int version;
    private List<SolutionData> solutions;
    private bool skipText;
    private SolutionData bestSolution;
    //[JsonIgnore]
    //private Option<int> bestScore;

    public LevelData(Guid levelId, int version) {
        this.levelId = levelId;
        this.version = version;
        solutions = new List<SolutionData>();
        skipText = false;
        EvaluateBestScore();
    }

    [System.Runtime.Serialization.OnSerializing]
    private void OnSerializing(System.Runtime.Serialization.StreamingContext context) {
        // Only serialize bestSolution if none of the current solutions match it.
        // For example, user makes a solution with a score of 50. They continue work on that solution, but wind up worsening the score to 60.
        // The original solution no longer exists as a visible solution, but we keep it serialized so it can be reverified, etc, while keeping the player's earned stars intact.
        if (bestSolution != null) {
            foreach (var solution in solutions) {
                if (solution.Score.TryGetValue(out int score) && score <= bestSolution.Score.ValueOrAssert()) {
                    bestSolution = null;
                    return;
                }
            }
        }
    }

    [System.Runtime.Serialization.OnSerialized]
    private void OnSerialized(System.Runtime.Serialization.StreamingContext context) {
        // Since we may have just cleared bestSolution, reset it now
        EvaluateBestScore();
    }

    [System.Runtime.Serialization.OnDeserialized]
    private void OnDeserialized(System.Runtime.Serialization.StreamingContext context) {
        EvaluateBestScore();
    }

    public void EvaluateBestScore() {
        bestSolution = null;
        foreach (var solution in solutions) {
            foreach (var score in solution.Score) {
                if (bestSolution == null || score <= bestSolution.Score.ValueOrAssert()) {
                    bestSolution = solution;
                }
            }
        }
    }

    public Guid LevelId => levelId;
    public int Version => version;
    public IReadOnlyList<SolutionData> Solutions => solutions;
    public bool SkipText => skipText;
    public Option<int> BestScore => (bestSolution?.Score).GetValueOrDefault();

    public SolutionData NewSolution() {
        LE name = LC.Temp("Solution " + solutions.Count);
        var data = new SolutionData(name, new ReplayLog.Serial(Array.Empty<Record>()), Option.None);
        solutions.Add(data);
        return data;
    }

    public SolutionData CloneSolution(int index) {
        RtlAssert.Within(index, 0, solutions.Count);
        var oldSolution = solutions[index];
        var json = oldSolution.AsJson();
        var newSolution = json.AsSerial<SolutionData>();
        newSolution.Name = LC.Temp(newSolution.Name.ToString() + " Copy");
        solutions.Insert(index + 1, newSolution);
        return newSolution;
    }

    public void DeleteSolution(int index) {
        RtlAssert.Within(index, 0, solutions.Count);
        solutions.RemoveAt(index);
        EvaluateBestScore();
    }

    public void SetSkipText() {
        skipText = true;
    }

    public bool IsComplete => bestSolution != null;

    public int StarCount(Level level) {
        if (bestSolution != null) {
            return level.StarsEarned(bestSolution.Score.ValueOrAssert());
        }
        return 0;
    }
}

[Serializable]
public sealed class UserData : IDeserializeTo {
    private SettingsData settings = new SettingsData();
    private Dictionary<Guid, LevelData> levels = new Dictionary<Guid, LevelData>();
    [JsonIgnore]
    private bool dirty;

    public UserData() {

    }

    public bool Dirty => dirty;

    public void SetDirty() => dirty = true;
    public void ClearDirty() => dirty = false;

    public SettingsData Settings => settings;

    public Option<LevelData> TryGetLevelData(Level level) => levels.GetOrNone(level.Id);

    public LevelData GetLevelData(Level level) {
        Assert.NotNull(level);
        foreach (var ld in TryGetLevelData(level)) {
            return ld;
        }
        var levelData = new LevelData(level.Id, level.Version);
        levels.Add(level.Id, levelData);
        SetDirty();
        return levelData;
    }

    public bool IsLevelComplete(Level level) {
        if (levels.TryGetValue(level.Id, out LevelData levelData)) {
            return levelData.IsComplete;
        }
        return false;
    }

    public bool IsUnlocked(CampaignChapter chapter) {
        var unlocked = StarCount(chapter.Campaign) >= chapter.UnlockRequirement.Stars;
        foreach (var levelId in chapter.UnlockRequirement.Levels) {
            if (chapter.Campaign.GetLevel(levelId).TryGetValue(out var campaignLevel)) {
                unlocked &= IsLevelComplete(campaignLevel.Level);
            } else {
                Debug.LogWarning($"Level '{levelId}' is a requirement in campaign '{chapter.Campaign.Id}', but the level was not found");
            }

        }
        return unlocked;
    }

    public bool IsUnlocked(CampaignLevel level) {
        var unlocked = StarCount(level.Chapter) >= level.UnlockRequirement.Stars;
        foreach (var levelId in level.UnlockRequirement.Levels) {
            if (level.Chapter.Campaign.GetLevel(levelId).TryGetValue(out var campaignLevel)) {
                unlocked &= IsLevelComplete(campaignLevel.Level);
            } else {
                Debug.LogWarning($"Level '{levelId}' is a requirement in campaign '{level.Chapter.Campaign.Id}', but the level was not found");
            }

        }
        return unlocked;
    }

    private int StarCount(Campaign campaign) {
        int stars = 0;
        foreach (var chapter in campaign.Chapters) {
            stars += StarCount(chapter);
        }
        return stars;
    }

    private int StarCount(CampaignChapter chapter) {
        int stars = 0;
        foreach (var level in chapter.Levels) {
            foreach (var levelData in TryGetLevelData(level.Level)) {
                stars += levelData.StarCount(level.Level);
            }
        }
        return stars;
    }

    private void CheckForOldSolutions() {
        foreach (var kvp in levels) {
            foreach (var level in CampaignLevel.Get(kvp.Key)) {
                Assert.GreaterOrEqual(level.Level.Version, kvp.Value.Version);
                if (level.Level.Version > kvp.Value.Version) {
                    foreach (var solution in kvp.Value.Solutions) {
                        if (solution.Score.HasValue) {
                            // TODO rerun solutions
                            solution.Score = Option.None;
                        }
                    }
                }
                kvp.Value.EvaluateBestScore();
            }
        }
    }

}

public sealed class SettingsData : IDeserializeTo {

    private const bool fullScreenDefault = false;
    private const bool windowedFullScreenDefault = false;
    private const int executionLookaheadDefault = 10;
    private const int previewLookaheadDefault = 10;
    private const float stepsPerSecondDefault = 16;
    private const float cameraPanSpeedDefault = 10f;
    private const float autoSaveIntervalDefault = 10f;
    private const float lookaheadMaxOpacityPointDefault = 1.2f;
    private const float lookaheadMinOpacityPointDefault = 0f;

    private int resolutionX;
    private int resolutionY;
    
    [DefaultValue(fullScreenDefault)]
    private bool fullScreen;
    [DefaultValue(windowedFullScreenDefault)]
    private bool windowedFullScreen;
    [DefaultValue(executionLookaheadDefault)]
    private int executionLookahead;
    [DefaultValue(previewLookaheadDefault)]
    private int previewLookahead;
    [DefaultValue(stepsPerSecondDefault)]
    private float stepsPerSecond;
    [DefaultValue(cameraPanSpeedDefault)]
    private float cameraPanSpeed;
    [DefaultValue(autoSaveIntervalDefault)]
    private float autoSaveInterval;
    [DefaultValue(lookaheadMaxOpacityPointDefault)]
    private float lookaheadMaxOpacityPoint;
    [DefaultValue(lookaheadMinOpacityPointDefault)]
    private float lookaheadMinOpacityPoint;

    public SettingsData() {
        fullScreen = fullScreenDefault;
        windowedFullScreen = windowedFullScreenDefault;
        executionLookahead = executionLookaheadDefault;
        previewLookahead = previewLookaheadDefault;
        stepsPerSecond = stepsPerSecondDefault;
        cameraPanSpeed = cameraPanSpeedDefault;
        autoSaveInterval = autoSaveIntervalDefault;
        lookaheadMaxOpacityPoint = lookaheadMaxOpacityPointDefault;
        lookaheadMinOpacityPoint = lookaheadMinOpacityPointDefault;
    }

    [ResolutionSettingValues(SettingsPage.Graphics, nameof(LD.SettingResolution))]
    public Vector2Int Resolution {
        get {
            if (resolutionX == 0 || resolutionY == 0) {
                var current = UnityEngine.Screen.currentResolution;
                return new Vector2Int(current.width, current.height);
            }
            return new Vector2Int(resolutionX, resolutionY);
        }
        set {
            resolutionX = value.x;
            resolutionY = value.y;
            UpdateResolution();
        }
    }

    [BoolSettingValues(SettingsPage.Graphics, nameof(LD.SettingFullScreen))]
    public bool FullScreen {
        get => fullScreen;
        set {
            fullScreen = value;
            UpdateResolution();
        }
    }

    [BoolSettingValues(SettingsPage.Graphics, nameof(LD.SettingWindowedFullScreen))]
    public bool WindowedFullScreen {
        get => windowedFullScreen;
        set {
            windowedFullScreen = value;
            UpdateResolution();
        }
    }

    private void UpdateResolution() {
        UnityEngine.Screen.SetResolution(resolutionX, resolutionY, FullScreenMode);
    }

    private FullScreenMode FullScreenMode => fullScreen ? (windowedFullScreen ? FullScreenMode.FullScreenWindow : FullScreenMode.ExclusiveFullScreen) : FullScreenMode.Windowed;

    public int ExecutionLookahead {
        get => executionLookahead;
        set => executionLookahead = value;
    }

    public int PreviewLookahead {
        get => previewLookahead;
        set => previewLookahead = value;
    }

    [FloatSettingValues(SettingsPage.Options, nameof(LD.SettingPlaySpeed), values: new[] { 1f, 2f, 4f, 8f, 16f, 32f, 64f, float.PositiveInfinity })]
    public float StepsPerSecond {
        get => stepsPerSecond;
        set => stepsPerSecond = value;
    }

    [FloatSettingValues(SettingsPage.Options, nameof(LD.SettingCameraSpeed), values: new[] { 1f, 2f, 4f, 8f, 16f, 32f })]
    public float CameraPanSpeed {
        get => cameraPanSpeed;
        set => cameraPanSpeed = value;
    }

    public float AutoSaveInterval {
        get => autoSaveInterval;
        set => autoSaveInterval = value;
    }

    public float LookaheadMaxOpacityPoint {
        get => lookaheadMaxOpacityPoint;
        set => lookaheadMaxOpacityPoint = value;
    }

    public float LookaheadMinOpacityPoint {
        get => lookaheadMinOpacityPoint;
        set => lookaheadMinOpacityPoint = value;
    }

    public static SettingAdapter GetAdapter(UserData userData, string settingPropertyName) {
        var optionsType = userData.Settings.GetType();
        var property = optionsType.GetProperty(settingPropertyName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
        Assert.NotNull(property);

        var attributes = property.GetCustomAttributes(typeof(SettingValuesAttribute), true);
        SettingValuesAttribute attribute;
        try {
            attribute = (SettingValuesAttribute)attributes.Single();
        } catch (InvalidOperationException) {
            throw RtlAssert.NotReached($"Setting property '{settingPropertyName}' has {attributes.Length} SettingValuesAttributes (exactly 1 is required)");
        }
        return attribute.MakeAdapter(userData.Settings, property);
    }
}