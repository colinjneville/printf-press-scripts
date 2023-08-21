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

[Serializable]
public sealed class Level : IDeserializeTo {
    private Guid id;
    private int version;
    private LE name;
    private WorkspaceLayer.Serial baseLayer;
    private int cutFrequency;
    private TestSuite.Serial testSuite;
    private ReplayLog.Serial referenceSolution;
    private List<int> starThresholds;
    private DialogSequence dialog;
    private CostOverride costOverride;

    public Level(Guid id, int version, LE name, WorkspaceLayer.Serial baseLayer, int cutFrequency, TestSuite.Serial testSuite, ReplayLog.Serial referenceSolution, IEnumerable<int> starThresholds, Option<DialogSequence> dialog = default, Option<CostOverride> costOverride = default) {
        this.id = id;
        this.version = version;
        this.name = name;
        this.baseLayer = baseLayer;
        this.cutFrequency = cutFrequency;
        this.testSuite = testSuite;
        this.referenceSolution = referenceSolution;
        this.starThresholds = starThresholds.ToList();
        this.dialog = dialog.ValueOrDefault;
        this.costOverride = costOverride.ValueOrDefault;
    }

    public Guid Id => id;
    public int Version => version;
    public LE Name => name;
    public WorkspaceLayer.Serial BaseLayer => baseLayer;
    public int CutFrequency => cutFrequency;
    public TestSuite.Serial TestSuite => testSuite;
    public ReplayLog.Serial ReferenceSolution => referenceSolution;
    public IReadOnlyList<int> StarThresholds => starThresholds;
    public Option<DialogSequence> Dialog => dialog.ToOption();
    public CostOverride CostOverride => costOverride ?? CostOverride.Default;

    public void TestReferenceSolution() {
        var workspace = new WorkspaceFull(this, referenceSolution);
        workspace.FindFirstFailedTestAsync(ReferenceSolutionFailed);
    }

    private void ReferenceSolutionFailed(int index, ExecutionContext context) {
        throw RtlAssert.NotReached($"Level '{name}' test failed on case '{index}'");
    }

    public void ApplyReferenceSolution(WorkspaceFull workspace) {
        workspace.ResetModification();
        var referenceSolution = this.referenceSolution.Deserialize(workspace);
        referenceSolution.ReplayModification(workspace);
    }

    public int StarsEarned(int solutionCost) {
        int i;
        for (i = 0; i < starThresholds.Count; ++i) {
            if (solutionCost > starThresholds[i]) {
                break;
            }
        }
        return i;
    }
}
