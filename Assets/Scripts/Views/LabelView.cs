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

partial class Label : IModel<Label.LabelView> {
    private ViewContainer<LabelView, Label> view;
    public Option<LabelView> View => view.View;
    public LabelView MakeView() => view.MakeView(this);
    public void ClearView() => view.ClearView();

    [RequireComponent(typeof(RectTransform))]
    public sealed class LabelView : MonoBehaviour, IView<Label>, IApertureTarget {
        public Label Model { get; set; }

        private LabelViewBit bit;

        private Option<string> textOverride;

        private void Awake() {
            bit = Utility.Instantiate(Overseer.GlobalAssets.LabelPrefab, transform);
        }

        void IView.StartNow() {
            bit.Label = Model;
            OnUpdateName();
        }
        void IView.OnDestroyNow() { }

        public Cryptex Cryptex {
            get => bit.Cryptex;
            set {
                // HACK
                bit.Workspace = (WorkspaceFull)value.Workspace;
                bit.Cryptex = value;
            }
        }

        public void OnUpdateName() {
            bit.Text = textOverride.ValueOr(Model.name.ToString());
        }

        public Option<string> TextOverride {
            get => textOverride;
            set {
                textOverride = value;
                OnUpdateName();
            }
        }

        public TMPro.TMP_Text TMP => bit.TMP;

        Bounds2D IApertureTarget.Bounds => ((IApertureTarget)bit).Bounds;
    }
}
