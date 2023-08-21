using Functional.Option;
using Newtonsoft.Json;
using IntervalTree;
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

public class ArrowInfo : IComparable<ArrowInfo> {
    private readonly TargetInfo targetInfo;
    private readonly int creationTime;
    private int level;
    private bool visible;

    public ArrowInfo(TargetInfo targetInfo, int creationTime) {
        this.targetInfo = targetInfo;
        this.creationTime = creationTime;
        level = -1;
    }

    public int CompareTo(ArrowInfo other) {
        // Swap the order because we want longer jumps first
        var c = Mathf.Abs(other.targetInfo.SourceHOffset - other.targetInfo.TargetHOffset).CompareTo(Mathf.Abs(targetInfo.SourceHOffset - targetInfo.TargetHOffset));
        if (c != 0) {
            return c;
        }
        // We don't actually care about the order beyond the length, but SortedDictionary needs an unambiguous order
        return objectIds.GetId(this, out var _).CompareTo(objectIds.GetId(other, out var _));
    }

    public TargetInfo TargetInfo => targetInfo;
    public int CreationTime => creationTime;
    public int Level {
        get => level;
        set => level = value;
    }
    public bool Visible {
        get => visible;
        set => visible = value;
    }

    public int From => Mathf.Min(targetInfo.SourceHOffset, targetInfo.TargetHOffset);
    public int To => Mathf.Max(targetInfo.SourceHOffset, targetInfo.TargetHOffset);
    public int Length => Mathf.Abs(targetInfo.SourceHOffset - targetInfo.TargetHOffset);

    private static System.Runtime.Serialization.ObjectIDGenerator objectIds = new System.Runtime.Serialization.ObjectIDGenerator();
}

public class LookaheadView : MonoBehaviour {
    private IntervalTree<int, ArrowInfo> ranges;
    private SortedDictionary<ArrowInfo, ArrowViewBit> arrows;
    private List<RectTransform> layers;

    private void Awake() {
        ranges = new IntervalTree<int, ArrowInfo>();
        arrows = new SortedDictionary<ArrowInfo, ArrowViewBit>();
        layers = new List<RectTransform>();
    }

    private void OnDestroy() {
        foreach (var arrow in arrows) {
            Utility.DestroyGameObject(arrow.Value);
        }
    }

    public void AddArrow(ArrowInfo arrow) {
        var arrowView = CreateArrowView(arrow);
        if (arrowView != null) {
            ranges.Add(arrow.From, arrow.To, arrow);
            arrows.Add(arrow, arrowView);
        }
    }

    public void RemoveArrow(ArrowInfo arrow) {
        if (arrows.TryGetValue(arrow, out var arrowView)) {
            Utility.DestroyGameObject(arrowView);
            ranges.Remove(arrow);
            arrows.Remove(arrow);
        }
    }

    public void UpdateArrowViews(int currentTime, int lookaheadCount) {
        // Reset all arrows to the indeterminate Level (-1)
        foreach (var arrow in arrows) {
            arrow.Key.Level = -1;
            arrow.Key.Visible = true;
        }

        // Going from longest arrow to shortest, put each arrow at the minimum level that does not conflict with any already-set (i.e. longer) arrows
        foreach (var arrow in arrows) {
            // Find all overlapping arrows, excluding those we haven't processed yet (Level == -1)
            var overlaps = ranges.Query(arrow.Key.From, arrow.Key.To).Where(a => a.Level >= 0 && a.Visible && SameZone(arrow.Key, a)).ToList();
            // Default to max if no other collisions/no unmarked levels
            arrow.Key.Level = overlaps.Count;

            for (int i = 0; i < overlaps.Count; ++i) {
                var overlap = overlaps[i];
                if (Shadows(overlap, arrow.Key)) {
                    bool reuseExisting = ReuseExisting(overlap, arrow.Key);
                    var overlapView = arrows[overlap];
                    if (arrow.Key.CreationTime < overlap.CreationTime) {

                        if (reuseExisting) {
                            // It doesn't matter which arrow is actually visible, just that we have the correct opacity, so re-update
                            // the previous arrow to our opacity, and make ourselves invisible
                            UpdateArrowView(overlap, overlapView, arrow.Key.CreationTime - currentTime, lookaheadCount);
                        }
                    } else {
                        arrow.Key.Visible = false;
                        DisableArrowView(arrow.Value);
                        break;
                    }
                    if (reuseExisting) {
                        arrow.Key.Visible = false;
                        DisableArrowView(arrow.Value);
                        break;
                    } else {
                        overlap.Visible = false;
                        DisableArrowView(overlapView);
                        arrow.Key.Level = overlap.Level;
                    }
                }

                int level = overlap.Level;
                int actualLevel = level >= 0 ? level : ~level;

                // "Mark" each level index that is taken by an overlapping arrow
                // We multi-purpose Level to hold the marker (by flipping the bits)
                if (actualLevel < overlaps.Count) {
                    int markerLevel = overlaps[actualLevel].Level;
                    // If markerLevel has not been already marked...
                    if (markerLevel >= 0) {
                        overlaps[actualLevel].Level = ~markerLevel;
                    }
                }
            }
            if (!arrow.Key.Visible) {
                continue;
            }


            for (int i = 0; i < overlaps.Count; ++i) {
                // Is this index unmarked?
                if (overlaps[i].Level >= 0) {
                    // Is this the first unmarked index?
                    if (i < arrow.Key.Level) {
                        arrow.Key.Level = i;
                    }
                } else {
                    // Revert the marking
                    overlaps[i].Level = ~overlaps[i].Level;
                }
            }
            Assert.NotNegative(arrow.Key.Level);

            UpdateArrowView(arrow.Key, arrow.Value, arrow.Key.CreationTime - currentTime, lookaheadCount);
        }
    }

    private bool SameZone(ArrowInfo a, ArrowInfo b) {
        TargetInfo.ActionType aAction = a.TargetInfo.Action;
        TargetInfo.ActionType bAction = b.TargetInfo.Action;

        // For now just check for the same Action, but later other multiple Action types will be in the same zone (such as Jump and Move)
        if (aAction == bAction) {
            if (aAction == TargetInfo.ActionType.Jump) {
                return true;
            }

            if (aAction == TargetInfo.ActionType.Shift) {
                return a.TargetInfo.SourceVOffset == b.TargetInfo.SourceVOffset;
            }

            if (aAction == TargetInfo.ActionType.Hop) {
                return true;
            }

            if (aAction == TargetInfo.ActionType.Invalid) {
                return true;
            }
        } else {
            if (aAction == TargetInfo.ActionType.Jump || bAction == TargetInfo.ActionType.Jump || aAction == TargetInfo.ActionType.Invalid) {
                return aAction == TargetInfo.ActionType.Hop || bAction == TargetInfo.ActionType.Hop || bAction == TargetInfo.ActionType.Invalid;
            }
        }
        return false;
    }

    // Assumes SameZone for the arrows is true
    // Returns shadowed ArrowInfo, null if no shadowing
    private bool Shadows(ArrowInfo a, ArrowInfo b) {
        switch (a.TargetInfo.Action) {
            case TargetInfo.ActionType.Jump:
                return a.From == b.From && a.To == b.To;
            case TargetInfo.ActionType.Shift:
                return true;
            case TargetInfo.ActionType.Hop:
                return true;
            case TargetInfo.ActionType.Invalid:
                return true;
        }
        return false;
    }

    private bool ReuseExisting(ArrowInfo a, ArrowInfo b) {
        switch (a.TargetInfo.Action) {
            case TargetInfo.ActionType.Jump:
                return true;
            case TargetInfo.ActionType.Shift:
                return false;
            case TargetInfo.ActionType.Hop:
                return false;
            case TargetInfo.ActionType.Invalid:
                return true;
        }
        return false;
    }

    private void DisableArrowView(ArrowViewBit arrowView) {
        arrowView.gameObject.SetActive(false);
    }

    private void UpdateArrowView(ArrowInfo arrow, ArrowViewBit arrowView, int timeDelta, int lookaheadCount) {
        arrowView.gameObject.SetActive(true);

        // Level may be marked (i.e. bit-flipped) if we are in the middle of reorganizing arrows
        int level = arrow.Level;
        if (level < 0) {
            level = ~level;
        }
        SetArrowLayer(arrow, arrowView, level);

        foreach (var userData in Overseer.UserDataManager.Active) {
            arrowView.Delay = (((float)timeDelta / lookaheadCount) - userData.Settings.LookaheadMaxOpacityPoint) / userData.Settings.LookaheadMinOpacityPoint;
            int i;
            for (i = arrowView.transform.GetSiblingIndex(); i > 0; --i) {
                if (arrowView.transform.parent.GetChild(i - 1).GetComponent<ArrowViewBit>().Delay >= arrowView.Delay) {
                    break;
                }
            }
            arrowView.transform.SetSiblingIndex(i);
        }
    }

    private ArrowViewBit CreateArrowView(ArrowInfo arrow) {
        // TODO UI SCALING
        bool flip = arrow.TargetInfo.TargetHOffset < arrow.TargetInfo.SourceHOffset;
        bool vflip = false;

        ArrowViewBit arrowView;
        Vector2 sizeDelta;
        Vector3 localPosition;
        switch (arrow.TargetInfo.Action) {
            case TargetInfo.ActionType.Jump:
                var flowArrowView = Utility.Instantiate(Overseer.GlobalAssets.FlowArrowPrefab, transform);
                arrowView = flowArrowView;

                sizeDelta = new Vector2(1f + 1.1f * arrow.Length, 1.5f);
                localPosition = new Vector2(arrow.From * 1.1f - 0.5f + (flip ? sizeDelta.x : 0f), -0.5f);
                break;
            case TargetInfo.ActionType.Shift:
                var shiftArrowView = Utility.Instantiate(Overseer.GlobalAssets.ShiftArrowPrefab, transform);
                arrowView = shiftArrowView;

                sizeDelta = new Vector2(1.1f * arrow.Length, 1f);
                localPosition = new Vector2(arrow.From * 1.1f + (flip ? sizeDelta.x : 0f), -3.5f - 1.5f * (arrow.TargetInfo.SourceVOffset));
                break;
            case TargetInfo.ActionType.Hop:
                var hopArrowView = Utility.Instantiate(Overseer.GlobalAssets.HopArrowPrefab, transform);
                arrowView = hopArrowView;

                hopArrowView.Distance = Mathf.Abs(arrow.TargetInfo.SourceVOffset - arrow.TargetInfo.TargetVOffset);
                sizeDelta = new Vector2(1f, 1f);
                localPosition = new Vector2(arrow.From * 1.1f, 0f);
                vflip = arrow.TargetInfo.SourceVOffset > arrow.TargetInfo.TargetVOffset;
                break;
            case TargetInfo.ActionType.Invalid:
                var invalidArrowView = Utility.Instantiate(Overseer.GlobalAssets.InvalidArrowPrefab, transform);
                arrowView = invalidArrowView;

                sizeDelta = new Vector2(1f, 1f);
                localPosition = new Vector2(arrow.From * 1.1f, 0f);
                break;
            default:
                return null;
        }
        arrowView.BaseColor = TapeValueColor.GetColor(arrow.TargetInfo.Roller.Color);

        var rt = arrowView.GetComponent<RectTransform>();
        rt.sizeDelta = sizeDelta;
        rt.localPosition = localPosition;
        rt.localScale = new Vector3(flip ? -1f : 1f, vflip ? -1f : 1f, 1f);

        return arrowView;
    }

    private void SetArrowLayer(ArrowInfo arrow, ArrowViewBit arrowView, int layerIndex) {
        var rt = arrowView.GetComponent<RectTransform>();

        // TODO UI SCALING
        switch (arrow.TargetInfo.Action) {
            case TargetInfo.ActionType.Jump:
                rt.sizeDelta = rt.sizeDelta.WithY(1.0f + arrow.Level * 0.6f);
                break;
            case TargetInfo.ActionType.Shift:
                // TODO
                break;
            case TargetInfo.ActionType.Hop:
                break;
        }

        while (layers.Count <= layerIndex) {
            var go = new GameObject(layerIndex.ToString());
            var layer = go.AddComponent<RectTransform>();
            go.transform.SetParent(transform, false);
            go.transform.SetAsFirstSibling();
            layers.Add(layer);
        }
        arrowView.transform.SetParent(layers[layerIndex], false);
    }
}