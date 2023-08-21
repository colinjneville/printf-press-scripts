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

partial class SelectValueInputMode : IModel<SelectValueInputMode.SelectValueInputModeView> {
    private ViewContainer<SelectValueInputModeView, SelectValueInputMode> view;
    public Option<SelectValueInputModeView> View => view.View;
    public SelectValueInputModeView MakeView() => view.MakeView(this);
    public void ClearView() => view.ClearView();

    public sealed class SelectValueInputModeView : MonoBehaviour, IView<SelectValueInputMode> {
        public SelectValueInputMode Model { get; set; }

        private RectTransform selection;
        private Option<Vector3> prevMousePosition;
        private Option<Vector3> mousePosition;
        private bool shift;

        void IView.StartNow() {
            selection = Utility.Instantiate(Overseer.GlobalAssets.SelectionRectanglePrefab, WorkspaceScene.Layer.Edit(this.GetScreen()));
            this.GetScreen().FixedProxy.CreateReceiver(selection.gameObject);
            //selection = Utility.CreateCubeObject("Selection Rectangle", Overseer.GlobalAssets.DefaultTransparentSolidMaterial, shareMaterial: false);
            //var selectionMesh = selection.GetComponent<MeshRenderer>();
            //selectionMesh.material.color = new Color32(0, 40, 200, 100);
        }

        void IView.OnDestroyNow() {
            Utility.DestroyGameObject(selection);
        }

        public void UpdatePosition(Modifiers modifiers, Vector3 cursorPosition) {
            shift = modifiers.HasShift();
            mousePosition = cursorPosition;
        }

        private void FixedUpdate() {
            // If we aren't in the middle of a multi-selection, hide the box and exit early
            if (!shift) {
                selection.gameObject.SetActive(false);
                return;
            }

            Vector3 start, end;
            if (Model.dragStart.TryGetValue(out start) && mousePosition.TryGetValue(out end)) {
                if (!prevMousePosition.Equals(end)) {
                    var bounds = Model.GetBounds(start, end);
                    var rt = selection.GetComponent<RectTransform>();
                    rt.anchorMin = bounds.min + new Vector3(0.5f, 0.5f);
                    rt.anchorMax = bounds.max + new Vector3(0.5f, 0.5f);
                    //selection.transform.localScale = bounds.size;
                    //selection.transform.localPosition = bounds.center;
                    var enclosed = Model.GetEnclosed(bounds, start, end);
                    // TODO highlight

                    prevMousePosition = end;
                    selection.gameObject.SetActive(true);
                }
            } else {
                selection.gameObject.SetActive(false);
            }
        }
    }
}
