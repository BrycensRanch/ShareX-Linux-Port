using System.Diagnostics;
using System.Reflection;
using System.Text;
using ShareX.Core.CLI;
using ShareX.Core.Hotkey;
using ShareX.Core.Task;
using ShareX.Core.Upload;
using ShareX.Core.Upload.BaseUploaders;
using ShareX.Core.Utils;
using ShareX.Core.Utils.Extensions;
using ShareX.Core.Utils.Miscellaneous;
using ShareX.Core.Watch;
using Xdg.Directories;

namespace ShareX.Core;

public class ShareX
{
        public const string AppName = "ShareX";
        public const string MutexName = "82E6AC09-0FEF-4390-AD9F-0DD3F5561EFC";
        public static readonly string PipeName = $"{Environment.MachineName}-{Environment.UserName}-{AppName}";

        public const ShareXBuild Build =
#if RELEASE
            ShareXBuild.Release;
#elif DEBUG
            ShareXBuild.Debug;
#else
            ShareXBuild.Unknown;
#endif

    public static string VersionText
    {
        get
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version!;
            var versionString = $"{version.Major}.{version.Minor}.{version.Revision}";
            if (version.Build > 0)
                versionString += $".{version.Build}";
            if (Dev)
                versionString += " Dev";
            if (Portable)
                versionString += " Portable";

            return versionString;
        }
    }

        public static void quit()
        {
            CloseSequence();
        }
        public static string Title
        {
            get
            {
                string title = $"{AppName} {VersionText}";

                if (Settings != null && Settings.DevMode)
                {
                    string info = Build.ToString();

                    if (IsAdmin)
                    {
                        info += ", Admin";
                    }

                    title += $" ({info})";
                }

                return title;
            }
        }

        public static string TitleShort
        {
            get
            {
                if (Settings != null && Settings.DevMode)
                {
                    return Title;
                }

                return AppName;
            }
        }

        public static bool Dev { get; } = true;
        public static bool MultiInstance { get; private set; }
        public static bool Portable { get; private set; }
        public static bool SilentRun { get; private set; }
        public static bool Sandbox { get; private set; }
        public static bool IsAdmin { get; private set; }
        public static bool SteamFirstTimeConfig { get; private set; }
        public static bool IgnoreHotkeyWarning { get; private set; }
        public static bool PuushMode { get; private set; }

        internal static ApplicationConfig Settings { get; set; }
        internal static TaskSettings DefaultTaskSettings { get; set; }
        internal static UploadersConfig UploadersConfig { get; set; }
        internal static HotkeysConfig HotkeysConfig { get; set; }

        internal static Stopwatch StartTimer { get; private set; }
        internal static HotkeyManager HotkeyManager { get; set; }
        internal static WatchFolderManager WatchFolderManager { get; set; }

        #region Paths

        private const string PersonalPathConfigFileName = "PersonalPath.cfg";

        public static readonly string DefaultPersonalFolder = Path.Combine(UserDirectory.DocumentsDir, AppName);
        public static readonly string PortablePersonalFolder = FileHelpers.GetAbsolutePath(AppName);

        private static string PersonalPathConfigFilePath
        {
            get
            {
                string relativePath = FileHelpers.GetAbsolutePath(PersonalPathConfigFileName);

                if (File.Exists(relativePath))
                {
                    return relativePath;
                }

                return CurrentPersonalPathConfigFilePath;
            }
        }

        private static readonly string CurrentPersonalPathConfigFilePath = Path.Combine(DefaultPersonalFolder, PersonalPathConfigFileName);

        private static readonly string PreviousPersonalPathConfigFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            AppName, PersonalPathConfigFileName);

        private static readonly string PortableCheckFilePath = FileHelpers.GetAbsolutePath("Portable");

        private static string CustomPersonalPath { get; set; }

        public static string PersonalFolder
        {
            get
            {
                if (!string.IsNullOrEmpty(CustomPersonalPath))
                {
                    return FileHelpers.ExpandFolderVariables(CustomPersonalPath);
                }

                return DefaultPersonalFolder;
            }
        }

        public const string HistoryFileName = "History.json";

        public static string HistoryFilePath
        {
            get
            {
                if (Sandbox) return null;

                return Path.Combine(PersonalFolder, HistoryFileName);
            }
        }

        public const string HistoryFileNameOld = "History.xml";

        public static string HistoryFilePathOld
        {
            get
            {
                if (Sandbox) return null;

                return Path.Combine(PersonalFolder, HistoryFileNameOld);
            }
        }

        public const string LogsFolderName = "Logs";

        public static string LogsFolder => Path.Combine(PersonalFolder, LogsFolderName);

        public static string LogsFilePath
        {
            get
            {
                // if (SystemOptions.DisableLogging)
                // {
                //     return null;
                // }

                string fileName = string.Format("ShareX-Log-{0:yyyy-MM}.txt", DateTime.Now);
                return Path.Combine(LogsFolder, fileName);
            }
        }

        public static string ScreenshotsParentFolder
        {
            get
            {
                if (Settings != null && Settings.UseCustomScreenshotsPath)
                {
                    string path = Settings.CustomScreenshotsPath;
                    string path2 = Settings.CustomScreenshotsPath2;

                    if (!string.IsNullOrEmpty(path))
                    {
                        path = FileHelpers.ExpandFolderVariables(path);

                        if (string.IsNullOrEmpty(path2) || Directory.Exists(path))
                        {
                            return path;
                        }
                    }

                    if (!string.IsNullOrEmpty(path2))
                    {
                        path2 = FileHelpers.ExpandFolderVariables(path2);

                        if (Directory.Exists(path2))
                        {
                            return path2;
                        }
                    }
                }

                return Path.Combine(PersonalFolder, "Screenshots");
            }
        }

        public static string ImageEffectsFolder => Path.Combine(PersonalFolder, "ImageEffects");

        private static string PersonalPathDetectionMethod;

        #endregion Paths

        private static bool closeSequenceStarted, restartRequested, restartAsAdmin;

        public void start()
        {
            start(Array.Empty<string>());
        }

        public void shutdown()
        {
            CloseSequence();
        }
        public void start(string[] args)
        {
            HandleExceptions();

            StartTimer = Stopwatch.StartNew();
            // TODO: Implement CLI in a better way than what it is now.
            // CLI = new CLIManager(args);
            // CLI.ParseCommands();

            if (CheckAdminTasks()) return; // If ShareX opened just for be able to execute task as Admin

            // SystemOptions.UpdateSystemOptions();
            UpdatePersonalPath();

            DebugHelper.Init(LogsFilePath);

            // MultiInstance = CLI.IsCommandExist("multi", "m");

            using var singleInstanceManager = new SingleInstanceManager(MutexName, PipeName, !MultiInstance, args);
            if (!singleInstanceManager.IsSingleInstance || singleInstanceManager.IsFirstInstance)
            {
                singleInstanceManager.ArgumentsReceived += SingleInstanceManager_ArgumentsReceived;


                Run();

                if (restartRequested)
                {
                    DebugHelper.WriteLine("Restart is not implemented.");
                }
            }

            DebugHelper.Flush();
        }


        private static void Run()
        {
            DebugHelper.WriteLine("ShareX  (Linux) starting.");
            DebugHelper.WriteLine("Version: " + VersionText);
            DebugHelper.WriteLine("Build: " + Build);
            DebugHelper.WriteLine("Command line: " + Environment.CommandLine);
            DebugHelper.WriteLine("Personal path: " + PersonalFolder);
            if (!string.IsNullOrEmpty(PersonalPathDetectionMethod))
            {
                DebugHelper.WriteLine("Personal path detection method: " + PersonalPathDetectionMethod);
            }
            DebugHelper.WriteLine("Operating system: " + Helpers.GetOperatingSystemProductName(true));
            IsAdmin = Helpers.IsAdministrator();
            DebugHelper.WriteLine("Running as elevated process: " + IsAdmin);

            // SilentRun = CLI.IsCommandExist("silent", "s");

            // IgnoreHotkeyWarning = CLI.IsCommandExist("NoHotkeys");

            CreateParentFolders();
            RegisterExtensions();
            CheckPuushMode();
            DebugWriteFlags();
            SettingManager.LoadInitialSettings();
            // SettingManager.LoadAllSettings();

            Uploader.UpdateServicePointManager();
            // CleanupManager.CleanupAsync();

        }

        public static void CloseSequence()
        {
            if (!closeSequenceStarted)
            {
                closeSequenceStarted = true;

                DebugHelper.WriteLine("ShareX closing.");

                WatchFolderManager?.Dispose();
                SettingManager.SaveAllSettings();

                DebugHelper.WriteLine("ShareX closed.");
            }
        }

        private static void SingleInstanceManager_ArgumentsReceived(string[] arguments)
        {
            string message = "Arguments received: ";

            if (arguments == null)
            {
                message += "null";
            }
            else
            {
                message += "\"" + string.Join(" ", arguments) + "\"";
            }

            DebugHelper.WriteLine(message);
        }

        private static void UpdatePersonalPath()
        {
            // Sandbox = CLI.IsCommandExist("sandbox");

            if (!Sandbox)
            {
                // if (CLI.IsCommandExist("portable", "p"))
                // {
                //     Portable = true;
                //     CustomPersonalPath = PortablePersonalFolder;
                //     PersonalPathDetectionMethod = "Portable CLI flag";
                // }
                if (File.Exists(PortableCheckFilePath))
                {
                    Portable = true;
                    CustomPersonalPath = PortablePersonalFolder;
                    PersonalPathDetectionMethod = $"Portable file ({PortableCheckFilePath})";
                }
                // else if (!string.IsNullOrEmpty(SystemOptions.PersonalPath))
                // {
                //     CustomPersonalPath = SystemOptions.PersonalPath;
                //     PersonalPathDetectionMethod = "Registry";
                // }
                else
                {
                    MigratePersonalPathConfig();

                    string customPersonalPath = ReadPersonalPathConfig();

                    if (!string.IsNullOrEmpty(customPersonalPath))
                    {
                        CustomPersonalPath = FileHelpers.GetAbsolutePath(customPersonalPath);
                        PersonalPathDetectionMethod = $"PersonalPath.cfg file ({PersonalPathConfigFilePath})";
                    }
                }

                if (!Directory.Exists(PersonalFolder))
                {
                    try
                    {
                        Directory.CreateDirectory(PersonalFolder);
                    }
                    catch (Exception e)
                    {
                        StringBuilder sb = new StringBuilder();

                        sb.AppendFormat("{0} \"{1}\"", "Unable to create personal folder!", PersonalFolder);
                        sb.AppendLine();

                        if (!string.IsNullOrEmpty(PersonalPathDetectionMethod))
                        {
                            sb.AppendLine("Personal path detection method: " + PersonalPathDetectionMethod);
                        }

                        sb.AppendLine();
                        sb.Append(e);

                        CustomPersonalPath = "";
                    }
                }
            }
        }

        private static void CreateParentFolders()
        {
            if (!Sandbox && Directory.Exists(PersonalFolder))
            {
                FileHelpers.CreateDirectory(SettingManager.BackupFolder);
                FileHelpers.CreateDirectory(ImageEffectsFolder);
                FileHelpers.CreateDirectory(ScreenshotsParentFolder);
            }
        }

        private static void RegisterExtensions()
        {
        }

        public static void UpdateHelpersSpecialFolders()
        {
            Dictionary<string, string> specialFolders = new Dictionary<string, string>();
            specialFolders.Add("ShareXImageEffects", ImageEffectsFolder);
            HelpersOptions.ShareXSpecialFolders = specialFolders;
        }

        private static void MigratePersonalPathConfig()
        {
            if (File.Exists(PreviousPersonalPathConfigFilePath))
            {
                try
                {
                    if (!File.Exists(CurrentPersonalPathConfigFilePath))
                    {
                        FileHelpers.CreateDirectoryFromFilePath(CurrentPersonalPathConfigFilePath);
                        File.Move(PreviousPersonalPathConfigFilePath, CurrentPersonalPathConfigFilePath);
                    }

                    File.Delete(PreviousPersonalPathConfigFilePath);
                    Directory.Delete(Path.GetDirectoryName(PreviousPersonalPathConfigFilePath));
                }
                catch (Exception e)
                {
                    e.ShowError();
                }
            }
        }

        public static string ReadPersonalPathConfig()
        {
            if (File.Exists(PersonalPathConfigFilePath))
            {
                return File.ReadAllText(PersonalPathConfigFilePath, Encoding.UTF8).Trim();
            }

            return "";
        }

        public static bool WritePersonalPathConfig(string path)
        {
            if (path == null)
            {
                path = "";
            }
            else
            {
                path = path.Trim();
            }

            bool isDefaultPath = string.IsNullOrEmpty(path) && !File.Exists(PersonalPathConfigFilePath);

            if (!isDefaultPath)
            {
                string currentPath = ReadPersonalPathConfig();

                if (!path.Equals(currentPath, StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        FileHelpers.CreateDirectoryFromFilePath(PersonalPathConfigFilePath);
                        File.WriteAllText(PersonalPathConfigFilePath, path, Encoding.UTF8);
                        return true;
                    }
                    catch (UnauthorizedAccessException e)
                    {
                        DebugHelper.WriteException(e);
                    }
                    catch (Exception e)
                    {
                        DebugHelper.WriteException(e);
                        e.ShowError();
                    }
                }
            }

            return false;
        }

        private static void HandleExceptions()
        {
#if DEBUG
            if (Debugger.IsAttached)
            {
                return;
            }
#endif

            // Add the event handler for handling non-UI thread exceptions to the event
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        private static void Application_ThreadException(object sender, ThreadExceptionEventArgs e) => OnError(e.Exception);
        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e) => OnError((Exception)e.ExceptionObject);
        private static void OnError(Exception e) => DebugHelper.WriteException(e);

        private static bool CheckAdminTasks()
        {
            // if (CLI.IsCommandExist("dnschanger"))
            // {
            //     return true;
            // }

            return false;
        }

        private static bool CheckPuushMode()
        {
            var puushPath = FileHelpers.GetAbsolutePath("puush");
            PuushMode = File.Exists(puushPath);
            return PuushMode;
        }

        private static void DebugWriteFlags()
        {
            var flags = new List<string>();

            if (Dev) flags.Add(nameof(Dev));
            if (MultiInstance) flags.Add(nameof(MultiInstance));
            if (Portable) flags.Add(nameof(Portable));
            if (SilentRun) flags.Add(nameof(SilentRun));
            if (Sandbox) flags.Add(nameof(Sandbox));
            if (IgnoreHotkeyWarning) flags.Add(nameof(IgnoreHotkeyWarning));
            // if (SystemOptions.DisableUpdateCheck) flags.Add(nameof(SystemOptions.DisableUpdateCheck));
            // if (SystemOptions.DisableUpload) flags.Add(nameof(SystemOptions.DisableUpload));
            // if (SystemOptions.DisableLogging) flags.Add(nameof(SystemOptions.DisableLogging));
            if (PuushMode) flags.Add(nameof(PuushMode));

            var output = string.Join(", ", flags);
            DebugHelper.WriteLine("Flags: " + output);
        }
    }

