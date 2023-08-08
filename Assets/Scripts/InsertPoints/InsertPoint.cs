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

[RequireComponent(typeof(RectTransform))]
public abstract class InsertPoint : MonoBehaviour {
    public WorkspaceFull Workspace { get; set; }

    public virtual Vector3 Origin => transform.position;

    public virtual bool AllowSnap => true;

    public virtual void OnStartSnap() { }
    public virtual void OnEndSnap() { }

    // HACK Since Cryptexes are the only objects that don't have discrete placement zones, the InsertPoint needs the mouse coordinates.
    // This callback allows CryptexInsertPoint to store the coorindates locally for when the insert actually happens
    public virtual void OnHoverHack(Vector2 xy) { }
}
