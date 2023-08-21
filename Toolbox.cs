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
using LD = LocalizationDefault;

public sealed partial class Toolbox {
    private ImmutableList<ToolboxPage> pages;
    private int pageIndex;

    public Toolbox(params ToolboxPage[] pages) {
        this.pages = pages.ToImmutableList();
    }

    public void SelectPage(int pageIndex) {
        var oldIndex = this.pageIndex;
        this.pageIndex = pageIndex;
        foreach (var view in View) {
            view.OnUpdateIndex(oldIndex, pageIndex);
        }
    }

    private static readonly Toolbox @default = 
        new Toolbox(
            new ToolboxPage(LC.Temp("Components"),
                new ToolboxItem<CryptexInsertPoint, Cryptex>(L.Inline(nameof(LD.Platen)),      Overseer.GlobalAssets.CryptexIcon,      CryptexInserter.Instance,   CryptexInsertSource.Instance),
                new ToolboxItem<TapeInsertPoint, Tape>      (LC.Temp("Blank Tape"),         Overseer.GlobalAssets.BlankTapeIcon,    TapeInserter.Instance,      new TapeInsertSource(Tape.SequenceType.Blank)),
                new ToolboxItem<TapeInsertPoint, Tape>      (LC.Temp("Number Tape"),        Overseer.GlobalAssets.NumberTapeIcon,   TapeInserter.Instance,      new TapeInsertSource(Tape.SequenceType.Integers)),
                new ToolboxItem<TapeInsertPoint, Tape>      (LC.Temp("Alpha Tape"),         Overseer.GlobalAssets.NumberTapeIcon,   TapeInserter.Instance,      new TapeInsertSource(Tape.SequenceType.Alphabet)),
                new ToolboxItem<RollerInsertPoint, Roller>  (LC.Temp("Head"),               Overseer.GlobalAssets.CryptexIcon,      RollerInserter.Instance,    new RollerInsertSource(true)),
                new ToolboxItem<RollerInsertPoint, Roller>  (LC.Temp("Auxiliary Head"),     Overseer.GlobalAssets.CryptexIcon,      RollerInserter.Instance,    new RollerInsertSource(false)),
                new ToolboxItem<FrameInsertPoint, Frame>    (LC.Temp("Read Frame"),         Overseer.GlobalAssets.CryptexIcon,      FrameInserter.Instance,     new FrameInsertSource(FrameFlags.FrameRead)),
                new ToolboxItem<FrameInsertPoint, Frame>    (LC.Temp("Write Frame"),        Overseer.GlobalAssets.CryptexIcon,      FrameInserter.Instance,     new FrameInsertSource(FrameFlags.FrameReadWrite))
            ),
            new ToolboxPage(LC.Temp("Labels"),
                new ToolboxItem<LabelInsertPoint, Label>(LC.Temp("Label 0"), Overseer.GlobalAssets.BlankTapeIcon, LabelInserter.Instance, new LabelInsertSource(LC.Temp("0"))),
                new ToolboxItem<LabelInsertPoint, Label>(LC.Temp("Label 1"), Overseer.GlobalAssets.BlankTapeIcon, LabelInserter.Instance, new LabelInsertSource(LC.Temp("1"))),
                new ToolboxItem<LabelInsertPoint, Label>(LC.Temp("Label 2"), Overseer.GlobalAssets.BlankTapeIcon, LabelInserter.Instance, new LabelInsertSource(LC.Temp("2"))),
                new ToolboxItem<LabelInsertPoint, Label>(LC.Temp("Label 3"), Overseer.GlobalAssets.BlankTapeIcon, LabelInserter.Instance, new LabelInsertSource(LC.Temp("3"))),
                new ToolboxItem<LabelInsertPoint, Label>(LC.Temp("Label 4"), Overseer.GlobalAssets.BlankTapeIcon, LabelInserter.Instance, new LabelInsertSource(LC.Temp("4"))),
                new ToolboxItem<LabelInsertPoint, Label>(LC.Temp("Label 5"), Overseer.GlobalAssets.BlankTapeIcon, LabelInserter.Instance, new LabelInsertSource(LC.Temp("5"))),
                new ToolboxItem<LabelInsertPoint, Label>(LC.Temp("Label 6"), Overseer.GlobalAssets.BlankTapeIcon, LabelInserter.Instance, new LabelInsertSource(LC.Temp("6"))),
                new ToolboxItem<LabelInsertPoint, Label>(LC.Temp("Label 7"), Overseer.GlobalAssets.BlankTapeIcon, LabelInserter.Instance, new LabelInsertSource(LC.Temp("7"))),
                new ToolboxItem<LabelInsertPoint, Label>(LC.Temp("Label 8"), Overseer.GlobalAssets.BlankTapeIcon, LabelInserter.Instance, new LabelInsertSource(LC.Temp("8"))),
                new ToolboxItem<LabelInsertPoint, Label>(LC.Temp("Label 9"), Overseer.GlobalAssets.BlankTapeIcon, LabelInserter.Instance, new LabelInsertSource(LC.Temp("9")))
            ),
            new ToolboxPage(LC.Temp("Instruction Values"),
                new ToolboxItem<TapeValueInsertPoint, TapeValueWrapper>(LC.Temp("mvu"), Overseer.GlobalAssets.BlankTapeIcon, TapeValueInserter.Instance, new TapeValueInsertSource(new TapeValueOpcode("mvu"))),
                new ToolboxItem<TapeValueInsertPoint, TapeValueWrapper>(LC.Temp("mvd"), Overseer.GlobalAssets.BlankTapeIcon, TapeValueInserter.Instance, new TapeValueInsertSource(new TapeValueOpcode("mvd"))),
                new ToolboxItem<TapeValueInsertPoint, TapeValueWrapper>(LC.Temp("mvl"), Overseer.GlobalAssets.BlankTapeIcon, TapeValueInserter.Instance, new TapeValueInsertSource(new TapeValueOpcode("mvl"))),
                new ToolboxItem<TapeValueInsertPoint, TapeValueWrapper>(LC.Temp("mvr"), Overseer.GlobalAssets.BlankTapeIcon, TapeValueInserter.Instance, new TapeValueInsertSource(new TapeValueOpcode("mvr"))),
                new ToolboxItem<TapeValueInsertPoint, TapeValueWrapper>(LC.Temp("shl"), Overseer.GlobalAssets.BlankTapeIcon, TapeValueInserter.Instance, new TapeValueInsertSource(new TapeValueOpcode("shl"))),
                new ToolboxItem<TapeValueInsertPoint, TapeValueWrapper>(LC.Temp("shr"), Overseer.GlobalAssets.BlankTapeIcon, TapeValueInserter.Instance, new TapeValueInsertSource(new TapeValueOpcode("shr"))),
                new ToolboxItem<TapeValueInsertPoint, TapeValueWrapper>(LC.Temp("jmp"), Overseer.GlobalAssets.BlankTapeIcon, TapeValueInserter.Instance, new TapeValueInsertSource(new TapeValueOpcode("jmp"))),
                new ToolboxItem<TapeValueInsertPoint, TapeValueWrapper>(LC.Temp("put"), Overseer.GlobalAssets.BlankTapeIcon, TapeValueInserter.Instance, new TapeValueInsertSource(new TapeValueOpcode("put"))),
                new ToolboxItem<TapeValueInsertPoint, TapeValueWrapper>(LC.Temp("ife"), Overseer.GlobalAssets.BlankTapeIcon, TapeValueInserter.Instance, new TapeValueInsertSource(new TapeValueOpcode("ife"))),
                new ToolboxItem<TapeValueInsertPoint, TapeValueWrapper>(LC.Temp("ifn"), Overseer.GlobalAssets.BlankTapeIcon, TapeValueInserter.Instance, new TapeValueInsertSource(new TapeValueOpcode("ifn"))),
                new ToolboxItem<TapeValueInsertPoint, TapeValueWrapper>(LC.Temp("ifg"), Overseer.GlobalAssets.BlankTapeIcon, TapeValueInserter.Instance, new TapeValueInsertSource(new TapeValueOpcode("ifg"))),
                new ToolboxItem<TapeValueInsertPoint, TapeValueWrapper>(LC.Temp("ifl"), Overseer.GlobalAssets.BlankTapeIcon, TapeValueInserter.Instance, new TapeValueInsertSource(new TapeValueOpcode("ifl"))),
                new ToolboxItem<TapeValueInsertPoint, TapeValueWrapper>(LC.Temp("out"), Overseer.GlobalAssets.BlankTapeIcon, TapeValueInserter.Instance, new TapeValueInsertSource(new TapeValueOpcode("out")))
            ),
            new ToolboxPage(LC.Temp("Integer Values"),
                new ToolboxItem<TapeValueInsertPoint, TapeValueWrapper>(LC.Temp("-10"), Overseer.GlobalAssets.BlankTapeIcon, TapeValueInserter.Instance, new TapeValueInsertSource(new TapeValueInt(-10))),
                new ToolboxItem<TapeValueInsertPoint, TapeValueWrapper>(LC.Temp("-9"), Overseer.GlobalAssets.BlankTapeIcon, TapeValueInserter.Instance, new TapeValueInsertSource(new TapeValueInt(-9))),
                new ToolboxItem<TapeValueInsertPoint, TapeValueWrapper>(LC.Temp("-8"), Overseer.GlobalAssets.BlankTapeIcon, TapeValueInserter.Instance, new TapeValueInsertSource(new TapeValueInt(-8))),
                new ToolboxItem<TapeValueInsertPoint, TapeValueWrapper>(LC.Temp("-7"), Overseer.GlobalAssets.BlankTapeIcon, TapeValueInserter.Instance, new TapeValueInsertSource(new TapeValueInt(-7))),
                new ToolboxItem<TapeValueInsertPoint, TapeValueWrapper>(LC.Temp("-6"), Overseer.GlobalAssets.BlankTapeIcon, TapeValueInserter.Instance, new TapeValueInsertSource(new TapeValueInt(-6))),
                new ToolboxItem<TapeValueInsertPoint, TapeValueWrapper>(LC.Temp("-5"), Overseer.GlobalAssets.BlankTapeIcon, TapeValueInserter.Instance, new TapeValueInsertSource(new TapeValueInt(-5))),
                new ToolboxItem<TapeValueInsertPoint, TapeValueWrapper>(LC.Temp("-4"), Overseer.GlobalAssets.BlankTapeIcon, TapeValueInserter.Instance, new TapeValueInsertSource(new TapeValueInt(-4))),
                new ToolboxItem<TapeValueInsertPoint, TapeValueWrapper>(LC.Temp("-3"), Overseer.GlobalAssets.BlankTapeIcon, TapeValueInserter.Instance, new TapeValueInsertSource(new TapeValueInt(-3))),
                new ToolboxItem<TapeValueInsertPoint, TapeValueWrapper>(LC.Temp("-2"), Overseer.GlobalAssets.BlankTapeIcon, TapeValueInserter.Instance, new TapeValueInsertSource(new TapeValueInt(-2))),
                new ToolboxItem<TapeValueInsertPoint, TapeValueWrapper>(LC.Temp("-1"), Overseer.GlobalAssets.BlankTapeIcon, TapeValueInserter.Instance, new TapeValueInsertSource(new TapeValueInt(-1))),
                new ToolboxItem<TapeValueInsertPoint, TapeValueWrapper>(LC.Temp("0"), Overseer.GlobalAssets.BlankTapeIcon, TapeValueInserter.Instance, new TapeValueInsertSource(new TapeValueInt(0))),
                new ToolboxItem<TapeValueInsertPoint, TapeValueWrapper>(LC.Temp("1"), Overseer.GlobalAssets.BlankTapeIcon, TapeValueInserter.Instance, new TapeValueInsertSource(new TapeValueInt(1))),
                new ToolboxItem<TapeValueInsertPoint, TapeValueWrapper>(LC.Temp("2"), Overseer.GlobalAssets.BlankTapeIcon, TapeValueInserter.Instance, new TapeValueInsertSource(new TapeValueInt(2))),
                new ToolboxItem<TapeValueInsertPoint, TapeValueWrapper>(LC.Temp("3"), Overseer.GlobalAssets.BlankTapeIcon, TapeValueInserter.Instance, new TapeValueInsertSource(new TapeValueInt(3))),
                new ToolboxItem<TapeValueInsertPoint, TapeValueWrapper>(LC.Temp("4"), Overseer.GlobalAssets.BlankTapeIcon, TapeValueInserter.Instance, new TapeValueInsertSource(new TapeValueInt(4))),
                new ToolboxItem<TapeValueInsertPoint, TapeValueWrapper>(LC.Temp("5"), Overseer.GlobalAssets.BlankTapeIcon, TapeValueInserter.Instance, new TapeValueInsertSource(new TapeValueInt(5))),
                new ToolboxItem<TapeValueInsertPoint, TapeValueWrapper>(LC.Temp("6"), Overseer.GlobalAssets.BlankTapeIcon, TapeValueInserter.Instance, new TapeValueInsertSource(new TapeValueInt(6))),
                new ToolboxItem<TapeValueInsertPoint, TapeValueWrapper>(LC.Temp("7"), Overseer.GlobalAssets.BlankTapeIcon, TapeValueInserter.Instance, new TapeValueInsertSource(new TapeValueInt(7))),
                new ToolboxItem<TapeValueInsertPoint, TapeValueWrapper>(LC.Temp("8"), Overseer.GlobalAssets.BlankTapeIcon, TapeValueInserter.Instance, new TapeValueInsertSource(new TapeValueInt(8))),
                new ToolboxItem<TapeValueInsertPoint, TapeValueWrapper>(LC.Temp("9"), Overseer.GlobalAssets.BlankTapeIcon, TapeValueInserter.Instance, new TapeValueInsertSource(new TapeValueInt(9))),
                new ToolboxItem<TapeValueInsertPoint, TapeValueWrapper>(LC.Temp("10"), Overseer.GlobalAssets.BlankTapeIcon, TapeValueInserter.Instance, new TapeValueInsertSource(new TapeValueInt(10)))
            ),
            new ToolboxPage(LC.Temp("Misc Values"),
                new ToolboxItem<TapeValueInsertPoint, TapeValueWrapper>(LC.Temp("blank"), Overseer.GlobalAssets.BlankTapeIcon, TapeValueInserter.Instance, new TapeValueInsertSource(TapeValueNull.Instance)),
                new ToolboxItem<TapeValueInsertPoint, TapeValueWrapper>(LC.Temp("f0"), Overseer.GlobalAssets.BlankTapeIcon, TapeValueInserter.Instance, new TapeValueInsertSource(new TapeValueFrame(0))),
                new ToolboxItem<TapeValueInsertPoint, TapeValueWrapper>(LC.Temp("f1"), Overseer.GlobalAssets.BlankTapeIcon, TapeValueInserter.Instance, new TapeValueInsertSource(new TapeValueFrame(1))),
                new ToolboxItem<TapeValueInsertPoint, TapeValueWrapper>(LC.Temp("f2"), Overseer.GlobalAssets.BlankTapeIcon, TapeValueInserter.Instance, new TapeValueInsertSource(new TapeValueFrame(2))),
                new ToolboxItem<TapeValueInsertPoint, TapeValueWrapper>(LC.Temp("@f0"), Overseer.GlobalAssets.BlankTapeIcon, TapeValueInserter.Instance, new TapeValueInsertSource(new TapeValueFrameRead(0))),
                new ToolboxItem<TapeValueInsertPoint, TapeValueWrapper>(LC.Temp("@f1"), Overseer.GlobalAssets.BlankTapeIcon, TapeValueInserter.Instance, new TapeValueInsertSource(new TapeValueFrameRead(1))),
                new ToolboxItem<TapeValueInsertPoint, TapeValueWrapper>(LC.Temp("@f2"), Overseer.GlobalAssets.BlankTapeIcon, TapeValueInserter.Instance, new TapeValueInsertSource(new TapeValueFrameRead(2))),
                new ToolboxItem<TapeValueInsertPoint, TapeValueWrapper>(LC.Temp("c0"), Overseer.GlobalAssets.BlankTapeIcon, TapeValueInserter.Instance, new TapeValueInsertSource(new TapeValueChannel(0)))//,
                //new ToolboxItem<TapeValueInsertPoint, TapeValueWrapper>(LC.Temp("c1"),    Overseer.GlobalAssets.BlankTapeIcon, TapeValueInserter.Instance, new TapeValueInsertSource(new TapeValueChannel(1))),
                //new ToolboxItem<TapeValueInsertPoint, TapeValueWrapper>(LC.Temp("c2"),    Overseer.GlobalAssets.BlankTapeIcon, TapeValueInserter.Instance, new TapeValueInsertSource(new TapeValueChannel(2)))
            ),
            new ToolboxPage(LC.Temp("Color Values"),
                new ToolboxItem<TapeValueInsertPoint, TapeValueWrapper>(LC.Temp("red"), Overseer.GlobalAssets.BlankTapeIcon, TapeValueInserter.Instance, new TapeValueInsertSource(new TapeValueColor(ColorId.Red))),
                new ToolboxItem<TapeValueInsertPoint, TapeValueWrapper>(LC.Temp("green"), Overseer.GlobalAssets.BlankTapeIcon, TapeValueInserter.Instance, new TapeValueInsertSource(new TapeValueColor(ColorId.Green))),
                new ToolboxItem<TapeValueInsertPoint, TapeValueWrapper>(LC.Temp("blue"), Overseer.GlobalAssets.BlankTapeIcon, TapeValueInserter.Instance, new TapeValueInsertSource(new TapeValueColor(ColorId.Blue))),
                new ToolboxItem<TapeValueInsertPoint, TapeValueWrapper>(LC.Temp("yellow"), Overseer.GlobalAssets.BlankTapeIcon, TapeValueInserter.Instance, new TapeValueInsertSource(new TapeValueColor(ColorId.Yellow)))
            ),
            new ToolboxPage(LC.Temp("Label Values"),
                new ToolboxItem<TapeValueInsertPoint, TapeValueWrapper>(LC.Temp(":0"), Overseer.GlobalAssets.BlankTapeIcon, TapeValueInserter.Instance, new TapeValueInsertSource(new TapeValueLabel("0"))),
                new ToolboxItem<TapeValueInsertPoint, TapeValueWrapper>(LC.Temp(":1"), Overseer.GlobalAssets.BlankTapeIcon, TapeValueInserter.Instance, new TapeValueInsertSource(new TapeValueLabel("1"))),
                new ToolboxItem<TapeValueInsertPoint, TapeValueWrapper>(LC.Temp(":2"), Overseer.GlobalAssets.BlankTapeIcon, TapeValueInserter.Instance, new TapeValueInsertSource(new TapeValueLabel("2"))),
                new ToolboxItem<TapeValueInsertPoint, TapeValueWrapper>(LC.Temp(":3"), Overseer.GlobalAssets.BlankTapeIcon, TapeValueInserter.Instance, new TapeValueInsertSource(new TapeValueLabel("3"))),
                new ToolboxItem<TapeValueInsertPoint, TapeValueWrapper>(LC.Temp(":4"), Overseer.GlobalAssets.BlankTapeIcon, TapeValueInserter.Instance, new TapeValueInsertSource(new TapeValueLabel("4"))),
                new ToolboxItem<TapeValueInsertPoint, TapeValueWrapper>(LC.Temp(":5"), Overseer.GlobalAssets.BlankTapeIcon, TapeValueInserter.Instance, new TapeValueInsertSource(new TapeValueLabel("5"))),
                new ToolboxItem<TapeValueInsertPoint, TapeValueWrapper>(LC.Temp(":6"), Overseer.GlobalAssets.BlankTapeIcon, TapeValueInserter.Instance, new TapeValueInsertSource(new TapeValueLabel("6"))),
                new ToolboxItem<TapeValueInsertPoint, TapeValueWrapper>(LC.Temp(":7"), Overseer.GlobalAssets.BlankTapeIcon, TapeValueInserter.Instance, new TapeValueInsertSource(new TapeValueLabel("7"))),
                new ToolboxItem<TapeValueInsertPoint, TapeValueWrapper>(LC.Temp(":8"), Overseer.GlobalAssets.BlankTapeIcon, TapeValueInserter.Instance, new TapeValueInsertSource(new TapeValueLabel("8"))),
                new ToolboxItem<TapeValueInsertPoint, TapeValueWrapper>(LC.Temp(":9"), Overseer.GlobalAssets.BlankTapeIcon, TapeValueInserter.Instance, new TapeValueInsertSource(new TapeValueLabel("9")))
            ),
            new ToolboxPage(LC.Temp("Char Values"),
                new ToolboxItem<TapeValueInsertPoint, TapeValueWrapper>(LC.Temp("a"), Overseer.GlobalAssets.BlankTapeIcon, TapeValueInserter.Instance, new TapeValueInsertSource(new TapeValueChar('a'))),
                new ToolboxItem<TapeValueInsertPoint, TapeValueWrapper>(LC.Temp("b"), Overseer.GlobalAssets.BlankTapeIcon, TapeValueInserter.Instance, new TapeValueInsertSource(new TapeValueChar('b'))),
                new ToolboxItem<TapeValueInsertPoint, TapeValueWrapper>(LC.Temp("c"), Overseer.GlobalAssets.BlankTapeIcon, TapeValueInserter.Instance, new TapeValueInsertSource(new TapeValueChar('c'))),
                new ToolboxItem<TapeValueInsertPoint, TapeValueWrapper>(LC.Temp("d"), Overseer.GlobalAssets.BlankTapeIcon, TapeValueInserter.Instance, new TapeValueInsertSource(new TapeValueChar('d'))),
                new ToolboxItem<TapeValueInsertPoint, TapeValueWrapper>(LC.Temp("e"), Overseer.GlobalAssets.BlankTapeIcon, TapeValueInserter.Instance, new TapeValueInsertSource(new TapeValueChar('e'))),
                new ToolboxItem<TapeValueInsertPoint, TapeValueWrapper>(LC.Temp("f"), Overseer.GlobalAssets.BlankTapeIcon, TapeValueInserter.Instance, new TapeValueInsertSource(new TapeValueChar('f'))),
                new ToolboxItem<TapeValueInsertPoint, TapeValueWrapper>(LC.Temp("g"), Overseer.GlobalAssets.BlankTapeIcon, TapeValueInserter.Instance, new TapeValueInsertSource(new TapeValueChar('g'))),
                new ToolboxItem<TapeValueInsertPoint, TapeValueWrapper>(LC.Temp("h"), Overseer.GlobalAssets.BlankTapeIcon, TapeValueInserter.Instance, new TapeValueInsertSource(new TapeValueChar('h'))),
                new ToolboxItem<TapeValueInsertPoint, TapeValueWrapper>(LC.Temp("i"), Overseer.GlobalAssets.BlankTapeIcon, TapeValueInserter.Instance, new TapeValueInsertSource(new TapeValueChar('i'))),
                new ToolboxItem<TapeValueInsertPoint, TapeValueWrapper>(LC.Temp("j"), Overseer.GlobalAssets.BlankTapeIcon, TapeValueInserter.Instance, new TapeValueInsertSource(new TapeValueChar('j'))),
                new ToolboxItem<TapeValueInsertPoint, TapeValueWrapper>(LC.Temp("k"), Overseer.GlobalAssets.BlankTapeIcon, TapeValueInserter.Instance, new TapeValueInsertSource(new TapeValueChar('k'))),
                new ToolboxItem<TapeValueInsertPoint, TapeValueWrapper>(LC.Temp("l"), Overseer.GlobalAssets.BlankTapeIcon, TapeValueInserter.Instance, new TapeValueInsertSource(new TapeValueChar('l'))),
                new ToolboxItem<TapeValueInsertPoint, TapeValueWrapper>(LC.Temp("m"), Overseer.GlobalAssets.BlankTapeIcon, TapeValueInserter.Instance, new TapeValueInsertSource(new TapeValueChar('m'))),
                new ToolboxItem<TapeValueInsertPoint, TapeValueWrapper>(LC.Temp("n"), Overseer.GlobalAssets.BlankTapeIcon, TapeValueInserter.Instance, new TapeValueInsertSource(new TapeValueChar('n'))),
                new ToolboxItem<TapeValueInsertPoint, TapeValueWrapper>(LC.Temp("o"), Overseer.GlobalAssets.BlankTapeIcon, TapeValueInserter.Instance, new TapeValueInsertSource(new TapeValueChar('o'))),
                new ToolboxItem<TapeValueInsertPoint, TapeValueWrapper>(LC.Temp("p"), Overseer.GlobalAssets.BlankTapeIcon, TapeValueInserter.Instance, new TapeValueInsertSource(new TapeValueChar('p'))),
                new ToolboxItem<TapeValueInsertPoint, TapeValueWrapper>(LC.Temp("q"), Overseer.GlobalAssets.BlankTapeIcon, TapeValueInserter.Instance, new TapeValueInsertSource(new TapeValueChar('q'))),
                new ToolboxItem<TapeValueInsertPoint, TapeValueWrapper>(LC.Temp("r"), Overseer.GlobalAssets.BlankTapeIcon, TapeValueInserter.Instance, new TapeValueInsertSource(new TapeValueChar('r'))),
                new ToolboxItem<TapeValueInsertPoint, TapeValueWrapper>(LC.Temp("s"), Overseer.GlobalAssets.BlankTapeIcon, TapeValueInserter.Instance, new TapeValueInsertSource(new TapeValueChar('s'))),
                new ToolboxItem<TapeValueInsertPoint, TapeValueWrapper>(LC.Temp("t"), Overseer.GlobalAssets.BlankTapeIcon, TapeValueInserter.Instance, new TapeValueInsertSource(new TapeValueChar('t'))),
                new ToolboxItem<TapeValueInsertPoint, TapeValueWrapper>(LC.Temp("u"), Overseer.GlobalAssets.BlankTapeIcon, TapeValueInserter.Instance, new TapeValueInsertSource(new TapeValueChar('u'))),
                new ToolboxItem<TapeValueInsertPoint, TapeValueWrapper>(LC.Temp("v"), Overseer.GlobalAssets.BlankTapeIcon, TapeValueInserter.Instance, new TapeValueInsertSource(new TapeValueChar('v'))),
                new ToolboxItem<TapeValueInsertPoint, TapeValueWrapper>(LC.Temp("w"), Overseer.GlobalAssets.BlankTapeIcon, TapeValueInserter.Instance, new TapeValueInsertSource(new TapeValueChar('w'))),
                new ToolboxItem<TapeValueInsertPoint, TapeValueWrapper>(LC.Temp("x"), Overseer.GlobalAssets.BlankTapeIcon, TapeValueInserter.Instance, new TapeValueInsertSource(new TapeValueChar('x'))),
                new ToolboxItem<TapeValueInsertPoint, TapeValueWrapper>(LC.Temp("y"), Overseer.GlobalAssets.BlankTapeIcon, TapeValueInserter.Instance, new TapeValueInsertSource(new TapeValueChar('y'))),
                new ToolboxItem<TapeValueInsertPoint, TapeValueWrapper>(LC.Temp("z"), Overseer.GlobalAssets.BlankTapeIcon, TapeValueInserter.Instance, new TapeValueInsertSource(new TapeValueChar('z')))
            )
        );

    public static Toolbox Default => @default;
}

public sealed partial class ToolboxPage {
    private LE name;
    private ImmutableList<ToolboxItem> items;

    public ToolboxPage(LE name, params ToolboxItem[] items) {
        this.name = name;
        this.items = items.ToImmutableList();
    }

    public LE Name => name;
}

public abstract partial class ToolboxItem {
    private LE name;
    private Sprite icon;
    private Transform itemView;

    protected ToolboxItem(LE name, Sprite icon) {
        this.name = name;
        this.icon = icon;
    }

    public LE Name => name;

    public abstract bool TryPoint(RaycastHit hit);

    public abstract DragOperation CreateDragOperation(WorkspaceFull workspace, Vector3 cursorPosition);

    protected abstract InsertSource Source { get; }
}

public abstract class Inserter {
    public abstract Option<InsertPoint> GetInsertPoint(IEnumerable<RaycastHit2D> hits, Option<Transform> ignore = default);
}

public abstract class InserterShim<TPoint> : Inserter where TPoint : InsertPoint {
    public sealed override Option<InsertPoint> GetInsertPoint(IEnumerable<RaycastHit2D> hits, Option<Transform> ignore = default) => GetInsertPointShim(hits, ignore).Cast<TPoint, InsertPoint>();

    protected abstract Option<TPoint> GetInsertPointShim(IEnumerable<RaycastHit2D> hits, Option<Transform> ignore = default);
}

public abstract class Inserter<TPoint, TModel> : InserterShim<TPoint> where TPoint : InsertPoint where TModel : IModelCo<MonoBehaviour> {
    protected override Option<TPoint> GetInsertPointShim(IEnumerable<RaycastHit2D> hits, Option<Transform> ignore = default) => GetInsertPoint(hits, ignore);

    public new Option<TPoint> GetInsertPoint(IEnumerable<RaycastHit2D> hits, Option<Transform> ignore = default) {
        foreach (var hit in hits) {
            var ip = hit.collider.GetComponent<TPoint>();
            if (ip != null) {
                bool skip = false;
                foreach (var ignoreValue in ignore) {
                    // HACK avoid trying to insert an object into its own InsertPoint.
                    // Assumes objects of the same type are "siblings", never "parents"/"children"
                    skip = ip.transform.IsChildOf(ignoreValue) || ignoreValue.transform.IsChildOf(ip.transform);
                }
                if (skip) {
                    continue;
                }
                ip.OnHoverHack(hit.point);
                return ip;
            }
        }
        return Option.None;
    }

    public bool Insert(TPoint insertPoint, TModel item) => InsertInternal(insertPoint, item, checkOnly: false);
    public bool CanInsert(TPoint insertPoint, TModel item) => InsertInternal(insertPoint, item, checkOnly: true);

    protected abstract bool InsertInternal(TPoint insertPoint, TModel item, bool checkOnly);

    public bool Move(ModifyPoint<TModel> modifyPoint, TPoint insertPoint) => MoveInternal(modifyPoint, insertPoint, checkOnly: false);
    public bool CanMove(ModifyPoint<TModel> modifyPoint, TPoint insertPoint) => MoveInternal(modifyPoint, insertPoint, checkOnly: true);

    private bool MoveInternal(ModifyPoint<TModel> modifyPoint, TPoint insertPoint, bool checkOnly) {
        var model = modifyPoint.Model;
        var view = model.MakeView();
        if (modifyPoint.CanDelete(true) && InsertInternal(insertPoint, model, checkOnly: true)) {
            // HACK If a part were dragged to it's own coresponding insert point, it would first be deleted, and the insert point would be invalid for the insert
            // Instead, check to make sure the insertion point and modification point are not parent and child, or vice versa. This currently holds true for the object hierarchy
            if (!insertPoint.transform.IsChildOf(view.transform) && !view.transform.IsChildOf(insertPoint.transform)) {
                if (!checkOnly) {
                    using (insertPoint.Workspace.NewModificationBatchFrame()) {
                        var result = modifyPoint.Delete(true);
                        result &= Insert(insertPoint, model);
                        Assert.True(result);
                    }
                }
                return true;
            }
        }
        
        return false;
    }

    public void Snap(TPoint insertPoint, TModel model, float z, Vector3 offset = default) {
        model.MakeView().transform.position = (insertPoint.Origin - offset).WithZ(z);
    }
}

public abstract class InsertSource {
    public abstract int Cost(WorkspaceFull workspace, CostOverride costs);
}

public abstract class InsertSource<TModel> : InsertSource {
    public abstract TModel Get(WorkspaceFull workspace);
}

public sealed class TapeValueInsertSource : InsertSource<TapeValueWrapper> {
    private TapeValue value;
    public TapeValueInsertSource(TapeValue value) {
        this.value = value;
    }

    public override TapeValueWrapper Get(WorkspaceFull workspace) => value.ToWrapper();

    public override int Cost(WorkspaceFull workspace, CostOverride costs) => costs[CostType.Write] + costs[value.Type.CostType];
}

public sealed class LabelInsertSource : InsertSource<Label> {
    private LE name;

    public LabelInsertSource(LE name) {
        this.name = name;
    }

    public override Label Get(WorkspaceFull workspace) => new Label(Overseer.NewGuid(), name);

    public override int Cost(WorkspaceFull workspace, CostOverride costs) => 0;
}

public sealed class ToolboxItem<TPoint, TModel> : ToolboxItem where TPoint : InsertPoint where TModel : IModelCo<MonoBehaviour> {
    private Inserter<TPoint, TModel> insert;
    private InsertSource<TModel> source;

    public ToolboxItem(LE name, Sprite icon, Inserter<TPoint, TModel> insert, InsertSource<TModel> source) : base(name, icon) {
        this.insert = insert;
        this.source = source;
    }

    public override bool TryPoint(RaycastHit hit) {
        var t = hit.collider.GetComponent<TPoint>();
        if (t != null) {
            return Insert(t);
        }
        return false;
    }

    private bool Insert(TPoint insertPoint) => insert.Insert(insertPoint, source.Get(insertPoint.Workspace));

    public override DragOperation CreateDragOperation(WorkspaceFull workspace, Vector3 cursorPosition) {
        var model = source.Get(workspace);
        return new DragOperation<TModel>(new ToolboxDragDefinition<TModel, TPoint>(insert), Vector3.zero, model);
    }

    protected override InsertSource Source => source;
}

public sealed class CryptexInsertSource : InsertSource<Cryptex> {
    // Make this private to avoid instantiating a ton of objects for no reason
    private CryptexInsertSource() { }

    public override Cryptex Get(WorkspaceFull workspace) {
        var locked = LockType.None;
#if DEBUG
        locked = Overseer.LockType;
#endif // DEBUG
        var guid = Overseer.NewGuid();
        var tapeGuid = Overseer.NewGuid();
        var cryptex = new Cryptex(guid, workspace, Vector2.zero, locked);
        var tape = new Tape(tapeGuid, Tape.SequenceType.Blank);
        cryptex.AddTape(0, tape);
        return cryptex;
    }

    public override int Cost(WorkspaceFull workspace, CostOverride costs) => costs[CostType.Cryptex];

    private static CryptexInsertSource instance = new CryptexInsertSource();
    public static CryptexInsertSource Instance => instance;
}

public sealed class CryptexInserter : Inserter<CryptexInsertPoint, Cryptex> {
    // Make this private to avoid instantiating a ton of objects for no reason
    private CryptexInserter() { }

    protected override bool InsertInternal(CryptexInsertPoint insertPoint, Cryptex cryptex, bool checkOnly) {
        if (!checkOnly) {
            using (insertPoint.Workspace.NewModificationBatchFrame()) {
                insertPoint.Workspace.ApplyModificationRecord(insertPoint.Workspace.AddCryptex(cryptex));
                var record = new Cryptex.MoveCryptexRecord(cryptex.Id, insertPoint.XY);
                insertPoint.Workspace.ApplyModificationRecord(record);
            }
        }
        return true;
    }

    private static CryptexInserter instance = new CryptexInserter();
    public static CryptexInserter Instance => instance;
}

public sealed class TapeInsertSource : InsertSource<Tape> {
    private Tape.SequenceType sequenceType;

    public TapeInsertSource(Tape.SequenceType sequenceType) {
        this.sequenceType = sequenceType;
    }

    public override Tape Get(WorkspaceFull workspace) {
        var locked = LockType.None;
#if       DEBUG
        locked = Overseer.LockType;
#endif // DEBUG
        var guid = Overseer.NewGuid();
        var tape = new Tape(guid, sequenceType, locked);
        var rt = tape.MakeView().GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(10f, 1f);

        return tape;
    }

    public override int Cost(WorkspaceFull workspace, CostOverride costs) => costs[CostType.Tape] + (sequenceType == Tape.SequenceType.Blank ? 0 : costs[CostType.TapeSequence]);
}

public sealed class TapeInserter : Inserter<TapeInsertPoint, Tape> {
    private int modifyPointOffset;

    public TapeInserter(int modifyPointOffset) {
        this.modifyPointOffset = modifyPointOffset;
    }

    protected override bool InsertInternal(TapeInsertPoint insertPoint, Tape tape, bool checkOnly) {
        if (insertPoint.Cryptex.Lock.HasFlag(LockType.Edit)) {
            return false;
        }

        int shiftOffset = 0;
        foreach (var insertPointTape in insertPoint.Tape) {
            shiftOffset = insertPointTape.ShiftOffset;
        }

        var records = insertPoint.Cryptex.AddTape(insertPoint.Index, tape).Yield();
        int offsetDiff = insertPoint.Offset + shiftOffset - modifyPointOffset - tape.ShiftOffset;
        if (offsetDiff != 0) {
            // Since the Tape has not actually been inserted yet, if we are moving the Tape to another Cryptex, Tape.ShiftRight will give us a Record with the wrong Cryptex id
            //records = records.Append(tape.ShiftRight(offsetDiff));
            records = records.Append(new Tape.ShiftRightRecord(insertPoint.Cryptex.Id, tape.Id, Guid.Empty, offsetDiff));
        }
        if (!checkOnly) {
            insertPoint.Workspace.ApplyModificationBatchRecord(records);
        }
        return true;
    }

    private static TapeInserter instance = new TapeInserter(0);
    public static TapeInserter Instance => instance;
}

public sealed class TapeValueInserter : Inserter<TapeValueInsertPoint, TapeValueWrapper> {
    // Make this private to avoid instantiating a ton of objects for no reason
    private TapeValueInserter() { }

    protected override bool InsertInternal(TapeValueInsertPoint insertPoint, TapeValueWrapper tapeValue, bool checkOnly) {
        if (insertPoint.Tape.Lock.HasFlag(LockType.Edit)) {
            return false;
        }

        var record = insertPoint.Tape.Write(insertPoint.Offset + insertPoint.Tape.ShiftOffset, tapeValue);
        if (!checkOnly) {
            insertPoint.Workspace.ApplyModificationRecord(record);
        }
        return true;
    }

    private static TapeValueInserter instance = new TapeValueInserter();
    public static TapeValueInserter Instance => instance;
}

public sealed class RollerInsertSource : InsertSource<Roller> {
    private bool isPrimary;

    public RollerInsertSource(bool isPrimary) {
        this.isPrimary = isPrimary;
    }

    public override Roller Get(WorkspaceFull workspace) {
        var locked = LockType.None;
#if       DEBUG
        locked = Overseer.LockType;
#endif // DEBUG
        var id = Overseer.NewGuid();

        var color = ColorId.Red;
        do {
            if (!workspace.GetRoller(color, isPrimary).HasValue) {
                return MakeRoller(id, color, locked);
            }
            color = color.Next();
        } while (color != ColorId.Red);

        // We tried, but there is no valid color. Let the Inserter reject the invalid roller
        return MakeRoller(id, color, locked);
    }

    private Roller MakeRoller(Guid id, ColorId color, LockType locked) {
        if (isPrimary) {
            return new InstructionRoller(id, color, new Frame(FrameFlags.FrameRead).Yield(), locked);
        } else {
            return new ProgrammableRoller(id, color, new Frame(FrameFlags.FrameRead).Yield(), locked);
        }
    }

    public override int Cost(WorkspaceFull workspace, CostOverride costs) {
        return costs[CostType.Roller] + costs[isPrimary ? CostType.RollerInstruction : CostType.RollerProgrammable];
    }
}

public sealed class RollerInserter : Inserter<RollerInsertPoint, Roller> {
    // Make this private to avoid instantiating a ton of objects for no reason
    private RollerInserter() { }

    protected override bool InsertInternal(RollerInsertPoint insertPoint, Roller item, bool checkOnly) {
        if (item.Frames.Count > insertPoint.Tape.Cryptex.Tapes.Count) {
            return false;
        }
        foreach (var roller in insertPoint.Workspace.GetRoller(item.Color, item.IsPrimary)) {
            // If we are trying to move the Roller, don't block ourselves
            if (roller.Id != item.Id) {
                return false;
            }
        }

        var records = Enumerable.Empty<Record>();

        records = records.Append(insertPoint.Tape.Cryptex.AddRoller(item));
        int offsetDiff = insertPoint.Offset + insertPoint.Tape.ShiftOffset - item.Offset;
        if (offsetDiff != 0) {
            records = records.Append(new Roller.MoveRightRecord(insertPoint.Tape.Cryptex.Id, item.Id, offsetDiff, Option.None));
        }
        int tapeDiff = insertPoint.Tape.Cryptex.Tapes.IndexOf(insertPoint.Tape);
        tapeDiff = Mathf.Min(tapeDiff, insertPoint.Tape.Cryptex.Tapes.Count - item.Frames.Count);
        tapeDiff -= item.TapeIndex;
        if (tapeDiff != 0) {
            records = records.Append(new Roller.MoveDownRecord(insertPoint.Tape.Cryptex.Id, item.Id, tapeDiff));
        }
        if (!checkOnly) {
            insertPoint.Workspace.ApplyModificationBatchRecord(records);
        }
        return true;
    }

    private static RollerInserter instance = new RollerInserter();
    public static RollerInserter Instance => instance;
}

public sealed class FrameInsertSource : InsertSource<Frame> {
    private FrameFlags flags;

    public FrameInsertSource(FrameFlags flags) {
        this.flags = flags;
    }

    public override Frame Get(WorkspaceFull workspace) {
        return new Frame(flags);
    }

    public override int Cost(WorkspaceFull workspace, CostOverride costs) => costs[CostType.Frame] + (flags.HasFlag(FrameFlags.CanRead) ? costs[CostType.FrameCanRead] : 0) + (flags.HasFlag(FrameFlags.CanWrite) ? costs[CostType.FrameCanWrite] : 0);
}

public sealed class FrameInserter : Inserter<FrameInsertPoint, Frame> {
    // Make this private to avoid instantiating a ton of objects for no reason
    private FrameInserter() { }

    protected override bool InsertInternal(FrameInsertPoint insertPoint, Frame frame, bool checkOnly) {
        if (insertPoint.Roller.Lock.HasFlag(LockType.Edit)) {
            return false;
        }

        var roller = insertPoint.Roller;
        var cryptex = roller.Cryptex;
        var records = Enumerable.Empty<Record>();

        // Don't allow adding Frames if it would make the Roller go past the last Tape
        if (roller.TapeIndex + roller.Frames.Count >= cryptex.Tapes.Count) {
            return false;
        }
        records = records.Append(insertPoint.Roller.AddFrame(insertPoint.Index, frame));
        if (!checkOnly) {
            insertPoint.Workspace.ApplyModificationBatchRecord(records);
        }
        return true;
    }

    private static FrameInserter instance = new FrameInserter();
    public static FrameInserter Instance => instance;
}

public sealed class LabelInserter : Inserter<LabelInsertPoint, Label> {
    // Make this private to avoid instantiating a ton of objects for no reason
    private LabelInserter() { }

    protected override bool InsertInternal(LabelInsertPoint insertPoint, Label item, bool checkOnly) {
        var cryptex = insertPoint.Cryptex;
        if (cryptex.GetLabel(insertPoint.Index).HasValue) {
            return false;
        }
        var record = cryptex.AddLabel(insertPoint.Index, item);
        if (!checkOnly) {
            insertPoint.Workspace.ApplyModificationRecord(record);
        }
        return true;
    }

    private static LabelInserter instance = new LabelInserter();
    public static LabelInserter Instance => instance;
}
