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

public abstract class DragDefinition<TModel> where TModel : IModelCo<MonoBehaviour> {
    public abstract void Position(TModel model, Vector3 position, IEnumerable<RaycastHit2D> hits, Vector3 modelOffset);

    public abstract bool TryInsert(TModel modifyPoint, Vector3 position, IEnumerable<RaycastHit2D> hits, Vector3 initialPosition, Vector3 modelOffset);

    public abstract void Cancel(TModel model, Vector3 initialPosition);

    protected const float viewZ = -5.0f;
}

public abstract class DragDefinition<TModel, TInsertPoint> : DragDefinition<TModel> where TModel : IModelCo<MonoBehaviour> where TInsertPoint : InsertPoint {
    private Inserter<TInsertPoint, TModel> inserter;
    private Option<TInsertPoint> currentSnap;

    public DragDefinition(Inserter<TInsertPoint, TModel> inserter) {
        this.inserter = inserter;
    }

    public Inserter<TInsertPoint, TModel> Inserter => inserter;

    public sealed override void Position(TModel model, Vector3 position, IEnumerable<RaycastHit2D> hits, Vector3 modelOffset) {
        if (inserter.GetInsertPoint(Hits(model, position, modelOffset, hits), model.MakeView().transform).TryGetValue(out TInsertPoint ip) && inserter.CanInsert(ip, model) && ip.AllowSnap) {
            SetCurrentSnap(ip);
            Snap(ip, model, viewZ);
        } else {
            ClearCurrentSnap();
            position = Camera.main.ScreenToWorldPoint(position);
            model.MakeView().transform.position = (position + modelOffset).WithZ(viewZ);
        }
    }

    public sealed override bool TryInsert(TModel model, Vector3 position, IEnumerable<RaycastHit2D> hits, Vector3 initialPosition, Vector3 modelOffset) {
        // TODO should this just use currentSnap?
        if (inserter.GetInsertPoint(Hits(model, position, modelOffset, hits), model.MakeView().transform).TryGetValue(out TInsertPoint ip) && CanInsert(ip, model)) {
            ClearCurrentSnap();
            Insert(ip, model);
            // Both moving and inserting actually serializes/deserializes a new instance, so always clear the view for this instance
            model.ClearView();
            return true;
        } else {
            Cancel(model, initialPosition);

            return false;
        }
    }

    private void SetCurrentSnap(TInsertPoint ip) {
        if (!currentSnap.Equals(ip)) {
            ClearCurrentSnap();
            ip.OnStartSnap();
            currentSnap = ip;
        }
    }

    private void ClearCurrentSnap() {
        foreach (var currentSnap in currentSnap) {
            currentSnap.OnEndSnap();
        }
        currentSnap = Option.None;
    }

    public sealed override void Cancel(TModel model, Vector3 initialPosition) {
        ClearCurrentSnap();
        CancelInternal(model, initialPosition);
    }

    protected abstract void CancelInternal(TModel model, Vector3 initialPosition);

    // Yeah, this is awful
    protected virtual IEnumerable<RaycastHit2D> Hits(TModel model, Vector3 mousePosition, Vector3 modelOffset, IEnumerable<RaycastHit2D> hits) => hits;

    private bool Insert(TInsertPoint insertPoint, TModel model) => InsertInternal(insertPoint, model, checkOnly: false);
    private bool CanInsert(TInsertPoint insertPoint, TModel model) => InsertInternal(insertPoint, model, checkOnly: true);

    protected abstract bool InsertInternal(TInsertPoint insertPoint, TModel model, bool checkOnly);

    protected virtual void Snap(TInsertPoint ip, TModel model, float z) => Inserter.Snap(ip, model, z);
}

public sealed class WorkspaceDragDefinition<TModel, TInsertPoint> : DragDefinition<TModel, TInsertPoint> where TModel : IModelCo<MonoBehaviour> where TInsertPoint : InsertPoint {
    private ModifyPoint<TModel> modifyPoint;

    public WorkspaceDragDefinition(Inserter<TInsertPoint, TModel> inserter, ModifyPoint<TModel> modifyPoint) : base(inserter) {
        this.modifyPoint = modifyPoint;
    }

    public ModifyPoint<TModel> ModifyPoint => modifyPoint;

    protected override IEnumerable<RaycastHit2D> Hits(TModel model, Vector3 mousePosition, Vector3 modelOffset, IEnumerable<RaycastHit2D> hits) {
        return InputManager.Raycast(Camera.main.ScreenToWorldPoint(mousePosition) + modelOffset + (ModifyPoint.Origin - model.MakeView().transform.position));
    }

    protected sealed override bool InsertInternal(TInsertPoint insertPoint, TModel model, bool checkOnly) {
        if (Inserter.CanMove(modifyPoint, insertPoint)) {
            if (!checkOnly) {
                modifyPoint.OnEndDrag();
                Inserter.Move(modifyPoint, insertPoint);
            }
            return true;
        }
        return false;
    }

    protected override void CancelInternal(TModel model, Vector3 initialPosition) {
        modifyPoint.OnEndDrag();
        model.MakeView().transform.position = initialPosition;
    }

    protected override void Snap(TInsertPoint ip, TModel model, float z) => Inserter.Snap(ip, model, z, ModifyPoint.Origin - model.MakeView().transform.position);
}

public sealed class ToolboxDragDefinition<TModel, TInsertPoint> : DragDefinition<TModel, TInsertPoint> where TModel : IModelCo<MonoBehaviour> where TInsertPoint : InsertPoint {
    public ToolboxDragDefinition(Inserter<TInsertPoint, TModel> inserter) : base(inserter) { }

    protected override bool InsertInternal(TInsertPoint insertPoint, TModel model, bool checkOnly) {
        if (checkOnly) {
            return Inserter.CanInsert(insertPoint, model);
        } else {
            return Inserter.Insert(insertPoint, model);
        }
    }

    protected override void CancelInternal(TModel model, Vector3 initialPosition) {
        model.ClearView();
    }
}

public abstract class DragOperation {
    private Vector3 offset;
    private Vector3 initialPosition;

    protected DragOperation(Vector3 offset, Vector3 initialPosition) {
        this.offset = offset;
        this.initialPosition = initialPosition;
    }

    public abstract void Mouse0Held(Vector3 position, Lazy<RaycastHit2D[]> hits);

    public abstract bool Mouse0Release(Vector3 position, Lazy<RaycastHit2D[]> hits);

    public abstract void Cancel();

    protected Vector3 Offset => offset;
    protected Vector3 InitialPosition => initialPosition;
}

public sealed class DragOperation<TModel> : DragOperation where TModel : IModelCo<MonoBehaviour> {
    private DragDefinition<TModel> dragDefinition;
    private TModel model;
    private TransformProxy proxy;
    private TransformProxyReceiver receiver;
    private bool deleteProxy;

    public DragOperation(DragDefinition<TModel> dragDefinition, Vector3 offset, TModel model) : base(offset, model.MakeView().transform.position) {
        this.dragDefinition = dragDefinition;
        this.model = model;
        var view = model.MakeView();

        if (view.transform.parent is null) {
            proxy = view.GetScreen().FixedProxy;
            deleteProxy = false;
        } else {
            proxy = view.transform.parent.CreateProxy("Drag Proxy", forwardActive: true, forwardDestruction: true, forwardTransformation: true);
            deleteProxy = true;
        }

        view.transform.SetParent(WorkspaceScene.Layer.Drag(view.GetScreen()), false);
        receiver = proxy.CreateReceiver(view.gameObject);

        var rt = view.GetComponent<RectTransform>();
        if (rt != null) {
            var diff = rt.anchorMax - rt.anchorMin;
            if (diff.x == 0f && rt.sizeDelta.x == 0f) {
                rt.sizeDelta = rt.sizeDelta.WithX(1f);
            }
            if (diff.y == 0f && rt.sizeDelta.y == 0f) {
                rt.sizeDelta = rt.sizeDelta.WithY(1f);
            }
        }
    }

    public override void Mouse0Held(Vector3 position, Lazy<RaycastHit2D[]> hits) => dragDefinition.Position(model, position, hits.Value, Offset);

    public override bool Mouse0Release(Vector3 position, Lazy<RaycastHit2D[]> hits) {
        ResetModel();
        return dragDefinition.TryInsert(model, position, hits.Value, InitialPosition, Offset);
    }

    public override void Cancel() {
        ResetModel();
        dragDefinition.Cancel(model, InitialPosition);
    }

    private void ResetModel() {
        model.MakeView().transform.SetParent(proxy.transform.parent, false);
        receiver.DestroyGameObject();
        if (deleteProxy) {
            proxy.DestroyGameObject();
        }
    }
}