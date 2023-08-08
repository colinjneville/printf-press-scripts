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

// TEST
//[RequireComponent(typeof(RectTransform))]
//[RequireComponent(typeof(BoxCollider2D))]
[ExecuteInEditMode]
//public sealed class RectColliderBinding : MonoBehaviour {
public sealed class RectColliderBinding : UnityEngine.EventSystems.UIBehaviour {
    /*
    private void LateUpdate() {
        // TODO PERF I can't find any way to track effective changes to transform size
        //if (transform.hasChanged) {
        var bounds = GetComponent<RectTransform>().GetWorldBounds();
        var collider = GetComponent<BoxCollider2D>();
        collider.size = bounds.Size;
        collider.offset = bounds.Center - (Vector2)transform.position;

        //transform.hasChanged = false;
        //}
        
    }*/

    protected override void OnRectTransformDimensionsChange() {
        var bounds = GetComponent<RectTransform>().GetWorldBounds();
        var collider = GetComponent<BoxCollider2D>();
        collider.size = bounds.Size;
        collider.offset = bounds.Center - (Vector2)transform.position;
    }
}

// TODO move elsewhere
public static class RectTransformExtensions {
    /// <summary>
    /// Gets the world AABB coordinates of a RectTransform (not thoroughly tested for rotation, etc.)
    /// </summary>
    public static Bounds2D GetWorldBounds(this RectTransform self) {
        // Why does Unity make this so difficult?
        var corners = new Vector3[4];
        self.GetWorldCorners(corners);
        var width = corners[2].x - corners[0].x;
        var height = corners[2].y - corners[0].y;
        var center = new Vector2((corners[2].x + corners[0].x) / 2f, (corners[2].y + corners[0].y) / 2f);
        return new Bounds2D(center, new Vector2(width, height));
    }
}
