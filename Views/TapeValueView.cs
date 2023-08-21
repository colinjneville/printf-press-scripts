using Functional.Option;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using L = LocalizationString;
using LC = LocalizationConstant;
using LE = ILocalizationExpression;
using LF = LocalizationFormat;
using LI = LocalizationInt;

public interface IPooled {
    //int Era { get; }

    void Init();

    void Return(int era);

    void ForceReturn();
}

public class PooledTracker<T> where T : class, IPooled {
    private T obj;
    private Pool<T> pool;
    private int era;
    private bool pooled;

    public PooledTracker(T obj, Pool<T> pool) {
        this.obj = obj;
        this.pool = pool;
    }

    public Option<T> Value => pooled ? Option.None : obj.ToOption();
    public int Era => era;
    public bool Pooled => pooled;

    public bool Return(int era) {
        if (!pooled && this.era == era) {
            pooled = true;
            pool.Return(this);
            return true;
        }
        return false;
    }

    public PooledRef<T> Ref => new PooledRef<T>(this);

    public void Recreate() {
        Assert.True(pooled);
        pooled = false;
        ++era;
    }
}

public class Pool<T> where T : class, IPooled {
    private List<PooledTracker<T>> pool = new List<PooledTracker<T>>();
    private Func<PooledTracker<T>> createFunc;
    private int reserveCount;

    public Pool(Func<PooledTracker<T>> createFunc, int reserveCount = 100) {
        this.createFunc = createFunc;
        this.reserveCount = reserveCount;
    }

    public PooledRef<T> Get() {
        PooledTracker<T> tracker;
        if (pool.Count == 0) {
            tracker = createFunc();
        } else {
            tracker = pool.Pop();
            tracker.Recreate();
        }

        tracker.Value.ValueOrAssert().Init();
        return tracker.Ref;
    }

    public void Fill(int count) {
        count = Mathf.Clamp(reserveCount - pool.Count, 0, count);
        for (int i = 0; i < count; ++i) {
            var tracker = createFunc();
            tracker.Ref.Return();
        }
    }

    public void Return(PooledTracker<T> tracker) {
        pool.Add(tracker);
    }
}

public struct PooledSharedRef<T> : IEnumerable<T> where T : class, IPooled {
    private readonly PooledTracker<T> obj;
    private readonly int era;

    public PooledSharedRef(PooledTracker<T> obj, int era) {
        this.obj = obj;
        this.era = era;
    }

    public Option<T> Value => obj is object && era == obj.Era ? obj.Value : Option.None;

    public IEnumerator<T> GetEnumerator() => Value.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

[Serializable]
public struct PooledRef<T> : IEnumerable<T> where T : class, IPooled {
    [SerializeField]
    private readonly PooledTracker<T> obj;
    [SerializeField]
    private readonly int era;

    public PooledRef(PooledTracker<T> obj) {
        Assert.NotRefNull(obj);
        this.obj = obj;
        era = obj.Era;
    }

    public Option<T> Value => obj is object && era == obj.Era ? obj.Value : Option.None;

    public bool Return() {
        foreach (var o in Value) {
            o.Return(era);
        }
        return false;
    }

    public PooledSharedRef<T> ToShared() => new PooledSharedRef<T>(obj, era);

    public IEnumerator<T> GetEnumerator() => Value.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

[RequireComponent(typeof(RectTransform))]
public sealed class TapeValueView : MonoBehaviour, IPooled, IApertureTarget {
    private PooledTracker<TapeValueView> tracker;
    private TapeValueViewBit bit;

    private void Awake() {
        tracker = new PooledTracker<TapeValueView>(this, pool);
        bit = Utility.Instantiate(Overseer.GlobalAssets.TapeValuePrefab, transform);
    }

    private static Pool<TapeValueView> pool = new Pool<TapeValueView>(Create, reserveCount: 100);
    public static PooledRef<TapeValueView> Get() => pool.Get();

    public static void FillPool(int count) => pool.Fill(count);

    public static PooledRef<TapeValueView> GetWith(TapeValue value) {
        var @ref = Get();
        foreach (var obj in @ref) {
            obj.TapeValue = value;
        }
        return @ref;
    }

    public void ForceReturn() => ((IPooled)this).Return(tracker.Era);

    void IPooled.Return(int era) {
        if (tracker.Return(era)) {
            if (!Overseer.Quitting && Overseer.Hideaway != null) {
                TapeValue = TapeValueNull.Instance;
                bit.FontSizeSync = Option.None;
                transform.SetParent(Overseer.Hideaway, false);
                gameObject.SetActive(false);

                //if (gameObject != null) {
                //    TapeValue = TapeValueNull.Instance;
                //    bit.FontSizeSync = Option.None;
                //    // TODO? Assume if the Hideaway is null, we've begun scene teardown, so just destroy the View
                //    if (Overseer.Hideaway == null) {
                //        Debug.Log("no hideaway! " + gameObject.GetInstanceID());
                //        Destroy(gameObject);
                //    } else {
                //        transform.SetParent(Overseer.Hideaway, false);
                //        gameObject.SetActive(false);
                //        cache.Push(this);
                //    }
                //} else {
                //    Debug.Log("already destroyed!");
                //}
            }
        }
    }

    private static PooledTracker<TapeValueView> Create() {
        var go = new GameObject(nameof(TapeValueView));
        go.AddComponent<RectTransform>().sizeDelta = Vector2.one;
        var tvv = go.AddComponent<TapeValueView>();
        return tvv.tracker;
    }

    void IPooled.Init() {
        gameObject.SetActive(true);
        var rt = GetComponent<RectTransform>();
        rt.SetParent(null, false);
        rt.localRotation = Quaternion.identity;
        rt.sizeDelta = Vector2.one;
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
    }

    public TapeValue TapeValue {
        get => bit.Value;
        set => bit.Value = value;
    }

    public Option<FontSizeSync> FontSizeSync {
        get => bit.FontSizeSync;
        set => bit.FontSizeSync = value;
    }

    [EasyButtons.Button("Force FSS Update", Mode = EasyButtons.ButtonMode.EnabledInPlayMode)]
    private void ForceFSSUpdate() {
        foreach (var fss in FontSizeSync) {
            Debug.LogWarning($"Forcing {fss.GetHashCode()} update");
            fss.RequireUpdate();
        }
    }

    Bounds2D IApertureTarget.Bounds => ((IApertureTarget)bit).Bounds;
}
