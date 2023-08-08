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

partial class MovePartInputMode : IModel<MovePartInputMode.MovePartInputModeView> {
    private ViewContainer<MovePartInputModeView, MovePartInputMode> view;
    public Option<MovePartInputModeView> View => view.View;
    public MovePartInputModeView MakeView() => view.MakeView(this);
    public void ClearView() => view.ClearView();

    public sealed class MovePartInputModeView : MonoBehaviour, IView<MovePartInputMode> {
        public MovePartInputMode Model { get; set; }
        void IView.StartNow() { }
        void IView.OnDestroyNow() { }
    }
}
