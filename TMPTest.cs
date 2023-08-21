using Functional.Option;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using L = LocalizationString;
using LC = LocalizationConstant;
using LE = ILocalizationExpression;
using LF = LocalizationFormat;
using LI = LocalizationInt;


public class TMPTest : TMP_InputField {

    public override void Rebuild(CanvasUpdate update) {
        Vector3 before = m_TextComponent.transform.localPosition;
        base.Rebuild(update);
        if (before != m_TextComponent.transform.localPosition) {
            //var rt = (RectTransform)typeof(TMP_InputField).GetField("caretRectTrans", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(this);
            //Debug.Log(rt.gameObject);
            //Debug.Log($"Before: {before} After: {m_TextComponent.transform.localPosition}");

        }
    }

    protected override void LateUpdate() {
        Vector3 before = m_TextComponent.transform.localPosition;
        base.LateUpdate();
        if (before != m_TextComponent.transform.localPosition) {
            Debug.Log($"Before: {before} After: {m_TextComponent.transform.localPosition}");
        }
    }
}