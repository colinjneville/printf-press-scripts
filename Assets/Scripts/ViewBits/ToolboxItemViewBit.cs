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

public sealed class ToolboxItemViewBit : MonoBehaviour {
#pragma warning disable CS0649
    [SerializeField]
    private ToolboxItemButton button;
    [SerializeField]
    private TMPro.TMP_Text nameText;
    [SerializeField]
    private UnityEngine.UI.Image iconImage;
    [SerializeField]
    private TMPro.TMP_Text costText;
#pragma warning restore CS0649

    private Option<FontSizeSync> nfss;
    private Option<FontSizeSync> cfss;

    public ToolboxItem ToolboxItem {
        get => button.ToolboxItem;
        set {
            button.ToolboxItem = value;
            nameText.text = value.Name.ToString();
        }
    }

    public Sprite Icon {
        get => iconImage.sprite;
        set => iconImage.sprite = value;
    }

    public int Cost {
        get => int.Parse(costText.text);
        set => costText.text = value.ToString();
    }

    public Option<FontSizeSync> NameFontSizeSync {
        get => nfss;
        set {
            foreach (var nfss in nfss) {
                nfss.RemoveText(nameText);
            }
            nfss = value;
            foreach (var nfss in nfss) {
                nfss.AddText(nameText);
            }
        }
    }

    public Option<FontSizeSync> CostFontSizeSync {
        get => cfss;
        set {
            foreach (var cfss in cfss) {
                cfss.RemoveText(costText);
            }
            cfss = value;
            foreach (var cfss in cfss) {
                cfss.AddText(costText);
            }
        }
    }
}
