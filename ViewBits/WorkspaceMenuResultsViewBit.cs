using Functional.Option;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

using L = LocalizationString;
using LC = LocalizationConstant;
using LE = ILocalizationExpression;
using LF = LocalizationFormat;
using LI = LocalizationInt;
using LD = LocalizationDefault;

public sealed class WorkspaceMenuResultsViewBit : MonoBehaviour {
#pragma warning disable CS0649
    [SerializeField]
    private TMPro.TMP_Text costText;
    [SerializeField]
    private LayoutGroup starContainer;

    [SerializeField]
    [HideInInspector]
    private List<StarViewBit> stars;
    [SerializeField]
    [HideInInspector]
    private int cost;
#pragma warning restore CS0649

    private void Awake() {
        stars = new List<StarViewBit>();
    }

    public int EarnedStars {
        get => stars.Count(s => s.Earned);
        set {
            for (int i = 0; i < stars.Count; ++i) {
                stars[i].Earned = i < value;
            }
        }
    }

    public int TotalStars {
        get => stars.Count;
        set {
            while (stars.Count > value) {
                int last = stars.Count - 1;
                Utility.DestroyGameObject(stars[last]);
                stars.RemoveAt(last);
            }
            while (stars.Count < value) {
                var star = Utility.Instantiate(Overseer.GlobalAssets.StarPrefab, starContainer.transform);
                stars.Add(star);
            }
        }
    }

    public int Cost {
        get => cost;
        set {
            cost = value;
            costText.text = LF.Inline(nameof(LD.UICurrencyCost)).Format((LI)value).ToString();
        }
    }
}
