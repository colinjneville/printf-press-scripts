using Functional.Option;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using UnityEngine;

using L = LocalizationString;
using LC = LocalizationConstant;
using LE = ILocalizationExpression;
using LF = LocalizationFormat;
using LI = LocalizationInt;


public class Test : MonoBehaviour {

    private void Start() {
        //var otape = new OutputTape();
        //otape.MakeView();
        //otape.Push(new TapeValueInt(3));
        //otape.Push(new TapeValueColor(ColorId.Blue));

        //var tip = new TapeInputSingle("poop");
        //var tipv = tip.MakeView();
        //tip.Right(false);
        //tip.Right(true);
        //tip.Right(true);

        //this.ExecuteNextFrame(() => { PowersOf2Example(); Overseer.Workspace.Value.Level.ApplyReferenceSolution(Overseer.Workspace.Value); });
    }

    public static void EditorLevel() {
        var levelId = new Guid("73C65F7D-F44F-40B5-9B4B-7D60EDB66524");

        var execCryptexId = Guid.NewGuid();
        var execTapeId0 = Guid.NewGuid();
        var execTapeId1 = Guid.NewGuid();
        var execTapeId2 = Guid.NewGuid();

        var cryptexId = Guid.NewGuid();

        var testSuite = new ExampleTestSuite(new TestCase(new ReplayLog()));

        var baseLayer = new WorkspaceLayer();

        var level = new Level(levelId, 1, LC.Temp("Editor"), baseLayer, 0, testSuite, new ReplayLog(), Array.Empty<int>());

        var workspace = new WorkspaceFull(level);

        var execCryptex = new Cryptex(execCryptexId, workspace, Vector2.zero);
        var cryptex = new Cryptex(cryptexId, workspace, new Vector2(0f, 10f));
        workspace.AddCryptex(execCryptex).Apply(workspace);
        workspace.AddCryptex(cryptex).Apply(workspace);
        var execTape0 = new Tape(execTapeId0, Tape.SequenceType.Blank);
        var execTape1 = new Tape(execTapeId1, Tape.SequenceType.Blank);
        var execTape2 = new Tape(execTapeId2, Tape.SequenceType.Blank);
        execCryptex.AddTape(0, execTape0).Apply(workspace);
        execCryptex.AddTape(1, execTape1).Apply(workspace);
        execCryptex.AddTape(2, execTape2).Apply(workspace);

        var execRollerId = Guid.NewGuid();

        var execRoller = new InstructionRoller(execRollerId, ColorId.Red, new[] { new Frame(FrameFlags.FrameRead), new Frame(FrameFlags.FrameRead), new Frame(FrameFlags.FrameRead) });
        execCryptex.AddRoller(execRoller).Apply(workspace);

        var newLevel = new Level(levelId, 1, LC.Temp("Editor"), workspace.SerializeCurrentLayer(), 0, level.TestSuite, level.ReferenceSolution, level.StarThresholds);

        Overseer.LoadLevel(newLevel, SolutionData.Dummy);
    }

    /*
    public void PowersOf2Example() {
        var levelId = Guid.NewGuid();

        var execCryptexId = Guid.NewGuid();
        var execTapeId0 = Guid.NewGuid();
        var execTapeId1 = Guid.NewGuid();
        var execTapeId2 = Guid.NewGuid();
        var execRollerId = Guid.NewGuid();

        var cryptexId = Guid.NewGuid();
        var tapeId0 = Guid.NewGuid();
        var rollerId = Guid.NewGuid();

        var testSuite = new ExampleTestSuite(new TestCase(
            new ReplayLog(Enumerable.Empty<Record>()), 
            new TapeValueInt(1),
            new TapeValueInt(2),
            new TapeValueInt(4),
            new TapeValueInt(8),
            new TapeValueInt(16),
            new TapeValueInt(32),
            new TapeValueInt(64)
            ));

        var baseLayer = new WorkspaceLayer();

        level = new Level(levelId, 1, LC.Temp("Powers of 2"), baseLayer, testSuite, new ReplayLog(), Array.Empty<int>());

        var workspace = level.MakeWorkspace();



        var execCryptex = new Cryptex(execCryptexId, workspace);
        var cryptex = new Cryptex(cryptexId, workspace);
        workspace.AddCryptex(0, execCryptex).Apply(workspace);
        workspace.AddCryptex(1, cryptex).Apply(workspace);

        var execTape0 = new Tape(execTapeId0, Tape.SequenceType.Blank);
        var execTape1 = new Tape(execTapeId1, Tape.SequenceType.Blank);
        var execTape2 = new Tape(execTapeId2, Tape.SequenceType.Blank);
        execCryptex.AddTape(0, execTape0).Apply(workspace);
        execCryptex.AddTape(1, execTape1).Apply(workspace);
        execCryptex.AddTape(2, execTape2).Apply(workspace);

        var tape0 = new Tape(tapeId0, Tape.SequenceType.Integers);
        cryptex.AddTape(0, tape0).Apply(workspace);

        var execRoller = new InstructionRoller(execRollerId, new[] { new Frame(FrameFlags.FrameRead), new Frame(FrameFlags.FrameRead), new Frame(FrameFlags.FrameRead) }, ColorId.Red);
        var roller = new ProgrammableRoller(rollerId, new[] { new Frame(FrameFlags.FrameRead) }, execRollerId, ColorId.Red);
        execCryptex.AddRoller(execRoller).Apply(workspace);
        cryptex.AddRoller(roller).Apply(workspace);

        var newBaseLayer = workspace.Export();

        var referenceSolution = new List<Record>();

        int offset = 0;
        referenceSolution.Add(workspace.GetCryptex(execCryptexId).Value.Tapes[0].Write(offset++, TapeValue.FromString("out")));
        referenceSolution.Add(workspace.GetCryptex(execCryptexId).Value.Tapes[0].Write(offset++, TapeValue.FromString("shl")));
        referenceSolution.Add(workspace.GetCryptex(execCryptexId).Value.Tapes[0].Write(offset++, TapeValue.FromString("jmp")));
        offset = 0;
        referenceSolution.Add(workspace.GetCryptex(execCryptexId).Value.Tapes[1].Write(offset++, TapeValue.FromString("@f0")));
        referenceSolution.Add(workspace.GetCryptex(execCryptexId).Value.Tapes[1].Write(offset++, TapeValue.FromString("@f0")));
        referenceSolution.Add(workspace.GetCryptex(execCryptexId).Value.Tapes[1].Write(offset++, TapeValue.FromString(":start")));
        offset = 0;
        referenceSolution.Add(workspace.GetCryptex(execCryptexId).Value.Tapes[2].Write(offset++, TapeValue.FromString("c0")));
        referenceSolution.Add(workspace.GetCryptex(execCryptexId).Value.Tapes[2].Write(offset++, TapeValue.FromString("f0")));
        referenceSolution.Add(workspace.GetCryptex(execCryptexId).Value.Tapes[2].Write(offset++, TapeValue.FromString("")));

        referenceSolution.Add(workspace.GetCryptex(execCryptexId).Value.AddLabel(0, new Label(Guid.NewGuid(), "start")));

        referenceSolution.Add(workspace.GetCryptex(cryptexId).Value.Rollers[0].MoveRight(1));

        var newLevel = new Level(levelId, 1, LC.Temp("Multiplication"), newBaseLayer, testSuite, new ReplayLog(referenceSolution), Array.Empty<int>());
        var newWorkspace = newLevel.MakeWorkspace();

        newLevel.TestReferenceSolution();

        Overseer.RegisterWorkspace(newWorkspace);
    }

    public void MultiplyTest() {
        var levelId = new Guid("F261DA94-4FF8-4192-8542-7B4DD41D1196");

        var execCryptexId = Guid.NewGuid();
        var execTapeId0 = Guid.NewGuid();
        var execTapeId1 = Guid.NewGuid();
        var execTapeId2 = Guid.NewGuid();
        var execRollerId = Guid.NewGuid();

        var cryptexId = Guid.NewGuid();
        var tapeId0 = Guid.NewGuid();
        var tapeId1 = Guid.NewGuid();
        var tapeId2 = Guid.NewGuid();
        var rollerId = Guid.NewGuid();

        var testSuite = new ExampleTestSuite(
            new TestCase(
                new ReplayLog(new Tape.ShiftLeftRecord(cryptexId, tapeId0, 3),
                              new Tape.ShiftLeftRecord(cryptexId, tapeId1, 7)),
                new TapeValueInt(21).Yield()));

        var baseLayer = new WorkspaceLayer();
        
        level = new Level(levelId, 1, LC.Temp("Multiplication"), baseLayer, testSuite, new ReplayLog(), Array.Empty<int>());

        var workspace = level.MakeWorkspace();

        var execCryptex = new Cryptex(execCryptexId, workspace);
        var cryptex = new Cryptex(cryptexId, workspace);
        workspace.AddCryptex(0, execCryptex).Apply(workspace);
        workspace.AddCryptex(1, cryptex).Apply(workspace);
        
        var execTape0 = new Tape(execTapeId0, Tape.SequenceType.Blank);
        var execTape1 = new Tape(execTapeId1, Tape.SequenceType.Blank);
        var execTape2 = new Tape(execTapeId2, Tape.SequenceType.Blank);
        execCryptex.AddTape(0, execTape0).Apply(workspace);
        execCryptex.AddTape(1, execTape1).Apply(workspace);
        execCryptex.AddTape(2, execTape2).Apply(workspace);

        var tape0 = new Tape(tapeId0, Tape.SequenceType.Integers);
        var tape1 = new Tape(tapeId1, Tape.SequenceType.Integers);
        var tape2 = new Tape(tapeId2, Tape.SequenceType.Integers);
        cryptex.AddTape(0, tape0).Apply(workspace);
        cryptex.AddTape(1, tape1).Apply(workspace);
        cryptex.AddTape(2, tape2).Apply(workspace);

        var execRoller = new InstructionRoller(execRollerId, new[] { new Frame(FrameFlags.FrameRead), new Frame(FrameFlags.FrameRead), new Frame(FrameFlags.FrameRead) }, ColorId.Red);
        var roller = new ProgrammableRoller(rollerId, new[] { new Frame(FrameFlags.FrameRead), new Frame(FrameFlags.FrameRead) }, execRollerId, ColorId.Red);
        execCryptex.AddRoller(execRoller).Apply(workspace);
        cryptex.AddRoller(roller).Apply(workspace);
        workspace.GetRoller(roller.Id).Value.MoveUp(1).Apply(workspace);

        var newBaseLayer = workspace.Export();

        var referenceSolution = new List<Record>();

        int offset = 0;
        referenceSolution.Add(workspace.GetCryptex(execCryptexId).Value.Tapes[0].Write(offset++, TapeValue.FromString("ife")));
        referenceSolution.Add(workspace.GetCryptex(execCryptexId).Value.Tapes[0].Write(offset++, TapeValue.FromString("jmp")));
        referenceSolution.Add(workspace.GetCryptex(execCryptexId).Value.Tapes[0].Write(offset++, TapeValue.FromString("shr")));
        referenceSolution.Add(workspace.GetCryptex(execCryptexId).Value.Tapes[0].Write(offset++, TapeValue.FromString("mvd")));
        referenceSolution.Add(workspace.GetCryptex(execCryptexId).Value.Tapes[0].Write(offset++, TapeValue.FromString("shl")));
        referenceSolution.Add(workspace.GetCryptex(execCryptexId).Value.Tapes[0].Write(offset++, TapeValue.FromString("mvu")));
        referenceSolution.Add(workspace.GetCryptex(execCryptexId).Value.Tapes[0].Write(offset++, TapeValue.FromString("jmp")));
        referenceSolution.Add(workspace.GetCryptex(execCryptexId).Value.Tapes[0].Write(offset++, TapeValue.FromString("mvd")));
        referenceSolution.Add(workspace.GetCryptex(execCryptexId).Value.Tapes[0].Write(offset++, TapeValue.FromString("out")));
        offset = 0;
        referenceSolution.Add(workspace.GetCryptex(execCryptexId).Value.Tapes[1].Write(offset++, TapeValue.FromString("@f0")));
        referenceSolution.Add(workspace.GetCryptex(execCryptexId).Value.Tapes[1].Write(offset++, TapeValue.FromString(":end")));
        referenceSolution.Add(workspace.GetCryptex(execCryptexId).Value.Tapes[1].Write(offset++, TapeValue.FromString("1")));
        referenceSolution.Add(workspace.GetCryptex(execCryptexId).Value.Tapes[1].Write(offset++, TapeValue.FromString("1")));
        referenceSolution.Add(workspace.GetCryptex(execCryptexId).Value.Tapes[1].Write(offset++, TapeValue.FromString("@f0")));
        referenceSolution.Add(workspace.GetCryptex(execCryptexId).Value.Tapes[1].Write(offset++, TapeValue.FromString("1")));
        referenceSolution.Add(workspace.GetCryptex(execCryptexId).Value.Tapes[1].Write(offset++, TapeValue.FromString(":start")));
        referenceSolution.Add(workspace.GetCryptex(execCryptexId).Value.Tapes[1].Write(offset++, TapeValue.FromString("1")));
        referenceSolution.Add(workspace.GetCryptex(execCryptexId).Value.Tapes[1].Write(offset++, TapeValue.FromString("@f1")));
        offset = 0;
        referenceSolution.Add(workspace.GetCryptex(execCryptexId).Value.Tapes[2].Write(offset++, TapeValue.FromString("0")));
        referenceSolution.Add(workspace.GetCryptex(execCryptexId).Value.Tapes[2].Write(offset++, TapeValue.FromString("")));
        referenceSolution.Add(workspace.GetCryptex(execCryptexId).Value.Tapes[2].Write(offset++, TapeValue.FromString("f0")));
        referenceSolution.Add(workspace.GetCryptex(execCryptexId).Value.Tapes[2].Write(offset++, TapeValue.FromString("")));
        referenceSolution.Add(workspace.GetCryptex(execCryptexId).Value.Tapes[2].Write(offset++, TapeValue.FromString("f1")));
        referenceSolution.Add(workspace.GetCryptex(execCryptexId).Value.Tapes[2].Write(offset++, TapeValue.FromString("")));
        referenceSolution.Add(workspace.GetCryptex(execCryptexId).Value.Tapes[2].Write(offset++, TapeValue.FromString("")));
        referenceSolution.Add(workspace.GetCryptex(execCryptexId).Value.Tapes[2].Write(offset++, TapeValue.FromString("")));
        referenceSolution.Add(workspace.GetCryptex(execCryptexId).Value.Tapes[2].Write(offset++, TapeValue.FromString("c0")));

        referenceSolution.Add(workspace.GetCryptex(execCryptexId).Value.AddLabel(0, new Label(Guid.NewGuid(), "start")));
        referenceSolution.Add(workspace.GetCryptex(execCryptexId).Value.AddLabel(7, new Label(Guid.NewGuid(), "end")));

        var newLevel = new Level(levelId, 1, LC.Temp("Multiplication"), newBaseLayer, testSuite, new ReplayLog(referenceSolution), Array.Empty<int>());
        var newWorkspace = newLevel.MakeWorkspace();

        newLevel.TestReferenceSolution();

        Overseer.RegisterWorkspace(newWorkspace);
    }

    public void FullMultiplication() {
        var levelId = Guid.NewGuid();

        var execCryptexId = Guid.NewGuid();
        var execTapeId0 = Guid.NewGuid();
        var execTapeId1 = Guid.NewGuid();
        var execTapeId2 = Guid.NewGuid();
        var execRollerId = Guid.NewGuid();

        var cryptexId = Guid.NewGuid();
        var tapeId0 = Guid.NewGuid();
        var tapeId1 = Guid.NewGuid();
        var tapeId2 = Guid.NewGuid();
        var tapeId3 = Guid.NewGuid();
        var rollerId = Guid.NewGuid();

        var testSuite = new ExampleTestSuite(
            new TestCase(
                new ReplayLog(new Tape.WriteRecord(cryptexId, tapeId0, 0, new TapeValueInt(3)),
                    new Tape.WriteRecord(cryptexId, tapeId0, 1, new TapeValueInt(7)),
                    new Tape.WriteRecord(cryptexId, tapeId0, 2, new TapeValueInt(8)),
                    new Tape.WriteRecord(cryptexId, tapeId0, 3, new TapeValueInt(4)),
                    new Tape.WriteRecord(cryptexId, tapeId0, 4, new TapeValueInt(1)),
                    new Tape.WriteRecord(cryptexId, tapeId0, 5, new TapeValueInt(9))),
                new TapeValueInt(21), 
                new TapeValueInt(32), 
                new TapeValueInt(9)));

        var baseLayer = new WorkspaceLayer();

        level = new Level(levelId, 1, LC.Temp("Multiplication"), baseLayer, testSuite, new ReplayLog(), Array.Empty<int>());

        var workspace = level.MakeWorkspace();

        var execCryptex = new Cryptex(execCryptexId, workspace);
        var cryptex = new Cryptex(cryptexId, workspace);
        workspace.AddCryptex(0, execCryptex).Apply(workspace);
        workspace.AddCryptex(1, cryptex).Apply(workspace);

        var execTape0 = new Tape(execTapeId0, Tape.SequenceType.Blank);
        var execTape1 = new Tape(execTapeId1, Tape.SequenceType.Blank);
        var execTape2 = new Tape(execTapeId2, Tape.SequenceType.Blank);
        execCryptex.AddTape(0, execTape0).Apply(workspace);
        execCryptex.AddTape(1, execTape1).Apply(workspace);
        execCryptex.AddTape(2, execTape2).Apply(workspace);

        var tape0 = new Tape(tapeId0, Tape.SequenceType.Blank);
        var tape1 = new Tape(tapeId1, Tape.SequenceType.Integers);
        var tape2 = new Tape(tapeId2, Tape.SequenceType.Integers);
        var tape3 = new Tape(tapeId3, Tape.SequenceType.Integers);
        cryptex.AddTape(0, tape0).Apply(workspace);
        cryptex.AddTape(1, tape1).Apply(workspace);
        cryptex.AddTape(2, tape2).Apply(workspace);
        cryptex.AddTape(3, tape3).Apply(workspace);

        var execRoller = new InstructionRoller(execRollerId, new[] { new Frame(FrameFlags.FrameRead), new Frame(FrameFlags.FrameRead), new Frame(FrameFlags.FrameRead) }, ColorId.Red);
        var roller = new ProgrammableRoller(rollerId, new[] { new Frame(FrameFlags.FrameRead), new Frame(FrameFlags.FrameRead), new Frame(FrameFlags.FrameRead) }, execRollerId, ColorId.Red);
        execCryptex.AddRoller(execRoller).Apply(workspace);
        cryptex.AddRoller(roller).Apply(workspace);
        workspace.GetRoller(roller.Id).Value.MoveUp(1).Apply(workspace);

        var newBaseLayer = workspace.Export();

        var referenceSolution = new List<Record>();

        int offset = 0;
        referenceSolution.Add(workspace.GetCryptex(execCryptexId).Value.Tapes[0].Write(offset++, TapeValue.FromString("shl")));
        referenceSolution.Add(workspace.GetCryptex(execCryptexId).Value.Tapes[0].Write(offset++, TapeValue.FromString("shl")));
        referenceSolution.Add(workspace.GetCryptex(execCryptexId).Value.Tapes[0].Write(offset++, TapeValue.FromString("shl")));
        referenceSolution.Add(workspace.GetCryptex(execCryptexId).Value.Tapes[0].Write(offset++, TapeValue.FromString("mvd")));
        referenceSolution.Add(workspace.GetCryptex(execCryptexId).Value.Tapes[0].Write(offset++, TapeValue.FromString("ife")));
        referenceSolution.Add(workspace.GetCryptex(execCryptexId).Value.Tapes[0].Write(offset++, TapeValue.FromString("jmp")));
        referenceSolution.Add(workspace.GetCryptex(execCryptexId).Value.Tapes[0].Write(offset++, TapeValue.FromString("shl")));
        referenceSolution.Add(workspace.GetCryptex(execCryptexId).Value.Tapes[0].Write(offset++, TapeValue.FromString("shr")));
        referenceSolution.Add(workspace.GetCryptex(execCryptexId).Value.Tapes[0].Write(offset++, TapeValue.FromString("jmp")));
        referenceSolution.Add(workspace.GetCryptex(execCryptexId).Value.Tapes[0].Write(offset++, TapeValue.FromString("out")));
        referenceSolution.Add(workspace.GetCryptex(execCryptexId).Value.Tapes[0].Write(offset++, TapeValue.FromString("shr")));
        referenceSolution.Add(workspace.GetCryptex(execCryptexId).Value.Tapes[0].Write(offset++, TapeValue.FromString("shr")));
        referenceSolution.Add(workspace.GetCryptex(execCryptexId).Value.Tapes[0].Write(offset++, TapeValue.FromString("mvu")));
        referenceSolution.Add(workspace.GetCryptex(execCryptexId).Value.Tapes[0].Write(offset++, TapeValue.FromString("shl")));
        referenceSolution.Add(workspace.GetCryptex(execCryptexId).Value.Tapes[0].Write(offset++, TapeValue.FromString("jmp")));
        offset = 0;
        referenceSolution.Add(workspace.GetCryptex(execCryptexId).Value.Tapes[1].Write(offset++, TapeValue.FromString("@f0")));
        referenceSolution.Add(workspace.GetCryptex(execCryptexId).Value.Tapes[1].Write(offset++, TapeValue.FromString("1")));
        referenceSolution.Add(workspace.GetCryptex(execCryptexId).Value.Tapes[1].Write(offset++, TapeValue.FromString("@f0")));
        referenceSolution.Add(workspace.GetCryptex(execCryptexId).Value.Tapes[1].Write(offset++, TapeValue.FromString("1")));
        referenceSolution.Add(workspace.GetCryptex(execCryptexId).Value.Tapes[1].Write(offset++, TapeValue.FromString("@f0")));
        referenceSolution.Add(workspace.GetCryptex(execCryptexId).Value.Tapes[1].Write(offset++, TapeValue.FromString(":out")));
        referenceSolution.Add(workspace.GetCryptex(execCryptexId).Value.Tapes[1].Write(offset++, TapeValue.FromString("@f1")));
        referenceSolution.Add(workspace.GetCryptex(execCryptexId).Value.Tapes[1].Write(offset++, TapeValue.FromString("1")));
        referenceSolution.Add(workspace.GetCryptex(execCryptexId).Value.Tapes[1].Write(offset++, TapeValue.FromString(":loop")));
        referenceSolution.Add(workspace.GetCryptex(execCryptexId).Value.Tapes[1].Write(offset++, TapeValue.FromString("@f2")));
        referenceSolution.Add(workspace.GetCryptex(execCryptexId).Value.Tapes[1].Write(offset++, TapeValue.FromString("@f1")));
        referenceSolution.Add(workspace.GetCryptex(execCryptexId).Value.Tapes[1].Write(offset++, TapeValue.FromString("@f2")));
        referenceSolution.Add(workspace.GetCryptex(execCryptexId).Value.Tapes[1].Write(offset++, TapeValue.FromString("1")));
        referenceSolution.Add(workspace.GetCryptex(execCryptexId).Value.Tapes[1].Write(offset++, TapeValue.FromString("1")));
        referenceSolution.Add(workspace.GetCryptex(execCryptexId).Value.Tapes[1].Write(offset++, TapeValue.FromString(":init")));
        offset = 0;
        referenceSolution.Add(workspace.GetCryptex(execCryptexId).Value.Tapes[2].Write(offset++, TapeValue.FromString("f1")));
        referenceSolution.Add(workspace.GetCryptex(execCryptexId).Value.Tapes[2].Write(offset++, TapeValue.FromString("f0")));
        referenceSolution.Add(workspace.GetCryptex(execCryptexId).Value.Tapes[2].Write(offset++, TapeValue.FromString("f2")));
        referenceSolution.Add(workspace.GetCryptex(execCryptexId).Value.Tapes[2].Write(offset++, TapeValue.FromString("")));
        referenceSolution.Add(workspace.GetCryptex(execCryptexId).Value.Tapes[2].Write(offset++, TapeValue.FromString("0")));
        referenceSolution.Add(workspace.GetCryptex(execCryptexId).Value.Tapes[2].Write(offset++, TapeValue.FromString("")));
        referenceSolution.Add(workspace.GetCryptex(execCryptexId).Value.Tapes[2].Write(offset++, TapeValue.FromString("f2")));
        referenceSolution.Add(workspace.GetCryptex(execCryptexId).Value.Tapes[2].Write(offset++, TapeValue.FromString("f0")));
        referenceSolution.Add(workspace.GetCryptex(execCryptexId).Value.Tapes[2].Write(offset++, TapeValue.FromString("")));
        referenceSolution.Add(workspace.GetCryptex(execCryptexId).Value.Tapes[2].Write(offset++, TapeValue.FromString("c0")));
        referenceSolution.Add(workspace.GetCryptex(execCryptexId).Value.Tapes[2].Write(offset++, TapeValue.FromString("f1")));
        referenceSolution.Add(workspace.GetCryptex(execCryptexId).Value.Tapes[2].Write(offset++, TapeValue.FromString("f2")));
        referenceSolution.Add(workspace.GetCryptex(execCryptexId).Value.Tapes[2].Write(offset++, TapeValue.FromString("")));
        referenceSolution.Add(workspace.GetCryptex(execCryptexId).Value.Tapes[2].Write(offset++, TapeValue.FromString("f0")));
        referenceSolution.Add(workspace.GetCryptex(execCryptexId).Value.Tapes[2].Write(offset++, TapeValue.FromString("")));

        referenceSolution.Add(workspace.GetCryptex(execCryptexId).Value.AddLabel(0, new Label(Guid.NewGuid(), "init")));
        referenceSolution.Add(workspace.GetCryptex(execCryptexId).Value.AddLabel(3, new Label(Guid.NewGuid(), "loop")));
        referenceSolution.Add(workspace.GetCryptex(execCryptexId).Value.AddLabel(9, new Label(Guid.NewGuid(), "out")));

        var newLevel = new Level(levelId, 1, LC.Temp("Multiplication"), newBaseLayer, testSuite, new ReplayLog(referenceSolution), Array.Empty<int>());
        var newWorkspace = newLevel.MakeWorkspace();

        newLevel.TestReferenceSolution();

        Overseer.RegisterWorkspace(newWorkspace);
    }
    */

#if false
    public void PatternMatchTest() {
        TapeValue r = TapeValueColor.Red;
        TapeValue g = TapeValueColor.Green;
        TapeValue b = TapeValueColor.Blue;
        TapeValue y = TapeValueColor.Yellow;

        var rollerId = Guid.NewGuid();

        var image = new[] {
            new[] { b, r, r, b, b, r, b, r, r, r, r, g, },
            new[] { b, y, r, b, r, r, b, b, g, r, y, g, },
            new[] { b, r, r, r, b, r, b, b, r, r, r, y, },
        };

        var execRollerId = Guid.NewGuid();

        var baseLayer = new Func<Workspace, WorkspaceLayer>((w) => new WorkspaceLayer(w,
            new Cryptex(Guid.NewGuid(), w, 
            new[] {
                new Tape(Guid.NewGuid()),
                new Tape(Guid.NewGuid()),
                new Tape(Guid.NewGuid()),
            },
            new[] {
                new InstructionRoller(execRollerId, new[] { new Frame(FrameFlags.FrameRead), new Frame(FrameFlags.FrameRead), new Frame(FrameFlags.FrameRead) }, ColorId.Red),
            }),
            new Cryptex(new Guid("98EEDC93-0109-4394-B504-0682D8E503E6"), w,
            new[] {
                new Tape(Guid.NewGuid(), true, r),
                new Tape(Guid.NewGuid(), image[0]),
                new Tape(Guid.NewGuid(), image[1]),
                new Tape(Guid.NewGuid(), image[2]),
                new Tape(Guid.NewGuid(), Tape.SequenceType.Integers),
            },
            new[] {
                new ProgrammableRoller(rollerId, new[] { new Frame(FrameFlags.FrameRead), new Frame(FrameFlags.FrameRead) }, execRollerId),
            })
        ));

        //var testSuite = new ExampleTestSuite(new TestCase(new TapeValueInt(21), Utility.MakeKVP(ColorId.Red, new Source(initScript))));
        var testSuite = new ExampleTestSuite(new TestCase(new TapeValueInt(10)));

        level = new Level(LC.Temp("Pattern Matching"), baseLayer, testSuite);

        workspace = level.MakeWorkspace();

#if UNITY_EDITOR
        /*
        workspace.GetSource(workspace.GetRoller(rollerId).Value).Value.Text = @"
 jmp @f0 ! @f1 ~ skip
 mvd 2
 jmp @f0 = @f1 ~ next
 mvr 1
 jmp @f0 ! @f1 ~ next
 shr 1 f1
 jmp @f0 ! @f1 ~ unshift
 shl 1 f1
 mvu 1
 jmp @f0 ! @f1 ~ next
 mvr 1
 jmp @f0 = @f1 ~ next
 shr 1 f1
 jmp @f0 ! @f1 ~ next
 mvd 1
 jmp @f0 ! @f1 ~ unhop
 mvd
 out @f1 c0
unhop:
 mvu 1
unshift:
 shl 1 f1
next:
 mvu
skip:
 mvr 1
";*/
#endif

        Overseer.RegisterWorkspace(workspace);
    }

    public void MinOfMaxTest() {
        string sourceA =
@"
begin:
 shr @f0 f1
 jmp @f1 > -1 ~ skip
 shr @f1 f1
skip:
 shl @f0 f1
 mvr 1
 shr 1 f1
 jmp @f0 ! null ~ begin
find:
 mvl 1
 shl 1 f1
 jmp @f0 ! @f1 ~ find
 shr @f1 f1
 out @f0 c1
reset:
 mvr 1
 jmp @f0 ! null ~ reset
 mvr 1
 jmp @f0 ! null ~ begin
 shl 999 f1
 out -1 c1
";

        sourceA =
        @"begin: shr @f0 f1
 jmp @f1 > -1 ~ skip
 shr @f1 f1
skip: shl @f0 f1
 mvr 1
 shr 1 f1
 jmp @f0 ! null ~ begin
find: mvl 1
 shl 1 f1
 jmp @f0 ! @f1 ~ find
 shr @f1 f1
 out @f0 c1
reset: mvr 1
 jmp @f0 ! null ~ reset
 mvr 1
 jmp @f0 ! null ~ begin
 shl 999 f1
 out -1 c1";

        string sourceB =
@"
first: jmp @c1 = null ~ first
loop:
jmp @f1 < 0 ~ notdone
 out @f0 c0
notdone:
 mvl @f1
wait:
jmp @c1 > @f0 ~ wait 
jmp loop
";
        var cryptexId = Guid.NewGuid();
        var inputTapeId = Guid.NewGuid();
        var rollerId0 = Guid.NewGuid();
        var rollerId1 = Guid.NewGuid();

        var execRollerId0 = Guid.NewGuid();
        var execRollerId1 = Guid.NewGuid();

        var baseLayer = new Func<Workspace, WorkspaceLayer>((w) => new WorkspaceLayer(w,
            new Cryptex(Guid.NewGuid(), w,
            new[] {
                new Tape(Guid.NewGuid()),
                new Tape(Guid.NewGuid()),
                new Tape(Guid.NewGuid()),
            },
            new[] {
                new InstructionRoller(execRollerId0, new[] {new Frame(FrameFlags.FrameRead), new Frame(FrameFlags.FrameRead), new Frame(FrameFlags.FrameRead) }, ColorId.Red),
                new InstructionRoller(execRollerId0, new[] {new Frame(FrameFlags.FrameRead), new Frame(FrameFlags.FrameRead), new Frame(FrameFlags.FrameRead) }, ColorId.Green),
            }),
            new Cryptex(cryptexId, w,
            new[] {
                new Tape(inputTapeId, Tape.SequenceType.Blank),
                new Tape(Guid.NewGuid(), Tape.SequenceType.Integers),
            },
            new[] {
                new ProgrammableRoller(rollerId0, new[] { new Frame(FrameFlags.FrameRead), new Frame(FrameFlags.FrameRead) }, execRollerId0),
                new ProgrammableRoller(rollerId1, new[] { new Frame(FrameFlags.FrameRead), new Frame(FrameFlags.FrameRead) }, execRollerId1),
            })
        ));

        //var testSuite = new ExampleTestSuite(new TestCase(new TapeValueInt(21), Utility.MakeKVP(ColorId.Red, new Source(initScript))));
        var testSuite = new HybridTestSuite(
            new ExampleTestSuite(
                new TestCase(
                    Tape.LogWriteBatch(cryptexId, inputTapeId, 0,
                        TapeValueInt.Batch(3, 9, 7).Append<TapeValue>(TapeValueNull.Instance).Concat(
                        TapeValueInt.Batch(4, 4, 6, 5)).Append(TapeValueNull.Instance).Append(new TapeValueInt(8))
                    ),
                    new TapeValueInt(6)
                )
            ), new GeneratedTestSuite(0, i => null));
        //new GeneratedTestSuite(20, seed => {
        //    UnityEngine.Random.InitState(1935672 + seed);
        //    int groups = UnityEngine.Random.Range(1, 20);
        //    int minMax = 100;
        //    for (int i = 0; i < groups; ++i) {
        //        int groupSize = UnityEngine.Random.Range(1, 10);
        //        int max = 0;
        //        for (int j = 0; j < groupSize; ++j) {
        //            int value = UnityEngine.Random.Range(0, 50);
        //            max = Mathf.Max(max, value);
        //        }
        //        minMax = Mathf.Min(minMax, max);
        //    }
        //
        //});

        level = new Level(LC.Temp("Minimum of Maximums"), baseLayer, testSuite);

        workspace = level.MakeWorkspace();

        //workspace.Cryptexes.First().RollersOfColor(ColorId.Green).First().MoveLeft(1);

#if UNITY_EDITOR
        /*
        workspace.GetSource(workspace.GetRoller(rollerId0).Value).Value.Text = sourceA;
        workspace.GetSource(workspace.GetRoller(rollerId1).Value).Value.Text = sourceB;
        */
#endif

        Overseer.RegisterWorkspace(workspace);
    }

    public void Palendrome() {
        var cryptexId = Guid.NewGuid();
        var rollerId = Guid.NewGuid();
        var inputTapeId = Guid.NewGuid();

        var execRollerId = Guid.NewGuid();

        var baseLayer = new Func<Workspace, WorkspaceLayer>((w) => new WorkspaceLayer(w,
            new Cryptex(Guid.NewGuid(), w,
            new[] {
                new Tape(Guid.NewGuid()),
                new Tape(Guid.NewGuid()),
                new Tape(Guid.NewGuid()),
            },
            new[] {
                new InstructionRoller(execRollerId, new[] { new Frame(FrameFlags.FrameRead), new Frame(FrameFlags.FrameRead), new Frame(FrameFlags.FrameRead) }, ColorId.Red),
            }),
            new Cryptex(cryptexId, w,
            new[] {
                new Tape(inputTapeId, Tape.SequenceType.Blank),
                new Tape(Guid.NewGuid(), Tape.SequenceType.Blank),
            },
            new[] {
                new ProgrammableRoller(rollerId, new[] { new Frame(FrameFlags.FrameRead), new Frame(FrameFlags.FrameReadWrite) }, execRollerId),
            })
        ));

        var testSuite = new HybridTestSuite(
            new ExampleTestSuite(
                new TestCase(
                    Tape.LogWriteBatch(cryptexId, inputTapeId, 0,
                        TapeValueChar.Batch("amanaplanacanalpanama")
                    ),
                    new TapeValueInt(1)
                )
            ), new GeneratedTestSuite(0, i => null));

        level = new Level(LC.Temp("Palendrome"), baseLayer, testSuite);

        workspace = level.MakeWorkspace();

#if UNITY_EDITOR
        /*
        workspace.GetSource(workspace.GetRoller(rollerId).Value).Value.Text =
@"start:
put @f0 f1
shl 1 f0
shr 1 f1
jmp @f0 ! null ~ start
reset: shl 1 f1
jmp @f1 ! null ~ reset
check: jmp @f0 = @f1 ~ same
out 0 c0
same: mvl 1
jmp @f0 ! null ~ check
out 1 c0
";*/
#endif

        Overseer.RegisterWorkspace(workspace);
    }

    public void JustRead() {
        var cryptexId = Guid.NewGuid();
        var tapeId0 = Guid.NewGuid();
        var rollerId = Guid.NewGuid();

        var execRollerId = Guid.NewGuid();

        var baseLayer = new Func<Workspace, WorkspaceLayer>((w) => new WorkspaceLayer(w,
            new Cryptex(Guid.NewGuid(), w,
            new[] {
                new Tape(Guid.NewGuid()),
                new Tape(Guid.NewGuid()),
                new Tape(Guid.NewGuid()),
            },
            new[] {
                new InstructionRoller(execRollerId, new[] { new Frame(FrameFlags.FrameRead), new Frame(FrameFlags.FrameRead), new Frame(FrameFlags.FrameRead) }, ColorId.Red),
            }),
            new Cryptex(cryptexId, w,
            new[] {
                new Tape(tapeId0, Tape.SequenceType.Integers),
            },
            new[] {
                new ProgrammableRoller(rollerId, new[] { new Frame(FrameFlags.FrameRead) }, execRollerId),
            })
        ));

        var testSuite = new ExampleTestSuite(new TestCase(
            new TapeValueInt(0),
            new TapeValueInt(1),
            new TapeValueInt(2),
            new TapeValueInt(3),
            new TapeValueInt(4),
            new TapeValueInt(5),
            new TapeValueInt(6)
            ));

        level = new Level(LC.Temp("Just Read"), baseLayer, testSuite);

        workspace = level.MakeWorkspace();

#if UNITY_EDITOR
        /*
        workspace.GetSource(workspace.GetRoller(rollerId).Value).Value.Text = @"
out @f0 c0
shl 1 f0
";
*/
#endif
        Overseer.RegisterWorkspace(workspace);
    }
#endif
}
