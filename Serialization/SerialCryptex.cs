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

partial class Cryptex : ISerializeTo<Cryptex.Serial> {
    [Serializable]
    public struct Serial : IDeserializeTo<Cryptex> {
        private Guid id;
        private Tape.Serial[] tapes;
        private Roller.Serial[] rollers;
        private Dictionary<int, Label.Serial> labels;
        private LockType locked;
        private float x;
        private float y;
        private bool rotated;

        public Serial(Guid id, IEnumerable<Tape> tapes, IEnumerable<Roller> rollers, IEnumerable<KeyValuePair<int, Label>> labels, Vector2 xy, bool rotated, LockType locked) {
            this.id = id;
            this.tapes = tapes.Select(t => t.Serialize()).ToArray();
            if (this.tapes.Length == 0) {
                this.tapes = null;
            }
            this.rollers = rollers.Select(r => r.Serialize()).ToArray();
            if (this.rollers.Length == 0) {
                this.rollers = null;
            }
            this.labels = labels.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Serialize());
            if (this.labels.Count == 0) {
                this.labels = null;
            }
            this.locked = locked;
            this.rotated = rotated;
            x = xy.x;
            y = xy.y;
        }

        public Guid Id => id;

        public IDictionary<int, Label.Serial> Labels => (IDictionary<int, Label.Serial>)labels ?? ImmutableDictionary<int, Label.Serial>.Empty;
        public IReadOnlyList<Tape.Serial> Tapes => (IReadOnlyList<Tape.Serial>)tapes ?? Array.Empty<Tape.Serial>();
        public IReadOnlyList<Roller.Serial> Rollers => (IReadOnlyList<Roller.Serial>)rollers ?? Array.Empty<Roller.Serial>();

        public Vector2 XY => new Vector2(x, y);

        public Cryptex Deserialize(Workspace workspace) {
            var cryptex = new Cryptex(id, workspace, XY, locked);
            cryptex.tapes.AddRange(Tapes.Select(t => t.Deserialize(workspace)));
            cryptex.rollers.AddRange(Rollers.Select(r => r.Deserialize(workspace)));
            foreach (var tape in cryptex.tapes) {
                tape.SetCryptex(cryptex);
            }
            foreach (var roller in cryptex.rollers) {
                roller.SetCryptex(cryptex);
            }
            foreach (var label in Labels) {
                cryptex.labels.Add(label.Key, label.Value.Deserialize(workspace));
            }
            cryptex.rotated = rotated;
            return cryptex;
        }

        public static IEnumerable<Record> Create(Serial to) => new WorkspaceFull.AddCryptexRecord(to).Yield();

        public static IEnumerable<Record> Destroy(Serial from) => new WorkspaceFull.RemoveCryptexRecord(from.Id).Yield();

        public static IEnumerable<Record> Transform(Serial from, Serial to) {
            var records = Enumerable.Empty<Record>();

            var cryptexId = from.Id;

            var fromRollers = Enumerable.Range(0, from.Rollers.Count).ToDictionary(n => from.Rollers[n].Id, n => from.Rollers[n]);
            var toRollers = new HashSet<Guid>(to.Rollers.Select(r => r.Id));

            // Remove deleted rollers before modifying tapes
            for (int fromRollerIndex = 0; fromRollerIndex < from.Rollers.Count; ++fromRollerIndex) {
                var fromRoller = from.Rollers[fromRollerIndex];
                var rollerId = fromRoller.Id;

                if (!toRollers.Contains(rollerId)) {
                    records = records.Concat(Roller.Serial.Destroy(from, fromRoller));
                }
            }

            {
                // TODO Is it ok to delete tapes that may have rollers on them?
                var fromTapes = Enumerable.Range(0, from.Tapes.Count).ToDictionary(n => from.Tapes[n].Id, n => (n, from.Tapes[n]));
                var toTapes = new HashSet<Guid>(to.Tapes.Select(t => t.Id));

                for (int fromTapeIndex = 0; fromTapeIndex < from.Tapes.Count; ++fromTapeIndex) {
                    var fromTape = from.Tapes[fromTapeIndex];
                    var tapeId = fromTape.Id;

                    if (!toTapes.Contains(tapeId)) {
                        records = records.Concat(Tape.Serial.Destroy(from, fromTapeIndex, fromTape));
                    }
                }

                for (int toTapeIndex = 0; toTapeIndex < to.Tapes.Count; ++toTapeIndex) {
                    var toTape = to.Tapes[toTapeIndex];
                    var tapeId = toTape.Id;

                    if (fromTapes.TryGetValue(tapeId, out var fromTapeTuple)) {
                        (int fromTapeIndex, var fromTape) = fromTapeTuple;
                        records = records.Concat(Tape.Serial.Transform(from, fromTape, toTape));
                        if (fromTapeIndex != toTapeIndex || fromTape.ShiftOffset != toTape.ShiftOffset) {
                            records = records.Append(new MoveTapeRecord(cryptexId, tapeId, toTapeIndex, toTape.ShiftOffset));
                        }
                    } else {
                        records = records.Concat(Tape.Serial.Create(from, toTapeIndex, toTape));
                    }
                }
            }

            // Now modify/add new rollers

            for (int toRollerIndex = 0; toRollerIndex < to.Rollers.Count; ++toRollerIndex) {
                var toRoller = to.Rollers[toRollerIndex];
                var rollerId = toRoller.Id;

                if (fromRollers.TryGetValue(rollerId, out var fromRoller)) {
                    records = records.Concat(Roller.Serial.Transform(from, fromRoller, toRoller));
                } else {
                    records = records.Concat(Roller.Serial.Create(from, toRoller));
                }
            }

            {
                var fromLabels = from.Labels.Values.ToDictionary(l => l.Id, l => l);
                //var fromLabels = Enumerable.Range(0, from.Labels.Count).ToDictionary(n => from.Labels[n].Id, n => from.Labels[n]);
                var toLabels = new HashSet<Guid>(to.Labels.Select(l => l.Value.Id));

                foreach (var labelKvp in from.Labels) {
                    var fromLabel = labelKvp.Value;
                    var labelId = fromLabel.Id;

                    if (!toLabels.Contains(labelId)) {
                        records = records.Concat(Label.Serial.Destroy(from, fromLabel));
                    }
                }

                foreach (var labelKvp in to.Labels) {
                    var toLabel = labelKvp.Value;
                    var labelId = toLabel.Id;

                    if (fromLabels.TryGetValue(labelId, out var fromLabel)) {
                        records = records.Concat(Label.Serial.Transform(from, labelKvp.Key, fromLabel, toLabel));
                    } else {
                        records = records.Concat(Label.Serial.Create(from, labelKvp.Key, toLabel));
                    }
                }
            }

            records = records.Append(new MoveCryptexRecord(cryptexId, to.XY));

            return records;
        }
    }

    public Serial Serialize() {
        return new Serial(id, tapes, rollers, labels, xy, rotated, locked);
    }

    public static implicit operator Serial(Cryptex self) => self.Serialize();
}
