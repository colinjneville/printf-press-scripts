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

partial class Label : ISerializeTo<Label.Serial> {
    [Serializable]
    public struct Serial : IDeserializeTo<Label> {
        private Guid id;
        private LE name;

        public Serial(Guid id, LE name) {
            this.id = id;
            this.name = name;
        }

        public Guid Id => id;

        public Label Deserialize(Workspace workspace) {
            return new Label(id, name);
        }

        public static IEnumerable<Record> Create(Cryptex.Serial cryptex, int index, Serial to) => new Cryptex.AddLabelRecord(cryptex.Id, index, to).Yield();
        public static IEnumerable<Record> Destroy(Cryptex.Serial cryptex, Serial from) => new Cryptex.RemoveLabelRecord(cryptex.Id, from.Id).Yield();
        public static IEnumerable<Record> Transform(Cryptex.Serial cryptex, int toIndex, Serial from, Serial to) {
            return Destroy(cryptex, from).Concat(Create(cryptex, toIndex, to));
            //new Cryptex.RenameLabelRecord(cryptex.Id, from.Id, to.name).Yield();
        }
    }

    public Serial Serialize() {
        return new Serial(id, name);
    }

    public static implicit operator Serial(Label self) => self.Serialize();
}
