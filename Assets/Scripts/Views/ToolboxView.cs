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

partial class Toolbox : IModel<Toolbox.ToolboxView> {
    private ViewContainer<ToolboxView, Toolbox> view;
    public Option<ToolboxView> View => view.View;
    public ToolboxView MakeView() => view.MakeView(this);
    public void ClearView() => view.ClearView();

    [RequireComponent(typeof(RectTransform))]
    public sealed class ToolboxView : MonoBehaviour, IView<Toolbox> {
        public Toolbox Model { get; set; }

        private const float tabHeight = 2f;

        private ToolboxViewBit bit;

        //private GameObject backdrop;
        private List<ToolboxTabViewBit> tabs;
        private FontSizeSync fss;

        private void Awake() {
            tabs = new List<ToolboxTabViewBit>();

            fss = new FontSizeSync(6f);
        }

        void IView.StartNow() {
            bit = Utility.Instantiate(Overseer.GlobalAssets.ToolboxPrefab, transform);

            int index = 0;
            float tabWidth = 1f / Model.pages.Count;
            foreach (var page in Model.pages) {
                var tab = Utility.Instantiate(Overseer.GlobalAssets.ToolboxTabPrefab, transform);
                tab.Text = page.Name.ToString();
                tab.Toolbox = Model;
                tab.PageIndex = index;
                tab.RectTransform.anchorMin = tab.RectTransform.anchorMin.WithX(index * tabWidth);
                tab.RectTransform.anchorMax = tab.RectTransform.anchorMax.WithX(index * tabWidth + tabWidth);
                tab.RectTransform.sizeDelta = new Vector2(0f, tabHeight);
                tab.RectTransform.anchoredPosition3D = new Vector3(tab.RectTransform.anchoredPosition.x, -tabHeight / 2f, -0.1f);

                fss.AddText(tab.TextMeshPro);

                tabs.Add(tab);

                ++index;
            }
            InitPage(Model.pageIndex);
        }

        void IView.OnDestroyNow() {
            for (int i = 0; i < Model.pages.Count; ++i) {
                Model.pages[i].ClearView();
            }
            foreach (var tab in tabs) {
                fss.RemoveText(tab.TextMeshPro);
            }
        }

        public void OnUpdateIndex(int oldIndex, int newIndex) {
            Model.pages[oldIndex].MakeView().gameObject.SetActive(false);
            InitPage(newIndex);
        }

        private void InitPage(int index) {
            var view = Model.pages[index].MakeView();
            view.gameObject.SetActive(true);
            view.transform.SetParent(transform, false);
            var rt = view.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = new Vector2(0f, -tabHeight);
            rt.anchoredPosition = new Vector2(0f, -0.5f);
        }
    }
}

partial class ToolboxPage : IModel<ToolboxPage.ToolboxPageView> {
    private ViewContainer<ToolboxPageView, ToolboxPage> view;
    public Option<ToolboxPageView> View => view.View;
    public ToolboxPageView MakeView() => view.MakeView(this);
    public void ClearView() => view.ClearView();

    [RequireComponent(typeof(RectTransform))]
    public sealed class ToolboxPageView : MonoBehaviour, IView<ToolboxPage> {
        public ToolboxPage Model { get; set; }

        private const float xMargin = 0.5f;
        private const float yMargin = 1f;
        private const float xSpacing = 0.25f;
        private const float ySpacing = 0.5f;
        private const float xItem = 2f;
        private const float yItem = 3f;

        private FontSizeSync nfss;
        private FontSizeSync cfss;

        private void Awake() {
            nfss = new FontSizeSync(0.1f);
            cfss = new FontSizeSync(0.1f);
        }

        private void OnEnable() {
            if (Model != null) {
                foreach (var item in Model.items) {
                    var view = item.MakeView();
                    // TODO this isn't enough to keep costs up to date.
                    // The one problem item is rollers, since placing the first of a color really places two.
                    view.RefreshCost();
                }
            }
        }

        void IView.StartNow() {
            for (int i = 0; i < Model.items.Count; ++i) {
                var view = Model.items[i].MakeView();
                var viewRt = view.GetComponent<RectTransform>();
                viewRt.SetParent(transform, false);
                viewRt.sizeDelta = new Vector2(xItem, yItem);
                viewRt.localPosition = new Vector3(0f, 0f, -0.1f);

                view.NameFontSizeSync = nfss;
                view.CostFontSizeSync = cfss;
                view.RefreshCost();
            }
        }
        void IView.OnDestroyNow() {
            foreach (var item in Model.items) {
                item.ClearView();
            }
        }

        private void Update() {
            if (transform.hasChanged) {
                var rt = GetComponent<RectTransform>();
                var rect = rt.rect;
                int itemsPerRow = Mathf.CeilToInt((rect.width - xMargin * 2 - xItem) / (xItem + xSpacing));
                float extraX = rect.width - ((itemsPerRow * (xItem + xSpacing)) + xMargin * 2f - xSpacing);
                var offset = new Vector3((rect.width - xItem) / -2f + xMargin + extraX / 2f, (rect.height - yItem) / 2f - yMargin, 0f);

                int i = 0, j = 0;
                foreach (var item in Model.items) {
                    var view = item.MakeView();
                    view.gameObject.SetActive(itemsPerRow > 0);
                    if (itemsPerRow > 0) {
                        view.transform.localPosition = new Vector3(i * (xItem + xSpacing), j * -(yItem + ySpacing), -0.1f) + offset;
                        if (++i == itemsPerRow) {
                            i = 0;
                            ++j;
                        }
                    }
                }

                transform.hasChanged = false;
            }

            nfss.Update();
            cfss.Update();
        }
    }
}

partial class ToolboxItem : IModel<ToolboxItem.ToolboxItemView> {
    private ViewContainer<ToolboxItemView, ToolboxItem> view;
    public Option<ToolboxItemView> View => view.View;
    public ToolboxItemView MakeView() => view.MakeView(this);
    public void ClearView() => view.ClearView();

    [RequireComponent(typeof(RectTransform))]
    public sealed class ToolboxItemView : MonoBehaviour, IView<ToolboxItem> {
        public ToolboxItem Model { get; set; }

        private ToolboxItemViewBit bit;

        private void Awake() {
            bit = Utility.Instantiate(Overseer.GlobalAssets.ToolboxItemPrefab, transform);
        }

        void IView.StartNow() {
            bit.ToolboxItem = Model;
            bit.Icon = Model.icon;
        }
        void IView.OnDestroyNow() { }

        public void RefreshCost() {
            var workspace = Overseer.Workspace.ValueOrAssert();
            bit.Cost = Model.Source.Cost(workspace, workspace.Level.CostOverride);
        }

        public Option<FontSizeSync> NameFontSizeSync {
            get => bit.NameFontSizeSync;
            set => bit.NameFontSizeSync = value;
        }

        public Option<FontSizeSync> CostFontSizeSync {
            get => bit.CostFontSizeSync;
            set => bit.CostFontSizeSync = value;
        }
    }
}