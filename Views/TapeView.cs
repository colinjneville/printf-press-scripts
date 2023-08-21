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

partial class Tape : IModel<Tape.TapeView> {
    [JsonIgnore]
    private ViewContainer<TapeView, Tape> view;
    public Option<TapeView> View => view.View;
    public TapeView MakeView() => view.MakeView(this);
    public void ClearView() => view.ClearView();

    [RequireComponent(typeof(RectTransform))]
    public sealed class TapeView : MonoBehaviour, IView<Tape>, IApertureTarget {
        public Tape Model { get; set; }

        private TapeViewBit bit;

        private FontSizeSync fontSizeSync;

        private Deque<PooledSharedRef<TapeValueRootViewBit>> visibleValues;
        private int visibleValuesIndex;
        private SortedDictionary<int, PooledRef<TapeValueRootViewBit>> cache;
        private const int maxUnusedCacheEntries = 20;

        // HACK There's some sort of bug in TextMeshPro that makes auto-sizing not work the first update, so we need to force one the frame after...
        private bool delayedUpdateHack;

        private const float valueSize = 0.75f;

        private void Awake() {
            bit = Utility.Instantiate(Overseer.GlobalAssets.TapePrefab, transform);

            visibleValues = new Deque<PooledSharedRef<TapeValueRootViewBit>>();
            cache = new SortedDictionary<int, PooledRef<TapeValueRootViewBit>>();
            fontSizeSync = new FontSizeSync() { AllowGrow = false, MinSize = 0.1f };
        }

        private void OnDestroy() {
            foreach (var kvp in cache) {
                kvp.Value.Return();
            }
        }

        void IView.StartNow() {
            // For TapeViews straight from the Toolbox
            // HACK
            var indicies = Cryptex.CryptexView.GetContainedValues(10f, -5f, true);
            OnUpdateValues(indicies.Item1, indicies.Item2);
            OnUpdateOffset(0, Model.ShiftOffset, animate: false);

            foreach (var kvp in Model.notes) {
                OnUpdateNote(kvp.Key);
            }
            foreach (var i in Model.breakpoints) {
                OnUpdateBreakpoint(i);
            }
        }
        void IView.OnDestroyNow() { }

        private void Update() {
            fontSizeSync.Update();
            
            if (!delayedUpdateHack) {
                fontSizeSync.RequireUpdate();
                delayedUpdateHack = true;
            }
        }

        private void LateUpdate() {
            if (!(Model.Cryptex is null)) {
                bit.SpriteRect.position = bit.SpriteRect.position.WithX(Camera.main.transform.position.x);
            }
        }

        internal void OnUpdateOffset(int oldShiftOffset, int newShiftOffset, bool animate = true) {
            var diff = oldShiftOffset - newShiftOffset;
            OnUpdateValues(visibleValuesIndex + diff, visibleValuesIndex + visibleValues.Count + diff);
            if (animate) {
                var valueContainerParent = bit.ValueContainer.parent;
                using (bit.Unlock()) {
                    valueContainerParent.localPosition = Vector3.zero.WithX(Model.ShiftOffset * Cryptex.CryptexView.ValueXDiff);
                }
            }
        }

        internal void OnUpdateValues(int newVisibleValuesIndex, int newVisibleValuesEndIndex) {
            newVisibleValuesIndex -= Model.ShiftOffset;
            newVisibleValuesEndIndex -= Model.ShiftOffset;
            if (visibleValuesIndex == newVisibleValuesIndex && visibleValuesIndex + visibleValues.Count == newVisibleValuesEndIndex) {
                // Nothing has changed;
                return;
            }

            // No overlap, just disable visibleValues and clear it
            if (visibleValuesIndex >= newVisibleValuesEndIndex || visibleValuesIndex + visibleValues.Count <= newVisibleValuesIndex) {
                visibleValuesIndex = newVisibleValuesIndex;
                foreach (var vvRef in visibleValues) {
                    foreach (var vv in vvRef) {
                        vv.gameObject.SetActive(false);
                    }
                }
                visibleValues.Clear();
            }

            while (visibleValuesIndex < newVisibleValuesIndex) {
                ++visibleValuesIndex;
                var viewRef = visibleValues.RemoveBack();
                foreach (var view in viewRef) {
                    view.gameObject.SetActive(false);
                }
            }
            while (visibleValuesIndex + visibleValues.Count > newVisibleValuesEndIndex) {
                var viewRef = visibleValues.RemoveFront();
                foreach (var view in viewRef) {
                    view.gameObject.SetActive(false);
                }
            }

            while (visibleValuesIndex > newVisibleValuesIndex) {
                --visibleValuesIndex;
                var viewRef = GetOrCreate(visibleValuesIndex);
                foreach (var view in viewRef) {
                    view.gameObject.SetActive(true);
                }
                visibleValues.AddBack(viewRef.ToShared());
            }
            while (visibleValuesIndex + visibleValues.Count < newVisibleValuesEndIndex) {
                var viewRef = GetOrCreate(visibleValuesIndex + visibleValues.Count);
                foreach (var view in viewRef) {
                    view.gameObject.SetActive(true);
                }
                visibleValues.AddFront(viewRef.ToShared());
            }

            //var valueContainerParent = bit.ValueContainer.parent;
            //using (bit.Unlock()) {
            //    valueContainerParent.localPosition = Vector3.zero.WithX(Model.ShiftOffset * Cryptex.CryptexView.ValueXDiff);
            //}

            fontSizeSync.RequireUpdate();
        }

        internal void OnUpdateNote(int index) {
            foreach (var viewRef in cache.GetOrNone(index)) {
                foreach (var view in viewRef.Value) {
                    view.HasNote = Model.notes.ContainsKey(index);
                }
            }
        }

        internal void OnUpdateBreakpoint(int index) {
            foreach (var viewRef in cache.GetOrNone(index)) {
                foreach (var view in viewRef.Value) {
                    view.HasBreakpoint = Model.breakpoints.Contains(index);
                }
            }
        }

        private PooledRef<TapeValueRootViewBit> GetOrCreate(int index) {
            PooledRef<TapeValueRootViewBit> viewRef;
            if (!cache.TryGetValue(index, out viewRef)) {
                viewRef = CreateOrStealCache();
                foreach (var view in viewRef) {
                    view.TapeValue = Model.Read(index + Model.ShiftOffset);
                    view.ValueIndex = index;
                    view.HasNote = Model.notes.ContainsKey(index);
                    view.HasBreakpoint = Model.breakpoints.Contains(index);
                    cache.Add(index, viewRef);
                }
                
            }
            return viewRef;
        }

        private PooledRef<TapeValueRootViewBit> CreateOrStealCache() {
            if (visibleValues.Count + maxUnusedCacheEntries >= cache.Count && cache.Count > 0) {
                var first = cache.First();
                if (first.Key < visibleValuesIndex) {
                    cache.Remove(first.Key);
                    return first.Value;
                }
                var last = cache.Last();
                if (last.Key > visibleValuesIndex + visibleValues.Count) {
                    cache.Remove(last.Key);
                    return last.Value;
                }
            }
            var rootRef = TapeValueRootViewBit.Get();
            foreach (var root in rootRef) {
                InitRoot(root);
            }
            return rootRef;
        }

        private const float tapeValueScale = 0.75f;

        private void InitRoot(TapeValueRootViewBit root) {
            root.transform.SetParent(bit.ValueContainer, false);
            
            root.Cryptex = Model.Cryptex;
            // TEMP
            //root.FontSizeSync = fontSizeSync;
            root.Tape = Model;
            if (!(Model.Cryptex is null)) {
                // HACK
                root.Workspace = (WorkspaceFull)Model.Cryptex.Workspace;
                root.TapeIndex = Model.Cryptex.Tapes.IndexOf(Model) + 1;
            }
        }

        internal void OnUpdateCryptex() {
            if (!(Model.Cryptex is null)) {
                foreach (var rootRef in cache.Values) {
                    foreach (var root in rootRef) {
                        root.Workspace = (WorkspaceFull)Model.Cryptex.Workspace;
                    }
                }
            }
        }

        internal void OnUpdateIndex() {
            int index = Model.Cryptex.Tapes.IndexOf(Model) + 1;
            foreach (var rootRef in cache.Values) {
                foreach (var root in rootRef) {
                    root.TapeIndex = index;
                }
            }
        }

        public void UpdateTapeValue(int index, TapeValue value) {
            PooledRef<TapeValueRootViewBit> viewRef;
            if (cache.TryGetValue(index, out viewRef)) {
                foreach (var view in viewRef) {
                    view.TapeValue = value;
                }
            }
        }

        public IApertureTarget GetValueTarget(int index) {
            if (cache.TryGetValue(index, out var viewRef)) {
                foreach (var view in viewRef) {
                    return view;
                }
            }
            return ApertureTarget.None;
        }

        Bounds2D IApertureTarget.Bounds => GetComponent<RectTransform>().GetWorldBounds();
    }

}
