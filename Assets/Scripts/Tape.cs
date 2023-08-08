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

public sealed partial class Tape {
    public enum SequenceType {
        // TapeValueNull
        Blank,
        // TapeValueInt
        Integers,
        // TapeValueColor
        Colors,
        ColorsLoop,
        // TapeValueChar
        Alphabet,
        // Mixed

        // Custom
        Custom = -1,
    }

    private Guid id;
    private Cryptex cryptex;
    private SequenceType sequence;
    private ImmutableList<TapeValue> customValues;
    private bool customLoop;
    private Dictionary<int, TapeValue> overwrites;
    private Dictionary<int, Note> notes;
    private HashSet<int> breakpoints;
    private LockType locked;

    private int shiftOffset;

    private Tape(Guid id, LockType locked) {
        this.id = id;
        this.locked = locked;
        overwrites = new Dictionary<int, TapeValue>();
        notes = new Dictionary<int, Note>();
        breakpoints = new HashSet<int>();
    }

    public Tape(Guid id, bool customLoop, IEnumerable<TapeValue> sequence, LockType locked = default) : this(id, locked) {
        this.sequence = SequenceType.Custom;
        this.customLoop = customLoop;
        customValues = ImmutableList.CreateRange(sequence);
    }

    public Tape(Guid id, SequenceType sequence, LockType locked = default) : this(id, locked) {
        this.sequence = sequence;
        customValues = ImmutableList<TapeValue>.Empty;
    }

    public Cryptex Cryptex => cryptex;
    public Guid Id => id;

    public LockType Lock => locked;

    public int ShiftOffset {
        get => shiftOffset;
        private set => SetShiftOffset(value, animate: true);
    }

    public void SetShiftOffset(int offset) => SetShiftOffset(offset, animate: false);

    private void SetShiftOffset(int offset, bool animate) {
        var old = shiftOffset;
        shiftOffset = offset;
        foreach (var view in View) {
            view.OnUpdateOffset(old, offset, animate);
        }
    }

    public void SetCryptex(Cryptex cryptex) {
        this.cryptex = cryptex;
    }

    public SequenceType Sequence => sequence;

    public IImmutableList<TapeValue> CustomValues => customValues;
    public bool CustomLoop => customLoop;

    public Option<Note> Note(int index) => notes.GetOrNone(index);

    public bool HasBreakpointRelative(int index) => HasBreakpoint(index - ShiftOffset);
    public bool HasBreakpoint(int index) => breakpoints.Contains(index);

    public Record Write(int index, TapeValue value) {
        return new WriteRecord(cryptex.Id, Id, index, value);
    }
    private void DoWrite(int index, TapeValue value) {
        index -= ShiftOffset;

        if (value is null || GetSequenceValue(index) == value) {
            overwrites.Remove(index);
        } else {
            overwrites[index] = value;
        }
        foreach (var viewValue in View) {
            viewValue.UpdateTapeValue(index, value);
        }
    }
    public Record Unwrite(int index) {
        return new WriteRecord(cryptex.Id, Id, index, GetSequenceValue(index - ShiftOffset));
    }

    public Record AddNote(int index, LE text) => new AddNoteRecord(cryptex.Id, Id, index, text);
    public Record RemoveNote(int index) => new RemoveNoteRecord(cryptex.Id, Id, index);

    public Record ShiftLeft(int count, Option<Roller> roller = default) {
        return new ShiftLeftRecord(cryptex.Id, Id, roller.Select(r => r.Id).ValueOrDefault, count);
    }
    private void DoShiftLeft(int count) {
        ShiftOffset -= count;
    }

    public Record ShiftRight(int count, Option<Roller> roller = default) {
        return new ShiftRightRecord(cryptex.Id, Id, roller.Select(r => r.Id).ValueOrDefault, count);
    }
    private void DoShiftRight(int count) {
        ShiftOffset += count;
    }

    public void InsertValue(int index, Option<TapeValue> insertedValue) {
        index += ShiftOffset;
        var view = View.ValueOrDefault;

        notes = notes.ToDictionary(kvp => kvp.Key + (kvp.Key >= index ? 1 : 0), kvp => kvp.Value);
        foreach (var kvp in notes) {
            if (kvp.Key >= index) {
                view?.OnUpdateNote(kvp.Key);
            }
        }


        breakpoints = new HashSet<int>(breakpoints.Select(i => i + (i >= index ? 1 : 0)));
        foreach (var i in breakpoints) {
            if (i >= index) {
                view?.OnUpdateBreakpoint(i);
            }
        }

        var changes = new List<int>(overwrites.Count);
        changes.AddRange(overwrites.Keys.Where(k => k >= index).OrderByDescending(k => k));
        foreach (var change in changes) {
            var value = overwrites[change];
            overwrites.Remove(change);
            if (GetSequenceValue(change + 1) != value) {
                overwrites.Add(change + 1, value);
            }
            
            if (!overwrites.ContainsKey(change - 1)) {
                view?.UpdateTapeValue(change, GetSequenceValue(change));
            }
            // Do this even if we didn't need the overwrite because if there was a colocated Remove, it skipped Update expecting us to do it
            view?.UpdateTapeValue(change + 1, value);
        }
        foreach (var insertedValueValue in insertedValue) {
            if (GetSequenceValue(index) != insertedValue) {
                overwrites.Add(index, insertedValueValue);
            }
        }
        view?.UpdateTapeValue(index, Read(index));
    }

    public void RemoveValue(int index) {
        index += ShiftOffset;
        var view = View.ValueOrDefault;

        if (notes.ContainsKey(index)) {
            notes.Remove(index);
        }
        notes = notes.ToDictionary(kvp => kvp.Key - (kvp.Key >= index ? 1 : 0), kvp => kvp.Value);
        foreach (var kvp in notes) {
            if (kvp.Key >= index) {
                view?.OnUpdateNote(kvp.Key);
            }
        }

        breakpoints.Remove(index);
        breakpoints = new HashSet<int>(breakpoints.Select(i => i - (i >= index ? 1 : 0)));
        foreach (var i in breakpoints) {
            if (i >= index) {
                view?.OnUpdateBreakpoint(i);
            }
        }

        // Remove the overwrite at the index if it exists
        overwrites.Remove(index);
        view?.UpdateTapeValue(index, GetSequenceValue(index));

        var changes = new List<int>(overwrites.Count);
        changes.AddRange(overwrites.Keys.Where(k => k > index).OrderBy(k => k));
        foreach (var change in changes) {
            var value = overwrites[change];
            overwrites.Remove(change);
            if (GetSequenceValue(change - 1) != value) {
                overwrites.Add(change - 1, value);
            }

            if (!overwrites.ContainsKey(change + 1)) {
                view?.UpdateTapeValue(change, GetSequenceValue(change));
            }

            // Do this even if we didn't need the overwrite because if there was a colocated Remove, it skipped Update expecting us to do it
            view?.UpdateTapeValue(change - 1, value);
        }
    }

    public TapeValue Read(int offset) {
        return GetValue(offset - ShiftOffset);
    }

    private TapeValue GetValue(int index) {
        TapeValue value;
        if (!overwrites.TryGetValue(index, out value)) {
            value = GetSequenceValue(index);
        }
        return value;
    }

    public bool IsModified(int index) {
        return GetSequenceValue(index) != Read(index);
    }

    public void ToggleBreakpoint(int index) {
        if (breakpoints.Contains(index)) {
            breakpoints.Remove(index);
        } else {
            breakpoints.Add(index);
        }

        foreach (var view in View) {
            view.OnUpdateBreakpoint(index);
        }
    }

    private TapeValue GetBlankValue(int index) {
        int cutFrequency = Cryptex is object ? Cryptex.Workspace.Level.CutFrequency : 0;
        if (cutFrequency == 0) {
            return TapeValueNull.Instance;
        }
        if (Utility.Mod(index + (cutFrequency + 1) / 2, cutFrequency + 1) == 0) {
            return TapeValueCut.Instance;
        } else {
            return TapeValueNull.Instance;
        }
    }

    public TapeValue GetSequenceValue(int index) {
        switch (Sequence) {
            case SequenceType.Blank:
                //return TapeValueNull.Instance;
                return GetBlankValue(index);
            case SequenceType.Integers:
                return new TapeValueInt(index);
            case SequenceType.Colors:
                if (index >= 0 && index < (int)ColorId.Count) {
                    return new TapeValueColor((ColorId)index);
                } else {
                    return TapeValueNull.Instance;
                }
            case SequenceType.ColorsLoop:
                var i = index % (int)ColorId.Count;
                if (i < 0) {
                    i += (int)ColorId.Count;
                }
                return new TapeValueColor((ColorId)i);
            case SequenceType.Alphabet:
                if (index >= 0 && index < 26) {
                    return new TapeValueChar((char)('a' + index));
                }
                return TapeValueNull.Instance;
            case SequenceType.Custom:
                if (!customLoop && (index < 0 || index >= customValues.Count)) {
                    return TapeValueNull.Instance;
                } else {
                    int j = index % customValues.Count;
                    if (j < 0) {
                        j += customValues.Count;
                    }
                    return customValues[j];
                }
            default:
                throw RtlAssert.NotReached();
        }
    }

    private static IEnumerable<CostType> GetTapeValueTypeCost(TapeValueType type) {
        var cost = CostType.Write.Yield();
        cost = cost.Append(type.CostType);
        return cost;
    }

    public abstract class BaseRecord : Cryptex.BaseRecord {
        private Guid tapeId;

        public BaseRecord(Guid cryptexId, Guid tapeId) : base(cryptexId) {
            this.tapeId = tapeId;
        }

        public Guid TapeId => tapeId;

        protected RecordObjectId TapeRecordId => new RecordObjectId(TapeId);

        protected Tape GetTape(Workspace workspace, Cryptex cryptex) {
            Tape tape;
            if (!cryptex.GetTape(TapeId).TryGetValue(out tape)) {
                throw new DataException(LC.Temp($"No Tape with id {TapeId}"));
            }
            return tape;
        }

        public static IEnumerable<CostType> GetTapeCosts(Workspace workspace, Tape.Serial tape) {
            var cost = CostType.Tape.Yield();
            if (tape.SequenceType != SequenceType.Blank) {
                cost = cost.Append(CostType.TapeSequence);
            }
            if (tape.Writes is object) {
                foreach (var write in tape.Writes) {
                    cost = cost.Concat(GetTapeValueTypeCost(write.Value.Deserialize(workspace).Type));
                }
            }
            return cost;
        }
    }

    public sealed class WriteRecord : BaseRecord {
        private int index;
        private TapeValue.Serial value;

        public WriteRecord(Guid cryptexId, Guid tapeId, int index, TapeValue.Serial value) : base(cryptexId, tapeId) {
            this.index = index;
            this.value = value;
        }

        public static WriteRecord Clear(Guid cryptexId, Guid tapeId, int index) => new WriteRecord(cryptexId, tapeId, index, default);

        public override IEnumerable<CostType> GetCosts(Workspace workspace) {
            var cryptex = GetCryptex(workspace);
            var tape = GetTape(workspace, cryptex);
            var oldValue = tape.Read(Index);
            var newValue = GetValue(workspace);
            var sequenceValue = tape.GetSequenceValue(index);

            var cost = Enumerable.Empty<CostType>();
            if (oldValue != sequenceValue) {
                // Refund the previous write
                cost = cost.Concat(GetTapeValueTypeCost(oldValue.Type).Select(c => ~c));
            }
            if (newValue != sequenceValue) {
                // Charge for the new write
                cost = cost.Concat(GetTapeValueTypeCost(newValue.Type));
            }
            return cost;
        }

        public int Index => index;
        private TapeValue GetValue(Workspace workspace) {
            if (value == default) {
                var cryptex = GetCryptex(workspace);
                var tape = GetTape(workspace, cryptex);
                return tape.GetSequenceValue(index);
            }
            return value.Deserialize(workspace);
        }

        public override IEnumerable<RecordObjectId> RequiredIds => new[] { CryptexRecordId, new RecordObjectId(TapeId, Index) };

        public override Record Apply(Workspace workspace, bool invertOnly = false) {
            var cryptex = GetCryptex(workspace);
            var tape = GetTape(workspace, cryptex);
            // TODO needs lock check only when running?
            //if (tape.Lock.HasFlag(LockType.Modify)) {
            //    throw new ValidationException(LC.Temp("Lock violation"));
            //}

            var oldValue = tape.Read(Index);
            if (!invertOnly) {
                if (!oldValue.IsCut) {
                    tape.DoWrite(Index, GetValue(workspace));
                }
            }
            return new WriteRecord(CryptexId, TapeId, Index, oldValue);
        }
    }

    public abstract class ShiftRecord : BaseRecord {
        private Guid rollerId;
        private int count;
        protected ShiftRecord(Guid cryptexId, Guid tapeId, Guid rollerId, int count) : base(cryptexId, tapeId) {
            this.rollerId = rollerId;
            this.count = count;
        }
        public int Count => count;
        protected abstract int SignedCount { get; }

        public Guid RollerId => rollerId;

        public override Record Apply(Workspace workspace, bool invertOnly = false) {
            var cryptex = GetCryptex(workspace);
            var tape = GetTape(workspace, cryptex);
            if (!invertOnly) {
                DoMove(tape);
            }
            return MakeRecord();
        }

        public override int GetEnergy(Workspace workspace) => Mathf.Abs(Count);

        public override IEnumerable<CostType> GetCosts(Workspace workspace) => Array.Empty<CostType>();

        public override IEnumerable<RecordObjectId> RequiredIds => new[] { CryptexRecordId, TapeRecordId };

        public override Option<TargetInfo> GetTargetInfo(Workspace workspace) {
            // No TargetInfo for shifts done as part of setup
            if (rollerId == Guid.Empty) {
                return Option.None;
            }
            var cryptex = GetCryptex(workspace);
            var tape = GetTape(workspace, cryptex);
            var index = cryptex.Tapes.IndexOf(tape);
            var roller = cryptex.GetRoller(rollerId).ValueOrAssert();
            return new TargetInfo(roller, roller.Offset - SignedCount, index, roller.Offset, index, TargetInfo.ActionType.Shift);
        }

        protected abstract void DoMove(Tape tape);

        protected abstract Record MakeRecord();
    }

    public sealed class ShiftLeftRecord : ShiftRecord {
        public ShiftLeftRecord(Guid cryptexId, Guid tapeId, Guid rollerId, int count) : base(cryptexId, tapeId, rollerId, count) { }

        protected override int SignedCount => -Count;
        protected override void DoMove(Tape tape) => tape.DoShiftLeft(Count);
        protected override Record MakeRecord() => new ShiftRightRecord(CryptexId, TapeId, RollerId, Count);
    }

    public sealed class ShiftRightRecord : ShiftRecord {
        public ShiftRightRecord(Guid cryptexId, Guid tapeId, Guid rollerId, int count) : base(cryptexId, tapeId, rollerId, count) { }

        protected override int SignedCount => Count;
        protected override void DoMove(Tape tape) => tape.DoShiftRight(Count);
        protected override Record MakeRecord() => new ShiftLeftRecord(CryptexId, TapeId, RollerId, Count);
    }

    // Add OR overwrite
    public sealed class AddNoteRecord : BaseRecord {
        private int index;
        private LE text;

        public AddNoteRecord(Guid cryptexId, Guid tapeId, int index, LE text) : base(cryptexId, tapeId) {
            this.index = index;
            this.text = text;
        }

        public override Record Apply(Workspace workspace, bool invertOnly = false) {
            var cryptex = GetCryptex(workspace);
            var tape = GetTape(workspace, cryptex);
            Record record;
            if (tape.Note(index).TryGetValue(out Note note)) {
                var oldText = note.Text;
                if (!invertOnly) {
                    note.Text = text;
                }
                record = new AddNoteRecord(CryptexId, TapeId, index, oldText);
            } else {
                if (!invertOnly) {
                    tape.notes.Add(index, new Note(text));
                }
                record = new RemoveNoteRecord(CryptexId, TapeId, index);
            }
            if (!invertOnly) {
                foreach (var view in tape.View) {
                    view.OnUpdateNote(index);
                }
            }

            return record;
        }

        public override IEnumerable<CostType> GetCosts(Workspace workspace) => Array.Empty<CostType>();

        public override IEnumerable<RecordObjectId> RequiredIds => new[] { CryptexRecordId, TapeRecordId };

        public override bool IsExecutionRelevant => false;
    }

    public sealed class RemoveNoteRecord : BaseRecord {
        private int index;

        public RemoveNoteRecord(Guid cryptexId, Guid tapeId, int index) : base(cryptexId, tapeId) {
            this.index = index;
        }

        public override Record Apply(Workspace workspace, bool invertOnly = false) {
            var cryptex = GetCryptex(workspace);
            var tape = GetTape(workspace, cryptex);
            var note = tape.notes[index];
            if (!invertOnly) {
                tape.notes.Remove(index);
                foreach (var view in tape.View) {
                    view.OnUpdateNote(index);
                }
            }

            return new AddNoteRecord(CryptexId, TapeId, index, note.Text);

        }

        public override IEnumerable<CostType> GetCosts(Workspace workspace) => Array.Empty<CostType>();

        public override IEnumerable<RecordObjectId> RequiredIds => new[] { CryptexRecordId, TapeRecordId };

        public override bool IsExecutionRelevant => false;
    }

    public static IEnumerable<Record> WriteRecordBatch(Guid cryptexId, Guid tapeId, int startIndex, params TapeValue[] values) => WriteRecordBatch(cryptexId, tapeId, startIndex, (IEnumerable<TapeValue>)values);

    public static IEnumerable<Record> WriteRecordBatch(Guid cryptexId, Guid tapeId, int startIndex, IEnumerable<TapeValue> values) {
        return values.Select((v, i) => new WriteRecord(cryptexId, tapeId, startIndex + i, v));
    }
}
