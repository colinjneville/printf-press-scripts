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

partial class PickPartInputMode : IModel<PickPartInputMode.PickPartInputModeView> {
    private ViewContainer<PickPartInputModeView, PickPartInputMode> view;
    public Option<PickPartInputModeView> View => view.View;
    public PickPartInputModeView MakeView() => view.MakeView(this);
    public void ClearView() => view.ClearView();

    public sealed class PickPartInputModeView : MonoBehaviour, IView<PickPartInputMode> {
        public PickPartInputMode Model { get; set; }

        private void OnEnable() {
            Camera.main.GetComponent<CameraPostRender>().PostRender += PostRender;
        }

        private void OnDisable() {
            Camera.main.GetComponent<CameraPostRender>().PostRender -= PostRender;
        }

        void IView.StartNow() { }
        void IView.OnDestroyNow() { }

        private void PostRender() {
            foreach (var item in Model.Item) {
                foreach (var view in item.View) {
                    var corners = new Vector3[4];
                    view.GetComponent<RectTransform>().GetWorldCorners(corners);

                    GL.Begin(GL.LINE_STRIP);
                    GL.Color(Color.red);
                    var start = Camera.main.WorldToScreenPoint(corners[0]).WithZ(0f) - new Vector3(1f, 1f, 0f);
                    var end = Camera.main.WorldToScreenPoint(corners[2]).WithZ(0f) + new Vector3(1f, 1f, 0f);
                    GL.Vertex(start);
                    GL.Vertex3(start.x, end.y, start.z);
                    GL.Vertex(end);
                    GL.Vertex3(end.x, start.y, start.z);
                    GL.Vertex(start);
                    GL.End();
                }
            }
        }
    }
}
