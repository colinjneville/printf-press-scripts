using Functional.Option;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using L = LocalizationString;
using LC = LocalizationConstant;
using LE = ILocalizationExpression;
using LF = LocalizationFormat;
using LI = LocalizationInt;

public sealed class TapeValueRootViewBit : MonoBehaviour, IApertureTarget, IPooled {
#pragma warning disable CS0649
    [SerializeField]
    private TapeModifyPoint tapeModifyPoint;
    [SerializeField]
    private TapeInsertPoint tapeInsertPoint;
    [SerializeField]
    private TapeValueModifyPoint tapeValueModifyPoint;
    [SerializeField]
    private TapeValueInsertPoint tapeValueInsertPoint;
    [SerializeField]
    private RollerInsertPoint rollerInsertPoint;
    [SerializeField]
    private Image noteMarker;
    [SerializeField]
    private Image breakpointMarker;
#pragma warning restore CS0649
    [SerializeField]
    [HideInInspector]
    private PooledRef<TapeValueView> tapeValueViewRef;

    private PooledTracker<TapeValueRootViewBit> tracker;

    private static Pool<TapeValueRootViewBit> pool = new Pool<TapeValueRootViewBit>(Create, reserveCount: 100);

    public static void FillPool(int count) => pool.Fill(count);

    public static PooledRef<TapeValueRootViewBit> Get() => pool.Get();

    public void ForceReturn() => ((IPooled)this).Return(tracker.Era);

    void IPooled.Return(int era) {
        if (tracker.Return(era)) {
            tapeValueViewRef.Return();
            gameObject.SetActive(false);

            if (!Overseer.Quitting && Overseer.Hideaway != null) {
                transform.SetParent(Overseer.Hideaway, false);
            }
        }
    }

    private void Awake() {
        tracker = new PooledTracker<TapeValueRootViewBit>(this, pool);
    }

    private static PooledTracker<TapeValueRootViewBit> Create() {
        var obj = Utility.Instantiate(Overseer.GlobalAssets.TapeValueRootPrefab, null);
        return obj.tracker;
    }

    void IPooled.Init() {
        Assert.NotHasValue(tapeValueViewRef.Value);
        tapeValueViewRef = TapeValueView.Get();

        foreach (var tapeValueView in tapeValueViewRef) {
            tapeValueView.gameObject.SetActive(true);

            var rt = tapeValueView.GetComponent<RectTransform>();
            rt.SetParent(transform, false);
            rt.anchorMin = new Vector2(0.125f, 0.125f);
            rt.anchorMax = new Vector2(0.875f, 0.875f);
            rt.sizeDelta = Vector2.zero;
        }
    }

    public WorkspaceFull Workspace {
        get => tapeInsertPoint.Workspace;
        set {
            tapeModifyPoint.Workspace = value;
            tapeInsertPoint.Workspace = value;
            tapeValueModifyPoint.Workspace = value;
            tapeValueInsertPoint.Workspace = value;
            rollerInsertPoint.Workspace = value;
        }
    }

    public Cryptex Cryptex {
        get => tapeInsertPoint.Cryptex;
        set => tapeInsertPoint.Cryptex = value;
    }

    public Tape Tape {
        get => tapeModifyPoint.Tape;
        set {
            tapeModifyPoint.Tape = value;
            tapeInsertPoint.Tape = value;
            tapeValueModifyPoint.Tape = value;
            tapeValueInsertPoint.Tape = value;
            rollerInsertPoint.Tape = value;
        }
    }

    public int TapeIndex {
        get => tapeInsertPoint.Index;
        set => tapeInsertPoint.Index = value;
    }

    public int ValueIndex {
        get => tapeInsertPoint.Offset;
        set {
            tapeModifyPoint.Offset = value;
            tapeInsertPoint.Offset = value;
            tapeValueModifyPoint.Offset = value;
            tapeValueInsertPoint.Offset = value;
            rollerInsertPoint.Offset = value;
            var rt = GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMin.WithX(value * 1.1f);
            rt.anchorMax = rt.anchorMax.WithX(value * 1.1f + 1f);
        }
    }

    public TapeValue TapeValue {
        get => tapeValueViewRef.Value.ValueOrAssert().TapeValue;
        set => tapeValueViewRef.Value.ValueOrAssert().TapeValue = value;
    }

    public bool HasNote {
        get => noteMarker.enabled;
        set => noteMarker.enabled = value;
    }

    public bool HasBreakpoint {
        get => breakpointMarker.enabled;
        set => breakpointMarker.enabled = value;
    }

    public Option<FontSizeSync> FontSizeSync {
        get => tapeValueViewRef.Value.ValueOrAssert().FontSizeSync;
        set => tapeValueViewRef.Value.ValueOrAssert().FontSizeSync = value;
    }

    Bounds2D IApertureTarget.Bounds => GetComponent<RectTransform>().GetWorldBounds();
}
