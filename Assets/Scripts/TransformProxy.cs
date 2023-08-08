using Functional.Option;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;

using L = LocalizationString;
using LC = LocalizationConstant;
using LE = ILocalizationExpression;
using LF = LocalizationFormat;
using LI = LocalizationInt;

public sealed class TransformProxy : UIBehaviour {
    [HideInInspector]
    [SerializeField]
    private List<TransformProxyReceiver> proxyChildren;
    [SerializeField]
    private bool forwardDestruction;
    [SerializeField]
    private bool forwardTransformation;
    [HideInInspector]
    [SerializeField]
    private bool forwardActive;
    [HideInInspector]
    [SerializeField]
    private int lastTransformChanged;

    protected override void Awake() {
        proxyChildren = new List<TransformProxyReceiver>();
        gameObject.name += $" ({gameObject.GetInstanceID()})";
    }

    protected override void OnEnable() {
        if (ForwardActive) {
            foreach (var child in proxyChildren) {
                child.gameObject.SetActive(true);
            }
        }
    }

    protected override void OnDisable() {
        if (ForwardActive) {
            foreach (var child in proxyChildren) {
                child.gameObject.SetActive(false);
            }
        }
    }

    protected override void OnRectTransformDimensionsChange() {
        MarkTransformChanged();
    }

    private void Update() {
        if (transform.hasChanged) {
            MarkTransformChanged();
        }
        transform.hasChanged = false;

        bool hasRt = GetComponent<RectTransform>() != null;
        foreach (var child in proxyChildren) {
            if (ForwardActive) {
                // TODO call all Update?
            }

            var childRt = child.GetComponent<RectTransform>();
            var childHasRt = childRt != null;
            if (hasRt && !childHasRt) {
                child.gameObject.AddComponent<RectTransform>();
            } else if (!hasRt && childHasRt) {
                Destroy(childRt);
            }
        }
    }

    protected override void OnDestroy() {
        // HACK
        // Unity has some sort of weird bug where doing this while quitting this will cause Unity to try to destroy Transforms (which Unity doesn't allow) for some reason
        // Since they will be destroyed anyway, just skip
        if (!Overseer.Quitting) {
            foreach (var child in proxyChildren) {
                if (ForwardDestruction) {
                    child.DestroyChildren();
                } else {
                    child.RescueChildren();
                }

                child.DestroyGameObject();
            }
        }
    }

    protected override void OnTransformParentChanged() {
        MarkTransformChanged();
    }

    private void MarkTransformChanged() {
        lastTransformChanged = Time.frameCount;
    }

    public bool TransformChanged => lastTransformChanged == Time.frameCount;

    public bool ForwardDestruction {
        get => forwardDestruction;
        set => forwardDestruction = value;
    }

    public bool ForwardTransformation {
        get => forwardTransformation;
        set => forwardTransformation = value;
    }

    public bool ForwardActive {
        get => forwardActive;
        set => forwardActive = value;
    }

    public TransformProxyReceiver CreateReceiver(GameObject go = null) {
        var pgo = new GameObject($"Transform Proxy Receiver ({gameObject.GetInstanceID()})");
        var receiver = pgo.AddComponent<TransformProxyReceiver>();
        receiver.proxy = this;
        if (GetComponent<RectTransform>() != null) {
            receiver.gameObject.AddComponent<RectTransform>();
        }
        if (go != null) {
            var oldParent = go.transform.parent;
            go.transform.SetParent(pgo.transform, false);
            pgo.transform.SetParent(oldParent, false);
        }
        receiver.UpdateTransformation();

        proxyChildren.Add(receiver);

        return receiver;
    }

    internal void RemoveReceiver(TransformProxyReceiver receiver) {
        proxyChildren.Remove(receiver);
    }

    [EasyButtons.Button("Create Proxy", Mode = EasyButtons.ButtonMode.DisabledInPlayMode)]
    private void CreateEmptyReceiver() {
        CreateReceiver();
    }
}

public sealed class TransformProxyReceiver : UIBehaviour {
    [SerializeField]
    internal TransformProxy proxy;
    [SerializeField]
    private bool forwardLocalDestruction;
    [SerializeField]
    private bool destroyWhenEmpty;
    [HideInInspector]
    [SerializeField]
    private bool transformChanged;
    [HideInInspector]
    [SerializeField]
    private bool isCopying;

    protected override void Awake() {
        destroyWhenEmpty = true;
        transformChanged = true;
    }

    private void LateUpdate() {
        // TODO TEMP There doesn't seem to be a comprehensive way to detect effective RectTransform changes, so unfortunately, we'll have to resync every frame
        transformChanged = true;

        if (proxy.TransformChanged || transformChanged) {
            UpdateTransformation();
        }
    }

    public bool ForwardLocalDestruction {
        get => forwardLocalDestruction;
        set => forwardLocalDestruction = value;
    }

    public bool DestroyWhenEmpty {
        get => destroyWhenEmpty;
        set => destroyWhenEmpty = value;
    }
    
    internal void UpdateTransformation() {
        var prevIsCopying = isCopying;
        try {
            isCopying = true;

            var rt = GetComponent<RectTransform>();

            if (rt != null) {
                var otherRt = proxy.GetComponent<RectTransform>();
                if (otherRt != null) {
                    var corners = new Vector3[4];
                    otherRt.GetWorldCorners(corners);
                    rt.sizeDelta = corners[2] - corners[0];
                }
            }

            var otherT = proxy.transform;
            var t = transform;

            var oldp = t.position;
            t.position = otherT.position;

            if (transform.parent == null) {
                transform.localScale = Vector3.one;
            } else {
                var scale = transform.parent.InverseTransformPoint(otherT.lossyScale);
                var zero = transform.parent.InverseTransformPoint(Vector3.zero);
                transform.localScale = scale - zero;
            }
            t.rotation = otherT.rotation;

            transformChanged = false;
        } finally {
            isCopying = prevIsCopying;
        }
    }

    internal void RescueChildren() {
        int prevChildCount;
        while (transform.childCount > 0) {
            prevChildCount = transform.childCount;
            // TODO should worldPositionStays be true or false?
            transform.GetChild(0).SetParent(transform.parent, false);
            Assert.NotEqual(prevChildCount, transform.childCount);
        }
    }

    internal void DestroyChildren() {
        bool active = gameObject.activeInHierarchy;
        while (transform.childCount > 0) {
            var child = transform.GetChild(0);
            child.gameObject.SetActive(active);
            child.SetParent(null, false);
            child.DestroyGameObject();
        }    
    }

    protected override void OnDestroy() {
        if (!ForwardLocalDestruction) {
            // This doesn't work because Unity does not make it possible to tell if an object is marked for destruction, and it also errors if you mess with transforms of an object marked for destruction...
            // Since we can't tell if our parent is being destroyed, we can't safely evacuate our children to it.
            //RescueChildren();
        }
        proxy.RemoveReceiver(this);
    }

    
    protected override void OnTransformParentChanged() {
        // TODO this doesn't cover all (or most) transform hierarchy changes
        transformChanged = true;
    }

    protected override void OnRectTransformDimensionsChange() {
        if (!isCopying) {
            transformChanged = true;
        }
    }

    private void OnTransformChildrenChanged() {
        if (DestroyWhenEmpty && transform.childCount == 0) {
            this.DestroyGameObject();
        }
    }

#if UNITY_EDITOR
    [EasyButtons.Button("Highlight Proxy", Mode = EasyButtons.ButtonMode.AlwaysEnabled)]
    private void HighlightProxy() {
        UnityEditor.EditorGUIUtility.PingObject(proxy);
    }
#endif
}
