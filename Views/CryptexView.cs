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


partial class Cryptex : IModel<Cryptex.CryptexView> {
    [JsonIgnore]
    private ViewContainer<CryptexView, Cryptex> view;
    public Option<CryptexView> View => view.View;
    public CryptexView MakeView() => view.MakeView(this);
    public void ClearView() => view.ClearView();

    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(RectTransform))]
    public sealed class CryptexView : MonoBehaviour, IView<Cryptex>, IApertureTarget {
        public Cryptex Model { get; set; }

        private CryptexViewBit bit;

        private Deque<LabelPointViewBit> visibleLabelPoints;
        private int visibleInsertersIndex;
        private Dictionary<int, LabelPointViewBit> cache;
        private const int maxUnusedCacheEntries = 20;

        private void Awake() {
            visibleLabelPoints = new Deque<LabelPointViewBit>();
            cache = new Dictionary<int, LabelPointViewBit>();
            bit = Utility.Instantiate(Overseer.GlobalAssets.CryptexPrefab, transform);
        }

        void IView.StartNow() {
            bit.Workspace = (WorkspaceFull)Model.Workspace;
            bit.Cryptex = Model;
            OnUpdateXY();

            int index = 0;
            foreach (var tape in Model.Tapes) {
                OnAddTape(index++, tape);
            }
            foreach (var roller in Model.Rollers) {
                OnAddRoller(roller);
            }
            foreach (var label in Model.labels) {
                OnAddLabel(label.Key, label.Value);
            }

            UpdateWidth();
        }

        void IView.OnDestroyNow() {
            foreach (var tape in Model.Tapes) {
                tape.ClearView();
            }
            foreach (var roller in Model.Rollers) {
                roller.ClearView();
            }
            foreach (var label in Model.labels) {
                label.Value.ClearView();
            }
        }

        private void Update() {
            UpdateWidth();
        }

        private void LateUpdate() {
            bit.OffsetRect.position = bit.OffsetRect.position.WithX(this.GetScreen().ViewProxy.transform.position.x);
        }

        private const float tapeHeight = 1f;
        private const float tapeMargin = 0.5f;
        private const float valueSpacing = 0.1f;

        private LabelPointViewBit GetOrCreate(int index) {
            LabelPointViewBit pointBit;
            if (!cache.TryGetValue(index, out pointBit)) {
                pointBit = CreateOrStealCache();
                pointBit.Index = index;
                
                cache.Add(index, pointBit);
            }
            return pointBit;
        }

        private LabelPointViewBit CreateOrStealCache() {
            if (visibleLabelPoints.Count + maxUnusedCacheEntries >= cache.Count) {
                foreach (var kvp in cache) {
                    if (kvp.Key < visibleInsertersIndex || kvp.Key > visibleInsertersIndex + visibleLabelPoints.Count) {
                        cache.Remove(kvp.Key);
                        return kvp.Value;
                    }
                }
            }
            return Create();
        }

        private LabelPointViewBit Create() {
            var pointBit = Utility.Instantiate(Overseer.GlobalAssets.LabelPointPrefab, bit.LabelInsertPointContainer);
            // HACK
            pointBit.Workspace = (WorkspaceFull)Model.Workspace;
            pointBit.Cryptex = Model;
            return pointBit;
        }

        private void PositionTapeView(Tape.TapeView view, int index) {
            var rt = view.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMin.WithY(GetTapeYMin(index));
            rt.anchorMax = rt.anchorMax.WithY(GetTapeYMax(index));
            //view.transform.localPosition = new Vector3(0f, GetTapeY(index), 0f);
        }

        public void AddArrow(ArrowInfo arrow) => bit.AddArrow(arrow);
        public void RemoveArrow(ArrowInfo arrow) => bit.RemoveArrow(arrow);
        public void UpdateArrows(int currentTime, int lookaheadCount) => bit.UpdateArrows(currentTime, lookaheadCount);

        //public static float GetTapeY(int index) => -TapeYDiff * index;
        public static float GetTapeYMin(int index) => (TapeYDiff + 1f) * -index;
        public static float GetTapeYMax(int index) => (TapeYDiff + 1f) * -index + 1f;

        public static float GetValueX(int index) => index * ValueXDiff;

        //public static float TapeYDiff => tapeHeight + tapeMargin;
        public static float TapeYDiff => 0.5f;

        public static float ValueXDiff => 1f + valueSpacing;

        public void OnUpdateXY() {
            var rt = GetComponent<RectTransform>();
            rt.anchorMin = Model.XY;
            rt.anchorMax = Model.XY + Vector2.one;
        }

        public void OnAddTape(int index, Tape tape) {
            var view = tape.MakeView();
            var rt = view.GetComponent<RectTransform>();
            rt.SetParent(bit.TapeContainer, false);
            rt.anchorMin = rt.anchorMin.WithX(0f);
            rt.anchorMax = rt.anchorMax.WithX(1f);
            rt.sizeDelta = Vector2.zero;
            view.OnUpdateCryptex();
            UpdateTapeViews(index);
            bit.TapeCount = Model.Tapes.Count;
        }

        public void OnRemoveTape(Tape tape) {
            tape.ClearView();
            UpdateTapeViews();
            bit.TapeCount = Model.Tapes.Count;
        }

        public void OnAddRoller(Roller roller) {
            var view = roller.MakeView();
            var rt = view.GetComponent<RectTransform>();
            rt.SetParent(bit.RollerContainer, false);
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
        }

        public void OnRemoveRoller(Roller roller) {
            roller.ClearView();
        }

        public void OnAddLabel(int index, Label label) {
            ShowLabel(index, label);
        }

        public void OnRemoveLabel(int index, Label label) {
            label.ClearView();
        }

        private void UpdateTapeViews(int startIndex = 0) {
            for (int i = startIndex; i < Model.tapes.Count; ++i) {
                var view = Model.tapes[i].MakeView();
                PositionTapeView(view, i);
                view.OnUpdateIndex();
            }
        }

        private void ShowLabel(int index, Label label) {
            var view = label.MakeView();
            view.Cryptex = Model;
            var rt = view.GetComponent<RectTransform>();
            rt.SetParent(bit.LabelContainer, false);
            rt.anchorMin = new Vector2(index * 1.1f, 1f);
            rt.anchorMax = new Vector2(index * 1.1f + 1f, 2f);
        }

        private void Sort2(float[] dists, ref int index0, ref int index1) {
            if (dists[index0] > dists[index1]) {
                int temp = index0;
                index0 = index1;
                index1 = temp;
            }
        }

        /// <returns>The (smaller, larger) distance from the origin point to the view border</returns>
        private (float, float) GetViewBounds(Vector3 origin, float sin, float cos, float xMin, float xMax, float yMin, float yMax) {
            const int right = 0;
            const int top = 1;
            const int left = 2;
            const int bottom = 3;

            float[] diff = {
                origin.x - xMax,
                origin.y - yMax,
                origin.x - xMin,
                origin.y - yMin,
            };

            float[] dist = {
                //diff[right] / sin,
                //diff[top] / cos,
                //diff[left] / sin,
                //diff[bottom] / cos,
                diff[right] / cos,
                diff[top] / sin,
                diff[left] / cos,
                diff[bottom] / sin,
            };

            int[] sort = { right, top, left, bottom };

            // Order the first and second pairs
            Sort2(dist, ref sort[0], ref sort[1]);
            Sort2(dist, ref sort[2], ref sort[3]);
            // loSort[3] is now the max of all [max(max(0, 1), max(2, 3))]
            Sort2(dist, ref sort[1], ref sort[3]);
            // loSort[0] is now the min of all [min(min(0, 1), min(2, 3))]
            Sort2(dist, ref sort[0], ref sort[2]);
            // Finally order the two middles
            Sort2(dist, ref sort[1], ref sort[2]);

            // If the ordering is HHVV or VVHH, (where H is a horizontal [top or bottom] and V is vertical [left or right]), the line does not cross the screen
            // If hi and lo both do this, the tape is not visible
            if (sort[0] % 2 == sort[1] % 2) {
                return (0f, 0f);
            }

            return (dist[sort[1]], dist[sort[2]]);
        }

        public static ValueTuple<int, int> GetContainedValues(float length, float minDist, bool fullValuesOnly = false) {
            float fracCount = (length + valueSpacing) / ValueXDiff;
            var visibleCount = fullValuesOnly ? Mathf.FloorToInt(fracCount) : Mathf.CeilToInt(fracCount);

            int newVisibleInsertersIndex = Mathf.CeilToInt((minDist - (ValueXDiff - 0.5f - (fullValuesOnly ? 1f : 0f))) / ValueXDiff);
            int newVisibleInsertersEndIndex = newVisibleInsertersIndex + visibleCount;

            return ValueTuple.Create(newVisibleInsertersIndex, newVisibleInsertersEndIndex);
        }

        private void UpdateWidth() {
            /*
            //float height = LabelViewBit.Height + (tapeHeight + tapeMargin) * (Model.tapes.Count + 0.5f);
            float height = LabelViewBit.Height + (tapeHeight + tapeMargin) * (Model.tapes.Count - 1) + 2f * tapeHeight;

            //var origin = transform.position + new Vector3(0f, 1.5f * tapeHeight + (tapeHeight + tapeMargin) * Model.tapes.Count, 0f);
            var origin = transform.position + new Vector3(0f, LabelViewBit.Height + tapeHeight / 2f - height / 2f, 0f);
            var loOrigin = origin - transform.up * height / 2f;
            var hiOrigin = origin + transform.up * height / 2f;

            var cam = Camera.main;
            var camMax = cam.ViewportToWorldPoint(new Vector3(1f, 1f, cam.nearClipPlane));
            var camMin = cam.ViewportToWorldPoint(new Vector3(0f, 0f, cam.nearClipPlane));
            float xLeft = camMin.x;
            float xRight = camMax.x;
            float yBottom = camMin.y;
            float yTop = camMax.y;

            // Apparently Vector3.Angle returns degrees when everything else in Unity uses radians... Fun times...
            var theta = Mathf.Deg2Rad * Vector3.Angle(transform.right, Vector3.right);
            float sin = Mathf.Sin(theta);
            float cos = Mathf.Cos(theta);

            var loDists = GetViewBounds(loOrigin, sin, cos, xLeft, xRight, yBottom, yTop);
            var hiDists = GetViewBounds(hiOrigin, sin, cos, xLeft, xRight, yBottom, yTop);

            float minDist = Mathf.Min(loDists.Item1, hiDists.Item1);
            float maxDist = Mathf.Max(loDists.Item2, hiDists.Item2);

            float cornerDist = Mathf.Abs(height * sin * cos);
            minDist -= cornerDist;
            maxDist += cornerDist;
            float length = maxDist - minDist;
            */

            var cam = Camera.main;
            var camLeft = cam.ViewportToWorldPoint(new Vector3(0f, 0f, cam.nearClipPlane)).x;
            var camRight = cam.ViewportToWorldPoint(new Vector3(1f, 1f, cam.nearClipPlane)).x;
            float length = camRight - camLeft;
            bit.Width = length;

            //var indicies = GetContainedValues(length, -maxDist);
            var indicies = GetContainedValues(length, camLeft - transform.position.x);

            int newVisibleInsertersIndex = indicies.Item1;
            int newVisibleInsertersEndIndex = indicies.Item2;

            foreach (var tapeView in Model.tapes.SelectMany(t => t.View)) {
                tapeView.OnUpdateValues(newVisibleInsertersIndex, newVisibleInsertersEndIndex);
            }

            // No overlap, just disable visibleValues and clear it
            if (visibleInsertersIndex >= newVisibleInsertersEndIndex || visibleInsertersIndex + visibleLabelPoints.Count <= newVisibleInsertersIndex) {
                visibleInsertersIndex = newVisibleInsertersIndex;
                foreach (var vi in visibleLabelPoints) {
                    vi.gameObject.SetActive(false);
                }
                visibleLabelPoints.Clear();
            }

            while (visibleInsertersIndex < newVisibleInsertersIndex) {
                ++visibleInsertersIndex;
                var view = visibleLabelPoints.RemoveBack();
                view.gameObject.SetActive(false);
            }
            while (visibleInsertersIndex + visibleLabelPoints.Count > newVisibleInsertersEndIndex) {
                var view = visibleLabelPoints.RemoveFront();
                view.gameObject.SetActive(false);
            }

            while (visibleInsertersIndex > newVisibleInsertersIndex) {
                --visibleInsertersIndex;
                var view = GetOrCreate(visibleInsertersIndex);
                view.gameObject.SetActive(true);
                visibleLabelPoints.AddBack(view);
            }
            while (visibleInsertersIndex + visibleLabelPoints.Count < newVisibleInsertersEndIndex) {
                var view = GetOrCreate(visibleInsertersIndex + visibleLabelPoints.Count);
                view.gameObject.SetActive(true);
                visibleLabelPoints.AddFront(view);
            }
        }

        Bounds2D IApertureTarget.Bounds => ((IApertureTarget)bit).Bounds;
    }
}
