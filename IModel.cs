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

public static class ViewExtensions {
    public static void HideView<TView>(this IModel<TView> self) where TView : MonoBehaviour, IView => SetActive(self, false);

    public static void UnhideView<TView>(this IModel<TView> self) where TView : MonoBehaviour, IView => SetActive(self, true);

    public static bool IsViewHidden<TView>(this IModel<TView> self) where TView : MonoBehaviour, IView {
        foreach (var view in self.View) {
            return view.gameObject.activeSelf;
        }
        // This is kind of misleading, since a non-existent view is neither Hidden or Shown
        return false;
    }

    private static void SetActive<TView>(IModel<TView> self, bool active) where TView : MonoBehaviour, IView {
        foreach (var view in self.View) {
            view.gameObject.SetActive(active);
        }
    }
}

public struct ViewContainer<T, TModel> where T : MonoBehaviour, IView<TModel> where TModel : IModel<T> {
    private Option<T> view;

    public Option<T> View => view;

    public T MakeView(TModel model) {
        T view;
        if (!this.view.TryGetValue(out view)) {
            view = Utility.NewGameObject<T>();
            view.Model = model;
            this.view = view;
            var rt = view.GetComponent<RectTransform>();
            if (rt != null) {
                rt.sizeDelta = Vector2.zero;
            }
            view.StartNow();
        }
        view.gameObject.SetActive(true);
        return view;
    }

    public void ClearView() {
        foreach (var viewValue in view) {
            Utility.DestroyGameObject(viewValue);
            view = Option.None;
            viewValue.OnDestroyNow();
        }
    }
}

public interface IModel {
    void ClearView();
}

public interface IModelCo<out TView> : IModel where TView : MonoBehaviour {
    TView MakeView();
}

public interface IModel<TView> : IModelCo<TView> where TView : MonoBehaviour, IView {
    Option<TView> View { get; }
}

public interface IView {
    void StartNow();
    void OnDestroyNow();
}

public interface IView<TModel> : IView where TModel : IModel {
    TModel Model { get; set; }
}