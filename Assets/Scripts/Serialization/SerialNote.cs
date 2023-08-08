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

partial class Note : ISerializeTo<Note.Serial> {
    [Serializable]
    [JsonConverter(typeof(SingleFieldConverter<Serial, LE>))]
    public struct Serial : IDeserializeTo<Note> {
        private LE text;

        public Serial(LE text) {
            this.text = text;
        }

        public Note Deserialize(Workspace workspace) {
            return new Note(text);
        }

        public static IEnumerable<Record> Create(Cryptex.Serial cryptex, Tape.Serial tape, int index, Serial to) => new Tape.AddNoteRecord(cryptex.Id, tape.Id, index, to.text).Yield();
        public static IEnumerable<Record> Destroy(Cryptex.Serial cryptex, Tape.Serial tape, int index, Serial from) => new Tape.RemoveNoteRecord(cryptex.Id, tape.Id, index).Yield();
        public static IEnumerable<Record> Transform(Cryptex.Serial cryptex, Tape.Serial tape, int index, Serial from, Serial to) {
            if (from.text != to.text) {
                return Create(cryptex, tape, index, to);
            } else {
                return Enumerable.Empty<Record>();
            }
        }
    }

    public Serial Serialize() {
        return new Serial(text);
    }

    public static implicit operator Serial(Note self) => self.Serialize();
}
