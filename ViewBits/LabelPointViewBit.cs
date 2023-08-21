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

public class LabelPointViewBit : MonoBehaviour {
#pragma warning disable CS0649
    [SerializeField]
    private LabelInsertPoint labelInsertPoint;
    [SerializeField]
    private TapeInsertPoint tapeInsertPoint;
#pragma warning restore CS0649

    public WorkspaceFull Workspace {
        get => labelInsertPoint.Workspace;
        set {
            labelInsertPoint.Workspace = value;
            tapeInsertPoint.Workspace = value;
        }
    }

    public Cryptex Cryptex {
        get => labelInsertPoint.Cryptex;
        set {
            labelInsertPoint.Cryptex = value;
            tapeInsertPoint.Cryptex = value;
        }
    }

    public int Index {
        get => labelInsertPoint.Index;
        set {
            labelInsertPoint.Index = value;
            tapeInsertPoint.Offset = value;
            var rt = GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMin.WithX(-0.05f + value * 1.1f);
            rt.anchorMax = rt.anchorMax.WithX(1.05f + value * 1.1f);
        }
    }
}
