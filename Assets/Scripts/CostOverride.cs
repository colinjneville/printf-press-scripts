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

public enum CostType {
    // Inverse refer to the negative of the cost (e.g. ((CostType)~(int)Cryptex) => -$X)
    Cryptex = 0,
    Tape,
    TapeSequence,
    Roller,
    RollerInstruction,
    RollerProgrammable,
    Frame,
    FrameCanRead,
    FrameCanWrite,
    FrameCanShift,
    Write,
    WriteBlank,
    WriteNumber, // Since Labels are essentially convenient Numbers, it doesn't make sense to consider them separate categories
    WriteLetter,
    WriteColor,
    WriteOpcode,
    WriteFrame,
    WriteFrameRead,
    WriteChannel,
    WriteInvalid,
    //Tick,
}

public static class CostTypeExtensions {
    public static int ToCost(this CostType costType, CostOverride costOverride) {
        bool negate = false;
        if (costType < 0) {
            costType = ~costType;
            negate = true;
        }
        var cost = costOverride[costType];
        if (negate) {
            cost *= -1;
        }
        return cost;
    }
}

[Serializable]
public sealed class CostOverride {
    private Dictionary<CostType, int> overrides;
    private Option<int> energyPerCost;

    public CostOverride(IEnumerable<KeyValuePair<CostType, int>> overrides, Option<int> energyPerCost = default) {
        this.overrides = new Dictionary<CostType, int>();
        foreach (var @override in overrides) {
            this.overrides.Add(@override.Key, @override.Value);
        }
        this.energyPerCost = energyPerCost;
    }

    public int this[CostType type] {
        get {
            if (overrides.TryGetValue(type, out int value)) {
                return value;
            }
            return GetBaseCost(type);
        }
    }

    private const int defaultEnergyPerCost = 100;

    public int EnergyPerCost => energyPerCost.ValueOr(defaultEnergyPerCost);

    public int GetEnergyCost(int energy) => (energy + EnergyPerCost - 1) / EnergyPerCost;

    private static int GetBaseCost(CostType costType) {
        switch (costType) {
            case CostType.Cryptex:
                return 10;
            case CostType.Tape:
                return 5;
            case CostType.TapeSequence:
                return 5;
            case CostType.Roller:
                return 5;
            case CostType.RollerInstruction:
                return 0;
            case CostType.RollerProgrammable:
                return 0;
            case CostType.Frame:
                return 1;
            case CostType.FrameCanRead:
                return 1;
            case CostType.FrameCanWrite:
                return 5;
            case CostType.FrameCanShift:
                return 0;
            case CostType.Write:
                return 1;
            case CostType.WriteBlank:
                return 0;
            case CostType.WriteNumber:
                return 0;
            case CostType.WriteLetter:
                return 0;
            case CostType.WriteColor:
                return 0;
            case CostType.WriteOpcode:
                return 0;
            case CostType.WriteFrame:
                return 0;
            case CostType.WriteFrameRead:
                return 0;
            case CostType.WriteChannel:
                return 0;
            case CostType.WriteInvalid:
                return 0;
            //case CostType.Tick:
            //    // Ticks are costed in subunits
            //    return 1;
            default:
                throw RtlAssert.NotReached();
        }
    }

    public static CostOverride Default { get; } = new CostOverride(Array.Empty<KeyValuePair<CostType, int>>());
}
