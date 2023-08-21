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
    public static readonly LD Ch00Lvl04_Name = new LD($"Editing");

    public static readonly LD Ch00Lvl04_00 = new LD($"Now that you know all pieces, I'll teach you how to construct your own devices.");
    public static readonly LD Ch00Lvl04_01 = new LD($"I'll teach you the basics now, but you'll also want to check out the manual I put together to get all the details.");
    public static readonly LD Ch00Lvl04_02 = new LD($"At the bottom of your workspace is a toolbox with the various components. Left click and drag a component from the toolbox to add it to the device. You can also left click and drag an existing component to move it.");
    public static readonly LD Ch00Lvl04_03 = new LD($"To remove a compoment, right click it. Note that some components are required, and cannot be removed or modified.");
    public static readonly LD Ch00Lvl04_04 = new LD($"To change a component, middle click it. What it means to change a component depends on its type, so either try it out yourself, or consult the manual.");
    public static readonly LD Ch00Lvl04_05 = new LD($"Use ctrl + z to undo actions, and crtl + y to redo them.");
    public static readonly LD Ch00Lvl04_06 = new LD($"You can write to the tapes as well. There are two ways to do so. The first is to drag nits from the toolbox onto a tape.");
    public static readonly LD Ch00Lvl04_07 = new LD($"To view nits in the toolbox, left click the tab with the type of nit you want. The toolbox doesn't have all possible nits, however, so I recommend you use the second method.");
    public static readonly LD Ch00Lvl04_08 = new LD($"To write directly, left click on a nit on the tape. This will open a text box where you can type the text representation of the nit.");
    public static readonly LD Ch00Lvl04_09 = new LD($"Use the arrow keys, space, and return to changewhich nit you are editing. Press escape to stop editing and discard any changes for the current nit.");
    public static readonly LD Ch00Lvl04_10 = new LD($"There are lots of other shortcuts, and even a way to edit multiple nits at once, but I won't go over them now - consult the manual later.");
    public static readonly LD Ch00Lvl04_11 = new LD($"Your task is print the opposite number of each number on the tape. So print '-3' for '3', '2' for '-2', '0' for '0', and so on.");

    public static readonly LD Ch00Lvl04_N1_0_0 = new LD($"The numbers to negate start here");
}
