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

partial class Roller : IModel<Roller.RollerView> {
    [JsonIgnore]
    private ViewContainer<RollerView, Roller> view;
    public Option<RollerView> View => view.View;
    public RollerView MakeView() => view.MakeView(this);
    public void ClearView() => view.ClearView();

    [RequireComponent(typeof(RectTransform))]
    public sealed class RollerView : MonoBehaviour, IView<Roller>, IApertureTarget {
        public Roller Model { get; set; }

        private RollerViewBit bit;

        private void Awake() {
            bit = Utility.Instantiate(Overseer.GlobalAssets.RollerPrefab, transform);
        }

        private void Start() {
            // HACK
            if (Model.Cryptex != null) {
                bit.Workspace = (WorkspaceFull)Model.Cryptex.Workspace;
            }

            UpdateFrames(0);
            UpdatePosition();
        }

        void IView.StartNow() {
            for (int i = 0; i < Model.frames.Count; ++i) {
                MakeFrameView(Model.frames[i]);
            }
            
            bit.Roller = Model;
            bit.LightColor = TapeValueColor.GetColor(Model.Color);
            bit.TopSprite = Model.IsPrimary ? Overseer.GlobalAssets.RollerTopAltSprite : Overseer.GlobalAssets.RollerTopSprite;
        }

        void IView.OnDestroyNow() {
            for (int i = 0; i < Model.frames.Count; ++i) {
                Model.frames[i].ClearView();
            }
            Utility.DestroyGameObject(bit);
        }

        private Frame.FrameView MakeFrameView(Frame frame) {
            var frameView = frame.MakeView();
            var rt = frameView.GetComponent<RectTransform>();
            rt.SetParent(bit.FrameContainer, false);
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            return frameView;
        }

        internal void OnUpdateCryptex() {
            bit.Workspace = (WorkspaceFull)Model.Cryptex.Workspace;
            foreach (var frame in Model.Frames) {
                frame.MakeView().OnUpdateRoller(Model);
            }
            UpdatePosition();
        }

        internal void OnUpdateMoveOffset() {
            using (bit.Unlock()) {
                UpdatePosition();
            }
        }

        internal void OnUpdateHopOffset() {
            using (bit.Unlock()) {
                UpdatePosition();
            }
        }

        internal void OnUpdateColor() {
            bit.LightColor = TapeValueColor.GetColor(Model.Color);
        }

        public void OnAddFrame(int index, Frame frame) {
            MakeFrameView(frame);
            UpdateFrames(index);
        }

        public void OnRemoveFrame(int index, Frame frame) {
            frame.ClearView();
            UpdateFrames(index);
        }

        private void UpdateFrames(int startIndex) {
            for (int i = startIndex; i < Model.frames.Count; ++i) {
                var view = Model.frames[i].MakeView();
                view.OnUpdateRoller(Model);
                var rt = view.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0f, i * -1.5f);
                rt.anchorMax = new Vector2(1f, i * -1.5f + 1f);
                view.OnUpdateIndex(i);
            }
        }

        private void UpdatePosition() {
            if (Model.Cryptex != null) {
                var tape = Model.Cryptex.Tapes[Model.hopOffset];
                var tapeView = tape.MakeView();
                transform.localPosition = tapeView.transform.localPosition + new Vector3(Cryptex.CryptexView.GetValueX(Model.moveOffset), 0f, -0.2f);
            }
        }

        Bounds2D IApertureTarget.Bounds {
            get {
                var bounds = ((IApertureTarget)bit).Bounds;

                if (Model.Frames.Count > 0) {
                    bounds.Encapsulate(((IApertureTarget)Model.Frames[Model.Frames.Count - 1].MakeView()).Bounds);
                }

                return bounds;
            }
        }
    }
}
