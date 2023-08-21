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

public sealed class LevelSequence {
    private sealed class Node {
        public Node(Level level, Option<Node> next = default) {
            Level = level;
            Next = next;
        }

        public Level Level { get; }
        public Option<Node> Next { get; }
    }


}
