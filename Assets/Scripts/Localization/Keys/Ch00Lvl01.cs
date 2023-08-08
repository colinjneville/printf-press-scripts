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
    public static readonly LD Ch00Lvl01_Name = new LD($"Auxiliary Heads");

    public static readonly LD Ch00Lvl01_00 = new LD($"Remember how I mentioned there were two kinds of heads? You won't be able to do much with just control heads.");
    public static readonly LD Ch00Lvl01_01 = new LD($"The other type is auxiliary heads.They lack the black stripe near the top that control heads have.");
    public static readonly LD Ch00Lvl01_02 = new LD($"Unlike control heads, auxiliary heads do not move on their own.Instead, a control head must execute an instruction to move them.");
    public static readonly LD Ch00Lvl01_03 = new LD($"'mvl' and 'mvr' move the auxiliary head of the matching color (shown by the light on the top of the head) left and right, respectively.");
    public static readonly LD Ch00Lvl01_04 = new LD($"The following nit indicates how far to move. Instead of a number, I'm using a 'frame reference' here.");
    public static readonly LD Ch00Lvl01_05 = new LD($"A frame reference is a special nit type - when read by the control head, it will substitute the nit from the matching auxiliary head.");
    public static readonly LD Ch00Lvl01_06 = new LD($"Frame references come in the form '@fX', where X is a number indicating which frame on the auxiliary head the read will take place. The topmost frame is '@f0', the second from the top is '@f1', etc.");
    public static readonly LD Ch00Lvl01_07 = new LD($"For example, the 'out' instruction will first print this 'h' - once the auxiliary head moves, it will print something different however.");
    public static readonly LD Ch00Lvl01_08 = new LD($"By using frame references you can change the function of your device without duplicating the same instructions over and over.");
    public static readonly LD Ch00Lvl01_09 = new LD($"Here, I've 'encoded' the same message from the previous device, but the control head only needs 3 instructions. Try it out to see how it works.");

    public static readonly LD Ch00Lvl01_N0_1_0 = new LD($"Counts as whatever nit is currently underneath the topmost frame of the auxiliary head");
    public static readonly LD Ch00Lvl01_N0_0_1 = new LD($"'mvr' moves the auxiliary head right");
    public static readonly LD Ch00Lvl01_N0_1_1 = new LD($"How many spaces to move the auxiliary head right (reads the nit from the second from the top frame of the auxiliary head)");

    public static readonly LD Ch00Lvl01_N1_0_n1 = new LD($"For this device, I have stored the nits to print on a separate platen. They can be read with the '@f0' nit");
    public static readonly LD Ch00Lvl01_N1_1_n1 = new LD($"Where the auxiliary head will move next. Read with the '@f1' nit");
}
