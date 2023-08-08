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

public sealed class WorkspaceViewBit : MonoBehaviour {
#pragma warning disable CS0649
    [SerializeField]
    private TMPro.TMP_Text costTmp;
    [SerializeField]
    private RectTransform toolboxParent;
    [SerializeField]
    private RectTransform noteParent;
    [SerializeField]
    private RectTransform noteEditParent;
#pragma warning restore CS0649

    private Option<Toolbox> toolbox;
    private Option<Note> note;
    private Option<TextHighlight> noteHighlight;

    public string CostText {
        get => costTmp.text;
        set => costTmp.text = value;
    }

    public Option<Toolbox> Toolbox {
        get => toolbox;
        set {
            if (toolbox != value) {
                foreach (var toolbox in toolbox) {
                    toolbox.ClearView();
                }
                toolbox = value;
                foreach (var toolbox in toolbox) {
                    var view = toolbox.MakeView();
                    view.transform.SetParent(toolboxParent, false);
                    view.GetComponent<RectTransform>().MatchParent();
                }
            }
        }
    }

    public Option<Note> Note {
        get => note;
        set {
            if (note != value) {
                foreach (var note in note) {
                    note.ClearView();
                }
                note = value;
                foreach (var note in note) {
                    var view = note.MakeView();
                    view.transform.SetParent(noteParent, false);
                    view.GetComponent<RectTransform>().MatchParent();
                }
            }
        }
    }

    public bool ToolboxEnabled {
        get {
            foreach (var toolbox in toolbox) {
                return !toolbox.IsViewHidden();
            }
            return false;
        }
        set {
            foreach (var toolbox in toolbox) {
                if (value) {
                    toolbox.UnhideView();
                } else {
                    toolbox.HideView();
                }
            }
        }
    }

    /*
    public Option<TextHighlight> NoteHighlight {
        get => noteHighlight;
        set {
            if (noteHighlight != value) {
                foreach (var noteHighlight in noteHighlight) {
                    noteHighlight.ClearView();
                }
                noteHighlight = value;
                foreach (var noteHighlight in noteHighlight) {
                    var view = noteHighlight.MakeView();
                    view.transform.SetParent(noteEditParent, false);
                    //view.GetComponent<RectTransform>().MatchParent();
                }
            }
        }
    }
    */
}
