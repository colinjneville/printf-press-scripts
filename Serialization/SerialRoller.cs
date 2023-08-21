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

partial class Roller : ISerializeTo<Roller.Serial> {
    [Serializable]
    public struct Serial : IDeserializeTo<Roller> {
        private enum RollerType {
            Instruction,
            Programmable,
        }

        private RollerType rollerType;
        private Guid id;
        private ColorId colorId;
        private Frame.Serial[] frames;
        private int moveOffset;
        private int hopOffset;
        private LockType locked;

        private Serial(RollerType rollerType, Guid id, ColorId colorId, IEnumerable<Frame> frames, int moveOffset, int hopOffset, LockType locked) {
            this.rollerType = rollerType;
            this.id = id;
            this.colorId = colorId;
            this.frames = frames.Select(f => f.Serialize()).ToArray();
            this.moveOffset = moveOffset;
            this.hopOffset = hopOffset;
            this.locked = locked;
        }

        public static Serial Instruction(Guid id, ColorId colorId, IEnumerable<Frame> frames, int moveOffset, int hopOffset, LockType locked = default) {
            return new Serial(RollerType.Instruction, id, colorId, frames, moveOffset, hopOffset, locked);
        }

        public static Serial Programmable(Guid id, ColorId colorId, IEnumerable<Frame> frames, int moveOffset, int hopOffset, LockType locked = default) {
            return new Serial(RollerType.Programmable, id, colorId, frames, moveOffset, hopOffset, locked);
        }

        public Guid Id => id;

        public IReadOnlyList<Frame.Serial> Frames => frames;

        public Roller Deserialize(Workspace workspace) {
            Roller roller;
            switch (rollerType) {
                case RollerType.Instruction:
                    roller = new InstructionRoller(id, colorId, frames.Select(f => f.Deserialize(workspace)), locked);
                    break;
                case RollerType.Programmable:
                    roller = new ProgrammableRoller(id, colorId, frames.Select(f => f.Deserialize(workspace)), locked);
                    break;
                default:
                    throw RtlAssert.NotReached();
            }
            
            roller.moveOffset = moveOffset;
            roller.hopOffset = hopOffset;
            return roller;
        }

        public static IEnumerable<Record> Create(Cryptex.Serial cryptex, Serial to) => new Cryptex.AddRollerRecord(cryptex.Id, to).Yield();
        public static IEnumerable<Record> Destroy(Cryptex.Serial cryptex, Serial from) => new Cryptex.RemoveRollerRecord(cryptex.Id, from.Id).Yield();
        public static IEnumerable<Record> Transform(Cryptex.Serial cryptex, Serial from, Serial to) {
            var records = Enumerable.Empty<Record>();
            int xDiff = to.moveOffset - from.moveOffset;
            int yDiff = to.hopOffset - from.hopOffset;

            // Destroy before move so we are never bounded on vertical moves
            for (int i = to.Frames.Count - 1; i >= from.Frames.Count; --i) {
                records = records.Concat(Frame.Serial.Destroy(cryptex, from, i, from.Frames[i]));
            }

            if (xDiff != 0) {
                records = records.Append(new MoveRightRecord(cryptex.Id, from.Id, xDiff, Option.None));
            }
            if (yDiff != 0) {
                records = records.Append(new MoveDownRecord(cryptex.Id, from.Id, yDiff));
            }

            int minFrames = Mathf.Min(from.Frames.Count, to.Frames.Count);

            
            for (int i = 0; i < minFrames; ++i) {
                if (from.Frames[i] != to.Frames[i]) {
                    records = records.Concat(Frame.Serial.Transform(cryptex, from, i, from.Frames[i], to.Frames[i]));
                }
            }
            for (int i = from.Frames.Count; i < to.Frames.Count; ++i) {
                records = records.Concat(Frame.Serial.Create(cryptex, from, i, to.Frames[i]));
            }

            if (from.colorId != to.colorId) {
                records = records.Append(new ChangeColorRecord(cryptex.Id, from.Id, to.colorId));
            }

            return records;
        }
    }

    public abstract Serial Serialize();

    public static implicit operator Serial(Roller self) => self.Serialize();
}

