using Functional.Option;
using Newtonsoft.Json;
using Sprache;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

using L = LocalizationString;
using LC = LocalizationConstant;
using LE = ILocalizationExpression;
using LF = LocalizationFormat;
using LI = LocalizationInt;

public sealed partial class MultiValueInputMode : ValueInputMode {
    private TapeSelectionPoint tapePointStart;
    private TapeSelectionPoint tapePointEnd;

    public MultiValueInputMode(InputManager manager) : base(manager) { }

    public override bool AlwaysActive => false;

    public TapeSelectionPoint TapePointStart {
        get => tapePointStart;
        set => tapePointStart = value;
    }

    public TapeSelectionPoint TapePointEnd {
        get => tapePointEnd;
        set => tapePointEnd = value;
    }

    public override void OnSelect() {
        Assert.NotNull(Cryptex);
        MakeView();
    }
    public override void OnDeselect() {
        ClearView();
    }

    // TODO
    public override bool Char(State state, Modifiers modifiers, char c) => true;

    public override bool Backspace(State state, Modifiers modifiers) {
        switch (state) {
            case State.Press:
                ClearSelected();
                break;
        }
        return true;
    }
    public override bool Delete(State state, Modifiers modifiers) {
        switch (state) {
            case State.Press:
                ClearSelected();
                break;
        }
        return true;
    }
    public override bool Escape(State state, Modifiers modifiers) {
        switch (state) {
            case State.Press:
                InputManager.Deselect();
                break;
        }
        return true;
    }

    public override bool Space(State state, Modifiers modifiers) => true;
    public override bool Return(State state, Modifiers modifiers) => true;

    public override bool Left(State state, Modifiers modifiers) {
        switch (state) {
            case State.Press:
                TapePointEnd = TapePointEnd.PrevOffset;
                UnselectIfNoShift(modifiers);
                break;
        }
        return true;
    }
    public override bool Right(State state, Modifiers modifiers) {
        switch (state) {
            case State.Press:
                TapePointEnd = TapePointEnd.NextOffset;
                UnselectIfNoShift(modifiers);
                break;
        }
        return true;
    }
    public override bool Up(State state, Modifiers modifiers) {
        switch (state) {
            case State.Press:
                if (TapePointEnd.TapeIndex > 0) {
                    TapePointEnd = TapePointEnd.PrevTape;
                }
                UnselectIfNoShift(modifiers);
                break;
        }
        return true;
    }
    public override bool Down(State state, Modifiers modifiers) {
        switch (state) {
            case State.Press:
                if (TapePointEnd.TapeIndex < Cryptex.Tapes.Count - 1) {
                    TapePointEnd = TapePointEnd.NextTape;
                }
                UnselectIfNoShift(modifiers);
                break;
        }
        return true;
    }

    public override bool Home(State state, Modifiers modifiers) {
        switch (state) {
            case State.Press:
                TapePointEnd = TapePointEnd.WithTape(0);
                UnselectIfNoShift(modifiers);
                break;
        }
        return true;
    }
    public override bool End(State state, Modifiers modifiers) {
        switch (state) {
            case State.Press:
                TapePointEnd = TapePointEnd.WithTape(Cryptex.Tapes.Count - 1);
                UnselectIfNoShift(modifiers);
                break;
        }
        return true;
    }

    private int MinOffsetIndex => Mathf.Min(TapePointStart.OffsetIndex, TapePointEnd.OffsetIndex);
    private int MaxOffsetIndex => Mathf.Max(TapePointStart.OffsetIndex, TapePointEnd.OffsetIndex);
    private int MinTapeIndex => Mathf.Min(TapePointStart.TapeIndex, TapePointEnd.TapeIndex);
    private int MaxTapeIndex => Mathf.Max(TapePointStart.TapeIndex, TapePointEnd.TapeIndex);

    private void GetMinMax(out int minOffsetIndex, out int maxOffsetIndex, out int minTapeIndex, out int maxTapeIndex) {
        minOffsetIndex = MinOffsetIndex;
        maxOffsetIndex = MaxOffsetIndex;
        minTapeIndex = MinTapeIndex;
        maxTapeIndex = MaxTapeIndex;
    }

    private struct DirectedRegion : IEnumerable<DirectedRow> {
        private TapeSelectionPoint start;
        private TapeSelectionPoint end;
        private bool rotated;
        public DirectedRegion(TapeSelectionPoint start, TapeSelectionPoint end, bool rotated) {
            this.start = start;
            this.end = end;
            this.rotated = rotated;
        }

        public IEnumerator<DirectedRow> GetEnumerator() {
            int startMajor, startMinor, endMajor, endMinor;
            if (rotated) {
                startMajor = start.TapeIndex;
                endMajor = end.TapeIndex;
                startMinor = start.OffsetIndex;
                endMinor = end.OffsetIndex;
            } else {
                startMajor = start.OffsetIndex;
                endMajor = end.OffsetIndex;
                startMinor = start.TapeIndex;
                endMinor = end.TapeIndex;
            }
            for (int i = startMajor; i <= endMajor; ++i) {
                yield return new DirectedRow(startMinor, endMinor, i, rotated);
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    private struct DirectedRow : IEnumerable<TapeSelectionPoint> {
        private int start;
        private int end;
        private int offset;
        private bool rotated;

        public DirectedRow(int start, int end, int offset, bool rotated) {
            Assert.GreaterOrEqual(end, start);
            this.start = start;
            this.end = end;
            this.offset = offset;
            this.rotated = rotated;
        }

        public IEnumerator<TapeSelectionPoint> GetEnumerator() {
            for (int i = start; i <= end; ++i) {
                yield return rotated ? new TapeSelectionPoint(offset, i) : new TapeSelectionPoint(i, offset);
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    private string GetNotesBlock(DirectedRow row) {
        var sb = new StringBuilder();
        int prevIndex = -1;
        int i = 0;
        foreach (var tsp in row) {
            var tape = Cryptex.Tapes[tsp.TapeIndex];
            foreach (var note in tape.Note(tsp.OffsetIndex)) {
                // LEs should be 'rasterized' here so that copy/pasted code is human readable (and prevent hand-written code from being unintentionally parsed)
                string text = note.Text.ToString();
                var explicitIndex = prevIndex + 1 == i ? Option.None : i.ToOption();
                prevIndex = i;

                using (var sr = new StringReader(text)) {
                    for (string line = sr.ReadLine(); line != null; line = sr.ReadLine()) {
                        bool isContinued = sr.Peek() >= 0;
                        sb.AppendLine(CreateNote(line, isContinued, explicitIndex));
                        explicitIndex = Option.None;
                    }
                }
            }
            ++i;
        }
        return sb.ToString();
    }

    private static string CreateNote(string text, bool isContinued, Option<int> explicitIndex) {
        var sb = new StringBuilder("! ");
        sb.Append(text);
        if (text.Length > 0 && text[text.Length - 1] == ']') {
            sb.Append(' ');
        }
        if (isContinued || explicitIndex.HasValue) {
            sb.Append('[');
            foreach (var explicitIndexValue in explicitIndex) {
                sb.Append(explicitIndexValue);
            }
            if (isContinued) {
                sb.Append(',');
            }
            sb.Append(']');
        }
        return sb.ToString();
    }

    public override bool Copy(Modifiers modifiers) {
        var options = new CopyPasteOptions(fill: false, includeExtras: true);
        GUIUtility.systemCopyBuffer = SerializeSelected(options);

        return true;
    }
    public override bool Cut(Modifiers modifiers) {
        Copy(modifiers);
        ClearSelected();
        return true;
    }
    public override bool Paste(Modifiers modifiers) {
        bool fill = modifiers.HasCtrl() || modifiers.HasShift();
        var options = new CopyPasteOptions(fill: fill, includeExtras: true);

        var text = GUIUtility.systemCopyBuffer;
        Overseer.Workspace.ValueOrAssert().ApplyModificationBatchRecord(DeserializeSelected(text, options));

        return true;
    }

    private sealed class CopyPasteOptions {
        public bool Fill { get; }
        public bool IncludeExtras { get; }

        public CopyPasteOptions(bool fill = false, bool includeExtras = false) {
            Fill = fill;
            IncludeExtras = includeExtras;
        }
    }

    private sealed class FileLine {
        public IReadOnlyList<IReadOnlyList<TapeValue>> MetaCommands { get; }
        public IReadOnlyList<NitNote> Notes { get; }
        public IReadOnlyList<TapeValue> TapeValues { get; }

        public FileLine(IEnumerable<IEnumerable<TapeValue>> metaCommands, IEnumerable<NitNote> notes, IEnumerable<TapeValue> tapeValues) {
            MetaCommands = metaCommands.Select(mc => mc.ToList()).ToList();
            Notes = notes.ToList();
            TapeValues = tapeValues.ToList();
        }
    }

    private sealed class NitNote {
        public string Text { get; }
        public int? Index { get; }

        public NitNote(string text, int? index) {
            Text = text;
            Index = index;
        }
    }

    private static readonly char[] lineEndChars = new[] { '\n', '\r' };

    private static readonly Parser<string> Comment = (
        from pound in Parse.Char('#')
        from text in Parse.CharExcept(lineEndChars).Many().Text()
        select text
        ).Named("comment");

    private static readonly Parser<object> BlankLine = (
        from l in Comment.XOr(Parse.WhiteSpace.Except(Parse.Chars(lineEndChars)).Many())
        from le in Parse.LineTerminator
        select (object)null
        ).Named("blank-line");

    private static readonly Parser<char> TapeValueChar = Parse.Or(Parse.LetterOrDigit, Parse.Chars(TapeValueFrameRead.Prefix, TapeValueLabel.Prefix)).Named("tape-value-char");

    private static readonly Parser<TapeValue> TapeValue = (
        from text in Parse.Text(TapeValueChar.Many())
        select text.Length == 0 ? null : global::TapeValue.FromString(text))
        .Named("tape-value");

    private static readonly Parser<IEnumerable<TapeValue>> MetaCommand = (
        from lb in Parse.Char('[')
        from tvl in TapeValueList
        from rb in Parse.Char(']')
        select tvl)
        .Named("meta-command");

    private static readonly Parser<IEnumerable<TapeValue>> MetaCommandLine = (
        from bl in BlankLine.Many()
        from mc in MetaCommand
        from le in Parse.LineTerminator
        select mc)
        .Named("meta-command-line");

    private static readonly Parser<NitNote> Note = (
        from bq in Parse.Char('"')
        from text in Parse.CharExcept(lineEndChars.Append('"')).Many().Text().Named("note-text")
        from eq in Parse.Char('"')
        from index in Parse.Char('@').Then(_ => Parse.Digit).Optional().Named("note-index")
        select new NitNote(text, index.IsDefined ? index.Get() - '0' : (int?)null)
        ).Named("note");

    private static readonly Parser<NitNote> NoteLine = (
        from bl in BlankLine.Many()
        from note in Note
        from el in Parse.LineTerminator
        select note
        ).Named("note-line");

    private static readonly Parser<IEnumerable<TapeValue>> TapeValueList = TapeValue.DelimitedBy(Parse.Char(' '), minimumCount: 1, maximumCount: null).Named("tape-value-list");

    private static readonly Parser<IEnumerable<TapeValue>> TapeValueListLine = (
        from bl in BlankLine.Many()
        from tvl in TapeValueList
        from le in Parse.LineTerminator
        select tvl)
        .Named("tape-value-list-line");

    private static readonly Parser<FileLine> LogicalLine = (
        from mcs in MetaCommandLine.Many()
        from ns in NoteLine.Many()
        from tvl in TapeValueListLine
        select new FileLine(mcs, ns, tvl))
        .Named("line");

    private static readonly Parser<IEnumerable<FileLine>> File = (
        from ls in LogicalLine.Many()
        from bl in BlankLine.Many()
        select ls)
        .Named("file");

    private IEnumerable<Record> DeserializeSelected(string text, CopyPasteOptions options) {
        GetMinMax(out int minOffset, out int maxOffset, out int minTape, out int maxTape);

        bool InTapeRange(int tapeIndex) => minTape <= tapeIndex && tapeIndex <= maxTape;
        bool InOffsetRange(int offsetIndex) => minOffset <= offsetIndex && offsetIndex <= maxOffset;

        var result = File.TryParse(text);
        if (result.WasSuccessful) {
            int offset = minOffset;

            do {
                foreach (var line in result.Value) {
                    foreach (var mc in line.MetaCommands) {
                        var firstType = mc[0].Type;
                        if (firstType == TapeValueType.Int) {
                            offset += mc[0].To<int>();
                        } else if (firstType == TapeValueType.Label) {
                            if (options.IncludeExtras && InOffsetRange(offset)) {
                                // TODO how should duplicate labels be handled?
                                yield return Cryptex.AddLabel(offset, new Label.Serial(Guid.NewGuid(), LC.User(mc[0].To<string>())));
                            }
                        }
                    }

                    if (options.IncludeExtras && InOffsetRange(offset)) {
                        int tapeIndex = 0;
                        foreach (var note in line.Notes) {
                            tapeIndex = note.Index ?? tapeIndex;
                            yield return Cryptex.Tapes[minTape + tapeIndex].AddNote(offset, LocalizationExpression.Parse(note.Text));
                            ++tapeIndex;
                        }
                    }

                    if (InOffsetRange(offset)) {
                        int tape = minTape;
                        do {
                            foreach (var value in line.TapeValues) {
                                if (value is object && InTapeRange(tape)) {
                                    yield return Cryptex.Tapes[tape].Write(offset, value);
                                }
                                ++tape;
                            }
                        } while (options.Fill && tape <= maxTape);
                    }

                    ++offset;
                }
            } while (options.Fill && offset <= maxOffset);
        }
    }

    private string SerializeSelected(CopyPasteOptions options) {
        var sb = new StringBuilder();
        GetMinMax(out int minOffset, out int maxOffset, out int minTape, out int maxTape);

        // How many offsets have we skipped? If any, we need to start the next unskipped offset with an offset meta command
        int skippedOffsets = 0;

        for (int i = minOffset; i <= maxOffset; ++i) {
            // So far, have we written nothing for this offset?
            bool isSkipping = true;
            // Is the next value we write the first modified value in this offset (i.e. have we written no values at this offset yet?)
            bool isFirstValue = true;
            // How many tapes has it been since we wrote our last modified value? Start at -1 to prevent a preceding space before the values
            int skippedValues = -1;

            // Our first indication we can't skip this offset could be either from a label, or from a modified value. 
            // However, the offset meta command must come before either of these, so if it is needed and hasn't been written yet, do that before writing anything else
            void DontSkip() {
                if (isSkipping) {
                    if (skippedOffsets > 0) {
                        WriteOffset(sb, skippedOffsets);
                        skippedOffsets = 0;
                    }
                    isSkipping = false;
                }
            }

            // Add a label meta command if needed
            foreach (var label in Cryptex.GetLabel(i)) {
                DontSkip();
                WriteLabel(sb, label.Name.ToString());
            }

            bool noteNeedsIndex = false;
            for (int j = minTape; j <= maxTape; ++j) {
                if (Cryptex.Tapes[j].Note(i).TryGetValue(out var note)) {
                    WriteNote(sb, note.Text.SerialString, noteNeedsIndex ? j - minTape : (int?)null);
                    noteNeedsIndex = false;
                } else {
                    noteNeedsIndex = true;
                }
            }

            for (int j = minTape; j <= maxTape; ++j) {
                // If we haven't written a value yet, hold off on adding spaces, because this offset's values may be skipped
                if (isFirstValue) {
                    ++skippedValues;
                } else {
                    sb.Append(' ');
                }

                if (Cryptex.Tapes[j].IsModified(i)) {
                    DontSkip();

                    // If we skipped over unmodified values at the start of the offset, put spaces for each one now
                    for (int k = 0; k < skippedValues; ++k) {
                        sb.Append(' ');
                    }
                    skippedValues = 0;
                    isFirstValue = false;

                    // Finally write the modified value
                    sb.Append(Cryptex.Tapes[j].Read(i).GetText());
                }
            }

            // If we wrote any values, add a new line
            if (!isFirstValue) {
                sb.AppendLine();
            }

            // If this offset was skipped, the next unskipped offset must use an offset meta command
            if (isSkipping) {
                ++skippedOffsets;
            }
        }

        return sb.ToString();
    }

    private void WriteMetaCommand(StringBuilder sb, params TapeValue[] values) => WriteMetaCommand(sb, (IEnumerable<TapeValue>)values);

    private void WriteMetaCommand(StringBuilder sb, IEnumerable<TapeValue> values) {
        sb.Append('[');
        sb.Append(string.Join(" ", values.Select(v => v.GetText().ToString())));
        sb.Append(']');
        sb.AppendLine();
    }

    private void WriteOffset(StringBuilder sb, int offset) => WriteMetaCommand(sb, new TapeValueInt(offset));
    private void WriteLabel(StringBuilder sb, string label) => WriteMetaCommand(sb, new TapeValueLabel(label));
    private void WriteNote(StringBuilder sb, string text, int? index) {
        sb.Append('"');
        sb.Append(text);
        sb.Append('"');
        if (index.HasValue) {
            sb.Append('@');
            sb.Append(index.Value);
        }
        sb.AppendLine();
    }

    private void ClearSelected() {
        var workspace = Overseer.Workspace.ValueOrAssert();
        using (workspace.NewModificationBatchFrame()) {
            foreach (var row in Region) {
                foreach (var tvp in row) {
                    var tape = Cryptex.Tapes[tvp.TapeIndex];

                    if (!tape.Lock.HasFlag(LockType.Edit)) {
                        if (tape.Note(tvp.OffsetIndex).HasValue) {
                            workspace.ApplyModificationRecord(tape.RemoveNote(tvp.OffsetIndex));
                        }
                        var record = tape.Unwrite(tvp.OffsetIndex);
                        workspace.ApplyModificationRecord(record);
                    }
                }
            }
        }
    }

    private void UnselectIfNoShift(Modifiers modifiers) {
        if (!modifiers.HasShift()) {
            TapePointStart = TapePointEnd;
        }
    }

    private DirectedRegion Region => new DirectedRegion(TapePointStart, TapePointEnd, Cryptex.Rotated);
}
