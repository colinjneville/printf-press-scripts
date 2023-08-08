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


public sealed partial class Cryptex {
    private Guid id;
    private Workspace workspace;
    private List<Tape> tapes;
    private List<Roller> rollers;
    private Dictionary<int, Label> labels;
    private LockType locked;
    private Vector2 xy;
    private bool rotated;

    public Cryptex(Guid id, Workspace workspace, Vector2 xy, LockType locked = default) {
        this.id = id;
        this.workspace = workspace;
        tapes = new List<Tape>();
        rollers = new List<Roller>();
        labels = new Dictionary<int, Label>();
        this.locked = locked;
        this.xy = xy;
        rotated = false;
    }

    public LockType Lock => locked;

    public Vector2 XY {
        get => xy;
        private set {
            xy = value;
            foreach (var view in View) {
                view.OnUpdateXY();
            }
        }
    }

    public bool Rotated => rotated;

    public Option<Tape> GetTape(Guid tapeId) => tapes.SingleOrDefault(t => t.Id == tapeId).ToOption();

    public Option<Roller> GetRoller(Guid rollerId) => rollers.SingleOrDefault(r => r.Id == rollerId).ToOption();

    public Option<Label> GetLabel(int index) => labels.GetOrNone(index);

    public Option<Label> GetLabel(Guid labelId) => labels.FirstOrDefault(kvp => kvp.Value.Id == labelId).Value.ToOption();

    public Option<Label> GetLabel(string name) => labels.FirstOrDefault(kvp => kvp.Value.Name.ToString() == name).Value.ToOption();

    public Option<int> GetLabelIndex(Guid labelId) {
        foreach (var kvp in labels) {
            if (kvp.Value.Id == labelId) {
                return kvp.Key;
            }
        }
        return Option.None;
    }

    public Option<int> GetLabelIndex(string name) {
        foreach (var label in labels) {
            if (label.Value.Name.ToString() == name) {
                return label.Key;
            }
        }
        return Option.None;
    }

    public Record MoveCryptex(Vector2 xy) {
        return new MoveCryptexRecord(Id, xy);
    }
    private void DoMoveCryptex(Vector2 xy) {
        XY = xy;
    }

    public Record AddTape(int index, Tape tape) {
        return new AddTapeRecord(Id, index, tape);
    }
    private void DoAddTape(int index, Tape tape) {
        tape.SetCryptex(this);
        tapes.Insert(index, tape);
        foreach (var view in View) {
            view.OnAddTape(index, tape);
        }
    }

    public Record RemoveTape(Tape tape) {
        return new RemoveTapeRecord(Id, tape.Id);
    }
    private void DoRemoveTape(Tape tape) {
        tapes.Remove(tape);
        foreach (var view in View) {
            view.OnRemoveTape(tape);
        }
    }
    public Record MoveTape(Tape tape, int index, int offset) => new MoveTapeRecord(Id, tape.Id, index, offset);
    
    private void DoMoveTape(Tape tape, int index, int offset) {
        DoRemoveTape(tape);
        tape.SetShiftOffset(offset);
        DoAddTape(index, tape);
    }


    public Record AddRoller(Roller roller) {
        return new AddRollerRecord(Id, roller);
    }
    private void DoAddRoller(Roller roller) {
        roller.SetCryptex(this);
        rollers.Add(roller);
        foreach (var view in View) {
            view.OnAddRoller(roller);
        }
    }

    public Record RemoveRoller(Roller roller) {
        return new RemoveRollerRecord(Id, roller.Id);
    }
    private void DoRemoveRoller(Roller roller) {
        rollers.Remove(roller);
        foreach (var view in View) {
            view.OnRemoveRoller(roller);
        }
    }

    public Record AddLabel(int index, Label.Serial label) {
        return new AddLabelRecord(Id, index, label);
    }
    private void DoAddLabel(int index, Label label) {
        Assert.False(labels.ContainsKey(index));
        labels.Add(index, label);
        foreach (var view in View) {
            view.OnAddLabel(index, label);
        }
    }

    public Record RemoveLabel(int index) {
        foreach (var label in GetLabel(index)) {
            return RemoveLabel(label.Id);
        }
        throw RtlAssert.NotReached();
    }
    public Record RemoveLabel(Guid labelId) {
        return new RemoveLabelRecord(Id, labelId);
    }

    private void DoRemoveLabel(Guid labelId) {
        var index = GetLabelIndex(labelId);

        foreach (var indexValue in index) {
            var label = GetLabel(indexValue);
            labels.Remove(indexValue);
            foreach (var view in View) {
                foreach (var labelValue in label) {
                    view.OnRemoveLabel(indexValue, labelValue);
                }
            }
            return;
        }
        Assert.NotReached();
    }

    public Record InsertColumn(int index) {
        return new InsertColumnRecord(Id, index, Array.Empty<TapeValue>());
    }
    private void DoInsertColumn(int index, IReadOnlyList<TapeValue> insertedValues) {
        int i = 0;
        foreach (var tape in Tapes) {
            if (!tape.Lock.HasFlag(LockType.Edit)) {
                tape.InsertValue(index, i < insertedValues.Count ? insertedValues[i++].ToOption() : Option.None);
            }
        }
    }

    public Record RemoveColumn(int index) {
        return new RemoveColumnRecord(Id, index);
    }
    private void DoRemoveColumn(int index) {
        foreach (var tape in Tapes) {
            if (!tape.Lock.HasFlag(LockType.Edit)) {
                tape.RemoveValue(index);
            }
        }
    }

    public Record Rotate() => new RotateRecord(Id);
    private void DoRotate() => rotated = !Rotated;

    public Record RenameLabel(Guid labelId, LE name) {
        return new RenameLabelRecord(Id, labelId, name);
    }

    private void DoRenameLabel(Guid labelId, LE name) {
        var index = GetLabelIndex(labelId).ValueOrAssert();
        var label = GetLabel(index).ValueOrAssert();
        label.Name = name;
    }

    public Guid Id => id;
    public Workspace Workspace => workspace;
    public IReadOnlyList<Tape> Tapes => tapes;
    public IReadOnlyList<Roller> Rollers => rollers;

    public abstract class BaseRecord : Record {
        private Guid cryptexId;

        protected BaseRecord(Guid cryptexId) {
            this.cryptexId = cryptexId;
        }

        public Guid CryptexId => cryptexId;

        protected RecordObjectId CryptexRecordId => new RecordObjectId(CryptexId);

        protected Cryptex GetCryptex(Workspace workspace) {
            if (workspace.GetCryptex(CryptexId).TryGetValue(out Cryptex cryptex)) {
                return cryptex;
            }
            throw new DataException(LC.Temp($"No Cryptex with id {CryptexId}"));
        }

        public IEnumerable<CostType> GetRollerCosts(Roller.Serial roller) {
            var cost = CostType.Roller.Yield();
            foreach (var frame in roller.Frames) {
                cost = cost.Concat(Roller.BaseRecord.GetFrameCosts(frame.Flags));
            }
            return cost;
        }
    }

    public sealed class MoveCryptexRecord : BaseRecord {
        private float x;
        private float y;

        public MoveCryptexRecord(Guid cryptexId, Vector2 xy) : base(cryptexId) {
            x = xy.x;
            y = xy.y;
        }

        public Vector2 XY => new Vector2(x, y);

        public override IEnumerable<RecordObjectId> RequiredIds => new RecordObjectId(CryptexId).Yield();

        public override IEnumerable<CostType> GetCosts(Workspace workspace) => Array.Empty<CostType>();

        public override Record Apply(Workspace workspace, bool invertOnly = false) {
            var cryptex = GetCryptex(workspace);
            var oldXY = cryptex.XY;
            if (!invertOnly) {
                cryptex.DoMoveCryptex(XY);
            }
            return new MoveCryptexRecord(CryptexId, oldXY);
        }

        public override bool IsExecutionRelevant => false;
    }

    public sealed class AddRollerRecord : BaseRecord {
        private Roller.Serial roller;

        public AddRollerRecord(Guid cryptexId, Roller.Serial roller) : base(cryptexId) {
            this.roller = roller;
        }

        public override IEnumerable<CostType> GetCosts(Workspace workspace) => GetRollerCosts(roller);

        public override IEnumerable<RecordObjectId> RequiredIds => new RecordObjectId(CryptexId).Yield();
        public override IEnumerable<RecordObjectId> AddedIds => new RecordObjectId(roller.Id).Yield();

        public override Record Apply(Workspace workspace, bool invertOnly = false) {
            var cryptex = GetCryptex(workspace);
            var roller = this.roller.Deserialize(workspace);
            if (!invertOnly) {
                cryptex.DoAddRoller(roller);
            }
            return new RemoveRollerRecord(CryptexId, roller.Id);
        }
    }

    public sealed class RemoveRollerRecord : Roller.BaseRecord {
        public RemoveRollerRecord(Guid cryptexId, Guid rollerId) : base(cryptexId, rollerId) { }

        public override IEnumerable<CostType> GetCosts(Workspace workspace) {
            var cryptex = GetCryptex(workspace);
            var roller = GetRoller(workspace, cryptex);
            return GetRollerCosts(roller).Select(c => ~c);
        }

        public override IEnumerable<RecordObjectId> RequiredIds => new RecordObjectId(CryptexId).Yield();
        public override Option<RecordObjectId> RemovedId => new RecordObjectId(RollerId);

        public override Record Apply(Workspace workspace, bool invertOnly = false) {
            var cryptex = GetCryptex(workspace);
            var roller = GetRoller(workspace, cryptex);
            if (!invertOnly) {
                cryptex.DoRemoveRoller(roller);
            }
            return new AddRollerRecord(CryptexId, roller);
        }
    }

    public sealed class RemoveTapeRecord : Tape.BaseRecord {
        public RemoveTapeRecord(Guid cryptexId, Guid tapeId) : base(cryptexId, tapeId) { }

        public override IEnumerable<CostType> GetCosts(Workspace workspace) {
            var cryptex = GetCryptex(workspace);
            var tape = GetTape(workspace, cryptex);
            return GetTapeCosts(workspace, tape).Select(c => ~c);
        }

        public override IEnumerable<RecordObjectId> RequiredIds => new RecordObjectId(CryptexId).Yield();
        public override Option<RecordObjectId> RemovedId => new RecordObjectId(TapeId);

        public override Record Apply(Workspace workspace, bool invertOnly = false) {
            var cryptex = GetCryptex(workspace);
            var tape = GetTape(workspace, cryptex);
            var index = cryptex.tapes.IndexOf(tape);
            if (!invertOnly) {
                cryptex.DoRemoveTape(tape);
            }
            return new AddTapeRecord(cryptex.Id, index, tape);
        }
    }

    public sealed class AddTapeRecord : BaseRecord {
        private int index;
        private Tape.Serial tape;

        public AddTapeRecord(Guid cryptexId, int index, Tape.Serial tape) : base(cryptexId) {
            this.index = index;
            this.tape = tape;
        }

        public override IEnumerable<CostType> GetCosts(Workspace workspace) => Tape.BaseRecord.GetTapeCosts(workspace, tape);

        public override IEnumerable<RecordObjectId> RequiredIds => new RecordObjectId(CryptexId).Yield();
        public override IEnumerable<RecordObjectId> AddedIds => new RecordObjectId(tape.Id).Yield();

        public override Record Apply(Workspace workspace, bool invertOnly = false) {
            var cryptex = GetCryptex(workspace);
            var tape = this.tape.Deserialize(workspace);
            if (!invertOnly) {
                cryptex.DoAddTape(index, tape);
            }
            return new RemoveTapeRecord(cryptex.Id, tape.Id);
        }
    }

    public sealed class MoveTapeRecord : Tape.BaseRecord {
        private int index;
        private int offset;
        
        public MoveTapeRecord(Guid cryptexId, Guid tapeId, int index, int offset) : base(cryptexId, tapeId) {
            this.index = index;
            this.offset = offset;
        }

        public override IEnumerable<CostType> GetCosts(Workspace workspace) => Enumerable.Empty<CostType>();

        public override Record Apply(Workspace workspace, bool invertOnly = false) {
            var cryptex = GetCryptex(workspace);
            var tape = GetTape(workspace, cryptex);
            int oldIndex = cryptex.Tapes.Select((t, i) => (t.Id, i)).Single(ti => ti.Id == TapeId).i;
            int oldOffset = tape.ShiftOffset;
            if (!invertOnly) {
                cryptex.DoMoveTape(tape, index, offset);
            }
            return new MoveTapeRecord(CryptexId, TapeId, oldIndex, oldOffset);
        }
    }

    public sealed class AddLabelRecord : BaseRecord {
        private int index;
        private Label.Serial label;

        public AddLabelRecord(Guid cryptexId, int index, Label.Serial label) : base(cryptexId) {
            this.index = index;
            this.label = label;
        }

        public override IEnumerable<CostType> GetCosts(Workspace workspace) => Array.Empty<CostType>();

        public override IEnumerable<RecordObjectId> AddedIds => new RecordObjectId(label.Id).Yield();

        public override IEnumerable<RecordObjectId> RequiredIds => new RecordObjectId(CryptexId).Yield();

        public override Record Apply(Workspace workspace, bool invertOnly = false) {
            var cryptex = GetCryptex(workspace);
            if (!invertOnly) {
                cryptex.DoAddLabel(index, label.Deserialize(workspace));
            }
            return new RemoveLabelRecord(CryptexId, label.Id);
        }
    }

    public sealed class RemoveLabelRecord : BaseRecord {
        private Guid labelId;

        public RemoveLabelRecord(Guid cryptexId, Guid labelId) : base(cryptexId) {
            this.labelId = labelId;
        }

        public override IEnumerable<CostType> GetCosts(Workspace workspace) => Array.Empty<CostType>();

        public override IEnumerable<RecordObjectId> RequiredIds => new RecordObjectId(CryptexId).Yield();
        public override Option<RecordObjectId> RemovedId => new RecordObjectId(labelId);

        public override Record Apply(Workspace workspace, bool invertOnly = false) {
            var cryptex = GetCryptex(workspace);
            int index = cryptex.GetLabelIndex(labelId).ValueOrAssert();
            var label = cryptex.GetLabel(index).ValueOrAssert();
            if (!invertOnly) {
                cryptex.DoRemoveLabel(labelId);
            }
            return new AddLabelRecord(CryptexId, index, label);
        }
    }

    public sealed class RenameLabelRecord : BaseRecord {
        private Guid labelId;
        private LE name;

        public RenameLabelRecord(Guid cryptexId, Guid labelId, LE name) : base(cryptexId) {
            this.labelId = labelId;
            this.name = name;
        }

        public Guid LabelId => labelId;

        public LE Name => name;

        public override Record Apply(Workspace workspace, bool invertOnly = false) {
            var cryptex = GetCryptex(workspace);
            int index = cryptex.GetLabelIndex(labelId).ValueOrAssert();
            var label = cryptex.GetLabel(index).ValueOrAssert();
            var oldLabelName = label.Name;
            if (!invertOnly) {
                cryptex.DoRenameLabel(LabelId, Name);
            }
            return new RenameLabelRecord(CryptexId, LabelId, oldLabelName);
        }

        public override IEnumerable<RecordObjectId> RequiredIds => new[] { new RecordObjectId(CryptexId), new RecordObjectId(labelId) };

        public override IEnumerable<CostType> GetCosts(Workspace workspace) => Array.Empty<CostType>();
    }

    public sealed class InsertColumnRecord : BaseRecord {
        private int index;
        private TapeValue[] values;

        public InsertColumnRecord(Guid cryptexId, int index, Option<IEnumerable<TapeValue>> insertedValues) : base(cryptexId) {
            this.index = index;
            values = null;
            foreach (var insertedValueValues in insertedValues) {
                values = insertedValueValues.ToArray();
            }
        }

        public int Index => index;

        public override Record Apply(Workspace workspace, bool invertOnly = false) {
            var cryptex = GetCryptex(workspace);
            if (!invertOnly) {
                cryptex.DoInsertColumn(index, values ?? Array.Empty<TapeValue>());
            }
            return new RemoveColumnRecord(CryptexId, index);
        }

        public override IEnumerable<CostType> GetCosts(Workspace workspace) => Array.Empty<CostType>();
    }

    public sealed class RemoveColumnRecord : BaseRecord {
        private int index;

        public RemoveColumnRecord(Guid cryptexId, int index) : base(cryptexId) {
            this.index = index;
        }

        public int Index => index;

        public override Record Apply(Workspace workspace, bool invertOnly = false) {
            var cryptex = GetCryptex(workspace);
            var values = new List<TapeValue>();
            bool anyOverwrite = false;
            foreach (var tape in cryptex.Tapes) {
                var value = tape.Read(index);
                values.Add(value);
                anyOverwrite |= value != tape.GetSequenceValue(index);
            }
            if (!invertOnly) {
                cryptex.DoRemoveColumn(index);
            }
            return new InsertColumnRecord(CryptexId, index, anyOverwrite ? values.ToOption<IEnumerable<TapeValue>>() : Option.None);
        }

        public override IEnumerable<CostType> GetCosts(Workspace workspace) => Array.Empty<CostType>();

        public override IEnumerable<RecordObjectId> RequiredIds => new RecordObjectId(CryptexId).Yield();
    }

    public sealed class RotateRecord : BaseRecord {
        public RotateRecord(Guid cryptexId) : base(cryptexId) { }

        public override Record Apply(Workspace workspace, bool invertOnly = false) {
            var cryptex = GetCryptex(workspace);
            if (!invertOnly) {
                cryptex.DoRotate();
            }
            // This Record toggles between 2 states, so the same Record can be reused
            return this;
        }

        public override IEnumerable<CostType> GetCosts(Workspace workspace) => Array.Empty<CostType>();

        public override bool IsExecutionRelevant => false;

        public override IEnumerable<RecordObjectId> RequiredIds => new RecordObjectId(CryptexId).Yield();
    }
}
