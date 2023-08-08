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

partial class TextHighlight : IModel<TextHighlight.TextHighlightView> {
    private ViewContainer<TextHighlightView, TextHighlight> view;
    public Option<TextHighlightView> View => view.View;
    public TextHighlightView MakeView() => view.MakeView(this);
    public void ClearView() => view.ClearView();

    [RequireComponent(typeof(RectTransform))]
    public sealed class TextHighlightView : MonoBehaviour, IView<TextHighlight> {
        public TextHighlight Model { get; set; }

        private TMPro.TMP_Text tmp;
        private TextHighlightViewBit bit;

        private void Awake() {
            bit = Utility.Instantiate(Overseer.GlobalAssets.TextHighlightPrefab, transform);
        }

        private void LateUpdate() {
            // HACK
            if (tmp.transform.hasChanged) {
                OnUpdate();
                tmp.transform.hasChanged = false;
            }
        }

        private void Start() {
            // This needs to be in Start and not StartNow because we need tmp to be set first
            OnUpdate();
        }

        void IView.StartNow() { }

        void IView.OnDestroyNow() {
            bit.DestroyGameObject();
        }

        public int IndexAtPoint(Vector3 point) => ScreenPosToCharIndex(point);


        private int GetLineNumber(int index) {
            if (index == tmp.text.Length) {
                return tmp.textInfo.lineCount - 1;
            } else {
                return tmp.textInfo.characterInfo[index].lineNumber;
            }
        }

        private float GetPosX(int index) {
            if (index == 0) {
                return tmp.textInfo.lineInfo[0].lineExtents.min.x;
            } else {
                if (tmp.textInfo.characterInfo.Length < index) {
                    Debug.Log("uh oh");
                }
                return tmp.textInfo.characterInfo[index - 1].bottomRight.x;
            }
        }

        private int ScreenPosToCharIndex(Vector3 screenPos) {
            // For some reason, GetCursorIndexFromPosition can return 1 for an empty string
            return Mathf.Min(TMPro.TMP_TextUtilities.GetCursorIndexFromPosition(tmp, screenPos, Camera.main), tmp.text.Length);
        }

        internal void SetTMP(TMPro.TMP_Text tmp) {
            Utility.AssignOnce(ref this.tmp, tmp);
        }

        internal void OnUpdate() {
            tmp.text = Model.Text;
            tmp.ForceMeshUpdate();

            var startIndex = Model.IsSelectAll ? 0 : Model.StartIndex;
            var endIndex = Model.IsSelectAll ? Model.FinalIndex : Model.EndIndex;

            var textInfo = string.IsNullOrEmpty(tmp.text) ? tmp.GetTextInfo(" ") : tmp.textInfo;
            var lineInfo = textInfo.lineInfo[GetLineNumber(startIndex)];

            Quaternion rotation = tmp.transform.rotation;
            float width;
            float xMid;
            float height = lineInfo.lineHeight;
            float yMid = (lineInfo.ascender + lineInfo.descender) / 2f;

            if (startIndex == endIndex) {
                width = height / 10f;
                xMid = GetPosX(startIndex);
            } else {
                width = Mathf.Abs(GetPosX(endIndex) - GetPosX(startIndex));
                xMid = (GetPosX(endIndex) + GetPosX(startIndex)) / 2f;
            }

            bit.Position = tmp.transform.position + rotation * new Vector3(xMid, yMid, 0f);
            bit.Size = new Vector2(width, height);
            bit.Rotation = rotation;
        }
    }
}

