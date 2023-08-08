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

partial class MultiValueInputMode : IModel<MultiValueInputMode.MultiValueInputModeView> {
    private ViewContainer<MultiValueInputModeView, MultiValueInputMode> view;
    public Option<MultiValueInputModeView> View => view.View;
    public MultiValueInputModeView MakeView() => view.MakeView(this);
    public void ClearView() => view.ClearView();

    public sealed class MultiValueInputModeView : MonoBehaviour, IView<MultiValueInputMode> {
        public MultiValueInputMode Model { get; set; }
        void IView.StartNow() { }
        void IView.OnDestroyNow() { }

        private void OnEnable() {
            Camera.main.GetComponent<CameraPostRender>().PostRender += PostRender;
        }

        private void OnDisable() {
            Camera.main.GetComponent<CameraPostRender>().PostRender -= PostRender;
        }

        private void PostRender() {
            GL.Begin(GL.LINE_STRIP);
            GL.Color(Color.red);
            var cryptexXY = new Vector2(Model.Cryptex.MakeView().transform.position.x, 0f);
            var corners = GetCorners(Model.TapePointStart, Model.TapePointEnd);
            var start = Camera.main.WorldToScreenPoint(cryptexXY + corners.Item1).WithZ(0f) - new Vector3(1f, 1f, 0f);
            var end = Camera.main.WorldToScreenPoint(cryptexXY + corners.Item2).WithZ(0f) + new Vector3(1f, 1f, 0f);
            GL.Vertex(start);
            GL.Vertex3(start.x, end.y, start.z);
            GL.Vertex(end);
            GL.Vertex3(end.x, start.y, start.z);
            GL.Vertex(start);
            GL.End();
        }

        private (Vector2, Vector2) GetCorners(TapeSelectionPoint start, TapeSelectionPoint end) {
            Vector3 startPoint = GetPosition(start);
            Vector3 endPoint = GetPosition(end);
            float xmin = Mathf.Min(startPoint.x, endPoint.x) - 0.5f;
            float xmax = Mathf.Max(startPoint.x, endPoint.x) + 0.5f;
            float ymin = Mathf.Min(startPoint.y, endPoint.y) - 0.5f;
            float ymax = Mathf.Max(startPoint.y, endPoint.y) + 0.5f;
            return (new Vector2(xmin, ymin), new Vector2(xmax, ymax));
        }

        private Vector3 GetPosition(TapeSelectionPoint point) {
            var tape = Model.Cryptex.Tapes[point.TapeIndex];
            var view = tape.MakeView();
            return new Vector3(Cryptex.CryptexView.GetValueX(point.OffsetIndex - tape.ShiftOffset), view.transform.position.y, 0f);
        }
    }
}
