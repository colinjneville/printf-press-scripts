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

[Flags]
public enum FrameFlags {
    None = 0,
    CanShift = 1 << 0,
    CanRead = 1 << 1,
    CanWrite = 1 << 2,
    CanOverlap = 1 << 3,

    FrameDummy = CanOverlap,
    FrameRead = CanShift | CanRead | CanOverlap,
    FrameWrite = CanShift | CanWrite | CanOverlap,
    FrameReadWrite = CanShift | CanRead | CanWrite | CanOverlap,
    FrameExclusive = None,
}

public sealed partial class Frame {
    private FrameFlags flags;
    private LockType locked;

    public Frame(FrameFlags flags, LockType locked = default) {
        this.flags = flags;
        this.locked = locked;
    }

    public FrameFlags Flags {
        get => flags;
        set {
            flags = value;
            foreach (var view in View) {
                view.OnUpdateFlags();
            }
        }
    }

    public LockType Lock => locked;

    public bool AllowShift => flags.HasFlag(FrameFlags.CanShift);
    public bool AllowRead => flags.HasFlag(FrameFlags.CanRead);
    public bool AllowWrite => flags.HasFlag(FrameFlags.CanWrite);
    public bool AllowOverlap => flags.HasFlag(FrameFlags.CanOverlap);
}
