using Functional.Option;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

using LE = ILocalizationExpression;
using L = LocalizationString;
using LC = LocalizationConstant;
using LF = LocalizationFormat;
using LI = LocalizationInt;

public sealed class CryptexInsertViewBit : MonoBehaviour {
#pragma warning disable CS0649
    [SerializeField]
    private CryptexInsertPoint insertPoint;
#pragma warning restore CS0649

    public WorkspaceFull Workspace {
        get => insertPoint.Workspace;
        set => insertPoint.Workspace = value;
    }
}
