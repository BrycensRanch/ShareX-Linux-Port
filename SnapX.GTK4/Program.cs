using System.Reflection;
using System.Text;
using GdkPixbuf;
using Gio;
using GObject;
using Gtk;
using SixLabors.ImageSharp;
using SnapX.Core;
using SnapX.Core.Upload;
using SnapX.Core.Utils;
using SnapX.Core.Utils.Miscellaneous;
using SnapX.Core.Utils.Native;
using AboutDialog = SnapX.GTK4.AboutDialog;
using MessageType = Gst.MessageType;

var snapx = new SnapX.Core.SnapX();
snapx.setQualifier(" GTK4");



var application = Gtk.Application.New("io.github.brycensranch.SnapX", ApplicationFlags.NonUnique);
var sigintReceived = false;

Console.CancelKeyPress += (_, ea) =>
{
    ea.Cancel = true;
    sigintReceived = true;

    DebugHelper.WriteLine("Received SIGINT (Ctrl+C)");
    snapx.shutdown();
    Environment.Exit(0);
};
application.OnActivate += (sender, eventArgs) =>
{
    var errorStarting = false;
    try
    {
        snapx.start(args);
    }
    catch (Exception e)
    {
        errorStarting = true;
        DebugHelper.Logger.Fatal(e.ToString());
        ShowErrorDialog(e, application);

    }

    if (!errorStarting)
    {
        DebugHelper.WriteLine("Internal Startup time: {0} ms", snapx.getStartupTime());
        if (snapx.isSilent()) return;

        if (SnapX.Core.SnapX.CLIManager.IsCommandExist("video"))
        {
            Gst.Module.Initialize();
            GstVideo.Module.Initialize();
            Gst.Application.Init();
            var ret = Gst.Functions.ParseLaunch(
                "playbin uri=playbin uri=https://ftp.nluug.nl/pub/graphics/blender/demo/movies/ToS/ToS-4k-1920.mov");
            ret.SetState(Gst.State.Playing);
            var bus = ret.GetBus();
            bus.TimedPopFiltered(Gst.Constants.CLOCK_TIME_NONE, MessageType.Eos | MessageType.Error);
            ret.SetState(Gst.State.Null);
        }


        var mainWindow = new ApplicationWindow();
        mainWindow.SetApplication(application);
        mainWindow.SetName("SnapX");
        async void HandleFileSelectionRequested(NeedFileOpenerEvent @event)
        {
            var dialog = new FileChooserDialog()
            {
                Title = @event.Title,
                Application = application,
            };
            if (@event.FileName != null) dialog.SetCurrentName(@event.FileName);
            dialog.SetCurrentFolder(Gio.Functions.FileNewForPath(@event.Directory));
            if (@event.Multiselect)
            {
                dialog.SetSelectMultiple(true);
                dialog.Show();

                UploadManager.UploadFile(dialog.GetFile()?.GetPath()!);
            }
            else
            {
                dialog.Show();
                var file = dialog.GetFile();
                if (file == null)
                {
                    mainWindow.Title = "SnapX | File upload cancelled";
                    return;
                }
                UploadManager.UploadFile(file.GetPath()!);
            }

        }
        var eventAggregator = snapx.getEventAggregator();
        eventAggregator.Subscribe<NeedFileOpenerEvent>(HandleFileSelectionRequested);

        var box = new Box();
        box.SetOrientation(Orientation.Vertical);
        var imageURLTextBox = new Entry();
        imageURLTextBox.PlaceholderText =
            "https://fedoramagazine.org/wp-content/uploads/2024/10/Whats-new-in-Fedora-KDE-41-2-816x431.jpg";

        var demoTestButton = new Button();
        demoTestButton.Label = "Upload Remote Image";
        demoTestButton.OnClicked += (_, __) =>
        {
            DebugHelper.WriteLine("Upload Demo Image triggered");

            try
            {
                UploadManager.DownloadAndUploadFile(imageURLTextBox.GetText());
            }
            catch (Exception ex)
            {
                DebugHelper.Logger.Error(ex.ToString());
                ShowErrorDialog(ex, application);
            }
        };
        box.Append(imageURLTextBox);
        box.Append(demoTestButton);
        mainWindow.SetChild(box);
        mainWindow.SetVisible(true);
        var dialog = new AboutDialog();
        dialog.SetApplication(application);
        dialog.AddCreditSection("ShareX Team", [Links.Jaex, Links.McoreD]);
        dialog.AddCreditSection("Mentions", ["benbryant0"]);
        var gtkVersion = $"{Gtk.Functions.GetMajorVersion()}.{Gtk.Functions.GetMinorVersion()}.{Gtk.Functions.GetMicroVersion()}";
        var osInfo = OsInfo.GetFancyOSNameAndVersion();
        var assembly = Assembly.GetExecutingAssembly();
        foreach (var resourceName in assembly.GetManifestResourceNames())
        {
            Console.WriteLine(resourceName);
        }
        var bytes = assembly.ReadResourceAsByteArray("SnapX.GTK4.SnapX_Logo.png");
        var image = SixLabors.ImageSharp.Image.Load(bytes);
        using var memoryStream = new MemoryStream();
        image.SaveAsPng(memoryStream);
        var imageBytes = memoryStream.ToArray();


        var pixbuf = PixbufLoader.FromBytes(imageBytes);
        var logo = Gdk.Texture.NewForPixbuf(pixbuf);
        // dialog.SetDecorated(false);
        // dialog.SetFocusable(false);
        // dialog.SetOpacity(0.8);
        // dialog.SetResizable(false);
        // dialog.SetCanTarget(false);
        // dialog.SetCanFocus(false);
        // dialog.SetDeletable(false);
        // dialog.SetSensitive(false);
        // dialog.SetFocusVisible(false);
        dialog.SetModal(true);
        dialog.SetLogo(logo);

        dialog.SystemInformation = $"OS: {osInfo}\nGTK Version: {gtkVersion}\n.NET Version: {dialog.internalAboutDialog.GetRuntime()}\nPlatform: {dialog.internalAboutDialog.GetOsPlatform()}";

        dialog.Show();
        // var window = Gtk.ApplicationWindow.New((Gtk.Application) sender);
        // window.Title = "Gtk4 Window";
        // window.SetDefaultSize(300, 300);
        // window.Show();
    }
};
static void ShowErrorDialog(Exception ex, Gtk.Application application = null)
{
    // Create the error dialog window
    var errorDialog = new Window()
    {
        DefaultWidth = 600,
        DefaultHeight = 400,
        Application = application,
        Title = Lang.SnapXFailedToStart,
        Modal = true,
    };

    // Create a vertical container for message and buttons
    var vbox = new Box();
    vbox.SetOrientation(Orientation.Vertical);
    vbox.SetSpacing(10);

    // Build the error message with exception message and stack trace
    var messageBuilder = new StringBuilder();
    messageBuilder.AppendLine(ex.GetType() + ": " + ex.Message);
    messageBuilder.AppendLine(ex.StackTrace);
    messageBuilder.AppendLine(Assembly.GetExecutingAssembly().GetName().Name + ": " + Assembly.GetExecutingAssembly().GetName().Version);

    // TextView to show the error message
    var textView = new TextView
    {
        Editable = false,
        WrapMode = WrapMode.WordChar,
        LeftMargin = 10,
        RightMargin = 10,
    };
    textView.Buffer!.Text = messageBuilder.ToString();

    // Scroll the text view inside a scrolled window
    var scrolledWindow = new ScrolledWindow
    {
        Child = textView
    };
    scrolledWindow.SetMinContentHeight(640);
    scrolledWindow.SetMinContentWidth(900);
    scrolledWindow.SetPropagateNaturalWidth(true);
    scrolledWindow.SetPropagateNaturalHeight(true);
    vbox.Append(scrolledWindow);

    var buttonBox = new Box()
    {
        Halign = Align.Center,
        Spacing = 10,
        MarginBottom = 10
    };

    // "Report Error" button
    var reportButton = new Button();
    reportButton.Label = Lang.ReportErrorToSentry;
    reportButton.OnClicked += (sender, e) => OnReportErrorClicked(ex);
    buttonBox.Append(reportButton);

    var githubButton = new Button();
    githubButton.Label = Lang.CreateGitHubIssue;
    githubButton.OnClicked += (sender, e) => onGitHubButtonClicked(ex);
    buttonBox.Append(githubButton);

    // "Copy Error" button
    var copyButton = new Button();
    copyButton.Label = Lang.CopyErrorToClipboard;
    copyButton.OnClicked += (sender, e) => OnCopyErrorClicked(ex);
    buttonBox.Append(copyButton);

    // Pack the buttons into the main vertical box
    vbox.Append(buttonBox);

    // Add everything to the window
    errorDialog.SetChild(vbox);

    errorDialog.Show();

    errorDialog.OnDestroy += (o, e) => Environment.Exit(1);
}

static void OnReportErrorClicked(Exception ex)
{
    DebugHelper.WriteLine("Sentry error reporting is not implemented.");
}

static void onGitHubButtonClicked(Exception ex)
{
    var newIssueURL = Helpers.GitHubIssueReport(ex);
    if (newIssueURL == null) return;
    URLHelpers.OpenURL(newIssueURL);
}
static void OnCopyErrorClicked(Exception ex)
{
    Clipboard.CopyText(ex.ToString());
    DebugHelper.WriteLine("Copied error to clipboard");
}
application.OnShutdown += (sender, eventArgs) =>
{
    sigintReceived = true;
    snapx.shutdown();
};
application.OnWindowAdded += (sender, eventArgs) =>
{
    DebugHelper.WriteLine("Actual UI Startup time: {0} ms", snapx.getStartupTime());
};

application.Run(0, args);
