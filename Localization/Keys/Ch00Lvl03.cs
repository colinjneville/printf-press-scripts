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

partial class LocalizationDefault {
    public static readonly LD Ch00Lvl03_Name = new LD($"Shifting");

    public static readonly LD Ch00Lvl03_00 = new LD($"There's one more set of instructions you should know before you get started: 'shr' (Shift Left) and 'shl' (Shift Left).");
    public static readonly LD Ch00Lvl03_01 = new LD($"The shift instructions use a new nit type - frame indexes. They are written like frame references, just without the '@'. Frame indexes are used to work with a specific frame on an auxiliary head.");
    public static readonly LD Ch00Lvl03_02 = new LD($"Shift instructions move a single tape - and all nits on it - left or right a given number of spaces.");
    public static readonly LD Ch00Lvl03_03 = new LD($"The first nit below it indicates the number of spaces to shift the tape.");
    public static readonly LD Ch00Lvl03_04 = new LD($"The second nit is a frame index - whichever tape is under that number frame on the auxiliary head will be the one moved.");
    public static readonly LD Ch00Lvl03_05 = new LD($"If you have a tape with a number line, you can use the shift instructions to do math - shl adds, and shr subtracts. This device takes groups of numbers and prints the sums of the individual groups.");

    public static readonly LD Ch00Lvl03_N0_0_0 = new LD($"'ife' executes the next instruction only if the nits are the same");
    public static readonly LD Ch00Lvl03_N0_2_0 = new LD($"Blank nits can be used for some instructions. Here we check if f1 has a blank nit (meaning we have completed the current group)");
    public static readonly LD Ch00Lvl03_N0_1_1 = new LD($"Label names can be used instead of numbers. Instead of counting the number of spaces you want to jump, use ':' followed by a label's name");
    public static readonly LD Ch00Lvl03_N0_0_2 = new LD($"'shl' shifts a tape under a frame a given number of spaces to the left. Only the tape moves, the roller stays in place");
    public static readonly LD Ch00Lvl03_N0_1_2 = new LD($"How many spaces to move the tape left. When shifting a number line tape, 'shl' effectively adds the this number to the number that was under the frame");
    public static readonly LD Ch00Lvl03_N0_2_2 = new LD($"Which tape to shift");
    public static readonly LD Ch00Lvl03_N0_0_5 = new LD($"'shl' shifts a tape under a frame a given number of spaces to the right");
    public static readonly LD Ch00Lvl03_N0_1_5 = new LD($"Subtracting the current number from a number line tape resets it to zero");

    public static readonly LD Ch00Lvl03_N1_0_0 = new LD($"The groups of numbers to sum start here");
    public static readonly LD Ch00Lvl03_N1_1_0 = new LD($"We can use this number line to keep track of the sum so far. We use 'shl' to add a number, and 'shr' to reset to zero once we have finished a group");
}
