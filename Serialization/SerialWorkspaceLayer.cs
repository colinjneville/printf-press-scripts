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

partial class WorkspaceLayer : ISerializeTo<WorkspaceLayer.Serial> {
    [Serializable]
    public struct Serial : IDeserializeTo<WorkspaceLayer> {
        private Cryptex.Serial[] cryptexes;

        public Serial(IEnumerable<Cryptex> cryptexes) {
            this.cryptexes = cryptexes.Select(c => c.Serialize()).ToArray();
        }

        public WorkspaceLayer Deserialize(Workspace workspace) {
            return new WorkspaceLayer(cryptexes.Select(c => c.Deserialize(workspace)));
        }

        public static IEnumerable<Record> Create(Serial to) => to.cryptexes.SelectMany(Cryptex.Serial.Create);

        // No reason to implement this
        //public static IEnumerable<Record> Destroy(Serial from) => 

        public static IEnumerable<Record> Transform(Serial from, Serial to) {
            var records = Enumerable.Empty<Record>();

            // TODO Is it ok to delete tapes that may have rollers on them?
            var fromCryptexes = Enumerable.Range(0, from.cryptexes.Length).ToDictionary(n => from.cryptexes[n].Id, n => (n, from.cryptexes[n]));
            var toCryptexes = Enumerable.Range(0, to.cryptexes.Length).ToDictionary(n => to.cryptexes[n].Id, n => (n, to.cryptexes[n]));

            for (int fromCryptexIndex = 0; fromCryptexIndex < from.cryptexes.Length; ++fromCryptexIndex) {
                var fromCryptex = from.cryptexes[fromCryptexIndex];
                var tapeId = fromCryptex.Id;

                if (!toCryptexes.ContainsKey(tapeId)) {
                    records = records.Concat(Cryptex.Serial.Destroy(fromCryptex));
                }
            }

            for (int toCryptexIndex = 0; toCryptexIndex < to.cryptexes.Length; ++toCryptexIndex) {
                var toCryptex = to.cryptexes[toCryptexIndex];
                var cryptexId = toCryptex.Id;

                if (fromCryptexes.TryGetValue(cryptexId, out var fromCryptexTuple)) {
                    (int fromCryptexIndex, var fromCryptex) = fromCryptexTuple;
                    records = records.Concat(Cryptex.Serial.Transform(fromCryptex, toCryptex));
                } else {
                    records = records.Concat(Cryptex.Serial.Create(toCryptex));
                }
            }

            return records;
        }
    }

    public Serial Serialize() {
        return new Serial(cryptexes.Values);
    }

    public static implicit operator Serial(WorkspaceLayer self) => self.Serialize();
}
