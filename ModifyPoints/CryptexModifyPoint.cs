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

public sealed class CryptexModifyPoint : ModifyPoint<CryptexInsertPoint, Cryptex> {
    public Cryptex Cryptex { get; set; }

    private CryptexRotateIconViewBit rotateIcon;

    public override bool AllowDrag => true;

    protected override bool DeleteInternal(bool forMove, bool checkOnly) {
        if ((Cryptex.Lock.HasFlag(LockType.Move) && forMove) || (Cryptex.Lock.HasFlag(LockType.Delete) && !forMove)) {
            return false;
        }
        if (!checkOnly) {
            Workspace.ApplyModificationRecord(Workspace.RemoveCryptex(Cryptex.Id));
        }
        return true;
    }

    public override bool Edit() {
        if (rotateIcon != null) {
            Utility.DestroyGameObject(rotateIcon);
            rotateIcon = null;
        }

        Workspace.ApplyModificationRecord(Cryptex.Rotate());
        foreach (var view in Cryptex.View) {
            rotateIcon = Utility.Instantiate(Overseer.GlobalAssets.CryptexRotateIconPrefab, WorkspaceScene.Layer.Edit(this.GetScreen()));
            var proxy = this.GetScreen().FixedProxy.CreateReceiver(rotateIcon.gameObject);
            // HACK
            var mouseX = Camera.main.ScreenToWorldPoint(Input.mousePosition).x;
            
            rotateIcon.transform.localPosition = (view.transform.position + new Vector3(0f, 1.5f, 0f)).WithX(mouseX);
            rotateIcon.Rotate = Cryptex.Rotated;
            rotateIcon.TargetAlpha = 0.5f;
            rotateIcon.Alpha = 0.5f;
            rotateIcon.RegisterDimensions();
            using (rotateIcon.Unlock()) {
                rotateIcon.TargetAlpha = 1.0f;
            }
            using (rotateIcon.Unlock()) {
                rotateIcon.TargetAlpha = 0.0f;
            }
        }
        return true;
    }

    protected override Inserter<CryptexInsertPoint, Cryptex> Inserter => CryptexInserter.Instance;

    public override Cryptex Model => Cryptex;
}
