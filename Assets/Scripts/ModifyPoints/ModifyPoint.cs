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

public abstract class ModifyPoint : MonoBehaviour {
    public WorkspaceFull Workspace { get; set; }

    public abstract bool AllowDrag { get; }

    public abstract Vector3 Origin { get; }

    public bool CanDelete(bool forMove) => DeleteWrap(forMove, checkOnly: true);

    public bool Delete(bool forMove) => DeleteWrap(forMove, checkOnly: false);

    protected abstract bool DeleteWrap(bool forMove, bool checkOnly);

    public virtual bool Edit() => false;
    public virtual bool EditAlt() => false;

    public abstract DragOperation CreateDragOperation(Vector3 cursorPosition);

    public virtual void OnStartHover() { }
    public virtual void OnEndHover() { }

    protected void ForceEndHover() {

    }

    public virtual void OnStartDrag() { }
    public virtual void OnEndDrag() { }
}

public abstract class ModifyPoint<TModel> : ModifyPoint where TModel : IModelCo<MonoBehaviour> {
    public abstract TModel Model { get; }

    public override Vector3 Origin => Model.MakeView().transform.position;

    protected sealed override bool DeleteWrap(bool forMove, bool checkOnly) {
        return DeleteInternal(forMove, checkOnly);
    }

    protected abstract bool DeleteInternal(bool forMove, bool checkOnly);
}

public abstract class ModifyPoint<TPoint, TModel> : ModifyPoint<TModel> where TPoint : InsertPoint where TModel : IModelCo<MonoBehaviour> {
    protected abstract Inserter<TPoint, TModel> Inserter { get; }

    public override DragOperation CreateDragOperation(Vector3 cursorPosition) {
        OnStartDrag();
        return new DragOperation<TModel>(new WorkspaceDragDefinition<TModel, TPoint>(Inserter, this), (Model.MakeView().transform.position - Camera.main.ScreenToWorldPoint(cursorPosition)).WithZ(0f), Model);
    }
}
