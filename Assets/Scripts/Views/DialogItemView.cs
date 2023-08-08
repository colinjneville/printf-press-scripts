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

partial class DialogItem : IModel<DialogItem.DialogItemView> {
    private ViewContainer<DialogItemView, DialogItem> view;
    public Option<DialogItemView> View => view.View;
    public DialogItemView MakeView() => view.MakeView(this);
    public void ClearView() => view.ClearView();

    [RequireComponent(typeof(RectTransform))]
    public sealed class DialogItemView : MonoBehaviour, IView<DialogItem> {
        public DialogItem Model { get; set; }

        private DialogItemViewBit bit;

        void IView.StartNow() {
            bit = Utility.Instantiate(Overseer.GlobalAssets.DialogItemPrefab, transform);

            bit.Text = Model.Text.ToString();

            foreach (var leftCharacterPath in Model.LeftCharacterPath) {
                bit.LeftCharacter = GetSprite(leftCharacterPath);
            }
            foreach (var rightCharacterPath in Model.RightCharacterPath) {
                bit.RightCharacter = GetSprite(rightCharacterPath);
            }
        }

        void IView.OnDestroyNow() { }

        private static Dictionary<string, Option<Sprite>> spriteCache = new Dictionary<string, Option<Sprite>>();
        private static Option<Sprite> GetSprite(string path) {
            Option<Sprite> value;
            if (!spriteCache.TryGetValue(path, out value)) {
                value = LoadSprite(path);
                spriteCache.Add(path, value);
            }
            return value;
        }

        private static Option<Sprite> LoadSprite(string path) {
            try {
                Debug.Log(System.IO.Path.Combine(Utility.InstallResourcesPath, path));
                var stream = System.IO.File.OpenRead(System.IO.Path.Combine(Utility.InstallResourcesPath, path));
                if (stream != null) {
                    var memoryStream = new System.IO.MemoryStream((int)stream.Length);
                    stream.CopyTo(memoryStream);
                    var texture = new Texture2D(1, 1);
                    texture.LoadImage(memoryStream.ToArray(), markNonReadable: true);
                    return Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                }
            } catch (Exception ex) {
                Debug.Log($"Unable to load image {path}: {ex}");
            }
            return Option.None;
        }

        public Option<IApertureTarget> ApertureTarget {
            get {
                foreach (var highlight in Model.TutorialHighlight) {
                    // HACK
                    var cvb = FindAnyObjectByType<ControlsViewBit>();
                    switch (highlight) {
                        case global::TutorialHighlight.PlayButton:
                            return cvb.RunTarget.ToOption();
                        case global::TutorialHighlight.StepButton:
                            return cvb.StepTarget.ToOption();
                        case global::TutorialHighlight.UnstepButton:
                            return cvb.UnstepTarget.ToOption();
                        case global::TutorialHighlight.PauseButton:
                            return cvb.BreakTarget.ToOption();
                        case global::TutorialHighlight.StopButton:
                            return cvb.StopTarget.ToOption();
                        case global::TutorialHighlight.ButtonPanel:
                            return cvb.PanelTarget.ToOption();
                        case global::TutorialHighlight.ExpectedOutput:
                            foreach (var workspace in Overseer.Workspace) {
                                foreach (var workspaceView in workspace.View) {
                                    return workspaceView.ExpectedOutputTarget.ToOption();
                                }
                            }
                            break;
                    }
                }

                foreach (var workspace in Overseer.Workspace) {
                    foreach (var cryptexHighlight in Model.CryptexHighlight) {
                        if (workspace.GetCryptex(cryptexHighlight).TryGetValue(out Cryptex cryptex)) {
                            return cryptex.MakeView();
                        } else {
                            Debug.LogWarning($"Cryptex '{cryptexHighlight}' was not found for highlight");
                        }
                    }
                    foreach (var tapeHighlight in Model.TapeHighlight) {
                        var tape = workspace.Cryptexes.SelectMany(c => c.Tapes).SingleOrDefault(t => t.Id == tapeHighlight);
                        if (tape != null) {
                            if (Model.TapeValueIndexHighlight.TryGetValue(out int index)) {
                                return tape.MakeView().GetValueTarget(index).ToOption();
                            } else {
                                return tape.MakeView();
                            }
                        } else {
                            Debug.LogWarning($"Tape '{tapeHighlight}' was not found for highlight");
                        }
                    }
                    foreach (var rollerHighlight in Model.RollerHighlight) {
                        if (workspace.GetRoller(rollerHighlight).TryGetValue(out Roller roller)) {
                            return roller.MakeView();
                        } else {
                            Debug.LogWarning($"Roller '{rollerHighlight}' was not found for highlight");
                        }
                    }
                    foreach (var labelHighlight in Model.LabelHighlight) {
                        var label = workspace.Cryptexes.SelectMany(c => c.GetLabel(labelHighlight)).SingleOrDefault();
                        if (label != null) {
                            return label.MakeView();
                        } else {
                            Debug.LogWarning($"Label '{labelHighlight}' was not found for highlight");
                        }
                    }
                }
                return Option.None;
            }
        }
    }
}
