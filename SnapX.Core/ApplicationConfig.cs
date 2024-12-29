using System.ComponentModel;
using System.Reflection;
using SixLabors.ImageSharp;
using SnapX.Core.Indexer;
using SnapX.Core.Job;
using SnapX.Core.Utils.Miscellaneous;
using SnapX.ScreenCaptureLib;

namespace SnapX.Core;

public class GeneralSettings
{
    public bool PlaySoundAfterCapture { get; set; } = true;
    public bool PlaySoundAfterUpload { get; set; } = true;
    public bool ShowToastNotificationAfterTaskCompleted { get; set; } = true;
    public double ToastWindowDuration { get; set; } = 3.0;
    public double ToastWindowFadeDuration { get; set; } = 1.0;
    public string ToastWindowPlacement { get; set; } = "BottomRight";
    public string ToastWindowSize { get; set; } = "400, 300";
    public string ToastWindowLeftClickAction { get; set; } = "OpenUrl";
    public string ToastWindowRightClickAction { get; set; } = "CloseNotification";
    public string ToastWindowMiddleClickAction { get; set; } = "AnnotateImage";
    public bool ToastWindowAutoHide { get; set; } = true;
    public bool UseCustomCaptureSound { get; set; } = false;
    public string CustomCaptureSoundPath { get; set; } = "";
    public bool UseCustomTaskCompletedSound { get; set; } = false;
    public string CustomTaskCompletedSoundPath { get; set; } = "";
    public bool UseCustomErrorSound { get; set; } = false;
    public string CustomErrorSoundPath { get; set; } = "";
    public bool DisableNotifications { get; set; } = false;
    public bool DisableNotificationsOnFullscreen { get; set; } = false;
}

public class GradientColor
{
    public string Color { get; set; }
    public double Location { get; set; }
}

public class CaptureSettings
{
    public bool ShowCursor { get; set; } = true;
    public double ScreenshotDelay { get; set; } = 1.0;
    public bool CaptureTransparent { get; set; } = true;
    public bool CaptureShadow { get; set; } = true;
    public int CaptureShadowOffset { get; set; } = 0;
    public bool CaptureClientArea { get; set; } = true;
    public bool CaptureAutoHideTaskbar { get; set; } = false;
    public string CaptureCustomRegion { get; set; } = String.Empty;
    public string CaptureCustomWindow { get; set; } = String.Empty;
    public RegionCaptureOptions RegionCaptureOptions { get; set; }
    public FFmpegOptions FFmpegOptions { get; set; }
    public int ScreenRecordFPS { get; set; } = 30;
    public int GIFFPS { get; set; } = 15;
    public bool ScreenRecordShowCursor { get; set; } = true;
    public bool ScreenRecordAutoStart { get; set; } = true;
    public double ScreenRecordStartDelay { get; set; } = 2.0;
    public bool ScreenRecordFixedDuration { get; set; } = true;
    public double ScreenRecordDuration { get; set; } = 300.0;
    public bool ScreenRecordTwoPassEncoding { get; set; } = false;
    public bool ScreenRecordAskConfirmationOnAbort { get; set; } = true;
    public bool ScreenRecordTransparentRegion { get; set; } = true;
    public OCROptions OCROptions { get; set; }
}

public class OCROptions
{
    public string Language { get; set; }
    public double ScaleFactor { get; set; }
    public bool SingleLine { get; set; }
    public bool Silent { get; set; }
    public bool AutoCopy { get; set; }
    public List<ServiceLink> ServiceLinks { get; set; }
    public bool CloseWindowAfterOpeningServiceLink { get; set; }
    public int SelectedServiceLink { get; set; }
}

public class ServiceLink
{
    public string Name { get; set; }
    public string URL { get; set; }
}

public class UploadSettings
{
    public bool UseCustomTimeZone { get; set; }
    private TimeZoneInfo _customTimeZone;

    public TimeZoneInfo CustomTimeZone
    {
        get => _customTimeZone ??= TimeZoneInfo.Local;
        set => _customTimeZone = value;
    }
    public string NameFormatPattern { get; set; }
    public string NameFormatPatternActiveWindow { get; set; }
    public bool FileUploadUseNamePattern { get; set; }
    public bool FileUploadReplaceProblematicCharacters { get; set; }
    public bool URLRegexReplace { get; set; }
    public string URLRegexReplacePattern { get; set; }
    public string URLRegexReplaceReplacement { get; set; }
    public bool ClipboardUploadURLContents { get; set; }
    public bool ClipboardUploadShortenURL { get; set; }
    public bool ClipboardUploadShareURL { get; set; }
    public bool ClipboardUploadAutoIndexFolder { get; set; }
    public List<string> UploaderFilters { get; set; }
}

public class ToolsSettings
{
    public string ScreenColorPickerFormat { get; set; }
    public string ScreenColorPickerFormatCtrl { get; set; }
    public string ScreenColorPickerInfoText { get; set; }
    public PinToScreenOptions PinToScreenOptions { get; set; }
    public IndexerSettings IndexerSettings { get; set; }
    public ImageBeautifierOptions ImageBeautifierOptions { get; set; }
    public ImageCombinerOptions ImageCombinerOptions { get; set; }
    public VideoConverterOptions VideoConverterOptions { get; set; }
    public VideoThumbnailOptions VideoThumbnailOptions { get; set; }
    public BorderlessWindowSettings BorderlessWindowSettings { get; set; }
}

public class PinToScreenOptions
{
    public int InitialScale { get; set; }
    public int ScaleStep { get; set; }
    public bool HighQualityScale { get; set; }
    public int InitialOpacity { get; set; }
    public int OpacityStep { get; set; }
    public string Placement { get; set; }
    public int PlacementOffset { get; set; }
    public bool TopMost { get; set; }
    public bool KeepCenterLocation { get; set; }
    public string BackgroundColor { get; set; }
    public bool Shadow { get; set; }
    public bool Border { get; set; }
    public int BorderSize { get; set; }
    public string BorderColor { get; set; }
    public string MinimizeSize { get; set; }
}

public class ImageBeautifierOptions
{
    public int Margin { get; set; }
    public int Padding { get; set; }
    public bool SmartPadding { get; set; }
    public int RoundedCorner { get; set; }
    public int ShadowRadius { get; set; }
    public int ShadowOpacity { get; set; }
    public int ShadowDistance { get; set; }
    public int ShadowAngle { get; set; }
    public string ShadowColor { get; set; }
    public string BackgroundType { get; set; }
    public BackgroundGradient BackgroundGradient { get; set; }
    public string BackgroundColor { get; set; }
    public string BackgroundImageFilePath { get; set; }
}

public class BackgroundGradient
{
    public string Type { get; set; }
    public List<GradientColor> Colors { get; set; }
}

public class ImageCombinerOptions
{
    public string Orientation { get; set; }
    public string Alignment { get; set; }
    public int Space { get; set; }
    public int WrapAfter { get; set; }
    public bool AutoFillBackground { get; set; }
}

public class VideoConverterOptions
{
    public string InputFilePath { get; set; }
    public string OutputFolderPath { get; set; }
    public string OutputFileName { get; set; }
    public string VideoCodec { get; set; }
    public int VideoQuality { get; set; }
    public bool VideoQualityUseBitrate { get; set; }
    public int VideoQualityBitrate { get; set; }
    public bool UseCustomArguments { get; set; }
    public string CustomArguments { get; set; }
    public bool AutoOpenFolder { get; set; }
}

public class VideoThumbnailOptions
{
    public string DefaultOutputDirectory { get; set; }
    public string LastVideoPath { get; set; }
    public string OutputLocation { get; set; }
    public string CustomOutputDirectory { get; set; }
    public string ImageFormat { get; set; }
    public int ThumbnailCount { get; set; }
    public string FilenameSuffix { get; set; }
    public bool RandomFrame { get; set; }
    public bool UploadThumbnails { get; set; }
    public bool KeepScreenshots { get; set; }
    public bool OpenDirectory { get; set; }
    public int MaxThumbnailWidth { get; set; }
    public bool CombineScreenshots { get; set; }
    public int Padding { get; set; }
    public int Spacing { get; set; }
    public int ColumnCount { get; set; }
    public bool AddVideoInfo { get; set; }
    public bool AddTimestamp { get; set; }
    public bool DrawShadow { get; set; }
    public bool DrawBorder { get; set; }
}

public class BorderlessWindowSettings
{
    public bool RememberWindowTitle { get; set; }
    public string WindowTitle { get; set; }
    public bool AutoCloseWindow { get; set; }
    public bool ExcludeTaskbarArea { get; set; }
}

public class AdvancedSettings
{
    public bool ProcessImagesDuringFileUpload { get; set; }
    public bool ProcessImagesDuringClipboardUpload { get; set; }
    public bool ProcessImagesDuringExtensionUpload { get; set; }
    public bool UseAfterCaptureTasksDuringFileUpload { get; set; }
    public bool TextTaskSaveAsFile { get; set; }
    public bool AutoClearClipboard { get; set; }
    public bool RegionCaptureDisableAnnotation { get; set; }
    public List<string> ImageExtensions { get; set; }
    public List<string> TextExtensions { get; set; }
    public ProxySettings ProxySettings { get; set; } = new();
    public bool EarlyCopyURL { get; set; }
    public bool UserAnalyticsTelemetry => true;
    public bool CrashReportingTelemetry => true;
    public string TextFileExtension { get; set; }
    public string TextFormat { get; set; }
    public string TextCustom { get; set; }
    public bool TextCustomEncodeInput { get; set; }
    public bool ResultForceHTTPS { get; set; } = true;
    public string ClipboardContentFormat { get; set; }
    public string BalloonTipContentFormat { get; set; }
    public string OpenURLFormat { get; set; }
    public int AutoShortenURLLength { get; set; }
    public bool AutoCloseAfterUploadForm { get; set; }
    public int NamePatternMaxLength { get; set; }
    public int NamePatternMaxTitleLength { get; set; }
}

public class QuickTaskPreset
{
    public string Name { get; set; }
    public List<QuickTaskInfo> AfterCaptureTasks { get; set; }
    public List<QuickTaskInfo> AfterUploadTasks { get; set; }
}

public class Theme
{
    public string Name { get; set; }
    public string BackgroundColor { get; set; }
    public string LightBackgroundColor { get; set; }
    public string DarkBackgroundColor { get; set; }
    public string TextColor { get; set; }
    public string BorderColor { get; set; }
    public string CheckerColor { get; set; }
    public string CheckerColor2 { get; set; }
    public int CheckerSize { get; set; }
    public string LinkColor { get; set; }
    public string MenuHighlightColor { get; set; }
    public string MenuHighlightBorderColor { get; set; }
    public string MenuBorderColor { get; set; }
    public string MenuCheckBackgroundColor { get; set; }
    public string MenuFont { get; set; }
    public string ContextMenuFont { get; set; }
    public int ContextMenuOpacity { get; set; }
    public string SeparatorLightColor { get; set; }
    public string SeparatorDarkColor { get; set; }
}

public class ProxySettings
{
    public string ProxyMethod { get; set; }
    public string Host { get; set; }
    public int Port { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
}

public class HistorySettings
{
    public bool RememberWindowState { get; set; }
    public WindowState WindowState { get; set; }
    public int SplitterDistance { get; set; }
    public bool RememberSearchText { get; set; }
    public string SearchText { get; set; }
}

public class WindowState
{
    public string Location { get; set; }
    public string Size { get; set; }
    public bool IsMaximized { get; set; }
}

public class ImageHistorySettings
{
    public bool RememberWindowState { get; set; }
    public WindowState WindowState { get; set; }
    public string ThumbnailSize { get; set; }
    public int MaxItemCount { get; set; }
    public bool FilterMissingFiles { get; set; }
    public bool RememberSearchText { get; set; }
    public string SearchText { get; set; }
}

public class PrintSettings
{
    public int Margin { get; set; }
    public bool AutoRotateImage { get; set; }
    public bool AutoScaleImage { get; set; }
    public bool AllowEnlargeImage { get; set; }
    public bool CenterImage { get; set; }
    public string TextFont { get; set; }
    public bool ShowPrintDialog { get; set; }
    public string DefaultPrinterOverride { get; set; }
}

public class RootConfiguration
{
    public AdvancedSettings AdvancedSettings { get; set; } = new();
    public GeneralSettings GeneralSettings { get; set; } = new();
    public CaptureSettings CaptureSettings { get; set; } = new();
    public TaskSettings DefaultTaskSettings = TaskSettings.GetDefaultTaskSettings();
    public DateTime FirstTimeRunDate = DateTime.Now;
    public string FileUploadDefaultDirectory = "";
    public int NameParserAutoIncrementNumber = 0;
    public List<QuickTaskPreset> QuickTaskPresets = new();
    // Main window
    public bool FirstTimeMinimizeToTray = true;
    public List<int> TaskListViewColumnWidths = new();
    public int PreviewSplitterDistance = 335;
    public SupportedLanguage Language = SupportedLanguage.English;
    public bool ShowTray = true;
    public bool SilentRun = false;
    public bool TrayIconProgressEnabled = true;
    public bool TaskbarProgressEnabled = true;
    public bool UseWhiteShareXIcon = false;
    public bool RememberMainFormSize = false;
    public Point MainFormPosition = Point.Empty;
    public Size MainFormSize = Size.Empty;
    public HotkeyType TrayLeftClickAction = HotkeyType.RectangleRegion;
    public HotkeyType TrayLeftDoubleClickAction = HotkeyType.OpenMainWindow;
    public HotkeyType TrayMiddleClickAction = HotkeyType.ClipboardUploadWithContentViewer;
    public bool AutoCheckUpdate = true;
    public UpdateChannel UpdateChannel = UpdateChannel.Release;
    // TEMP: For backward compatibility
    public bool CheckPreReleaseUpdates = false;
    public bool UseCustomTheme { get; set; }
    public List<Theme> Themes { get; set; }
    public int SelectedTheme { get; set; }
    public bool UseCustomScreenshotsPath = false;
    public string CustomScreenshotsPath = "";
    public string SaveImageSubFolderPattern = "%y-%mo";
    public string SaveImageSubFolderPatternWindow = "";
    public bool ShowMenu = true;
    public TaskViewMode TaskViewMode = TaskViewMode.ThumbnailView;
    public bool ShowThumbnailTitle = true;
    public ThumbnailTitleLocation ThumbnailTitleLocation = ThumbnailTitleLocation.Top;
    public Size ThumbnailSize = new Size(200, 150);
    public ThumbnailViewClickAction ThumbnailClickAction = ThumbnailViewClickAction.Default;
    public bool ShowColumns = true;
    public ImagePreviewVisibility ImagePreview = ImagePreviewVisibility.Automatic;
    public ImagePreviewLocation ImagePreviewLocation = ImagePreviewLocation.Side;
    public bool AutoCleanupBackupFiles = false;
    public bool AutoCleanupLogFiles = false;
    public int CleanupKeepFileCount = 10;
    public ProxyInfo ProxySettings = new();
    public int UploadLimit = 5;
    public int BufferSizePower = 5;
    public List<string> ClipboardContentFormats { get; set; } = new();
    public int MaxUploadFailRetry = 1;
    public bool UseSecondaryUploaders = false;
    public List<Upload.ImageDestination> SecondaryImageUploaders = new();
    public List<Upload.TextDestination> SecondaryTextUploaders = new();
    public List<Upload.FileDestination> SecondaryFileUploaders = new();
    public bool HistorySaveTasks = true;
    public bool HistoryCheckURL = false;
    public List<RecentTask> RecentTasks { get; set; }
    public bool RecentTasksSave = false;
    public int RecentTasksMaxCount = 10;
    public bool RecentTasksShowInMainWindow = true;
    public bool RecentTasksShowInTrayMenu = true;
    public bool RecentTasksTrayMenuMostRecentFirst = false;
    public HistorySettings HistorySettings = new();
    public ImageHistorySettings ImageHistorySettings = new();
    public bool DontShowPrintSettingsDialog { get; set; }
    // public PrintSettings PrintSettings { get; set; }
    public Rectangle AutoCaptureRegion = Rectangle.Empty;
    public decimal AutoCaptureRepeatTime = 60;
    public bool AutoCaptureMinimizeToTray = true;
    public bool AutoCaptureWaitUpload = true;
    public Rectangle ScreenRecordRegion = Rectangle.Empty;
    public List<HotkeyType> ActionsToolbarList = new() { HotkeyType.RectangleRegion, HotkeyType.PrintScreen, HotkeyType.ScreenRecorder,
        HotkeyType.None, HotkeyType.FileUpload, HotkeyType.ClipboardUploadWithContentViewer };
    public bool ActionsToolbarRunAtStartup = false;
    public Point ActionsToolbarPosition = Point.Empty;
    public bool ActionsToolbarLockPosition = false;
    public bool ActionsToolbarStayTopMost = true;
    public List<Color> RecentColors = new();
    [Category("Application"), DefaultValue(false), Description("Calculate and show file sizes in binary units (KiB, MiB etc.)")]
    public bool BinaryUnits { get; set; }
    //
    [Category("Application"), DefaultValue(false), Description("Show most recent task first in main window.")]
    public bool ShowMostRecentTaskFirst { get; set; }
    //
    [Category("Application"), DefaultValue(false), Description("Show only customized tasks in main window workflows.")]
    public bool WorkflowsOnlyShowEdited { get; set; }
    //
    [Category("Application"), DefaultValue(false), Description("Automatically expand capture menu when you open the tray menu.")]
    public bool TrayAutoExpandCaptureMenu { get; set; }
    //
    [Category("Application"), DefaultValue(true), Description("Show tips and hotkeys in main window when task list is empty.")]
    public bool ShowMainWindowTip { get; set; }
    //
    [Category("Application"), DefaultValue(""),
     Description("Browser path for your favorite browser for SnapX Web Extension.")]
    public string BrowserPath = "";
    //
    //
    [Category("Application"), DefaultValue(false),
     Description("Save settings after task completed but only if there is no other active tasks.")]
    public bool SaveSettingsAfterTaskCompleted { get; set; } = false;
    //
    [Category("Application"), DefaultValue(false),
     Description("In main window when task is completed automatically select it.")]
    public bool AutoSelectLastCompletedTask { get; set; } = false;
    //
    [Category("Application"), DefaultValue(false), Description("Ultra secret mode.")]
    public bool DevMode
    {
        get
        {
#if DEBUG
            return true;
#else
            return false;
#endif
        }
    }
    //
    [Category("Hotkey"), DefaultValue(false), Description("Disables hotkeys.")]
    public bool DisableHotkeys { get; set; }
    //
    [Category("Hotkey"), DefaultValue(false), Description("If active window is fullscreen then hotkeys won't be executed.")]
    public bool DisableHotkeysOnFullscreen { get; set; }
    //
    private int hotkeyRepeatLimit;
    //
    [Category("Hotkey"), DefaultValue(500), Description("If you hold hotkeys then it will only trigger every this milliseconds.")]
    public int HotkeyRepeatLimit
    {
        get
        {
            return hotkeyRepeatLimit;
        }
        set
        {
            hotkeyRepeatLimit = Math.Max(value, 200);
        }
    }
    [Category("Clipboard"), DefaultValue(true), Description("Show clipboard content viewer when using clipboard upload in main window.")]
    public bool ShowClipboardContentViewer { get; set; }
    //
    [Category("Image"), DefaultValue(false), Description("Strip color space information chunks from PNG image.")]
    public bool PNGStripColorSpaceInformation { get; set; }
    //
    [Category("Image"), DefaultValue(true), Description("If JPEG exif contains orientation data then rotate image accordingly.")]
    public bool RotateImageByExifOrientationData { get; set; }
    //
    [Category("Upload"), DefaultValue(false), Description("Can be used to disable uploading application wide.")]
    public bool DisableUpload { get; set; }
    //
    [Category("Upload"), DefaultValue(false), Description("Accept invalid SSL certificates when uploading.")]
    public bool AcceptInvalidSSLCertificates { get; set; }
    //
    [Category("Upload"), DefaultValue(true), Description("Ignore emojis while URL encoding upload results.")]
    public bool URLEncodeIgnoreEmoji { get; set; }
    //
    [Category("Upload"), DefaultValue(true), Description("Show first time upload warning.")]
    public bool ShowUploadWarning { get; set; }
    //
    [Category("Upload"), DefaultValue(true), Description("Show more than 10 files upload warning.")]
    public bool ShowMultiUploadWarning { get; set; }
    //
    [Category("Upload"), DefaultValue(100), Description("Large file size defined in MB. SnapX will warn before uploading large files. 0 disables this feature.")]
    public int ShowLargeFileSizeWarning { get; set; }
    //
    [Category("Paths"),
     Description(
         "Custom uploaders configuration path. If you have already configured this setting in another device and you are attempting to use the same location, then backup the file before configuring this setting and restore after exiting SnapX.")]
    public string CustomUploadersConfigPath = "";
    //
    [Category("Paths"), Description("Custom hotkeys configuration path. If you have already configured this setting in another device and you are attempting to use the same location, then backup the file before configuring this setting and restore after exiting SnapX.")]
    public string CustomHotkeysConfigPath = "";
    [Category("Paths"), Description("Custom screenshot path (secondary location). If custom screenshot path is temporarily unavailable (e.g. network share), SnapX will use this location (recommended to be a local path).")]
    public string CustomScreenshotsPath2 = "";
    //
    [Category("Drag and drop window"), DefaultValue(150), Description("Size of drop window.")]
    public int DropSize { get; set; }

    [Category("Drag and drop window"), DefaultValue(5), Description("Position offset of drop window.")]
    public int DropOffset { get; set; }
    [Category("Drag and drop window"), DefaultValue(100), Description("Opacity of drop window.")]
    public int DropOpacity { get; set; }

    [Category("Drag and drop window"), DefaultValue(255), Description("When you drag file to drop window then opacity will change to this.")]
    public int DropHoverOpacity { get; set; }

    // [Category("Drag and drop window"), DefaultValue(ContentAlignment.BottomRight), Description("Where drop window will open.")]
    // public ContentAlignment DropAlignment { get; set; }
    public string ApplicationVersion => Assembly.GetExecutingAssembly().GetName().Version!.ToString();
}

