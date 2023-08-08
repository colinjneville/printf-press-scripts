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

public sealed class ToolboxItemButton : Button {
    private ToolboxItem toolboxItem;
    private Option<DragOperation> dragOperation;

    public ToolboxItem ToolboxItem {
        get => toolboxItem;
        set => toolboxItem = value;
    }

    public override bool AllowMouse0 => true;

    public override bool Mouse0(InputMode.State state, InputMode.Modifiers modifiers, Vector3 position, Lazy<RaycastHit2D[]> hits, bool overButton) {
        switch (state) {
            case InputMode.State.Held:
                if (!dragOperation.HasValue) {
                    if (overButton) {

                    } else {
                        foreach (var workspace in Overseer.Workspace) {
                            var worldPosition = Camera.main.ScreenToWorldPoint(position);
                            dragOperation = toolboxItem.CreateDragOperation(workspace, worldPosition);
                        }
                    }
                }

                foreach (var dragOperation in dragOperation) {
                    dragOperation.Mouse0Held(position, hits);
                }
                break;
            case InputMode.State.Release:
                if (overButton && !dragOperation.HasValue) {
                    // HACK really hacky way of communicating with a specific InputMode
                    var workspaceScene = FindAnyObjectByType<WorkspaceScene>();
                    if (workspaceScene != null) {
                        workspaceScene.SetPickPart(toolboxItem);
                    }
                }

                foreach (var dragOperation in dragOperation) {
                    if (!overButton) {
                        dragOperation.Mouse0Release(position, hits);
                    }
                    this.dragOperation = Option.None;
                }
                break;
        }
        return true;
    }
}
