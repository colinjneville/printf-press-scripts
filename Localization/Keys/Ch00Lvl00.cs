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
    public static readonly LD Ch00Lvl00_Name = new LD("Introduction");

    public static readonly LD Ch00Lvl00_00 = new LD($"These are the devices I whipped up to help me weave through the Tangle.");
    public static readonly LD Ch00Lvl00_01 = new LD($"They may look unassuming, but these little guys can work their way through an assignment a hundred times faster than me.");
    public static readonly LD Ch00Lvl00_02 = new LD($"First, we attach the paper tapes we want to process to one of these backplates, which are called platens.");
    public static readonly LD Ch00Lvl00_03 = new LD($"The platens don't really do anything on their own - they just hold all the important parts together. Usually, you'll need at least two platens - I'll explain why later.");
    public static readonly LD Ch00Lvl00_04 = new LD($"As you may have noticed, we call these thin strips of paper that make up the Tangle 'tapes.' Tapes practically <b>go on forever,</b> but they can slide left and right across the platen.");
    public static readonly LD Ch00Lvl00_05 = new LD($"The tapes have all sorts of information printed on them, in small chunks we call 'notational units,' or 'nits' for short.");
    public static readonly LD Ch00Lvl00_06 = new LD($"There are several types of nits we print. Letters, numbers, colors, to name a few.");
    public static readonly LD Ch00Lvl00_07 = new LD($"Heads are the workhorses of the apparatus. Heads can scan the contents of the tapes, and they can move freely over the platen and tapes.");
    public static readonly LD Ch00Lvl00_08 = new LD($"Heads are made up of 'frames,' each of which fits over a single nit on a tape. The frames are stacked vertically such that each is on a different tape.");
    public static readonly LD Ch00Lvl00_09 = new LD($"There are actually two different kinds of heads - this one is a 'control head.'");
    public static readonly LD Ch00Lvl00_10 = new LD($"Once the device is turned on, a control head will move steadily to the right along the tapes. Each time it does, it will scan the nits underneath it, looking for instuction nits.");
    public static readonly LD Ch00Lvl00_11 = new LD($"Instructions are a special type of nit I devised to operate the heads. You can recognize them by their <b>three-letter</b> groupings.");
    public static readonly LD Ch00Lvl00_12 = new LD($"When a control frame reads an instruction, it will do something based on the type of instruction and the nits below it.");
    public static readonly LD Ch00Lvl00_13 = new LD($"For example, 'out' is the instruction I use to print out the completed analysis, one nit at a time.");
    public static readonly LD Ch00Lvl00_14 = new LD($"The next nit below 'out' will be printed to our output tape.");
    public static readonly LD Ch00Lvl00_15 = new LD($"The last nit should always be 'c0' - I had some other plans, but never got around to implementing them.");
    public static readonly LD Ch00Lvl00_16 = new LD($"There's one other instruction here, 'jmp', which jumps the control head to the right.");
    public static readonly LD Ch00Lvl00_17 = new LD($"The next nit indicates how many steps to jump. If the number is negative, the control head will move to the left.");
    public static readonly LD Ch00Lvl00_18 = new LD($"Keep in mind the control head will still move one space to the right immediately after jumping.");
    public static readonly LD Ch00Lvl00_19 = new LD($"The arrows above the platen will help you visualize how the control head will move.");
    public static readonly LD Ch00Lvl00_20 = new LD($"Now it's time to see it in action. The buttons up here operate the device.");
    public static readonly LD Ch00Lvl00_21 = new LD($"The Run button (F5) turns it on and keeps it running until analysis completes, or until something goes wrong.");
    public static readonly LD Ch00Lvl00_22 = new LD($"The Step button (F6) turns the device on, but only for a single step. Each time you push the button, it will advance a single step. This is useful to inspect how your device is working.");
    public static readonly LD Ch00Lvl00_23 = new LD($"The Unstep button (F7) undoes the last step. If you want to go back to check why something happened, you can use this button.");
    public static readonly LD Ch00Lvl00_24 = new LD($"The Pause button (F8) halts the device after pressing the Run button.");
    public static readonly LD Ch00Lvl00_25 = new LD($"Finally, the Stop button (F9) resets the device to how it was before it started running.");
    public static readonly LD Ch00Lvl00_26 = new LD($"In the upper right, you can see the expected output on a dangling tape. As the device prints output, it will show up on another tape next to it.");
    public static readonly LD Ch00Lvl00_27 = new LD($"My notes, marked with red trianges, can be viewed by hovering over them. Then, try stepping through to see how the control head works.");

    public static readonly LD Ch00Lvl00_N0_0_n9 = new LD($"You can press F1 to repeat each level's introduction");
    public static readonly LD Ch00Lvl00_N0_0_0 = new LD($"Instructions tell the device what work to do.\n'out' prints a nit to be compared to the expected solution");
    public static readonly LD Ch00Lvl00_N0_1_0 = new LD($"The nit you want to print goes here");
    public static readonly LD Ch00Lvl00_N0_0_5 = new LD($"'jmp' moves the control head a given number of spaces to the right");
    public static readonly LD Ch00Lvl00_N0_1_5 = new LD($"This number determines how far to move right (a negative number moves to the left)");
    public static readonly LD Ch00Lvl00_N0_1_n2 = new LD($"Once all the nits have been printed to match the expected result (or if an unexpected nit is printed), the device automatically stops");
}

