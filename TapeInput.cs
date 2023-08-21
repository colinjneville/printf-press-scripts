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

public struct TapeSelectionPoint {
    private int tapeIndex;
    private int offsetIndex;

    public TapeSelectionPoint(int tapeIndex, int offsetIndex) {
        this.tapeIndex = tapeIndex;
        this.offsetIndex = offsetIndex;
    }

    public int TapeIndex => tapeIndex;
    public int OffsetIndex => offsetIndex;

    public TapeSelectionPoint WithTape(int tapeIndex) => new TapeSelectionPoint(tapeIndex, offsetIndex);
    public TapeSelectionPoint WithOffset(int offsetIndex) => new TapeSelectionPoint(tapeIndex, offsetIndex);
    public TapeSelectionPoint NextTape => new TapeSelectionPoint(tapeIndex + 1, offsetIndex);
    public TapeSelectionPoint PrevTape => new TapeSelectionPoint(tapeIndex - 1, offsetIndex);
    public TapeSelectionPoint NextOffset => new TapeSelectionPoint(tapeIndex, offsetIndex + 1);
    public TapeSelectionPoint PrevOffset => new TapeSelectionPoint(tapeIndex, offsetIndex - 1);
}
