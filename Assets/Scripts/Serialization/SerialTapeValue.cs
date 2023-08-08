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

partial class TapeValue : ISerializeTo<TapeValue.Serial> {
    [Serializable]
    [JsonConverter(typeof(SingleFieldConverter<Serial, string>))]
    public readonly struct Serial : IDeserializeTo<TapeValue> {
        private readonly string data;

        public Serial(string data) {
            this.data = data;
        }

        public TapeValue Deserialize(Workspace workspace) => TapeValue.FromString(data);

        public override string ToString() => data;

        public static IEnumerable<Record> Create(Cryptex.Serial cryptex, Tape.Serial tape, int index, Serial to) => new Tape.WriteRecord(cryptex.Id, tape.Id, index, to).Yield();
        public static IEnumerable<Record> Destroy(Cryptex.Serial cryptex, Tape.Serial tape, int index, Serial from) => Tape.WriteRecord.Clear(cryptex.Id, tape.Id, index).Yield();
        public static IEnumerable<Record> Transform(Cryptex.Serial cryptex, Tape.Serial tape, int index, Serial from, Serial to) {
            if (from == to) {
                return Array.Empty<Record>();
            }
            return Create(cryptex, tape, index, to);
        }

        public static bool operator ==(Serial a, Serial b) => a.data == b.data;
        public static bool operator !=(Serial a, Serial b) => a.data != b.data;

        public override bool Equals(object obj) => obj is Serial s && data == s.data;
        public override int GetHashCode() => data.GetHashCode();
    }

    // TODO serialization may not persist between locale switches
    public Serial Serialize() => new Serial(GetText().ToString());

    public static implicit operator Serial(TapeValue self) => self.Serialize();
}

public static class Extensions {
    public static string AsString(this IEnumerable<TapeValue.Serial> self) => string.Join(" ", self.Select(s => s.ToString()));

    public static IEnumerable<TapeValue.Serial> AsTapeValueSerials(this string self) => self.Split(' ').Select(s => new TapeValue.Serial(s));
}
