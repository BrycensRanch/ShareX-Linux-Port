using Avalonia.Controls;

namespace SnapX.Avalonia;

public class AboutDialog : SnapX.CommonUI.AboutDialog
{
    private Window _aboutWindow;

    public AboutDialog()
    {
        _aboutWindow = new Window
        {
            Title = GetTitle(),
            Width = 400,
            Height = 300,
            Icon = null,
            IsVisible = true,
            CanResize = false
        };

    }
    public override string GetTitle() => "About SnapX";

    public override void Show()
    {
        string output =
            $"{GetDescription()}\n" +
            $"Version: {GetVersion()}\n" +
            $"{GetCopyright()}\n" +
            $"Licensed under {GetLicense()}\n" +
            $"GitHub: {GetWebsite()}\n" +
            $"OS: {GetSystemInfo()} ({GetOsArchitecture()})\n" +
            $".NET Version: {GetRuntime()}\n" +
            $"Platform: {GetOsPlatform()}\n";

        _aboutWindow.Content = output;
    }

    public void Close()
    {
        _aboutWindow.Close();
    }
}