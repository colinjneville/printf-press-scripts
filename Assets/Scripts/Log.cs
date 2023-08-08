using Functional.Option;
using JsonSubTypes;
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

public abstract class Log {
    public static IEnumerable<Record> Prune(IEnumerable<Record> records) {
        var requiredIds = new Dictionary<RecordObjectId, List<int>>();
        
        var remainingRecords = records.ToList();

        int recordIndex = 0;
        foreach (var record in records) {
            // Put this Record down as being dependant on each Id in RequiredIds
            Debug.Log($"{recordIndex}:");
            // If this Record removes an Id, blank all Records that were dependant on the Id
            foreach (var id in record.RemovedId) {
                Debug.Log($"- {id}");

                List<int> indicies;
                if (requiredIds.TryGetValue(id, out indicies)) {
                    foreach (var index in indicies) {
                        Debug.Log($"Null at {index}");
                        remainingRecords[index] = null;
                    }
                    indicies.Clear();
                }
            }

            foreach (var id in record.RequiredIds) {
                void RequireId(RecordObjectId roi) {
                    Debug.Log($"@ {roi}");
                    List<int> indicies;
                    if (!requiredIds.TryGetValue(roi, out indicies)) {
                        indicies = new List<int>();
                        requiredIds.Add(roi, indicies);
                    }
                    indicies.Add(recordIndex);
                }

                RequireId(id);
                if (id.Index.HasValue) {
                    RequireId(id.Parent);
                }
            }

            ++recordIndex;
        }

        // Now return all non-null Records
        return remainingRecords.Where(r => r != null);
    }
}

partial class ReplayLog : ISerializeTo<ReplayLog.Serial> {
    [Serializable]
    public struct Serial : IDeserializeTo<ReplayLog> {
        private Record[] records;

        public Serial(IEnumerable<Record> records) {
            this.records = records.ToArray();
        }

        public ReplayLog Deserialize(Workspace workspace) {
            return new ReplayLog(records);
        }

        public bool Empty => records.Length == 0;
    }

    public Serial Serialize() {
        return new Serial(log);
    }

    public static implicit operator Serial(ReplayLog self) => self.Serialize();
}

public sealed partial class ReplayLog : Log {
    private Queue<Record> log;

    public ReplayLog(params Record[] records) : this((IEnumerable<Record>) records) { }
    public ReplayLog(IEnumerable<Record> records) {
        log = new Queue<Record>(records);
    }

    public void ReplayModification(WorkspaceFull workspace) {
        // TEST
        //var fatSize = log.Count;
        //var trimLog = Prune(log).ToList();
        //var trimSize = trimLog.Count;
        //if (fatSize != trimSize) {
        //    Debug.Log($"Trimmed {fatSize} records to {trimSize} records");
        //} else {
        //    Debug.Log($"No trim on {fatSize} records");
        //}
        workspace.ApplyModificationBatchRecord(log);
        //workspace.ApplyModificationBatchRecord(trimLog);
    }

    public void ReplayDirect(Workspace workspace) {
        foreach (var record in log) {
            record.Apply(workspace);
        }
    }

    public bool Empty => log.Count == 0;
}

public sealed class UndoLog : Log {
    public interface IBatchFrame : IDisposable {

    }

    private class BatchFrame : IBatchFrame {
        private UndoLog log;

        public BatchFrame(UndoLog log) {
            Assert.NotNull(log);
            this.log = log;
        }

        public void Dispose() {
            if (log != null) {
                log.EndBatchFrame();
                log = null;
            }
        }
    }

    private Workspace workspace;
    
    private List<Record> log;
    private List<Record> invertLog;
    private List<int> batchEnds;
    private Option<int> redoBatchPosition;
    private Option<Action> onChange;

    private bool isInUndo;
    private int batchFrameDepth;
    private bool hasPendingChanges;

    public UndoLog(Workspace workspace, Option<Action> onChange = default) {
        this.workspace = workspace;
        log = new List<Record>();
        invertLog = new List<Record>();
        batchEnds = new List<int>();
        this.onChange = onChange;
        hasPendingChanges = true;
    }

    public bool HasPendingChanges => hasPendingChanges;

    private void SetPendingChangesFlag() {
        hasPendingChanges = true;
        foreach (var onChange in onChange) {
            onChange();
        }
    }
    public void ClearPendingChangesFlag() => hasPendingChanges = false;

    public bool IsInBatchFrame => batchFrameDepth > 0;

    public Option<TargetInfo> AddAndApply(Record record) {
        if (!isInUndo) {
            ClearRedo();
            var targetInfo = record.GetTargetInfo(workspace);

            var invertRecord = record.Apply(workspace);
            log.Add(record);
            invertLog.Add(invertRecord);
            if (!IsInBatchFrame) {
                EndBatch();
            }
            return targetInfo;
        }
        return Option.None;
    }

    private void ClearRedo() {
        foreach (var redoBatchPosition in redoBatchPosition) {
            if (redoBatchPosition == 0) {
                log.Clear();
                invertLog.Clear();
            } else {
                int remainingCount = batchEnds[redoBatchPosition - 1];
                log.RemoveRange(remainingCount, log.Count - remainingCount);
                invertLog.RemoveRange(remainingCount, invertLog.Count - remainingCount);
            }
            batchEnds.RemoveRange(redoBatchPosition, batchEnds.Count - redoBatchPosition);
            this.redoBatchPosition = Option.None;
        }
    }

    private void EndBatchFrame() {
        Assert.False(CanRedo);
        Assert.True(IsInBatchFrame);
        if (--batchFrameDepth == 0) {
            EndBatch();
        }
    }

    public IBatchFrame NewBatchFrame() {
        ++batchFrameDepth;
        return new BatchFrame(this);
    }

    public void Clear() {
        Assert.False(IsInBatchFrame);
        log.Clear();
        invertLog.Clear();
        batchEnds.Clear();
        redoBatchPosition = Option.None;
        SetPendingChangesFlag();
    }

    private void EndBatch() {
        // Don't add empty batches
        if (log.Count > 0 && (batchEnds.Count == 0 || batchEnds[batchEnds.Count - 1] < log.Count)) {
            batchEnds.Add(log.Count);
            SetPendingChangesFlag();
        }
    }

    public void UndoAll() {
        while (CanUndo) {
            Undo();
        }
    }

    public IEnumerable<CostType> Undo() => Undo(out var _, out var _);

    public IEnumerable<CostType> Undo(out int energy, out IEnumerable<TargetInfo> targetInfos) {
        Assert.False(IsInBatchFrame);
        var costs = new List<CostType>();
        var targetInfosList = new List<TargetInfo>();
        energy = 0;

        if (CanUndo) {
            Assert.False(isInUndo);
            isInUndo = true;
            try {
                int redoBatchPosition = this.redoBatchPosition.ValueOr(batchEnds.Count) - 1;
                int startUndoPos = batchEnds[redoBatchPosition] - 1;
                int endUndoPos;
                if (redoBatchPosition == 0) {
                    endUndoPos = 0;
                } else {
                    endUndoPos = batchEnds[redoBatchPosition - 1];
                }
                this.redoBatchPosition = redoBatchPosition;

                foreach (var i in Utility.CountTo(startUndoPos, endUndoPos)) {
                    costs.AddRange(invertLog[i].GetCosts(workspace));
                    var origLog = invertLog[i].Apply(workspace, invertOnly: true);
                    invertLog[i].Apply(workspace);

                    foreach (var targetInfo in origLog.GetTargetInfo(workspace)) {
                        targetInfosList.Add(targetInfo);
                    }
                    energy += origLog.GetEnergy(workspace);
                }

                SetPendingChangesFlag();
            } finally {
                isInUndo = false;
            }
        }

        targetInfos = targetInfosList;
        return costs.Reverse<CostType>();
    }

    public IEnumerable<CostType> Redo() {
        Assert.False(IsInBatchFrame);
        var costs = new List<CostType>();

        foreach (var redoBatchPosition in redoBatchPosition) {
            Assert.False(isInUndo);
            isInUndo = true;
            try {
                int oldIndex;
                if (redoBatchPosition == 0) {
                    oldIndex = 0;
                } else {
                    oldIndex = batchEnds[redoBatchPosition - 1];
                }

                int newIndex = batchEnds[redoBatchPosition] - 1;
                if (redoBatchPosition == batchEnds.Count - 1) {
                    // This is the last redo until we have redone all undo
                    this.redoBatchPosition = Option.None;
                } else {
                    this.redoBatchPosition = redoBatchPosition + 1;
                }

                foreach (var i in Utility.CountTo(oldIndex, newIndex)) {
                    costs.AddRange(log[i].GetCosts(workspace));
                    log[i].Apply(workspace);
                }

                SetPendingChangesFlag();
            } finally {
                isInUndo = false;
            }
        }
        return costs;
    }

    public bool CanUndo => redoBatchPosition.ValueOr(log.Count) > 0;

    public bool CanRedo => redoBatchPosition.HasValue;


    public ReplayLog ToReplayLog() {
        int count;
        if (this.redoBatchPosition.TryGetValue(out int redoBatchPosition)) {
            if (redoBatchPosition == 0) {
                count = 0;
            } else {
                count = batchEnds[redoBatchPosition - 1];
            }
        } else {
            count = log.Count;
        }

        return new ReplayLog(log.Take(count));
    }
}
