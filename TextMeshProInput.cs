using Functional.Option;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using L = LocalizationString;
using LC = LocalizationConstant;
using LE = ILocalizationExpression;
using LF = LocalizationFormat;
using LI = LocalizationInt;


public class TextMeshProInput : TMP_InputField {
    private MeshRenderer cursor;
    private Mesh cursorMesh;

    protected override void Awake() {
        base.Awake();

        var go = new GameObject("3D Cursor");
        go.layer = 5;

        var filter = go.AddComponent<MeshFilter>();
        cursor = go.AddComponent<MeshRenderer>();
        cursorMesh = new Mesh();
        cursorMesh.MarkDynamic();
        filter.sharedMesh = cursorMesh;
        cursor.material = Overseer.GlobalAssets.DefaultTransparentSolidMaterial;

        caretWidth = 0;
    }

    protected override void Start() {
        base.Start();
        
        cursor.transform.SetParent(textComponent.transform, false);
        cursor.transform.localScale = new Vector3(1f, 1f, -1f);

        onTextSelection.AddListener(OnTextSelection);
        onEndTextSelection.AddListener(OnEndTextSelection);
    }

    protected override void OnDestroy() {
        onEndTextSelection.RemoveListener(OnEndTextSelection);
        onTextSelection.RemoveListener(OnTextSelection);

        base.OnDestroy();
    }

    public override void Rebuild(CanvasUpdate update) {
        if (update == CanvasUpdate.LatePreRender) {
            base.Rebuild(update);
            if (stringPositionInternal == stringSelectPositionInternal) {
                CaretMeshModifier(mesh);
                cursor.transform.localScale = Vector3.one;
            } else {
                cursor.transform.localScale = new Vector3(1f, 1f, -1f);
            }

            UpdateMesh();
        }
    }


    private void OnTextSelection(string text, int start, int end) {
        //UpdateMesh();
        //cursor.transform.localScale = new Vector3(1f, 1f, -1f);
    }

    private void OnEndTextSelection(string text, int start, int end) {
        //UpdateMesh(CaretMeshModifier);
        //CaretMeshModifier(mesh);
        //cursor.transform.localScale = Vector3.one;
    }

    private void UpdateMesh(Action<Mesh> meshModifier = null) {
        cursorMesh.Clear(false);
        cursorMesh.vertices = mesh.vertices;
        cursorMesh.normals = mesh.normals;
        cursorMesh.triangles = mesh.triangles;

        meshModifier?.Invoke(cursorMesh);
        var colors = mesh.colors;
        if (colors.Length > 0) {
            cursor.material.color = colors[0];
            //Debug.Log(colors[0]);
            //Debug.Log(cursorMesh.)
        }
    }

    private void CaretMeshModifier(Mesh mesh) {
        var verts = mesh.vertices;
        if (verts.Length >= 4) {
            var height = verts[1].y - verts[0].y;
            var width = height / 10f;
            verts[2] = verts[1] + new Vector3(width, 0f, 0f);
            verts[3] = verts[0] + new Vector3(width, 0f, 0f);
            mesh.vertices = verts;
        }
    }
}
