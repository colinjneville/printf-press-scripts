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

[Flags]
public enum LockType {
    None = 0x0,
    Delete = 0x1,
    Move = 0x2,
    Edit = 0x4,
}

public abstract class Workspace {
    private Level level;

    private WorkspaceLayer baseLayer;

    protected Workspace(Level level) {
        this.level = level;
    }

    public Level Level => level;

    public abstract WorkspaceLayer CurrentLayer { get; }

    public Option<ExecutionContext> ExecutionContext => CurrentLayer.ExecutionContext;

    public abstract ExecutionContext CreateExecutionContext();

    public IReadOnlyCollection<Cryptex> Cryptexes => CurrentLayer.Cryptexes;
    public IEnumerable<Roller> Rollers => CurrentLayer.Cryptexes.SelectMany(c => c.Rollers);

    public Option<Cryptex> GetCryptex(Guid id) => CurrentLayer.GetCryptex(id);

    public Option<Roller> GetRoller(Guid id) {
        return Rollers.SingleOrDefault(r => r.Id == id).ToOption();
    }

    public Option<Roller> GetRoller(ColorId color, bool primaryRoller) {
        return Rollers.SingleOrDefault(r => r.Color == color && r.IsPrimary == primaryRoller).ToOption();
    }

    public Option<InstructionRoller> GetPrimaryRoller(ColorId color) => GetRoller(color, true).ExplicitCast<Roller, InstructionRoller>();

    public Option<ProgrammableRoller> GetSecondaryRoller(ColorId color) => GetRoller(color, false).ExplicitCast<Roller, ProgrammableRoller>();

    protected WorkspaceLayer BaseLayer {
        get => baseLayer;
        set => baseLayer = value;
    }
}

public sealed class WorkspaceLight : Workspace {
    private TestCase testCase;

    public WorkspaceLight(Workspace workspace, TestCase testCase) : base(workspace.Level) {
        this.testCase = testCase;
        BaseLayer = workspace.CurrentLayer.Serialize().Deserialize(this);
    }

    public override ExecutionContext CreateExecutionContext() {
        var ec = new ExecutionContext(this, testCase);
        BaseLayer.SetContext(ec);
        return ec;
    }

    public override WorkspaceLayer CurrentLayer => BaseLayer;
}

public abstract class WorkspaceFullShim : Workspace {
    protected WorkspaceFullShim(Level level) : base(level) { }

    public override ExecutionContext CreateExecutionContext() => CreateExecutionContextShim();

    protected abstract ExecutionContextFull CreateExecutionContextShim();
}

public sealed partial class WorkspaceFull : WorkspaceFullShim {
    private TestSuite testSuite;
    
    private Option<WorkspaceLayer> modificationLayer;
    private Option<WorkspaceLayer> testCaseLayer;
    private Option<WorkspaceLayer> executionLayer;
    private Option<WorkspaceLayer> executionLookaheadLayer;
    // These layers are not considered for anything but visibility
    private Option<WorkspaceLayer> testCaseStashLayer;
    private Option<WorkspaceLayer> executionStashLayer;

    private Guid executableCryptexId;

    private Option<SolutionData> solutionData;
    private int modificationCost;

    private Toolbox toolbox;

    private UndoLog modificationLog;

    public WorkspaceFull(Level level, SolutionData solutionData) : this(level, solutionData.Log) {
        this.solutionData = solutionData;
    }

    public WorkspaceFull(Level level, ReplayLog.Serial initialModification) : this(level) {
        StartModification();
        try {
            initialModification.Deserialize(this).ReplayModification(this);
        } catch (Exception e) {
            Debug.LogWarning(e);
            // TODO notify user solution could not be applied (probably because the Level was updated)
        }
    }

    public WorkspaceFull(Level level) : base(level) {
        BaseLayer = level.BaseLayer.Deserialize(this);
        // TEMP
        if (BaseLayer.Cryptexes.Count > 0) {
            executableCryptexId = BaseLayer.Cryptexes.First().Id;
        }
        testSuite = level.TestSuite.Deserialize(this);

        // TODO custom Toolboxes
        toolbox = Toolbox.Default;
    }

    public WorkspaceLayer.Serial Export() => CurrentLayer.Serialize();

    public TestSuite TestSuite => testSuite;

    public Toolbox Toolbox => toolbox;

    public bool IsExecutable(Guid cryptexId) => cryptexId == executableCryptexId;
    public bool IsExecutable(Cryptex cryptex) => IsExecutable(cryptex.Id);

    public int ModificationCost {
        get => modificationCost;
        private set {
            modificationCost = value;
            foreach (var view in View) {
                view.OnUpdateCost();
            }
        }
    }

    protected override ExecutionContextFull CreateExecutionContextShim() => CreateExecutionContext();

    public new ExecutionContextFull CreateExecutionContext() {
        Assert.NotHasValue(executionLayer);
        var ec = CreateExecutionContextInternal();

        CloneLayer(out executionLookaheadLayer, ref executionLayer, inPlace: true);
        foreach (var view in View) {
            view.OnStartExecution();
        }

        return ec;
    }

    private ExecutionContextFull CreateExecutionContextInternal(TestCase testCase = null) {
        var loadedTestCase = testCase ?? TestSuite.TestCases.First();
        LoadTestCase(loadedTestCase);
        CloneLayer(out executionLayer, ref testCaseLayer);

        ExecutionContextFull ec;
        if (testCase == null) {
            ec = new ExecutionContextFullFirst(this);
        } else {
            ec = new ExecutionContextFull(this, testCase);
        }
        CurrentLayer.SetContext(ec);

        return ec;
    }

    private void LoadTestCase(TestCase testCase) {
        ClearLayer(ref testCaseLayer);
        ClearLayer(ref executionLayer);
        CloneLayer(out testCaseLayer, ref modificationLayer);
        testCase.Initialization.ReplayDirect(this);
    }

    private void ClearLayer(ref Option<WorkspaceLayer> layer) {
        layer = Option.None;
    }
    private void SetLayer(ref Option<WorkspaceLayer> oldLayer, WorkspaceLayer newLayer) {
        oldLayer = newLayer;
    }

    private void CloneLayer(out Option<WorkspaceLayer> destination, ref WorkspaceLayer source, bool inPlace = false) {
        var copy = source.Serialize().Deserialize(this);
        if (inPlace) {
            destination = copy;
        } else {
            destination = source;
            source = copy;
        }
    }
    private void CloneLayer(out Option<WorkspaceLayer> destination, ref Option<WorkspaceLayer> source, bool inPlace = false) {
        destination = Option.None;
        foreach (var sourceValue in source) {
            var copy = sourceValue.Serialize().Deserialize(this);
            if (inPlace) {
                destination = copy;
            } else {
                destination = sourceValue;
                source = copy;
            }
        }
    }

    private void ForceLayerViewRefresh() {
        foreach (var view in View) {
            view.ForceLayerRefresh();
        }
    }

    public void StartModification() {
        Assert.NotHasValue(modificationLayer);
        var baseLayer = BaseLayer;
        CloneLayer(out modificationLayer, ref baseLayer);
        BaseLayer = baseLayer;
        modificationLog = new UndoLog(this, (Action)OnModificationLogChange);
    }

    public void ResetModification() {
        Assert.HasValue(modificationLayer);
        modificationLog.UndoAll();
    }

    public UndoLog.IBatchFrame NewModificationBatchFrame() {
        Assert.HasValue(modificationLayer);
        Assert.Equal(modificationLayer.Value, CurrentLayer);
        return modificationLog.NewBatchFrame();
    }

    private void OnModificationLogChange() {
        foreach (var view in View) {
            view.OnModificationLogChange();
        }
    }

    public void ApplyModificationRecord(Record modificationRecord) {
        ApplyModificationBatchRecord(modificationRecord.Yield());
    }

    public void ApplyModificationBatchRecord(params Record[] modificationRecords) => ApplyModificationBatchRecord((IEnumerable<Record>)modificationRecords);

    public void ApplyModificationBatchRecord(IEnumerable<Record> modificationRecords) {
        Assert.HasValue(modificationLayer);
        Assert.Equal(modificationLayer.Value, CurrentLayer);

        bool isExecutionRelevant = false;

        using (modificationLog.NewBatchFrame()) {
            foreach (var modificationRecord in modificationRecords) {
                foreach (var costType in modificationRecord.GetCosts(this)) {
                    ModificationCost += costType.ToCost(Level.CostOverride);
                }
                modificationLog.AddAndApply(modificationRecord);

                isExecutionRelevant |= modificationRecord.IsExecutionRelevant;
            }
        }

        // If this is only part of a multi-part change (e.g. delete + add), wait for the whole change to complete
        //if (!modificationLog.IsInBatchFrame) {
        //    pendingModifications = true;
        //    if (isExecutionRelevant) {
        //        ClearSolutionScore();
        //        OnModificationLogChange();
        //    }
        //}
    }

    public void UndoModification() {
        Assert.HasValue(modificationLayer);
        Assert.Equal(modificationLayer.Value, CurrentLayer);
        var undone = modificationLog.Undo();
        foreach (var undoneCost in undone) {
            ModificationCost += undoneCost.ToCost(Level.CostOverride);
        }
    }

    public void RedoModification() {
        Assert.HasValue(modificationLayer);
        Assert.Equal(modificationLayer.Value, CurrentLayer);
        var redone = modificationLog.Redo();
        foreach (var redoneCost in redone) {
            ModificationCost += redoneCost.ToCost(Level.CostOverride);
        }
    }

    public void StopExecution() {
        Assert.HasValue(executionLayer);

        ClearLayer(ref testCaseLayer);
        ClearLayer(ref executionLayer);

        ForceLayerViewRefresh();
        OnModificationLogChange();
        foreach (var view in View) {
            view.OnEndExecution();
        }
    }

    public void CreateTestCasePreview() {
        LoadTestCase(testSuite.TestCases.First());
        foreach (var view in View) {
            view.OnCreateTestCasePreview();
        }

        //OnModificationLogChange();
    }

    public void ClearTestCasePreview() {
        ClearLayer(ref testCaseLayer);
        foreach (var view in View) {
            view.OnClearTestCasePreview();
        }

        OnModificationLogChange();
    }

    private void StashLayer() {
        Assert.HasValue(testCaseLayer);
        Assert.HasValue(executionLayer);
        Assert.NotHasValue(testCaseStashLayer);
        Assert.NotHasValue(executionStashLayer);

        CloneLayer(out testCaseStashLayer, ref testCaseLayer);
        CloneLayer(out executionStashLayer, ref executionLayer);

        // HACK
        UnityEngine.Object.FindAnyObjectByType<Controls>()?.ClearContext();
    }

    private void UnstashLayer() {
        Assert.HasValue(testCaseStashLayer);
        Assert.HasValue(executionStashLayer);
        Assert.NotHasValue(testCaseLayer);
        Assert.NotHasValue(executionLayer);

        CloneLayer(out testCaseLayer, ref testCaseStashLayer);
        CloneLayer(out executionLayer, ref executionStashLayer);

        // HACK
        foreach (var executionLayer in executionLayer) {
            foreach (var ec in executionLayer.ExecutionContext) {
                UnityEngine.Object.FindAnyObjectByType<Controls>()?.InjectContext((ExecutionContextFull)ec);
            }
        }

        ClearStash();
    }

    private void ClearStash() {
        ClearLayer(ref executionStashLayer);
        ClearLayer(ref testCaseStashLayer);
    }

    public void VerifySolution(int firstEnergy) {
        Overseer.Instance.StartCoroutine(FindFirstFailedTestAsync(AdditionalTestFailed, AdditionalTestsPassed(firstEnergy), skipFirst: true));
    }

    private void AdditionalTestFailed(int index, ExecutionContextFull context) {
        Debug.Log($"Solution failed on test case {index}");
        foreach (var ec in ExecutionContext) {
            // HACK WorkspaceFull's layers should always have ExecutionContextFulls
            ((ExecutionContextFull)ec).ClearView();
        }

        ClearStash();
        // HACK
        UnityEngine.Object.FindAnyObjectByType<Controls>()?.InjectContext(context);
    }

    private Action<int> AdditionalTestsPassed(int firstEnergy) => (maxEnergy) => AdditionalTestsPassedInternal(Mathf.Max(firstEnergy, maxEnergy));

    private void AdditionalTestsPassedInternal(int maxEnergy) {
        foreach (var solutionData in solutionData) {
            int score = modificationCost + Level.CostOverride.GetEnergyCost(maxEnergy);
            solutionData.Score = score;
            foreach (var userData in Overseer.UserDataManager.Active) {
                userData.SetDirty();
            }

            foreach (var view in View) {
                view.OnTestsPassed(score);
            }

            Debug.Log($"Solution succeeded with cost of {score}");
            foreach (var userData in Overseer.UserDataManager.Active) {
                var levelData = userData.GetLevelData(Level);
                // Check if this is a new best solution
                levelData.EvaluateBestScore();
            }
        }

        StopExecution();
        UnstashLayer();
    }

    public System.Collections.IEnumerator FindFirstFailedTestAsync(bool skipFirst = false) => FindFirstFailedTestAsync(default, default, skipFirst);

    public System.Collections.IEnumerator FindFirstFailedTestAsync(Action<int, ExecutionContextFull> failureCallback, bool skipFirst = false) => FindFirstFailedTestAsync(failureCallback, default, skipFirst);

    public System.Collections.IEnumerator FindFirstFailedTestAsync(Action<int, ExecutionContextFull> failureCallback, Action<int> successCallback, bool skipFirst = false) => FindFirstFailedTestAsync(failureCallback.ToOption(), successCallback.ToOption(), skipFirst);

    public System.Collections.IEnumerator FindFirstFailedTestAsync(Option<Action<int, ExecutionContextFull>> failureCallback = default, Option<Action<int>> successCallback = default, bool skipFirst = false) {
        // We have to make some progress on executing the tests, even if it impacts framerate.
        const int minimumStepsPerFrame = 100;
        int stepsPerFrame = 0;

        int testNum = 0;
        int maxEnergy = 0;

        StashLayer();

        foreach (var testCase in testSuite.TestCases) {
            if (skipFirst) {
                skipFirst = false;
            } else {
                StopExecution();
                var ec = CreateExecutionContextInternal(testCase);
                ec.Start();

                bool success;
                while (!ec.Success.TryGetValue(out success)) {
                    ec.Step();
                    if (++stepsPerFrame >= minimumStepsPerFrame && Overseer.FrameTime.TimeSliceExceeded()) {
                        yield return null;
                        stepsPerFrame = 0;
                    }
                }
                
                if (ec.Success == false) {
                    foreach (var failureCallbackValue in failureCallback) {
                        failureCallbackValue(testNum, ec);
                    }
                    // A test failed, no need to run any after
                    yield break;
                } else {
                    maxEnergy = Mathf.Max(maxEnergy, ec.EnergyUse);
                }
            }
            ++testNum;
        }

        foreach (var successCallbackValue in successCallback) {
            successCallbackValue(maxEnergy);
        }
    }

    public void Close() {
        FlushSolutionData();
    }

    public void FlushSolutionData() {
        foreach (var solutionData in solutionData) {
            if (modificationLog.HasPendingChanges) {
                foreach (var modificationLayer in modificationLayer) {
                    var records = WorkspaceLayer.Serial.Transform(Level.BaseLayer, modificationLayer);

                    //solutionData.Log = modificationLog.ToReplayLog();
                    solutionData.Log = new ReplayLog(records);
                    foreach (var data in Overseer.UserDataManager.Active) {
                        data.SetDirty();
                    }
                    modificationLog.ClearPendingChangesFlag();
                }
            }
        }
    }

    public void SetSolutionScore(int score) {
        foreach (var solutionData in solutionData) {
            solutionData.Score = score;
            foreach (var userData in Overseer.UserDataManager.Active) {
                var levelData = userData.GetLevelData(Level);
                levelData.EvaluateBestScore();
            }
        }
    }

    public void ClearSolutionScore() {
        foreach (var solutionData in solutionData) {
            solutionData.Score = Option.None;
            // BestScore can never get worse, so no need to reevaluate
        }
    }

#if UNITY_EDITOR

    private const string EditorDirectory = @"D:\Temp\";

    private const string BaseLayerFile = EditorDirectory + @"baselayer.json";
    private const string ReferenceSolutionFile = EditorDirectory + @"referencesolution.json";
    private const string TestCaseFile = EditorDirectory + @"testcase{0}.json";
    private const string NewLevelFile = EditorDirectory + @"newlevel.json";

    public WorkspaceLayer.Serial SerializeBaseLayer() {
        foreach (var layer in modificationLayer) {
            return layer.Serialize();
        }
        throw new InvalidOperationException();
    }

    public void LoadBaseLayer(WorkspaceLayer.Serial baseLayerSerial) {
        Assert.HasValue(modificationLayer);
        Assert.Equal(modificationLayer.Value, CurrentLayer);
        var baseLayer = baseLayerSerial.Deserialize(this);
        SetLayer(ref modificationLayer, baseLayer);
    }

    public void ClearModificationLog() {
        Assert.HasValue(modificationLayer);
        Assert.Equal(modificationLayer.Value, CurrentLayer);
        modificationLog.Clear();
    }

    public ReplayLog.Serial SerializeModifications() {
        Assert.HasValue(modificationLayer);
        Assert.Equal(modificationLayer.Value, CurrentLayer);
        return new ReplayLog.Serial(WorkspaceLayer.Serial.Transform(Level.BaseLayer, modificationLayer.Value));
    }
#endif //UNITY_EDITOR

    public override WorkspaceLayer CurrentLayer => executionLayer.ValueOr(testCaseLayer.ValueOr(modificationLayer.ValueOr(BaseLayer)));

    private WorkspaceLayer VisibleLayer => executionStashLayer.ValueOr(testCaseStashLayer.ValueOr(executionLayer.ValueOr(testCaseLayer.ValueOr(modificationLayer.ValueOr(BaseLayer)))));

    public WorkspaceLayer.Serial SerializeCurrentLayer() => CurrentLayer.Serialize();

    private void DoAddCryptex(Cryptex cryptex) {
        // TEMP
        if (CurrentLayer.Cryptexes.Count == 0) {
            executableCryptexId = cryptex.Id;
        }
        CurrentLayer.AddCryptex(cryptex);
    }

    private void DoRemoveCryptex(Guid id) {
        CurrentLayer.RemoveCryptex(id);
    }

    public Record AddCryptex(Cryptex.Serial cryptex) {
        return new AddCryptexRecord(cryptex);
    }

    public Record RemoveCryptex(Guid cryptexId) {
        return new RemoveCryptexRecord(cryptexId);
    }

    public sealed class AddCryptexRecord : Record {
        private Cryptex.Serial cryptex;

        public AddCryptexRecord(Cryptex.Serial cryptex) {
            this.cryptex = cryptex;
        }

        public override IEnumerable<CostType> GetCosts(Workspace workspace) => CostType.Cryptex.Yield();

        public override IEnumerable<RecordObjectId> RequiredIds => new RecordObjectId(cryptex.Id).Yield();

        public override Record Apply(Workspace workspace, bool invertOnly = false) {
            if (!invertOnly) {
                // HACK
                var workspaceFull = workspace as WorkspaceFull;
                if (workspaceFull == null) {
                    throw new DataException(LC.Temp($"Attempted to apply a {nameof(AddCryptexRecord)} to a non-{nameof(WorkspaceFull)}"));
                } else {
                    workspaceFull.DoAddCryptex(cryptex.Deserialize(workspace));
                }
            }
            return new RemoveCryptexRecord(cryptex.Id);
        }
    }

    public sealed class RemoveCryptexRecord : Cryptex.BaseRecord {
        public RemoveCryptexRecord(Guid cryptexId) : base(cryptexId) { }

        public override IEnumerable<CostType> GetCosts(Workspace workspace) => (~CostType.Cryptex).Yield();

        public override Option<RecordObjectId> RemovedId => new RecordObjectId(CryptexId);

        public override Record Apply(Workspace workspace, bool invertOnly = false) {
            var cryptex = GetCryptex(workspace).Serialize();
            if (!invertOnly) {
                // HACK
                var workspaceFull = workspace as WorkspaceFull;
                if (workspaceFull == null) {
                    throw new DataException(LC.Temp($"Attempted to apply a {nameof(RemoveCryptexRecord)} to a non-{nameof(WorkspaceFull)}"));
                } else {
                    workspaceFull.DoRemoveCryptex(CryptexId);
                }
            }
            return new AddCryptexRecord(cryptex);
        }
    }
}
