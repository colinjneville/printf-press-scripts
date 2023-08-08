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

public sealed partial class ExecutionContextFullFirst : ExecutionContextFull {
    private WorkspaceLight lookaheadWorkspace;
    private ExecutionContext lookaheadExecutionContext;
    private int lookaheadSteps;
    private int lookaheadExcessSteps;
    // Circular queue
    private readonly IEnumerable<TargetInfo>[] targetInfos;
    private int targetInfosIndex;

    private TargetInfoManager manager;

    // During this step of execution, we have read from a breakpoint nit. Once the step is done, Unstep, and set the next flag
    private bool breakFlag;
    // If this flag is set, do not stop and unstep if the break flag is set (as we have just triggered that breakpoint)
    private bool breakOverrideFlag;

    public ExecutionContextFullFirst(WorkspaceFull workspace) : base(workspace, workspace.TestSuite.TestCases.First()) {
        // Lock in lookahead at time of creation, because changing in the middle will break things
        foreach (var userData in Overseer.UserDataManager.Active) {
            lookaheadSteps = userData.Settings.ExecutionLookahead;
        }

        if (UseLookahead) {
            manager = new TargetInfoManager(workspace, lookaheadSteps);
            targetInfos = new IEnumerable<TargetInfo>[lookaheadSteps];
            for (int i = 0; i < targetInfos.Length; ++i) {
                targetInfos[i] = Array.Empty<TargetInfo>();
            }
            lookaheadWorkspace = new WorkspaceLight(workspace, TestCase);
            lookaheadExecutionContext = lookaheadWorkspace.CreateExecutionContext();
            for (int i = 0; i < lookaheadSteps; ++i) {
                AdvanceLookahead();
            }
        }
    }

    private IEnumerable<TargetInfo> PushTargetInfos(IEnumerable<TargetInfo> targetInfos) {
        var removed = this.targetInfos[targetInfosIndex];
        this.targetInfos[targetInfosIndex++] = targetInfos;
        targetInfosIndex %= lookaheadSteps;
        UpdateTargetInfos(removed, lookaheadExecutionContext.StepsTaken + lookaheadExcessSteps - lookaheadSteps, targetInfos, lookaheadExecutionContext.StepsTaken + lookaheadExcessSteps);
        return removed;
    }

    private IEnumerable<TargetInfo> PopTargetInfos(IEnumerable<TargetInfo> targetInfos) {
        --targetInfosIndex;
        if (targetInfosIndex < 0) {
            targetInfosIndex += lookaheadSteps;
        }
        var removed = this.targetInfos[targetInfosIndex];
        this.targetInfos[targetInfosIndex] = targetInfos;
        UpdateTargetInfos(removed, lookaheadExecutionContext.StepsTaken + lookaheadExcessSteps + 1, targetInfos, lookaheadExecutionContext.StepsTaken + lookaheadExcessSteps - lookaheadSteps + 1);
        return removed;
    }

    private void UpdateTargetInfos(IEnumerable<TargetInfo> removed, int removedTime, IEnumerable<TargetInfo> added, int addedTime) {
        manager.RemoveTargetInfos(removed, removedTime, StepsTaken);
        manager.AddTargetInfos(added, addedTime, StepsTaken);
    }

    private bool UseLookahead => lookaheadSteps > 0;

    protected override void TestCaseSuccess() {
        Workspace.VerifySolution(EnergyUse);
    }

    protected override IEnumerable<TargetInfo> ExecuteStep() {
        if (UseLookahead) {
            AdvanceLookahead();
        }
        var result = base.ExecuteStep();
        if (breakFlag && !breakOverrideFlag) {
            if (CurrentState == State.Running) {
                Break();
                if (CanUndo) {
                    Undo();
                }
            }
            breakFlag = false;
            breakOverrideFlag = true;
        } else {
            breakOverrideFlag = false;
        }

        return result;
    }

    private void AdvanceLookahead() {
        if (lookaheadExecutionContext.CurrentState == State.Idle) {
            lookaheadExecutionContext.Start();
        }

        if (lookaheadExecutionContext.CurrentState == State.Stopped) {
            ++lookaheadExcessSteps;
            PushTargetInfos(Array.Empty<TargetInfo>());
        } else if (lookaheadExecutionContext.CurrentState == State.Break) {
            PushTargetInfos(lookaheadExecutionContext.Step());
        }
        // Use our own time (StepsTaken), not the lookahead time
        manager.UpdateArrows(Workspace.CurrentLayer.Cryptexes, StepsTaken);
    }

    protected override int Undo(out IEnumerable<TargetInfo> targetInfos) {
        var energy = base.Undo(out targetInfos);

        if (lookaheadExcessSteps == 0) {
            Assert.True(lookaheadExecutionContext.CanUndo);
            lookaheadExecutionContext.Unstep();
            targetInfos = targetInfos.Concat(CheckStep());
        } else {
            --lookaheadExcessSteps;
        }

        // The TargetInfos generated here are from our main context, but TargetInfoManager uses the lookahead context TargetInfos.
        // Find the lookahead's Roller using the id
        PopTargetInfos(targetInfos.Select(ti => ti.WithRoller(lookaheadExecutionContext.Workspace.GetRoller(ti.Roller.Id).ValueOrAssert())));

        // Use our own time (StepsTaken), not the lookahead time
        manager.UpdateArrows(Workspace.CurrentLayer.Cryptexes, StepsTaken);

        breakOverrideFlag = false;

        return energy;
    }

    public IEnumerable<TargetInfo> ActiveTargetInfo {
        get {
            IEnumerable<TargetInfo> targetInfos = Enumerable.Empty<TargetInfo>();
            for (int i = targetInfosIndex; i < lookaheadSteps; ++i) {
                targetInfos = targetInfos.Concat(this.targetInfos[i]);
            }
            for (int i = 0; i < targetInfosIndex; ++i) {
                targetInfos = targetInfos.Concat(this.targetInfos[i]);
            }
            return targetInfos;
        }
    }

    public override void SetBreakFlag() => breakFlag = true;
}

public partial class ExecutionContextFull : ExecutionContext {
    private WorkspaceFull workspace;

    private int energyUse;
    //private int executionCost;

    public ExecutionContextFull(WorkspaceFull workspace, TestCase testCase) : base(workspace, testCase) {
        this.workspace = workspace;
    }

    public new WorkspaceFull Workspace => workspace;

    public int EnergyUse {
        get => energyUse;
        protected set {
            energyUse = value;
            foreach (var view in View) {
                view.OnUpdateEnergy();
            }
        }
    }

    public override Option<TargetInfo> ExecuteRecord(Record record) {
        EnergyUse += record.GetEnergy(Workspace);
        return base.ExecuteRecord(record);
    }

    protected override int Undo(out IEnumerable<TargetInfo> targetInfos) {
        var energy = base.Undo(out targetInfos);
        EnergyUse -= energy;
        return energy;
    }
}

public sealed class ExecutionContextPreview : ExecutionContext, IDisposable {
    private WorkspaceFull parentWorkspace;
    private ExecutionContextFullFirst.TargetInfoManager targetInfoManager;

    public ExecutionContextPreview(WorkspaceFull workspace) : base(new WorkspaceLight(workspace, workspace.TestSuite.TestCases.First()), workspace.TestSuite.TestCases.First()) {
        // HACK normally Workspaces create ExecutionContexts, not the other way around
        Workspace.CurrentLayer.SetContext(this);
        parentWorkspace = workspace;
        int lookahead = 0;
        foreach (var userData in Overseer.UserDataManager.Active) {
            lookahead = userData.Settings.ExecutionLookahead;
        }
        targetInfoManager = new ExecutionContextFullFirst.TargetInfoManager(workspace, lookahead);

        TestCase.Initialization.ReplayDirect(Workspace);

        if (CurrentState == State.Idle) {
            Start();
        }

        try {
            for (int i = 0; i < lookahead && CurrentState == State.Break; ++i) {
                targetInfoManager.AddTargetInfos(Step(), i, 0);
            }
        } catch (Exception ex) {
            // Since this is just to build a best-effort preview, swallow all exceptions
            // TODO Build a list of expected exception types (or ideally coalesce all 'reasonable' errors to one exception type) and only output unexpcected errors
            Debug.Log($"Preview failed: {ex}");
        }
        targetInfoManager.UpdateArrows(parentWorkspace.CurrentLayer.Cryptexes, 0);
    }

    public void Dispose() {
        targetInfoManager.Dispose();
    }
}

public partial class ExecutionContext {
    public enum State {
        Idle,
        Running,
        Break,
        Stopped,
    }

    private Workspace workspace;
    private TestCase testCase;
    private int testCaseResultIndex;
    private State state;
    private UndoLog undoLog;
    private TapeValue[] output;
    private TapeValue[] pendingOutput;
    private Option<bool> success;
    private int stepsTaken;

    private List<StepNotificationTicket> nextStepCallbacks;

    private const int channelCount = 10;

    private const int maxSteps = 100_000;

    public ExecutionContext(Workspace workspace, TestCase testCase) {
        this.workspace = workspace;
        this.testCase = testCase;
        state = State.Idle;
        output = new TapeValue[channelCount + 1];
        pendingOutput = new TapeValue[channelCount + 1];
        for (int i = 0; i < channelCount + 1; ++i) {
            output[i] = TapeValueNull.Instance;
            pendingOutput[i] = TapeValueNull.Instance;
        }

        undoLog = new UndoLog(workspace);
        nextStepCallbacks = new List<StepNotificationTicket>();
    }

    public Workspace Workspace => workspace;
    public State CurrentState => state;
    public TestCase TestCase => testCase;

    public bool CanUndo => undoLog.CanUndo;

    public Option<bool> Success => success;

    protected int TestCaseResultIndex => testCaseResultIndex;

    public int StepsTaken => stepsTaken;

    public TapeValue GetOutput(int channel) {
        Assert.Within(channel, 0, channelCount + 1);
        return output[channel];
    }

    public Record Output(int channel, TapeValue value) {
        return new OutputRecord(channel, value);
    }
    private void DoOutput(int channel, TapeValue value) {
        Assert.Within(channel, 0, channelCount + 1);
        pendingOutput[channel] = value;
    }


    private void InState(params State[] states) {
        foreach (var expectedState in states) {
            if (state == expectedState) {
                return;
            }
        }
        Assert.NotReached();
    }

    private void OutState(State state) {
        this.state = state;
    }

    public void Start() {
        InState(State.Idle);

        OutState(State.Break);
    }

    public IEnumerable<TargetInfo> Step() {
        InState(State.Break);

        var targetInfos = ExecuteStep();

        // Keep whatever state we are now (Break or Output)
        return targetInfos;
    }

    public IEnumerable<TargetInfo> Unstep() {
        InState(State.Break, State.Stopped);

        Undo(out var targetInfos);

        OutState(State.Break);

        return targetInfos;
    }

    public void Continue() {
        InState(State.Break);

        OutState(State.Running);
    }

    public void Break() {
        InState(State.Running);

        OutState(State.Break);
    }

    public void Stop() {
        InState(State.Running, State.Break);

        StopExecution();

        OutState(State.Stopped);
    }

    protected int Undo() => Undo(out var _);

    protected virtual int Undo(out IEnumerable<TargetInfo> targetInfos) {
        if (CanUndo) {
            FireCallbacks();
        }
        
        undoLog.Undo(out int energy, out targetInfos);

        return energy;
    }

    private void FireCallbacks() {
        foreach (var callback in nextStepCallbacks) {
            callback.Invoke();
        }
        nextStepCallbacks.Clear();
    }

    public IEnumerable<TargetInfo> CheckStep() {
        var targetInfos = Enumerable.Empty<TargetInfo>();
        if (stepsTaken < maxSteps) {
            foreach (var roller in Workspace.Rollers) {
                targetInfos = targetInfos.Concat(roller.CheckStep(this));
            }
        }

        return targetInfos;
    }

    protected virtual IEnumerable<TargetInfo> ExecuteStep() {
        var targetInfos = Enumerable.Empty<TargetInfo>();

        if (stepsTaken >= maxSteps) {
            success = false;
            return targetInfos;
        }

        FireCallbacks();

        using (undoLog.NewBatchFrame()) {
            // Step all ProgrammableRollers before InstructionRollers (so that PRs can read from IRs before they jump)
            foreach (var roller in Workspace.Rollers.Where(r => !r.IsPrimary)) {
                targetInfos = targetInfos.Concat(roller.Step(this));
            }
            foreach (var roller in Workspace.Rollers.Where(r => r.IsPrimary)) {
                targetInfos = targetInfos.Concat(roller.Step(this));
            }
            ExecuteRecord(new AdvanceAllInstructionRollersRecord());

            ApplyOutput();
        }
        return targetInfos;
    }

    private sealed class StepNotificationTicket : IDisposable {
        private Action callback;

        public StepNotificationTicket(Action callback) {
            Assert.NotNull(callback);
            this.callback = callback;
        }

        public void Invoke() {
            callback?.Invoke();
            Dispose();
        }

        public void Dispose() {
            callback = null;
        }
    }

    public IDisposable NotifyOnNextStep(Action callback) {
        var ticket = new StepNotificationTicket(callback);
        nextStepCallbacks.Add(ticket);
        return ticket;
    }

    public void ExecuteToCompletion() {
        for (int i = 0; i < maxSteps; ++i) {
            if (Success.HasValue) {
                return;
            }
            ExecuteStep();
        }
        // If we exceed a step limit, count it as failure
        success = false;
    }

    public virtual void SetBreakFlag() {
        // Do nothing by default - only ExecutionContextFullFirst should actually be breaking
    }

    private void ApplyTestOutput(TapeValue output) {
        foreach (var view in View) {
            view.OnUpdateOutput(output);
        }

        if (output == testCase.ExpectedResult[testCaseResultIndex]) {
            if (++testCaseResultIndex == testCase.ExpectedResult.Count) {
                Debug.Log("Success!");
                foreach (var view in View) {
                    view.OnOutputComplete(success: true);
                }

                success = true;
                StopExecution();

                TestCaseSuccess();
            }
        } else {
            Debug.Log(string.Format("Failed! Expected {0}, got {1}", testCase.ExpectedResult[testCaseResultIndex++], output));
            success = false;
            StopExecution();

            foreach (var view in View) {
                view.OnOutputComplete(success: false);
            }
        }
    }

    protected virtual void TestCaseSuccess() { }

    private TapeValue RollbackTestOutput() {
        foreach (var view in View) {
            view.OnUpdateOutput();
        }

        success = Option.None;
        // TODO resume execution if stopped?
        return testCase.ExpectedResult[--testCaseResultIndex];
    }

    private void StopExecution() {
        state = State.Stopped;
    }

    public void Update() {
        if (state == State.Running) {
            foreach (var userData in Overseer.UserDataManager.Active) {
                int steps = Mathf.FloorToInt(Time.time * userData.Settings.StepsPerSecond) - Mathf.FloorToInt((Time.time - Time.deltaTime) * userData.Settings.StepsPerSecond);
                if (float.IsPositiveInfinity(userData.Settings.StepsPerSecond)) {
                    steps = int.MaxValue;
                }
                for (; steps > 0; --steps) {
                    ExecuteStep();
                    if (state != State.Running || Overseer.FrameTime.TimeSliceExceeded()) {
                        break;
                    }
                }
            }
        }
    }

    private void ApplyOutput() {
        for (int i = 0; i < channelCount + 1; ++i) {
            ExecuteRecord(new ApplyOutputRecord(i));

            if (pendingOutput[i] != TapeValueNull.Instance) {
                ExecuteRecord(new OutputRecord(i, TapeValueNull.Instance));
            }
        }
    }

    public virtual Option<TargetInfo> ExecuteRecord(Record record) {
        return undoLog.AddAndApply(record);
    }

    public abstract class BaseRecord : Record {
        protected ExecutionContext GetExecutionContext(Workspace workspace) {
            foreach (var ec in workspace.ExecutionContext) {
                return ec;
            }
            throw new DataException(LC.Temp($"No ExecutionContext"));
        }
    }

    public abstract class ChannelRecord : BaseRecord {
        private int channel;

        public ChannelRecord(int channel) {
            this.channel = channel;
        }

        public int Channel => channel;
    }

    public sealed class ApplyOutputRecord : ChannelRecord {
        public ApplyOutputRecord(int channel) : base(channel) { }

        public override Record Apply(Workspace workspace, bool invertOnly = false) {
            var ec = GetExecutionContext(workspace);
            var oldValue = ec.GetOutput(Channel);
            if (!invertOnly) {
                ec.output[Channel] = ec.pendingOutput[Channel];

                if (Channel == 0 && !ec.output[0].IsNull) {
                    ec.ApplyTestOutput(ec.output[0]);
                }
            }

            return new RollbackOutputRecord(Channel, oldValue);
        }

        public override IEnumerable<CostType> GetCosts(Workspace workspace) => Array.Empty<CostType>();
    }

    public sealed class RollbackOutputRecord : ChannelRecord {
        private TapeValue value;
        public RollbackOutputRecord(int channel, TapeValue value) : base(channel) {
            this.value = value;
        }

        public TapeValue Value => value;

        public override Record Apply(Workspace workspace, bool invertOnly = false) {
            var ec = GetExecutionContext(workspace);
            var oldValue = ec.output[Channel];
            if (!invertOnly) {
                ec.output[Channel] = Value;

                if (Channel == 0 && !oldValue.IsNull) {
                    ec.RollbackTestOutput();
                }
            }

            return new ApplyOutputRecord(Channel);
        }

        public override IEnumerable<CostType> GetCosts(Workspace workspace) => Array.Empty<CostType>();
    }

    public sealed class OutputRecord : ChannelRecord {
        private TapeValue value;
        public OutputRecord(int channel, TapeValue value) : base(channel) {
            this.value = value;
        }
        public TapeValue Value => value;

        public override Record Apply(Workspace workspace, bool invertOnly = false) {
            var ec = GetExecutionContext(workspace);
            var oldValue = ec.pendingOutput[Channel];
            if (!invertOnly) {
                ec.DoOutput(Channel, Value);
            }
            return new OutputRecord(Channel, oldValue);
        }

        public override IEnumerable<CostType> GetCosts(Workspace workspace) => Array.Empty<CostType>();
    }

    public abstract class AllInstructionRollersRecord : BaseRecord {
        public sealed override Record Apply(Workspace workspace, bool invertOnly = false) {
            var ec = GetExecutionContext(workspace);
            if (!invertOnly) {
                foreach (var roller in ec.Workspace.Rollers.Where(r => r.IsPrimary)) {
                    ModifyRoller(workspace, roller);
                }
                ec.stepsTaken += StepModifier;
            }
            return UndoLog;
        }

        protected abstract void ModifyRoller(Workspace workspace, Roller roller);

        protected abstract int StepModifier { get; }

        protected abstract Record UndoLog { get; }
    }

    public sealed class AdvanceAllInstructionRollersRecord : AllInstructionRollersRecord {
        public override IEnumerable<CostType> GetCosts(Workspace workspace) => Array.Empty<CostType>();

        public override int GetEnergy(Workspace workspace) => workspace.Rollers.Count(r => r.IsPrimary);

        protected override void ModifyRoller(Workspace workspace, Roller roller) {
            roller.MoveRight(1).Apply(workspace);
        }

        protected override int StepModifier => 1;

        protected override Record UndoLog => new RecedeAllInstructionRollersRecord();
    }

    public sealed class RecedeAllInstructionRollersRecord : AllInstructionRollersRecord {
        public override IEnumerable<CostType> GetCosts(Workspace workspace) => Array.Empty<CostType>();

        public override int GetEnergy(Workspace workspace) => workspace.Rollers.Count(r => r.IsPrimary);

        protected override void ModifyRoller(Workspace workspace, Roller roller) {
            roller.MoveLeft(1).Apply(workspace);
        }

        protected override int StepModifier => -1;

        protected override Record UndoLog => new AdvanceAllInstructionRollersRecord();
    }
}
