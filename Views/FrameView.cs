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

partial class Frame : IModel<Frame.FrameView> {
    [JsonIgnore]
    private ViewContainer<FrameView, Frame> view;
    public Option<FrameView> View => view.View;
    public FrameView MakeView() => view.MakeView(this);
    public void ClearView() => view.ClearView();

    [RequireComponent(typeof(RectTransform))]
    public sealed class FrameView : MonoBehaviour, IView<Frame>, IApertureTarget {
        public Frame Model { get; set; }

        private FrameViewBit bit;

        private void Awake() {
            bit = Utility.Instantiate(Overseer.GlobalAssets.FramePrefab, transform);
        }

        void IView.StartNow() {
            OnUpdateFlags();
        }

        void IView.OnDestroyNow() { }

        public void OnUpdateRoller(Roller roller) {
            // HACK
            if (roller.Cryptex != null) {
                bit.Workspace = (WorkspaceFull)roller.Cryptex.Workspace;
            }
            bit.Roller = roller;
        }

        public void OnUpdateIndex(int index) {
            bit.Index = index;
            OnUpdateBridge(index != 0);
        }

        public void OnUpdateBridge(bool visible) {
            bit.IsBridgeVisible = visible;
        }

        public void OnUpdateFlags() {
            switch (Model.flags) {
                case FrameFlags.FrameRead:
                    bit.FrameSprite = Overseer.GlobalAssets.FrameSprite;
                    break;
                case FrameFlags.FrameReadWrite:
                    bit.FrameSprite = Overseer.GlobalAssets.FrameAltSprite;
                    break;
            }
        }

        Bounds2D IApertureTarget.Bounds => ((IApertureTarget)bit).Bounds;
    }
}
