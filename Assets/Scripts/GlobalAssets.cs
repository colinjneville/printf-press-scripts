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


public class GlobalAssets : MonoBehaviour {
#pragma warning disable CS0649

    #region    Prefabs

    [SerializeField]
    private Screen screenPrefab;
    public Screen ScreenPrefab => screenPrefab;

    [SerializeField]
    private ControlsViewBit controlsPrefab;
    public ControlsViewBit ControlsPrefab => controlsPrefab;

    [SerializeField]
    private ChapterButtonViewBit chapterButtonPrefab;
    public ChapterButtonViewBit ChapterButtonPrefab => chapterButtonPrefab;

    [SerializeField]
    private LevelButtonViewBit levelButtonPrefab;
    public LevelButtonViewBit LevelButtonPrefab => levelButtonPrefab;

    [SerializeField]
    private SolutionButtonViewBit solutionButtonPrefab;
    public SolutionButtonViewBit SolutionButtonPrefab => solutionButtonPrefab;

    [SerializeField]
    private NewSolutionButtonViewBit newSolutionButtonPrefab;
    public NewSolutionButtonViewBit NewSolutionButtonPrefab => newSolutionButtonPrefab;

    [SerializeField]
    private QuitButton quitButtonPrefab;
    public QuitButton QuitButtonPrefab => quitButtonPrefab;

    [SerializeField]
    private DialogItemViewBit dialogItemPrefab;
    public DialogItemViewBit DialogItemPrefab => dialogItemPrefab;

    [SerializeField]
    private DialogSequenceViewBit dialogSequencePrefab;
    public DialogSequenceViewBit DialogSequencePrefab => dialogSequencePrefab;

    [SerializeField]
    private ToolboxViewBit toolboxPrefab;
    public ToolboxViewBit ToolboxPrefab => toolboxPrefab;

    [SerializeField]
    private ToolboxTabViewBit toolboxTabPrefab;
    public ToolboxTabViewBit ToolboxTabPrefab => toolboxTabPrefab;

    [SerializeField]
    private ToolboxItemViewBit toolboxItemPrefab;
    public ToolboxItemViewBit ToolboxItemPrefab => toolboxItemPrefab;

    [SerializeField]
    private SingleValueInputModeViewBit singleValueInputModePrefab;
    public SingleValueInputModeViewBit SingleValueInputModePrefab => singleValueInputModePrefab;

    [SerializeField]
    private TapeValueViewBit tapeValuePrefab;
    public TapeValueViewBit TapeValuePrefab => tapeValuePrefab;

    [SerializeField]
    private TapeValueRootViewBit tapeValueRootPrefab;
    public TapeValueRootViewBit TapeValueRootPrefab => tapeValueRootPrefab;

    [SerializeField]
    private FrameViewBit framePrefab;
    public FrameViewBit FramePrefab => framePrefab;

    [SerializeField]
    private RollerViewBit rollerPrefab;
    public RollerViewBit RollerPrefab => rollerPrefab;

    [SerializeField]
    private LabelViewBit labelPrefab;
    public LabelViewBit LabelPrefab => labelPrefab;

    [SerializeField]
    private LabelPointViewBit labelPointPrefab;
    public LabelPointViewBit LabelPointPrefab => labelPointPrefab;

    [SerializeField]
    private TapeViewBit tapePrefab;
    public TapeViewBit TapePrefab => tapePrefab;

    [SerializeField]
    private CryptexViewBit cryptexPrefab;
    public CryptexViewBit CryptexPrefab => cryptexPrefab;

    [SerializeField]
    private WorkspaceViewBit workspacePrefab;
    public WorkspaceViewBit WorkspacePrefab => workspacePrefab;

    [SerializeField]
    private ExecutionContextViewBit executionContextPrefab;
    public ExecutionContextViewBit ExecutionContextPrefab => executionContextPrefab;

    [SerializeField]
    private OutputTapeViewBit outputTapePrefab;
    public OutputTapeViewBit OutputTapePrefab => outputTapePrefab;

    [SerializeField]
    private CryptexInsertViewBit cryptexInsertPrefab;
    public CryptexInsertViewBit CryptexInsertPrefab => cryptexInsertPrefab;

    [SerializeField]
    private StarViewBit starPrefab;
    public StarViewBit StarPrefab => starPrefab;

    [SerializeField]
    private RectTransform selectionRectanglePrefab;
    public RectTransform SelectionRectanglePrefab => selectionRectanglePrefab;

    [SerializeField]
    private TextHighlightViewBit textHighlightPrefab;
    public TextHighlightViewBit TextHighlightPrefab => textHighlightPrefab;

    [SerializeField]
    private FlowArrowViewBit flowArrowPrefab;
    public FlowArrowViewBit FlowArrowPrefab => flowArrowPrefab;

    [SerializeField]
    private ShiftArrowViewBit shiftArrowPrefab;
    public ShiftArrowViewBit ShiftArrowPrefab => shiftArrowPrefab;

    [SerializeField]
    private HopArrowViewBit hopArrowPrefab;
    public HopArrowViewBit HopArrowPrefab => hopArrowPrefab;

    [SerializeField]
    private HopArrowSegmentViewBit hopArrowSegmentPrefab;
    public HopArrowSegmentViewBit HopArrowSegmentPrefab => hopArrowSegmentPrefab;

    [SerializeField]
    private InvalidArrowViewBit invalidArrowPrefab;
    public InvalidArrowViewBit InvalidArrowPrefab => invalidArrowPrefab;

    [SerializeField]
    private CryptexRotateIconViewBit cryptexRotateIconPrefab;
    public CryptexRotateIconViewBit CryptexRotateIconPrefab => cryptexRotateIconPrefab;

    [SerializeField]
    private WorkspaceMenuHelpViewBit helpMenuPrefab;
    public WorkspaceMenuHelpViewBit HelpMenuPrefab => helpMenuPrefab;

    [SerializeField]
    private WorkspaceMenuResultsViewBit resultsMenuPrefab;
    public WorkspaceMenuResultsViewBit ResultsMenuPrefab => resultsMenuPrefab;

    [SerializeField]
    private SettingWheelViewBit settingWheelPrefab;
    public SettingWheelViewBit SettingWheelPrefab => settingWheelPrefab;

    [SerializeField]
    private SettingsViewBit settingsPrefab;
    public SettingsViewBit SettingsPrefab => settingsPrefab;

    [SerializeField]
    private SettingsPageViewBit settingsPagePrefab;
    public SettingsPageViewBit SettingsPagePrefab => settingsPagePrefab;

    [SerializeField]
    private SettingsPageNameViewBit settingsPageNamePrefab;
    public SettingsPageNameViewBit SettingsPageNamePrefab => settingsPageNamePrefab;

    [SerializeField]
    private ApertureViewBit aperturePrefab;
    public ApertureViewBit AperturePrefab => aperturePrefab;

    [SerializeField]
    private NoteViewBit notePrefab;
    public NoteViewBit NotePrefab => notePrefab;

    #endregion Prefabs

    #region    Materials

    [SerializeField]
    private Material defaultMaterial;
    public Material DefaultMaterial => defaultMaterial;

    [SerializeField]
    private Material defaultCutoutMaterial;
    public Material DefaultCutoutMaterial => defaultCutoutMaterial;

    [SerializeField]
    private Material defaultColorMaterial;
    public Material DefaultColorMaterial => defaultColorMaterial;

    [SerializeField]
    private Material defaultTransparentMaterial;
    public Material DefaultTransparentMaterial => defaultTransparentMaterial;

    [SerializeField]
    private Material defaultTransparentSolidMaterial;
    public Material DefaultTransparentSolidMaterial => defaultTransparentSolidMaterial;

    [SerializeField]
    private Material uiHighlightMaterial;
    public Material UIHighlightMaterial => uiHighlightMaterial;

    #endregion Materials

    #region    Meshes

    [SerializeField]
    private Mesh quadMesh;
    public Mesh QuadMesh => quadMesh;

    [SerializeField]
    private Mesh cubeMesh;
    public Mesh CubeMesh => cubeMesh;

    #endregion Meshes

    #region    Fonts

    [SerializeField]
    private TMPro.TMP_FontAsset defaultFont;
    public TMPro.TMP_FontAsset DefaultFont => defaultFont;

    #endregion Fonts

    #region    Textures
    #endregion Textures

    #region    Sprites

    [SerializeField]
    private Sprite frameSprite;
    public Sprite FrameSprite => frameSprite;

    [SerializeField]
    private Sprite frameAltSprite;
    public Sprite FrameAltSprite => frameAltSprite;

    [SerializeField]
    private Sprite rollerTopSprite;
    public Sprite RollerTopSprite => rollerTopSprite;

    [SerializeField]
    private Sprite rollerTopAltSprite;
    public Sprite RollerTopAltSprite => rollerTopAltSprite;

    [SerializeField]
    private Sprite colorValueSprite;
    public Sprite ColorValueSprite => colorValueSprite;

    [SerializeField]
    private Sprite cutSprite;
    public Sprite CutSprite => cutSprite;

    [SerializeField]
    private Sprite cryptexIcon;
    public Sprite CryptexIcon => cryptexIcon;

    [SerializeField]
    private Sprite blankTapeIcon;
    public Sprite BlankTapeIcon => blankTapeIcon;

    [SerializeField]
    private Sprite numberTapeIcon;
    public Sprite NumberTapeIcon => numberTapeIcon;

    [SerializeField]
    private Sprite rollerIcon;
    public Sprite RollerIcon => rollerIcon;

    [SerializeField]
    private Sprite starEarnedIcon;
    public Sprite StarEarnedIcon => starEarnedIcon;

    [SerializeField]
    private Sprite starUnearnedIcon;
    public Sprite StarUnearnedIcon => starUnearnedIcon;

    [SerializeField]
    private Sprite cryptexRotateFalseIcon;
    public Sprite CryptexRotateFalseIcon => cryptexRotateFalseIcon;

    [SerializeField]
    private Sprite cryptexRotateTrueIcon;
    public Sprite CryptexRotateTrueIcon => cryptexRotateTrueIcon;

    #endregion Sprites

    #region    Text

    [SerializeField]
    private TextAsset defaultCampaignJson;
    public TextAsset DefaultCampaignJson => defaultCampaignJson;

    #endregion Text

    #region    Audio

    [SerializeField]
    private AudioClip windIntroClip;
    public AudioClip WindIntroClip => windIntroClip;

    [SerializeField]
    private AudioClip windLoopClip;
    public AudioClip WindLoopClip => windLoopClip;

    [SerializeField]
    private AudioClip bellClip;
    public AudioClip BellClip => bellClip;

    #endregion Audio

#pragma warning restore CS0649

    public static string InitScene => "init";
    public static string MenuScene => "menu";
    public static string WorkspaceScene = "workspace";
}
