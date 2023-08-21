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

partial class Frame : ISerializeTo<Frame.Serial> {
    [Serializable]
    [JsonConverter(typeof(SingleFieldConverter<Serial, FrameFlags>))]
    public struct Serial : IDeserializeTo<Frame> {
        private FrameFlags flags;

        public Serial(FrameFlags flags, LockType locked) {
            this.flags = flags | (FrameFlags)((int)locked << 8);
        }

        public FrameFlags Flags => flags;

        public Frame Deserialize(Workspace workspace) {
            var locked = (LockType)((int)flags >> 8);
            return new Frame(flags, locked);
        }

        public override bool Equals(object obj) => obj is Serial s && flags == s.flags;
        public override int GetHashCode() => flags.GetHashCode();

        public static bool operator ==(Serial a, Serial b) => a.Equals(b);
        public static bool operator !=(Serial a, Serial b) => !a.Equals(b);

        public static IEnumerable<Record> Create(Cryptex.Serial cryptex, Roller.Serial roller, int index, Serial to) => new Roller.AddFrameRecord(cryptex.Id, roller.Id, index, to).Yield();
        public static IEnumerable<Record> Destroy(Cryptex.Serial cryptex, Roller.Serial roller, int index, Serial from) => new Roller.RemoveFrameRecord(cryptex.Id, roller.Id, index).Yield();
        public static IEnumerable<Record> Transform(Cryptex.Serial cryptex, Roller.Serial roller, int index, Serial from, Serial to) => new Roller.ModifyFrameRecord(cryptex.Id, roller.Id, index, to.Flags).Yield();
    }

    public Serial Serialize() {
        return new Serial(flags, locked);
    }

    public static implicit operator Serial(Frame self) => self.Serialize();
}
