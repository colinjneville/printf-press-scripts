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

public sealed class TweenTest : MonoBehaviour/*, ITweenController*/ {
    private bool done = false;

    private void Start() {
        if (!done) {
            var tween = GetComponent<Tween>();
            var parent = transform.parent;
            tween.Mode = Tween.AddMode.Sequence;
            using (tween.Unlock()) {
                parent.position += new Vector3(5f, 5f);
            }
            using (tween.Unlock()) {
                parent.position += new Vector3(-10f, 5f);
            }
            done = true;
        }
    }


    private IntegralFloat velocity;
    /*
    void ITweenController.StartStep(Tween.IStep step) {
        velocity = new IntegralFloat(1f, 10f);
    }

    void ITweenController.ContinueStep(Tween.IStep step, float timeDelta) {
        var tween = step.Tween;
        foreach (var target in tween.Target) {
            float positionDelta;
            velocity = velocity.Advance(timeDelta, out positionDelta);
            tween.RectTransform.position = Vector2.MoveTowards(tween.RectTransform.position, step.Position, positionDelta);
        }
    }

    void ITweenController.EndStep(Tween.IStep step) {
    }*/
}

/*
public sealed class TweenTest : MonoBehaviour, ITweenImpetus, ITweenForce {
    void ITweenImpetus.TweenImpetus(Tween tween) {
        tween.PositionVelocity = 15f;
        tween.PositionDirection = 90f;
    }

    void ITweenForce.TweenForce(Tween tween, float timeDelta) {
        foreach (var target in tween.Target) {
            var targetAngle = Vector2.SignedAngle(Vector2.right, Tween.GetPosition(target) - Tween.GetPosition(tween.Transform));
            //bool rotLeft = Mathf.DeltaAngle
            var newAngle = Mathf.MoveTowardsAngle(tween.PositionDirection, targetAngle, 720f * timeDelta);
        }

        //tween.PositionVelocityMax = 2f + (tween.TargetPosition - tween.TransformPosition).magnitude * 8f;
        //tween.PositionAccelerationValue = tween.PositionAccelerationValue.magnitude * tween.PositionDirection;
        //tween.PositionJerk = tween.PositionJerk.magnitude * tween.PositionDirection;
        //var angle = Vector2.SignedAngle(Vector2.right, tween.PositionVelocityValue);
        //var targetAngle = Vector2.SignedAngle(Vector2.right, tween.TargetPosition - tween.TransformPosition);
        //
        //var newAngle = Mathf.MoveTowardsAngle(angle, targetAngle, 3600f * timeDelta);
        //var normalized = new Vector2(Mathf.Cos(Mathf.Deg2Rad * newAngle), Mathf.Sin(Mathf.Deg2Rad * newAngle));
        //tween.PositionVelocityValue = normalized * tween.PositionVelocityValue;
        //tween.PositionAccelerationValue = normalized * tween.PositionAccelerationValue.magnitude;
        //tween.PositionJerk = normalized * tween.PositionJerk.magnitude;
        //
        //Debug.DrawLine(tween.TransformPosition, tween.TransformPosition + new Vector3(Mathf.Cos(Mathf.Deg2Rad * angle), Mathf.Sin(Mathf.Deg2Rad * angle)), Color.red);
        //Debug.DrawLine(tween.TransformPosition, tween.TransformPosition + new Vector3(Mathf.Cos(Mathf.Deg2Rad * targetAngle), Mathf.Sin(Mathf.Deg2Rad * targetAngle)), Color.green);
        //Debug.DrawLine(tween.TransformPosition, tween.TransformPosition + new Vector3(Mathf.Cos(Mathf.Deg2Rad * newAngle), Mathf.Sin(Mathf.Deg2Rad * newAngle)), Color.blue);
    }

    private bool done = false;

    private void Start() {
        if (!done) {
            var tween = GetComponent<Tween>();
            var parent = transform.parent;
            using (tween.Unlock()) {
                parent.position += new Vector3(5f, 5f);
            }
            done = true;
        }
    }
}
*/
