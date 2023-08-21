using Functional.Option;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

using LE = ILocalizationExpression;
using LD = LocalizationDefault;
using L = LocalizationString;
using LC = LocalizationConstant;
using LF = LocalizationFormat;
using LI = LocalizationInt;

partial class Note : IModel<Note.NoteView> {
    [JsonIgnore]
    private ViewContainer<NoteView, Note> view;
    public Option<NoteView> View => view.View;
    public NoteView MakeView() => view.MakeView(this);
    public void ClearView() => view.ClearView();

    [RequireComponent(typeof(RectTransform))]
    public sealed class NoteView : MonoBehaviour, IView<Note> {
        public Note Model { get; set; }

        private NoteViewBit bit;

        private Option<string> textOverride;

        void IView.StartNow() {
            bit = Utility.Instantiate(Overseer.GlobalAssets.NotePrefab, transform);
            bit.transform.SetParent(transform, false);
            bit.GetComponent<RectTransform>().MatchParent();
        }

        void IView.OnDestroyNow() {
            Utility.DestroyGameObject(bit);
        }

        private void Start() {
            OnUpdateText();
        }

        public Option<string> TextOverride {
            get => textOverride;
            set {
                textOverride = value;
                OnUpdateText();
            }
        }

        public TMPro.TMP_Text TMP => bit.TMP;

        internal void OnUpdateText() {
            bit.Text = textOverride.ValueOr(Model.Text.ToString());
        }
    }
}
