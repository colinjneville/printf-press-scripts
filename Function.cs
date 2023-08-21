using Functional.Option;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

using LE = ILocalizationExpression;
using L = LocalizationString;
using LC = LocalizationConstant;
using LF = LocalizationFormat;
using LI = LocalizationInt;

public sealed class TypeSet {
    private bool inverse;
    private ImmutableArray<TapeValueType> types;

    public TypeSet(bool inverse, params TapeValueType[] types) : this(inverse, (IEnumerable<TapeValueType>)types) { }
    public TypeSet(params TapeValueType[] types) : this((IEnumerable<TapeValueType>)types) { }
    public TypeSet(IEnumerable<TapeValueType> types) : this(false, types) { }
    public TypeSet(bool inverse, IEnumerable<TapeValueType> types) {
        this.inverse = inverse;
        this.types = types.ToImmutableArray();
    }

    public bool ContainsType(TapeValueType type) {
        return types.Contains(type) ^ inverse;
    }

    public TypeSet Intersect(TypeSet other) {
        if (inverse && other.inverse) {
            return new TypeSet(true, types.Union(other.types));
        } else if (inverse || other.inverse) {
            var first = inverse ? other.types : types;
            var second = inverse ? types : other.types;
            return new TypeSet(false, first.RemoveRange(second));
        } else {
            return new TypeSet(false, types.Intersect(other.types));
        }
    }

    public bool Empty => types.Count() == 0 && !inverse;
    public Option<TapeValueType> Single {
        get {
            if (!inverse) {
                if (types.Length == 1) {
                    return types[0];
                }
            }
            return Option.None;
        }
    }

    private static readonly TypeSet all = new TypeSet(inverse: true);
    public static TypeSet All => all;
}

public sealed class Parameter {
    private LE name;
    private Property properties;
    private TypeSet types;

    public enum Property {
        None = 0,
        Optional = 1,
        Params = 2,
        DisallowTypes = 4,
    }

    public Parameter(LE name, params TapeValueType[] types) : this(name, Property.None, types) { }
    public Parameter(LE name, Property properties, params TapeValueType[] types) : this(name, properties, new TypeSet(properties.HasFlag(Property.DisallowTypes), types)) { }
    public Parameter(LE name, TypeSet types) : this(name, Property.None, types) { }
    public Parameter(LE name, Property properties, TypeSet types) {
        Assert.False(types.Empty);
        this.name = name;
        this.properties = properties;
        this.types = types;
    }

    public LE Name => name;
    public Property Properties => properties;
    public TypeSet Types => types;

    public bool AllowsType(TapeValueType type) => types.ContainsType(type);
    public bool AllowsAnyType(TypeSet possibleTypes) => !types.Intersect(possibleTypes).Empty;
}

public abstract class Callable {
    private LE name;
    private ImmutableArray<Parameter> parameters;
    protected Callable(LE name, params Parameter[] parameters) : this(name, (IEnumerable<Parameter>)parameters) { }
    protected Callable(LE name, IEnumerable<Parameter> parameters) {
        this.name = name;
        this.parameters = ImmutableArray.CreateRange(parameters);
    }
    public LE Name => name;
    public IImmutableList<Parameter> Parameters => parameters;

    public override string ToString() => Name.ToString();

    public virtual bool ValidateArguments(IReadOnlyList<TapeValue> arguments) {
        if (arguments.Count() > parameters.Length && (parameters.Length == 0 || !parameters.Last().Properties.HasFlag(Parameter.Property.Params))) {
            //throw new SyntaxException(LC.Temp("Incorrect number of arguments"));
            Debug.Log("too long");
            return false;
        }
        int index = 0;
        foreach (var argument in arguments) {
            var parameter = parameters[Mathf.Min(parameters.Length - 1, index)];
            if (!parameter.AllowsType(argument.Type)) {
                //throw new SyntaxException(LC.Temp("Invalid argument type {0} at index {1} for function {2}", argument.Type, index, Name));
                Debug.Log($"wrong type {index}: {argument.Type}");
                return false;
            }

            ++index;
        }
        if (index < parameters.Length && !(parameters[index].Properties.HasFlag(Parameter.Property.Optional) || parameters[index].Properties.HasFlag(Parameter.Property.Params))) {
            //throw new SyntaxException(LC.Temp("Not enough parameters"));
            Debug.Log("too short");
            return false;
        }
        return true;
    }
}

public struct TargetInfo {
    public enum ActionType {
        Jump = 1,
        Move,
        Hop,
        Invalid,
        Stall,
        Shift,
    }

    private Roller roller;
    private int sourceHOffset;
    private int sourceVOffset;
    private int targetHOffset;
    private int targetVOffset;
    private ActionType action;

    public TargetInfo(Roller roller, int sourceHOffset, int sourceVOffset, int targetHOffset, int targetVOffset, ActionType action) {
        this.roller = roller;
        this.sourceHOffset = sourceHOffset;
        this.sourceVOffset = sourceVOffset;
        this.targetHOffset = targetHOffset;
        this.targetVOffset = targetVOffset;
        this.action = action;
    }

    public Roller Roller => roller;
    public int SourceHOffset => sourceHOffset;
    public int SourceVOffset => sourceVOffset;
    public int TargetHOffset => targetHOffset;
    public int TargetVOffset => targetVOffset;
    public ActionType Action => action;

    public override string ToString() => $"{roller.Id} ({sourceHOffset}, {sourceVOffset}) -> ({targetHOffset}, {targetVOffset}) {action}";

    public static TargetInfo Invalid(Roller roller, int hOffset, int vOffset) => new TargetInfo(roller, hOffset, vOffset, hOffset, vOffset, ActionType.Invalid);

    public TargetInfo WithRoller(Roller roller) => new TargetInfo(roller, sourceHOffset, sourceVOffset, targetHOffset, targetVOffset, action);
}

public abstract class Statement : Callable {
    protected Statement(LE name, params Parameter[] parameters) : this(name, (IEnumerable<Parameter>)parameters) { }
    protected Statement(LE name, IEnumerable<Parameter> parameters) : this(false, name, parameters) { }
    protected Statement(bool noLookup, LE name, params Parameter[] parameters) : this(noLookup, name, (IEnumerable<Parameter>)parameters) { }
    protected Statement(bool noLookup, LE name, IEnumerable<Parameter> parameters) : base(name, parameters) {
        Assert.False(lookup.ContainsKey(name.ToString()));
        if (!noLookup) {
            lookup.Add(name.ToString(), this);
        }
    }

    private static Dictionary<string, Statement> lookup = new Dictionary<string, Statement>();
    public static Option<Statement> TryLookup(string functionName) {
        Statement f;
        if (lookup.TryGetValue(functionName, out f)) {
            return f;
        }
        return Option.None;
    }
    public static Statement Lookup(string functionName) {
        foreach (var f in TryLookup(functionName)) {
            return f;
        }
        throw new SyntaxException(LC.Temp($"Unknown function {functionName}"));
    }

    private IEnumerable<TapeValue> TrimArguments(Option<ProgrammableRoller> secondaryRoller, IReadOnlyList<TapeValue> arguments) {
        // Trim all trailing null arguments for optional paramters
        int nonNullCount;
        var isParams = Parameters.Count == 0 ? false : Parameters[Parameters.Count - 1].Properties.HasFlag(Parameter.Property.Params);
        for (nonNullCount = arguments.Count; nonNullCount > 0; --nonNullCount) {
            if (nonNullCount <= Parameters.Count) {
                var properties = Parameters[nonNullCount - 1].Properties;
                // If there is a null for a mandatory argument, presume it is intentional
                if (!arguments[nonNullCount - 1].IsNull || !(properties.HasFlag(Parameter.Property.Optional) || properties.HasFlag(Parameter.Property.Params))) {
                    break;
                }
            } else if (isParams && !arguments[nonNullCount - 1].IsNull) {
                break;
            }
        }
        return arguments.Take(nonNullCount);
    }

    private IEnumerable<TapeValue> ProcessArguments(ExecutionContext ec, Option<ProgrammableRoller> secondaryRoller, IReadOnlyList<TapeValue> arguments) {
        var processedArguments = new TapeValue[arguments.Count];
        for (int i = 0; i < arguments.Count; ++i) {
            // TODO clean this up?
            var value = arguments[i];
            if (value.Type == TapeValueType.FrameRead) {
                foreach (var secondaryRollerValue in secondaryRoller) {
                    value = secondaryRollerValue.Read(value.To<int>(), ec);
                }
            }
            processedArguments[i] = value;
        }
        return processedArguments;
    }

    /*
    public Option<TargetInfo> Target(ExecutionContext ec, Roller roller, IReadOnlyList<TapeValue> arguments) {
        var trimmedArguments = TrimArguments(roller, arguments).ToArray();
        var processedArguments = ProcessArguments(roller, trimmedArguments).ToArray();
        if (!ValidateArguments(processedArguments)) {
            return TargetInternal(ec, roller, trimmedArguments, processedArguments);
        }
        return Option.None;
    }

    protected virtual Option<TargetInfo> TargetInternal(ExecutionContext ec, Roller roller, IReadOnlyList<TapeValue> trimmedArguments, IReadOnlyList<TapeValue> processedArguments) => Option.None;
    */

    // The only purpose here is to get any TargetInfo.ActionType.Invalids after undo/redo, as they have no corresponding log records
    public IEnumerable<TargetInfo> CheckExecute(ExecutionContext ec, InstructionRoller primaryRoller, IReadOnlyList<TapeValue> arguments) => Validate(ec, primaryRoller, arguments, doExecute: false);

    public IEnumerable<TargetInfo> Execute(ExecutionContext ec, InstructionRoller primaryRoller, IReadOnlyList<TapeValue> arguments) => Validate(ec, primaryRoller, arguments, doExecute: true);

    private IEnumerable<TargetInfo> Validate(ExecutionContext ec, InstructionRoller primaryRoller, IReadOnlyList<TapeValue> arguments, bool doExecute) {
        var secondaryRoller = primaryRoller.Secondary;

        var processedArguments = ProcessArguments(ec, secondaryRoller, TrimArguments(secondaryRoller, arguments).ToArray()).ToArray();
        if (ValidateArguments(processedArguments)) {
            if (doExecute) {
                var records = ExecuteInternal(ec, primaryRoller, processedArguments);
                return records.SelectMany(r => ec.ExecuteRecord(r));
            }
        } else {
            return TargetInfo.Invalid(primaryRoller, primaryRoller.Offset, primaryRoller.TapeIndex).Yield();
        }
        return Array.Empty<TargetInfo>();
    }

    protected abstract IEnumerable<Record> ExecuteInternal(ExecutionContext ec, InstructionRoller primaryRoller, IReadOnlyList<TapeValue> arguments);

    private sealed class MoveUpFunction : Statement {
        public MoveUpFunction() : base(LC.Temp("mvu"), new Parameter(LC.Temp("tapes"), Parameter.Property.Optional, TapeValueType.Int)) { }

        protected override IEnumerable<Record> ExecuteInternal(ExecutionContext ec, InstructionRoller primaryRoller, IReadOnlyList<TapeValue> arguments) {
            foreach (var secondaryRoller in primaryRoller.Secondary) {
                int count = arguments.Count == 0 ? secondaryRoller.Cryptex.Tapes.Count : arguments[0].To<int>();
                return secondaryRoller.MoveUp(count).Yield();
            }
            return Array.Empty<Record>();
        }
    }
    private static Statement hopUp = new MoveUpFunction();
    public static Statement HopUp => hopUp;

    private sealed class MoveDownFunction : Statement {
        public MoveDownFunction() : base(LC.Temp("mvd"), new Parameter(LC.Temp("tapes"), Parameter.Property.Optional, TapeValueType.Int)) { }

        protected override IEnumerable<Record> ExecuteInternal(ExecutionContext ec, InstructionRoller primaryRoller, IReadOnlyList<TapeValue> arguments) {
            foreach (var secondaryRoller in primaryRoller.Secondary) {
                int count = arguments.Count == 0 ? secondaryRoller.Cryptex.Tapes.Count : arguments[0].To<int>();
                return secondaryRoller.MoveDown(count).Yield();
            }
            return Array.Empty<Record>();
        }
    }
    private static Statement hopDown = new MoveDownFunction();
    public static Statement HopDown => hopDown;

    private sealed class MoveLeftFunction : Statement {
        public MoveLeftFunction() : base(LC.Temp("mvl"), new Parameter(LC.Temp("steps"), TapeValueType.Int)) { }

        protected override IEnumerable<Record> ExecuteInternal(ExecutionContext ec, InstructionRoller primaryRoller, IReadOnlyList<TapeValue> arguments) {
            foreach (var secondaryRoller in primaryRoller.Secondary) {
                return secondaryRoller.MoveLeft(arguments[0].To<int>(), TargetInfo.ActionType.Move).Yield();
            }
            return Array.Empty<Record>();
        }
    }
    private static Statement moveLeft = new MoveLeftFunction();
    public static Statement MoveLeft => moveLeft;

    private sealed class MoveRightFunction : Statement {
        public MoveRightFunction() : base(LC.Temp("mvr"), new Parameter(LC.Temp("steps"), TapeValueType.Int)) { }

        protected override IEnumerable<Record> ExecuteInternal(ExecutionContext ec, InstructionRoller primaryRoller, IReadOnlyList<TapeValue> arguments) {
            foreach (var secondaryRoller in primaryRoller.Secondary) {
                return secondaryRoller.MoveRight(arguments[0].To<int>(), TargetInfo.ActionType.Move).Yield();
            }
            return Array.Empty<Record>();
        }
    }
    private static Statement moveRight = new MoveRightFunction();
    public static Statement MoveRight => moveRight;

    private sealed class ShiftLeftFunction : Statement {
        public ShiftLeftFunction() : base(LC.Temp("shl"),
            new Parameter(LC.Temp("steps"), TapeValueType.Int, TapeValueType.Null),
            new Parameter(LC.Temp("frame"), Parameter.Property.Params, TapeValueType.Frame)) { }

        protected override IEnumerable<Record> ExecuteInternal(ExecutionContext ec, InstructionRoller primaryRoller, IReadOnlyList<TapeValue> arguments) {
            foreach (var secondaryRoller in primaryRoller.Secondary) {
                if (arguments[0].Type != TapeValueType.Null) {
                    foreach (var arg in arguments.Skip(1)) {
                        yield return secondaryRoller.ShiftLeft(arg.To<int>(), arguments[0].To<int>());
                    }
                }
            }
        }
    }
    private static Statement shiftLeft = new ShiftLeftFunction();
    public static Statement ShiftLeft => shiftLeft;

    private sealed class ShiftRightFunction : Statement {
        public ShiftRightFunction() : base(LC.Temp("shr"),
            new Parameter(LC.Temp("steps"), TapeValueType.Int, TapeValueType.Null),
            new Parameter(LC.Temp("frame"), Parameter.Property.Params, TapeValueType.Frame)) { }

        protected override IEnumerable<Record> ExecuteInternal(ExecutionContext ec, InstructionRoller primaryRoller, IReadOnlyList<TapeValue> arguments) {
            foreach (var secondaryRoller in primaryRoller.Secondary) {
                if (arguments[0].Type != TapeValueType.Null) {
                    foreach (var arg in arguments.Skip(1)) {
                        yield return secondaryRoller.ShiftRight(arg.To<int>(), arguments[0].To<int>());
                    }
                }
            }
        }
    }
    private static Statement shiftRight = new ShiftRightFunction();
    public static Statement ShiftRight => shiftRight;

    private sealed class OutputFunction : Statement {
        public OutputFunction() : base(LC.Temp("out"),
            new Parameter(LC.Temp("value"), Parameter.Property.DisallowTypes)) { }

        protected override IEnumerable<Record> ExecuteInternal(ExecutionContext ec, InstructionRoller primaryRoller, IReadOnlyList<TapeValue> arguments) {
            return ec.Output(0, arguments[0]).Yield();
        }
    }
    private static Statement output = new OutputFunction();
    public static Statement Output => output;

    private sealed class JumpFunction : Statement {
        public JumpFunction() : base(LC.Temp("jmp"),
            new Parameter(LC.Temp("offset"), TapeValueType.Int, TapeValueType.Label, TapeValueType.Null),
            new Parameter(LC.Temp("offset_store"), Parameter.Property.Optional, TapeValueType.Frame)) { }


        protected override IEnumerable<Record> ExecuteInternal(ExecutionContext ec, InstructionRoller primaryRoller, IReadOnlyList<TapeValue> arguments) {
            IEnumerable<Record> GetRecords(int offset, int? frameIndex) {
                var records = new List<Record>(2);
                if (offset != 0) {
                    var moveRecord = primaryRoller.MoveRight(offset, TargetInfo.ActionType.Jump);
                    // Currently MoveRightRecord does not do any bounds correction, but check here in case that changes
                    offset = moveRecord.Count;
                    records.Add(moveRecord);
                }
                if (frameIndex.HasValue) {
                    foreach (var secondaryRoller in primaryRoller.Secondary) {
                        records.Add(secondaryRoller.ShiftRight(frameIndex.Value, offset));
                    }
                }
                return records;
            }

            int? storeFrameIndex = null;
            if (arguments.Count > 1) {
                storeFrameIndex = arguments[1].To<int>();
            }

            if (arguments[0].Type == TapeValueType.Int) {
                return GetRecords(arguments[0].To<int>(), storeFrameIndex);
            } else if (arguments[0].Type == TapeValueType.Label) {
                foreach (var labelValue in primaryRoller.Cryptex.GetLabelIndex(arguments[0].To<string>())) {
                    int diff = labelValue - primaryRoller.Offset - 1;
                    return GetRecords(diff, storeFrameIndex);
                }
                // Label not found, ignore the jmp
            }
            return Enumerable.Empty<Record>();
        }
    }
    private static Statement jump = new JumpFunction();
    public static Statement Jump => jump;

    private abstract class BranchFunction : Statement {
        protected BranchFunction(LE name, params Parameter[] parameters) : base(name, parameters) { }

        protected override IEnumerable<Record> ExecuteInternal(ExecutionContext ec, InstructionRoller primaryRoller, IReadOnlyList<TapeValue> arguments) {
            if (!DoNext(arguments)) {
                return GetRecord(ec, primaryRoller);
            }
            return Array.Empty<Record>();
        }

        protected abstract Option<Record> GetRecord(ExecutionContext ec, InstructionRoller primaryRoller);

        protected abstract bool DoNext(IReadOnlyList<TapeValue> arguments);

        protected Option<Record> GetRightRecord(ExecutionContext ec, InstructionRoller primaryRoller) => primaryRoller.MoveRight(1, TargetInfo.ActionType.Jump);
        protected Option<Record> GetDownRecord(ExecutionContext ec, InstructionRoller primaryRoller) => primaryRoller.MoveDown(1);
    }

    private abstract class BranchEqualFunction : BranchFunction {
        protected BranchEqualFunction(LE name) : base(name,
            new Parameter(LC.Temp("operand0"), TypeSet.All),
            new Parameter(LC.Temp("operand1"), TypeSet.All)) { }

        protected override bool DoNext(IReadOnlyList<TapeValue> arguments) => arguments[0].Equals(arguments[1]);
    }

    private abstract class BranchNotEqualFunction : BranchFunction {
        protected BranchNotEqualFunction(LE name) : base(name,
            new Parameter(LC.Temp("operand0"), TypeSet.All),
            new Parameter(LC.Temp("operand1"), TypeSet.All)) { }

        protected override bool DoNext(IReadOnlyList<TapeValue> arguments) => !arguments[0].Equals(arguments[1]);
    }

    private abstract class BranchGreaterFunction : BranchFunction {
        protected BranchGreaterFunction(LE name) : base(name,
            new Parameter(LC.Temp("operand0"), new TypeSet(TapeValueType.Int)),
            new Parameter(LC.Temp("operand1"), new TypeSet(TapeValueType.Int))) { }

        protected override bool DoNext(IReadOnlyList<TapeValue> arguments) => arguments[0].To<int>() > arguments[1].To<int>();
    }

    private abstract class BranchLessFunction : BranchFunction {
        protected BranchLessFunction(LE name) : base(name,
            new Parameter(LC.Temp("operand0"), new TypeSet(TapeValueType.Int)),
            new Parameter(LC.Temp("operand1"), new TypeSet(TapeValueType.Int))) { }

        protected override bool DoNext(IReadOnlyList<TapeValue> arguments) => arguments[0].To<int>() < arguments[1].To<int>();
    }

    private sealed class IfEqualFunction : BranchEqualFunction {
        public IfEqualFunction() : base(LC.Temp("ife")) { }

        protected override Option<Record> GetRecord(ExecutionContext ec, InstructionRoller primaryRoller) => GetRightRecord(ec, primaryRoller);
    }
    private static Statement ifEqual = new IfEqualFunction();
    public static Statement IfEqual => ifEqual;

    private sealed class IfNotEqualFunction : BranchNotEqualFunction {
        public IfNotEqualFunction() : base(LC.Temp("ifn")) { }

        protected override Option<Record> GetRecord(ExecutionContext ec, InstructionRoller primaryRoller) => GetRightRecord(ec, primaryRoller);
    }
    private static Statement ifNotEqual = new IfNotEqualFunction();
    public static Statement IfNotEqual => ifNotEqual;

    private sealed class IfGreaterFunction : BranchGreaterFunction {
        public IfGreaterFunction() : base(LC.Temp("ifg")) { }

        protected override Option<Record> GetRecord(ExecutionContext ec, InstructionRoller primaryRoller) => GetRightRecord(ec, primaryRoller);
    }
    private static Statement ifGreater = new IfGreaterFunction();
    public static Statement IfGreater => ifGreater;

    private sealed class IfLessFunction : BranchLessFunction {
        public IfLessFunction() : base(LC.Temp("ifl")) { }

        protected override Option<Record> GetRecord(ExecutionContext ec, InstructionRoller primaryRoller) => GetRightRecord(ec, primaryRoller);
    }
    private static Statement ifLess = new IfLessFunction();
    public static Statement IfLess => ifLess;


    private sealed class SwitchEqualFunction : BranchEqualFunction {
        public SwitchEqualFunction() : base(LC.Temp("swe")) { }

        protected override Option<Record> GetRecord(ExecutionContext ec, InstructionRoller primaryRoller) => GetRightRecord(ec, primaryRoller);
    }
    private static Statement switchEqual = new SwitchEqualFunction();
    public static Statement SwitchEqual => switchEqual;

    private sealed class SwitchNotEqualFunction : BranchNotEqualFunction {
        public SwitchNotEqualFunction() : base(LC.Temp("swn")) { }

        protected override Option<Record> GetRecord(ExecutionContext ec, InstructionRoller primaryRoller) => GetRightRecord(ec, primaryRoller);
    }
    private static Statement switchNotEqual = new SwitchNotEqualFunction();
    public static Statement SwitchNotEqual => switchNotEqual;

    private sealed class SwitchGreaterFunction : BranchGreaterFunction {
        public SwitchGreaterFunction() : base(LC.Temp("swg")) { }

        protected override Option<Record> GetRecord(ExecutionContext ec, InstructionRoller primaryRoller) => GetRightRecord(ec, primaryRoller);
    }
    private static Statement switchGreater = new SwitchGreaterFunction();
    public static Statement SwitchGreater => switchGreater;

    private sealed class SwitchLessFunction : BranchLessFunction {
        public SwitchLessFunction() : base(LC.Temp("swl")) { }

        protected override Option<Record> GetRecord(ExecutionContext ec, InstructionRoller primaryRoller) => GetRightRecord(ec, primaryRoller);
    }
    private static Statement switchLess = new SwitchLessFunction();
    public static Statement SwitchLess => switchLess;


    private sealed class WriteFunction : Statement {
        public WriteFunction() : base(LC.Temp("put"),
            new Parameter(LC.Temp("value"), Parameter.Property.DisallowTypes),
            new Parameter(LC.Temp("frame"), Parameter.Property.Params, TapeValueType.Frame)) { }

        protected override IEnumerable<Record> ExecuteInternal(ExecutionContext ec, InstructionRoller primaryRoller, IReadOnlyList<TapeValue> arguments) {
            foreach (var arg in arguments.Skip(1)) {
                foreach (var secondaryRoller in primaryRoller.Secondary) {
                    yield return secondaryRoller.Write(arg.To<int>(), arguments[0]);
                }
            }
        }
    }
    private static Statement write = new WriteFunction();
    public static Statement Write => write;

    private sealed class ChangeFunction : Statement {
        public ChangeFunction() : base(LC.Temp("chg"),
            new Parameter(LC.Temp("color"), Parameter.Property.Params, TapeValueType.Color)) { }

        protected override IEnumerable<Record> ExecuteInternal(ExecutionContext ec, InstructionRoller primaryRoller, IReadOnlyList<TapeValue> arguments) {
            foreach (var arg in arguments) {
                var color = arg.To<ColorId>();
                // Skip the color if there already exists a Primary Roller of that color (including the color we already are)
                if (!ec.Workspace.GetPrimaryRoller(color).HasValue) {
                    return primaryRoller.ChangeColor(color).Yield();
                }
            }
            return Array.Empty<Record>();
        }
    }
    private static Statement change = new ChangeFunction();
    public static Statement Change => change;

    private sealed class EndFunction : Statement {
        public EndFunction() : base(LC.Temp("end")) { }

        protected override IEnumerable<Record> ExecuteInternal(ExecutionContext ec, InstructionRoller primaryRoller, IReadOnlyList<TapeValue> arguments) {
            if (primaryRoller.TapeIndex > 0) {
                return new Roller.MoveUpRecord(primaryRoller.Cryptex.Id, primaryRoller.Id, 1).Yield();
            }
            return Array.Empty<Record>();
        }
    }
}
