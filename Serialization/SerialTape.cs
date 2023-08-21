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

partial class Tape : ISerializeTo<Tape.Serial> {
    [Serializable]
    public struct Serial : IDeserializeTo<Tape> {
        private Guid id;
        private SequenceType sequenceType;
        private Dictionary<int, TapeValue.Serial> writes;
        private Dictionary<int, Note.Serial> notes;
        private TapeValue.Serial[] customValues;
        private bool customLoop;
        private int[] breakpoints;
        private int shiftOffset;
        private LockType locked;

        public Serial(Guid id, SequenceType sequenceType, IEnumerable<KeyValuePair<int, TapeValue>> writes, IEnumerable<KeyValuePair<int, Note>> notes, IEnumerable<TapeValue> customValues, ICollection<int> breakpoints, bool customLoop, int shiftOffset, LockType locked) {
            this.id = id;
            this.sequenceType = sequenceType;
            this.writes = writes.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Serialize());
            if (this.writes.Count == 0) {
                this.writes = null;
            }
            this.notes = notes.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Serialize());
            if (this.notes.Count == 0) {
                this.notes = null;
            }

            this.customValues = customValues.Select(v => v.Serialize()).ToArray();
            if (this.customValues.Length == 0) {
                this.customValues = null;
            }
            if (breakpoints.Count == 0) {
                this.breakpoints = null;
            } else {
                this.breakpoints = breakpoints.ToArray();
            }
            this.customLoop = customLoop;
            this.shiftOffset = shiftOffset;
            this.locked = locked;
        }

        public Guid Id => id;

        public int ShiftOffset => shiftOffset;

        public SequenceType SequenceType => sequenceType;
        public IReadOnlyDictionary<int, TapeValue.Serial> Writes => (IReadOnlyDictionary<int, TapeValue.Serial>)writes ?? ImmutableDictionary<int, TapeValue.Serial>.Empty;
        public IReadOnlyDictionary<int, Note.Serial> Notes => (IReadOnlyDictionary<int, Note.Serial>)notes ?? ImmutableDictionary<int, Note.Serial>.Empty;
        public IReadOnlyCollection<int> Breakpoints => (IReadOnlyCollection<int>)breakpoints ?? Array.Empty<int>();
        public IReadOnlyList<TapeValue.Serial> CustomValues => (IReadOnlyList<TapeValue.Serial>)customValues ?? Array.Empty<TapeValue.Serial>();

        public Tape Deserialize(Workspace workspace) {
            Tape tape;
            if (sequenceType == SequenceType.Custom) {
                tape = new Tape(id, customLoop, CustomValues.Select(v => v.Deserialize(workspace)), locked);
            } else {
                tape = new Tape(id, sequenceType, locked);
            }
            foreach (var write in Writes) {
                tape.overwrites.Add(write.Key, write.Value.Deserialize(workspace));
            }
            foreach (var note in Notes) {
                tape.notes.Add(note.Key, note.Value.Deserialize(workspace));
            }
            tape.breakpoints.UnionWith(Breakpoints);

            tape.ShiftOffset = shiftOffset;
            return tape;
        }

        public static IEnumerable<Record> Create(Cryptex.Serial cryptex, int index, Serial to) => new Cryptex.AddTapeRecord(cryptex.Id, index, to).Yield();
        public static IEnumerable<Record> Destroy(Cryptex.Serial cryptex, int index, Serial from) => new Cryptex.RemoveTapeRecord(cryptex.Id, from.Id).Yield();
        public static IEnumerable<Record> Transform(Cryptex.Serial cryptex, Serial from, Serial to) {
            var records = Enumerable.Empty<Record>();
            foreach (var toWriteKvp in to.Writes) {
                if (from.Writes.TryGetValue(toWriteKvp.Key, out var fromWrite)) {
                    records = records.Concat(TapeValue.Serial.Transform(cryptex, from, toWriteKvp.Key, fromWrite, toWriteKvp.Value));
                } else {
                    records = records.Concat(TapeValue.Serial.Create(cryptex, from, toWriteKvp.Key, toWriteKvp.Value));
                }
            }
            foreach (var fromWriteKvp in from.Writes) {
                if (!to.Writes.ContainsKey(fromWriteKvp.Key)) {
                    records = records.Concat(TapeValue.Serial.Destroy(cryptex, from, fromWriteKvp.Key, fromWriteKvp.Value));
                }
            }
            foreach (var toNoteKvp in to.Notes) {
                if (from.Notes.TryGetValue(toNoteKvp.Key, out var fromNote)) {
                    records = records.Concat(global::Note.Serial.Transform(cryptex, from, toNoteKvp.Key, fromNote, toNoteKvp.Value));
                } else {
                    records = records.Concat(global::Note.Serial.Create(cryptex, from, toNoteKvp.Key, toNoteKvp.Value));
                }
            }
            foreach (var fromNoteKvp in from.Notes) {
                if (!to.Notes.ContainsKey(fromNoteKvp.Key)) {
                    records = records.Concat(global::Note.Serial.Destroy(cryptex, from, fromNoteKvp.Key, fromNoteKvp.Value));
                }
            }
            return records;
        }
    }

    public Serial Serialize() {
        return new Serial(id, sequence, overwrites, notes, customValues, breakpoints, customLoop, ShiftOffset, locked);
    }

    public static implicit operator Serial(Tape self) => self.Serialize();
}
