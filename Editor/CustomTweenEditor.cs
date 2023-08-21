using Functional.Option;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

using L = LocalizationString;
using LC = LocalizationConstant;
using LE = ILocalizationExpression;
using LF = LocalizationFormat;
using LI = LocalizationInt;


[CustomPropertyDrawer(typeof(CustomTween.DimensionWrapper))]
public sealed class CustomTweenDimensionDrawer : PropertyDrawer {
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        bool didChange = false;
        bool didChangeStrategy = false;

        var wrapper = (CustomTween.DimensionWrapper)fieldInfo.GetValue(property.serializedObject.targetObject);
        var tween = (CustomTween)property.serializedObject.targetObject;
        tween.InitializeDimensions();

        EditorGUI.BeginProperty(position, label, property);

        var typeProp = property.FindPropertyRelative("type");

        using (var changeScope = new EditorGUI.ChangeCheckScope()) {
            EditorGUILayout.PropertyField(typeProp, new GUIContent(ObjectNames.NicifyVariableName(fieldInfo.Name)));
            didChange |= changeScope.changed;
            didChangeStrategy |= changeScope.changed;
        }
            

        if (wrapper.Dimension != null && wrapper.Dimension.Strategy != null) {
            var strategyType = wrapper.Dimension.Strategy.GetType();
            var attribute = strategyType.GetCustomAttribute<TweenStrategyAttribute>(inherit: true) ?? TweenStrategyAttribute.Default;
            var serialStrategy = new SerializedObject((ScriptableObject)wrapper.Dimension.Strategy);

            using (var changeScope = new EditorGUI.ChangeCheckScope()) {
                if (!attribute.HideState) {
                    var scratchProp = serialStrategy.FindProperty("scratchState");
                    EditorGUILayout.PropertyField(scratchProp, new GUIContent("Initial State"), includeChildren: true);
                }

                // TODO this won't pick up base class private fields
                foreach (var field in strategyType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic)) {
                    var fieldProp = serialStrategy.FindProperty(field.Name);
                    EditorGUILayout.PropertyField(fieldProp, includeChildren: true);
                }

                if (changeScope.changed) {
                    serialStrategy.ApplyModifiedProperties();
                }
            }
        }

        EditorGUI.EndProperty();

        if (didChange) {
            property.serializedObject.ApplyModifiedProperties();
            if (didChangeStrategy) {
                // Go through the actual Setter
                wrapper.StrategyType = wrapper.StrategyType;
            }
        }
    }
}

