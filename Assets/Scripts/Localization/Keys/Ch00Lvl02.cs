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
    public static readonly LD Ch00Lvl02_Name = new LD($"Tests and Branching");

    public static readonly LD Ch00Lvl02_00 = new LD($"I've got one last component to tell you about. You can get by without them, but tags let you skip the math when setting up jumps.");
    public static readonly LD Ch00Lvl02_01 = new LD($"Instead of counting the number of spaces to jump, you can use a tag reference nit to jump directly to the tag. Tag references start with a colon, followed by the name of the tag, like ':start'.");
    public static readonly LD Ch00Lvl02_02 = new LD($"The previous two examples could only print the one message, which doesn't help with our Tangle problem.");
    public static readonly LD Ch00Lvl02_03 = new LD($"My usual plan of attack is to feed a tape from the Tangle into the device, have it do some analysis, and print out the information I need.");
    public static readonly LD Ch00Lvl02_04 = new LD($"For example, buried in this region of the Tangle, you can find a printout of the operating rate per hour of two of our widgets. We only care about the maximum for each hour, so we can print the greater of the two and turn two tapes into just one!");
    public static readonly LD Ch00Lvl02_05 = new LD($"You can hold down this button (F12) to see what the tapes loaded from the Tangle will look like. The device must be able to match the expected output in the upper right.");
    public static readonly LD Ch00Lvl02_06 = new LD($"The tapes and expected outputs you see when holding the button are only one example - you can't just print out those exact nits and call it a day.");
    public static readonly LD Ch00Lvl02_07 = new LD($"Once the device works on the first set of tapes, you'll need to test it on other sets of tapes - those will be done in the background. If any of those fail, you'll have to make corrections to the device and run it again.");
    public static readonly LD Ch00Lvl02_08 = new LD($"Finally, I have a new set of instructions to show you.The 'ifX' instructions conditionally skip the next instruction - i.e. move right one extra space - if their condition isn't met.");
    public static readonly LD Ch00Lvl02_09 = new LD($"The first nit below the instruction is compared to the second nit below. Here, I use 'ifg' (If Greater) to determine if the number on the top tape is greater than the number on the bottom tape.");
    public static readonly LD Ch00Lvl02_10 = new LD($"If it is greater, the control head will move to the right as normal. Otherwise, the control head will move an extra space to the right, skipping that instruction.");
    public static readonly LD Ch00Lvl02_11 = new LD($"Like 'ifg', there are also 'ife' (If Equal), 'ifn' (If Not Equal), and 'ifl' (If Less) instructions.");

    public static readonly LD Ch00Lvl02_N0_0_0 = new LD($"'ifg' only executes the next instruction if the first number is greater than the second");
    public static readonly LD Ch00Lvl02_N0_1_0 = new LD($"If the number at f0 is not greater than the number at f1, the next instruction ('jmp') will be skipped");
    public static readonly LD Ch00Lvl02_N0_0_2 = new LD($"Output the bottom number");
    public static readonly LD Ch00Lvl02_N0_0_4 = new LD($"Output the top number");
    public static readonly LD Ch00Lvl02_N0_0_5 = new LD($"Move to the next pair of numbers");

    public static readonly LD Ch00Lvl02_N1_0_0 = new LD($"The top set of numbers");
    public static readonly LD Ch00Lvl02_N1_1_0 = new LD($"The bottom set of numbers");
}
