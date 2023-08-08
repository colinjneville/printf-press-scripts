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

public enum TutorialHighlight {
    PlayButton,
    StepButton,
    UnstepButton,
    PauseButton,
    StopButton,
    ButtonPanel,
    ExpectedOutput,
}

[Serializable]
public sealed partial class DialogItem {
    private LE text;
    private string leftCharacterPath;
    private string rightCharacterPath;
    private Guid cryptexHighlight;
    private Guid tapeHighlight;
    private Guid rollerHighlight;
    private Guid labelHighlight;
    private int? tapeValueIndexHighlight;
    private TutorialHighlight? tutorialHighlight;

    public DialogItem(LE text, Option<string> leftCharacterPath = default, Option<string> rightCharacterPath = default, Option<Guid> cryptexHighlight = default, Option<Guid> tapeHighlight = default, Option<Guid> rollerHighlight = default, Option<Guid> labelHighlight = default, Option<int> tapeValueIndexHighlight = default, Option<TutorialHighlight> tutorialHighlight = default) {
        this.text = text;
        this.leftCharacterPath = leftCharacterPath.ValueOrDefault;
        this.rightCharacterPath = rightCharacterPath.ValueOrDefault;
        this.cryptexHighlight = cryptexHighlight.ValueOrDefault;
        this.tapeHighlight = tapeHighlight.ValueOrDefault;
        this.rollerHighlight = rollerHighlight.ValueOrDefault;
        this.labelHighlight = labelHighlight.ValueOrDefault;
        this.tapeValueIndexHighlight = tapeValueIndexHighlight.Select(i => (int?)i).ValueOrDefault;
        this.tutorialHighlight = tutorialHighlight.Select(v => (TutorialHighlight?)v).ValueOrDefault;

        if (this.tapeValueIndexHighlight.HasValue && !tapeHighlight.HasValue) {
            Debug.LogWarning("Missing Tape id");
        }
    }

    public LE Text => text;

    public Option<string> LeftCharacterPath => string.IsNullOrWhiteSpace(leftCharacterPath) ? Option.None : leftCharacterPath.ToOption();
    public Option<string> RightCharacterPath => string.IsNullOrWhiteSpace(rightCharacterPath) ? Option.None : rightCharacterPath.ToOption();

    public Option<Guid> CryptexHighlight => cryptexHighlight == Guid.Empty ? Option.None : cryptexHighlight.ToOption();
    public Option<Guid> TapeHighlight => tapeHighlight == Guid.Empty ? Option.None : tapeHighlight.ToOption();
    public Option<Guid> RollerHighlight => rollerHighlight == Guid.Empty ? Option.None : rollerHighlight.ToOption();
    public Option<Guid> LabelHighlight => labelHighlight == Guid.Empty ? Option.None : labelHighlight.ToOption();
    public Option<int> TapeValueIndexHighlight => tapeValueIndexHighlight.HasValue ? tapeValueIndexHighlight.Value.ToOption() : Option.None;
    public Option<TutorialHighlight> TutorialHighlight => tutorialHighlight.HasValue ? tutorialHighlight.Value.ToOption() : Option.None;
}
