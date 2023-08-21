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

public readonly struct RecordObjectId : IEquatable<RecordObjectId> {
    public Guid Id { get; }
    public int? Index { get; }

    public RecordObjectId(Guid id, int? index = default) {
        Id = id;
        Index = index;
    }

    public RecordObjectId Parent => new RecordObjectId(Id);

    public bool Equals(RecordObjectId obj) => Id == obj.Id && Index == obj.Index;

    public override bool Equals(object obj) => obj is RecordObjectId && Equals((RecordObjectId)obj);

    public override int GetHashCode() => Utility.GetCompositeHashCode(Id, Index);

    public override string ToString() => Id.ToString() + (Index.HasValue ? $" @ {Index}" : "");
}

public abstract class Record {
    public abstract Record Apply(Workspace workspace, bool invertOnly = false);

    public abstract IEnumerable<CostType> GetCosts(Workspace workspace);

    public virtual int GetEnergy(Workspace workspace) => 0;

    public virtual IEnumerable<RecordObjectId> AddedIds => Array.Empty<RecordObjectId>();

    public virtual IEnumerable<RecordObjectId> RequiredIds => Array.Empty<RecordObjectId>();
    //public abstract IEnumerable<RecordObjectId> RequiredIds { get; }

    public virtual Option<RecordObjectId> RemovedId => Option.None;

    public virtual Option<TargetInfo> GetTargetInfo(Workspace workspace) => Option.None;

    /// <summary>
    /// Does this record affect execution? Or is it only for adding "meta-data" to the solution?
    /// </summary>
    public virtual bool IsExecutionRelevant => true;

    public static JsonConverter SerializationConverter { get; }

    static Record() {
        int id = 0;
        SerializationConverter = JsonSubtypesConverterBuilder.Of(typeof(Record), "type")
            .RegisterSubtype(typeof(NoopRecord), id++)
            .RegisterSubtype(typeof(Cryptex.AddLabelRecord), id++)
            .RegisterSubtype(typeof(Cryptex.AddRollerRecord), id++)
            .RegisterSubtype(typeof(Cryptex.AddTapeRecord), id++)
            .RegisterSubtype(typeof(Cryptex.RemoveLabelRecord), id++)
            .RegisterSubtype(typeof(Cryptex.RemoveRollerRecord), id++)                              // 5
            .RegisterSubtype(typeof(Cryptex.RemoveTapeRecord), id++)
            .RegisterSubtype(typeof(ExecutionContext.ChannelRecord), id++)
            .RegisterSubtype(typeof(ExecutionContext.AllInstructionRollersRecord), id++) // TODO not needed?
            .RegisterSubtype(typeof(ExecutionContext.ApplyOutputRecord), id++)
            .RegisterSubtype(typeof(ExecutionContext.RollbackOutputRecord), id++)                   // 10
            .RegisterSubtype(typeof(ExecutionContext.OutputRecord), id++)
            .RegisterSubtype(typeof(ExecutionContext.RecedeAllInstructionRollersRecord), id++)
            .RegisterSubtype(typeof(ExecutionContext.AdvanceAllInstructionRollersRecord), id++)
            .RegisterSubtype(typeof(Roller.AddFrameRecord), id++)
            .RegisterSubtype(typeof(Roller.RemoveFrameRecord), id++)                                // 15
            .RegisterSubtype(typeof(Roller.MoveRightRecord), id++)
            .RegisterSubtype(typeof(Roller.MoveUpRecord), id++)
            .RegisterSubtype(typeof(Roller.MoveLeftRecord), id++)
            .RegisterSubtype(typeof(Roller.MoveDownRecord), id++)
            .RegisterSubtype(typeof(Tape.WriteRecord), id++)                                        // 20
            .RegisterSubtype(typeof(Tape.ShiftLeftRecord), id++)
            .RegisterSubtype(typeof(Tape.ShiftRightRecord), id++)
            .RegisterSubtype(typeof(WorkspaceFull.AddCryptexRecord), id++)
            .RegisterSubtype(typeof(WorkspaceFull.RemoveCryptexRecord), id++)
            .RegisterSubtype(typeof(Cryptex.MoveCryptexRecord), id++)                               // 25
            .RegisterSubtype(typeof(Cryptex.RenameLabelRecord), id++)
            .RegisterSubtype(typeof(Roller.ChangeColorRecord), id++)
            .RegisterSubtype(typeof(Cryptex.RotateRecord), id++)
            .RegisterSubtype(typeof(Roller.ModifyFrameRecord), id++)
            .RegisterSubtype(typeof(Tape.AddNoteRecord), id++)                                      // 30
            .RegisterSubtype(typeof(Tape.RemoveNoteRecord), id++)
            .RegisterSubtype(typeof(Cryptex.InsertColumnRecord), id++)
            .RegisterSubtype(typeof(Cryptex.RemoveColumnRecord), id++)
            .RegisterSubtype(typeof(Cryptex.MoveTapeRecord), id++)
        // New Record types must be added to the end!
        .SerializeDiscriminatorProperty(addDiscriminatorFirst: true).Build();
    }
}

public sealed class NoopRecord : Record {
    public override Record Apply(Workspace workspace, bool invertOnly = false) => this;

    public override IEnumerable<CostType> GetCosts(Workspace workspace) => Array.Empty<CostType>();

    public override bool IsExecutionRelevant => false;
}