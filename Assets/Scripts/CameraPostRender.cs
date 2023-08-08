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

public sealed class CameraPostRender : MonoBehaviour {
    private event Action onPostRender;

    private Material material;

    private void Awake() {
        material = new Material(Shader.Find("Hidden/Internal-Colored"));
        material.hideFlags = HideFlags.HideAndDontSave;
        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcColor);
        //material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.DstAlpha);
        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
        material.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
        material.SetInt("_ZWrite", 0);
        material.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.Always);

        onPostRender = null;
    }

    private void OnDestroy() {
        Destroy(material);
    }

    private void OnPostRender() {
        GL.PushMatrix();
        try {
            GL.LoadPixelMatrix();
            material.SetPass(0);
            onPostRender?.Invoke();
        } finally {
            GL.PopMatrix();
        }
    }

    public event Action PostRender {
        add {
            onPostRender += value;
        }
        remove {
            onPostRender -= value;
        }
    }
}
