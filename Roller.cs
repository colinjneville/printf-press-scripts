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

public abstract partial class Roller {
    private Guid id;
    private Cryptex cryptex;
    private ColorId color;
    private List<Frame> frames;
    protected int moveOffset;
    protected int hopOffset;
    protected LockType locked;

    protected Roller(Guid id, ColorId color, IEnumerable<Frame> frames, LockType locked) {
        this.id = id;
        this.color = color;
        this.frames = frames.ToList();
        this.locked = locked;
    }

    public abstract IEnumerable<TargetInfo> Step(ExecutionContext ec);

    public virtual IEnumerable<TargetInfo> CheckStep(ExecutionContext ec) => Array.Empty<TargetInfo>();

    public ColorId Color => color;

    public Guid Id => id;
    public int TapeIndex => hopOffset;
    public int Offset => moveOffset;

    public Cryptex Cryptex => cryptex;

    public abstract bool IsPrimary { get; }

    public abstract Option<InstructionRoller> Primary { get; }
    public abstract Option<ProgrammableRoller> Secondary { get; }

    public LockType Lock => locked;

    public void SetCryptex(Cryptex cryptex) {
        this.cryptex = cryptex;
        foreach (var view in View) {
            view.OnUpdateCryptex();
        }
    }

    public IReadOnlyList<Frame> Frames => frames;

    public Record ChangeColor(ColorId color) {
        return new ChangeColorRecord(Cryptex.Id, Id, color);
    }
    private void DoChangeColor(ColorId color) {
        this.color = color;
        foreach (var view in View) {
            view.OnUpdateColor();
        }
    }

    public Record AddFrame(int index, Frame.Serial frame) {
        RtlAssert.Within(index, 0, frames.Count + 1);
        return new AddFrameRecord(Cryptex.Id, Id, index, frame);
    }
    private void DoAddFrame(int index, Frame.Serial frame) {
        var instance = frame.Deserialize(Cryptex.Workspace);
        frames.Insert(index, instance);
        
        foreach (var view in View) {
            view.OnAddFrame(index, instance);
        }
    }

    public Record RemoveFrame(Frame frame) {
        foreach (var index in GetFrameIndex(frame)) {
            return RemoveFrame(index);
        }
        throw RtlAssert.NotReached();
    }
    public Record RemoveFrame(int index) {
        RtlAssert.Within(index, 0, frames.Count);
        return new RemoveFrameRecord(Cryptex.Id, Id, index);
    }
    private void DoRemoveFrame(int index) {
        var frame = frames[index];
        frames.RemoveAt(index);

        foreach (var view in View) {
            view.OnRemoveFrame(index, frame);
        }
    }

    public Record ModifyFrame(int index, FrameFlags flags) {
        RtlAssert.Within(index, 0, frames.Count);
        return new ModifyFrameRecord(Cryptex.Id, Id, index, flags);
    }
    private void DoModifyFrame(int index, FrameFlags flags) {
        var frame = frames[index];
        frame.Flags = flags;
    }

    public Option<int> GetFrameIndex(Frame frame) {
        for (int i = 0; i < frames.Count; ++i) {
            if (frame == frames[i]) {
                return i;
            }
        }
        return Option.None;
    }

    public Frame GetFrame(int index) {
        if (index >= frames.Count) {
            throw new ExecutionException(LC.Temp(String.Format("Roller does not have Part with index '{0}'", index)));
        }
        return frames[index];
    }

    public Tape GetTape(int frameIndex) {
        // TODO should this be an error for all cases?
        if (hopOffset + frameIndex >= Cryptex.Tapes.Count) {
            throw new ExecutionException(LC.Temp(String.Format("Roller part '{0}' is not on a Tape", frameIndex)));
        }
        return Cryptex.Tapes[hopOffset + frameIndex];
    }

    public TapeValue Read(int frameIndex, Option<ExecutionContext> ec = default) {
        if (!GetFrame(frameIndex).AllowRead) {
            throw new ExecutionException(LC.Temp(String.Format("Roller part '{0}' cannot Read", frameIndex)));
        }

        var tape = GetTape(frameIndex);
        if (tape.HasBreakpointRelative(moveOffset)) {
            foreach (var ecValue in ec) {
                ecValue.SetBreakFlag();
            }
        }
        return tape.Read(moveOffset);
    }

    public Record Write(int frameIndex, TapeValue writeValue) {
        if (!GetFrame(frameIndex).AllowWrite) {
            throw new ExecutionException(LC.Temp($"Roller part '{frameIndex}' cannot Write"));
        }
        return GetTape(frameIndex).Write(moveOffset, writeValue);
    }

    public MoveUpRecord MoveUp(int count) {
        return new MoveUpRecord(Cryptex.Id, Id, Mathf.Min(count, hopOffset));
    }
    private void DoMoveUp(int count) {
        hopOffset -= count;

        foreach (var view in View) {
            view.OnUpdateHopOffset();
        }
    }

    public MoveDownRecord MoveDown(int count) {
        return new MoveDownRecord(Cryptex.Id, Id, Mathf.Min(count, Cryptex.Tapes.Count - hopOffset - frames.Count));
    }
    private void DoMoveDown(int count) {
        hopOffset += count;

        foreach (var view in View) {
            view.OnUpdateHopOffset();
        }
    }

    public Record ShiftLeft(int frameIndex, int count) {
        if (!GetFrame(frameIndex).AllowShift) {
            throw new ExecutionException(LC.Temp("Roller part cannot shift"));
        }
        return GetTape(frameIndex).ShiftLeft(count, this);
    }

    public Record ShiftRight(int frameIndex, int count) {
        if (!GetFrame(frameIndex).AllowShift) {
            throw new ExecutionException(LC.Temp("Roller part cannot shift"));
        }
        return GetTape(frameIndex).ShiftRight(count, this);
    }

    public MoveLeftRecord MoveLeft(int count, Option<TargetInfo.ActionType> action = default) {
        //if (locked) {
        //    throw new ExecutionException(LC.Temp("Roller is locked"));
        //}
        return new MoveLeftRecord(Cryptex.Id, Id, count, action);
    }
    private void DoMoveLeft(int count) {
        moveOffset -= count;

        foreach (var view in View) {
            view.OnUpdateMoveOffset();
        }
    }

    public MoveRightRecord MoveRight(int count, Option<TargetInfo.ActionType> action = default) {
        //if (locked) {
        //    throw new ExecutionException(LC.Temp("Roller is locked"));
        //}
        return new MoveRightRecord(Cryptex.Id, Id, count, action);
    }
    private void DoMoveRight(int count) {
        moveOffset += count;

        foreach (var view in View) {
            view.OnUpdateMoveOffset();
        }
    }

    public virtual void SetSkipFlag() { throw new NotSupportedException(); }

    public abstract class BaseRecord : Cryptex.BaseRecord {
        private Guid rollerId;

        protected BaseRecord(Guid cryptexId, Guid rollerId) : base(cryptexId) {
            this.rollerId = rollerId;
        }

        public Guid RollerId => rollerId;

        protected Roller GetRoller(Workspace workspace, Cryptex cryptex) {
            Roller roller;
            if (!cryptex.GetRoller(RollerId).TryGetValue(out roller)) {
                throw new DataException(LC.Temp($"No Roller with id {RollerId}"));
            }
            return roller;
        }

        public static IEnumerable<CostType> GetFrameCosts(FrameFlags flags) {
            var cost = CostType.Frame.Yield();
            if (flags.HasFlag(FrameFlags.CanRead)) {
                cost = cost.Append(CostType.FrameCanRead);
            }
            if (flags.HasFlag(FrameFlags.CanWrite)) {
                cost = cost.Append(CostType.FrameCanWrite);
            }
            if (flags.HasFlag(FrameFlags.CanShift)) {
                cost = cost.Append(CostType.FrameCanShift);
            }
            return cost;
        }
    }

    public sealed class ChangeColorRecord : BaseRecord {
        private ColorId color;

        public ChangeColorRecord(Guid cryptexId, Guid rollerId, ColorId color) : base(cryptexId, rollerId) {
            this.color = color;
        }

        public ColorId Color => color;

        public override Record Apply(Workspace workspace, bool invertOnly = false) {
            var cryptex = GetCryptex(workspace);
            var roller = GetRoller(workspace, cryptex);
            var oldColor = roller.Color;
            if (!invertOnly) {
                roller.DoChangeColor(Color);
            }
            return new ChangeColorRecord(CryptexId, RollerId, oldColor);
        }

        public override IEnumerable<CostType> GetCosts(Workspace workspace) => Array.Empty<CostType>();
    }

    public abstract class MoveRecord : BaseRecord {
        private int count;
        private TargetInfo.ActionType action;

        protected MoveRecord(Guid cryptexId, Guid rollerId, int count, Option<TargetInfo.ActionType> action) : base(cryptexId, rollerId) {
            this.count = count;
            this.action = action.ValueOrDefault;
        }

        public int Count => count;

        public Option<TargetInfo.ActionType> Action => action == default ? Option.None : action.ToOption();

        public override Record Apply(Workspace workspace, bool invertOnly = false) {
            var cryptex = GetCryptex(workspace);
            var roller = GetRoller(workspace, cryptex);
            if (!invertOnly) {
                DoMove(roller);
            }
            return MakeLog();
        }

        public override IEnumerable<CostType> GetCosts(Workspace workspace) => Array.Empty<CostType>();

        public override int GetEnergy(Workspace workspace) => Mathf.Abs(Count);

        public override Option<TargetInfo> GetTargetInfo(Workspace workspace) {
            foreach (var action in Action) {
                var cryptex = GetCryptex(workspace);
                var roller = GetRoller(workspace, cryptex);
                var offset = Offset;
                Assert.True(offset.x == 0 || offset.y == 0);
                if (offset.x == 0 && offset.y == 0) {
                    return Option.None;
                }
                // Jumps need their source to be one more to the left because this record is separate from the AdvanceAllRecord
                int advanceAllCompensation = action == TargetInfo.ActionType.Jump ? 1 : 0;

                // HACK
                var actionType = action;
                if (advanceAllCompensation != 0 && advanceAllCompensation == -offset.x) {
                    actionType = TargetInfo.ActionType.Stall;
                }
                return new TargetInfo(roller, roller.moveOffset, roller.hopOffset, roller.moveOffset + offset.x + advanceAllCompensation, roller.hopOffset + offset.y, actionType);
            }
            return Option.None;
        }

        protected abstract Vector2Int Offset { get; }

        protected abstract void DoMove(Roller roller);
        protected abstract Record MakeLog();
    }

    public sealed class MoveLeftRecord : MoveRecord {
        public MoveLeftRecord(Guid cryptexId, Guid rollerId, int count, Option<TargetInfo.ActionType> action) : base(cryptexId, rollerId, count, action) { }

        protected override Vector2Int Offset => new Vector2Int(-Count, 0);

        protected override void DoMove(Roller roller) => roller.DoMoveLeft(Count);
        protected override Record MakeLog() => new MoveRightRecord(CryptexId, RollerId, Count, Action);
    }

    public sealed class MoveRightRecord : MoveRecord {
        public MoveRightRecord(Guid cryptexId, Guid rollerId, int count, Option<TargetInfo.ActionType> action) : base(cryptexId, rollerId, count, action) { }

        protected override Vector2Int Offset => new Vector2Int(Count, 0);

        protected override void DoMove(Roller roller) => roller.DoMoveRight(Count);
        protected override Record MakeLog() => new MoveLeftRecord(CryptexId, RollerId, Count, Action);
    }

    public sealed class MoveUpRecord : MoveRecord {
        public MoveUpRecord(Guid cryptexId, Guid rollerId, int count) : base(cryptexId, rollerId, count, TargetInfo.ActionType.Hop) { }

        protected override Vector2Int Offset => new Vector2Int(0, -Count);

        protected override void DoMove(Roller roller) => roller.DoMoveUp(Count);
        protected override Record MakeLog() => new MoveDownRecord(CryptexId, RollerId, Count);
    }

    public sealed class MoveDownRecord : MoveRecord {
        public MoveDownRecord(Guid cryptexId, Guid rollerId, int count) : base(cryptexId, rollerId, count, TargetInfo.ActionType.Hop) { }

        protected override Vector2Int Offset => new Vector2Int(0, Count);

        protected override void DoMove(Roller roller) => roller.DoMoveDown(Count);
        protected override Record MakeLog() => new MoveUpRecord(CryptexId, RollerId, Count);
    }

    public sealed class AddFrameRecord : BaseRecord {
        private int index;
        private Frame.Serial frame;

        public AddFrameRecord(Guid cryptexId, Guid rollerId, int index, Frame.Serial frame) : base(cryptexId, rollerId) {
            this.index = index;
            this.frame = frame;
        }

        public override IEnumerable<CostType> GetCosts(Workspace workspace) => GetFrameCosts(frame.Flags);

        public override Record Apply(Workspace workspace, bool invertOnly = false) {
            var cryptex = GetCryptex(workspace);
            var roller = GetRoller(workspace, cryptex);
            if (!invertOnly) {
                roller.DoAddFrame(index, frame);
            }
            return new RemoveFrameRecord(CryptexId, RollerId, index);
        }
    }

    public sealed class RemoveFrameRecord : BaseRecord {
        private int index;

        public RemoveFrameRecord(Guid cryptexId, Guid rollerId, int index) : base(cryptexId, rollerId) {
            this.index = index;
        }

        public override IEnumerable<CostType> GetCosts(Workspace workspace) {
            var cryptex = GetCryptex(workspace);
            var roller = GetRoller(workspace, cryptex);
            var frame = roller.Frames[index];
            return GetFrameCosts(frame.Flags).Select(c => ~c);
        }

        public override Record Apply(Workspace workspace, bool invertOnly = false) {
            var cryptex = GetCryptex(workspace);
            var roller = GetRoller(workspace, cryptex);
            var frame = roller.Frames[index];
            if (!invertOnly) {
                roller.DoRemoveFrame(index);
            }
            return new AddFrameRecord(CryptexId, RollerId, index, frame);
        }
    }

    public sealed class ModifyFrameRecord : BaseRecord {
        private int index;
        private FrameFlags flags;

        public ModifyFrameRecord(Guid cryptexId, Guid rollerId, int index, FrameFlags flags) : base(cryptexId, rollerId) {
            this.index = index;
            this.flags = flags;
        }

        public override IEnumerable<CostType> GetCosts(Workspace workspace) {
            var cryptex = GetCryptex(workspace);
            var roller = GetRoller(workspace, cryptex);
            var frame = roller.Frames[index];
            return GetFrameCosts(flags).Concat(GetFrameCosts(frame.Flags).Select(c => ~c));
        }

        public override Record Apply(Workspace workspace, bool invertOnly = false) {
            var cryptex = GetCryptex(workspace);
            var roller = GetRoller(workspace, cryptex);
            var frame = roller.Frames[index];
            var oldFlags = frame.Flags;
            if (!invertOnly) {
                roller.DoModifyFrame(index, flags);
            }
            return new ModifyFrameRecord(CryptexId, RollerId, index, oldFlags);
        }
    }
}

public sealed class InstructionRoller : Roller {
    public InstructionRoller(Guid id, ColorId color, IEnumerable<Frame> frames, LockType locked = default) : base(id, color, frames, locked) { }

    public override bool IsPrimary => true;

    public override Option<InstructionRoller> Primary => this;

    public override Option<ProgrammableRoller> Secondary => Cryptex.Workspace.GetSecondaryRoller(Color);

    public override Serial Serialize() {
        return Serial.Instruction(Id, Color, Frames, moveOffset, hopOffset);
    }

    public override IEnumerable<TargetInfo> Step(ExecutionContext ec) {
        var targetInfos = Enumerable.Empty<TargetInfo>();
        if (IsOnCut(ec)) {
            // TODO compensate for +2 energy
            targetInfos = TargetInfo.Invalid(this, Offset, TapeIndex).Yield();
            ec.ExecuteRecord(MoveLeft(1));
            return targetInfos;
        }

        foreach (var statement in GetStatement(ec)) {
            var values = GetArguments(ec);
            targetInfos = targetInfos.Concat(statement.Execute(ec, this, values));
        }
        return targetInfos.ToImmutableList();
    }

    public override IEnumerable<TargetInfo> CheckStep(ExecutionContext ec) {
        if (IsOnCut(ec)) {
            return TargetInfo.Invalid(this, Offset, TapeIndex).Yield();
        }

        foreach (var statement in GetStatement(ec)) {
            var values = GetArguments(ec);
            return statement.CheckExecute(ec, this, values).ToImmutableList();
        }
        return Array.Empty<TargetInfo>();
    }

    private bool IsOnCut(ExecutionContext ec) {
        for (int i = 0; i < Frames.Count; ++i) {
            if (Read(i).IsCut) {
                return true;
            }
        }
        return false;
    }

    private Option<Statement> GetStatement(ExecutionContext ec) {
        var op = Read(0, ec);
        if (op.Type == TapeValueType.Opcode) {
            var opString = op.To<string>();
            return Statement.Lookup(opString);
        }
        op = Read(Frames.Count - 1, ec);
        if (op.Type == TapeValueType.Opcode) {
            var opString = op.To<string>();
            return Statement.Lookup(opString);
        }

        return Option.None;
    }

    private IReadOnlyList<TapeValue> GetArguments(ExecutionContext ec) {
        var values = new List<TapeValue>();
        for (int i = 0; i < Frames.Count; ++i) {
            var value = Read(i, ec);
            if (value.Type != TapeValueType.Opcode) {
                values.Add(value);
            }
        }
        return values;
    }
}

public sealed class ProgrammableRoller : Roller {
    public ProgrammableRoller(Guid id, ColorId color, IEnumerable<Frame> frames, LockType locked = default) : base(id, color, frames, locked) { }

    public override bool IsPrimary => false;

    public override Option<InstructionRoller> Primary => Cryptex.Workspace.GetPrimaryRoller(Color);

    public override Option<ProgrammableRoller> Secondary => this;

    public override Serial Serialize() {
        return Serial.Programmable(Id, Color, Frames, moveOffset, hopOffset);
    }

    public override IEnumerable<TargetInfo> Step(ExecutionContext ec) => Array.Empty<TargetInfo>();
}
