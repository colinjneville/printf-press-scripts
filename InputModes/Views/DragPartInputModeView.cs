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

partial class DragPartInputMode : IModel<DragPartInputMode.DragPartInputModeView> {
    private ViewContainer<DragPartInputModeView, DragPartInputMode> view;
    public Option<DragPartInputModeView> View => view.View;
    public DragPartInputModeView MakeView() => view.MakeView(this);
    public void ClearView() => view.ClearView();

    public sealed class DragPartInputModeView : MonoBehaviour, IView<DragPartInputMode> {
        public DragPartInputMode Model { get; set; }

        void IView.StartNow() { }

        void IView.OnDestroyNow() { }
    }
}
